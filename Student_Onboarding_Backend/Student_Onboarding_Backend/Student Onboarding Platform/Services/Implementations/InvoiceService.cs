using Microsoft.Extensions.Logging;
using Student_Onboarding_Platform.Data.Repositories.Interfaces;
using Student_Onboarding_Platform.Models.DTOs.Common;
using Student_Onboarding_Platform.Models.DTOs.Invoice;
using Student_Onboarding_Platform.Models.Entities;
using Student_Onboarding_Platform.Services.Interfaces;

namespace Student_Onboarding_Platform.Services.Implementations;

public class InvoiceService : IInvoiceService
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IOrganizationSettingsRepository _orgRepository;
    private readonly ICourseRegistrationRepository _registrationRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<InvoiceService> _logger;

    public InvoiceService(
        IInvoiceRepository invoiceRepository,
        IOrganizationSettingsRepository orgRepository,
        ICourseRegistrationRepository registrationRepository,
        ICourseRepository courseRepository,
        IUserRepository userRepository,
        ILogger<InvoiceService> logger)
    {
        _invoiceRepository = invoiceRepository;
        _orgRepository = orgRepository;
        _registrationRepository = registrationRepository;
        _courseRepository = courseRepository;
        _userRepository = userRepository;
        _logger = logger;
    }

    // ── Get or create ──────────────────────────────────────────────────────

    public async Task<ApiResponse<InvoiceDto>> GetOrCreateForRegistrationAsync(
        Guid registrationId, Guid? requestingUserId, bool isAdmin)
    {
        var registration = await _registrationRepository.GetByIdAsync(registrationId);
        if (registration == null)
            return ApiResponse<InvoiceDto>.Fail("Course registration not found.");

        if (!isAdmin && registration.UserId != requestingUserId)
            return ApiResponse<InvoiceDto>.Fail("You are not authorized to access this invoice.");

        var existing = await _invoiceRepository.GetByRegistrationIdAsync(registrationId);
        if (existing != null)
        {
            var items = await _invoiceRepository.GetItemsAsync(existing.Id);
            return ApiResponse<InvoiceDto>.Ok(BuildDto(existing, items));
        }

        // Students can only obtain an invoice once a payment has been recorded.
        if (!isAdmin && string.Equals(registration.PaymentStatus, "Pending", StringComparison.OrdinalIgnoreCase))
            return ApiResponse<InvoiceDto>.Fail("An invoice is available only after your payment is recorded.");

        var created = await CreateForRegistrationAsync(registration);
        return ApiResponse<InvoiceDto>.Ok(created);
    }

    private async Task<InvoiceDto> CreateForRegistrationAsync(CourseRegistration registration)
    {
        var user = await _userRepository.GetByIdAsync(registration.UserId);
        var course = await _courseRepository.GetByIdAsync(registration.CourseId);
        var org = await _orgRepository.GetAsync();

        var now = DateTime.UtcNow;
        var paymentDate = registration.PaymentDate ?? (IsPaidLike(registration.PaymentStatus) ? now : (DateTime?)null);
        var year = (paymentDate ?? now).Year;
        var seq = await _invoiceRepository.GetMaxSequenceForYearAsync(year) + 1;
        var prefix = string.IsNullOrWhiteSpace(org?.InvoicePrefix) ? "INV" : org!.InvoicePrefix;

        var unitPrice = course?.OfferPrice ?? course?.Fees ?? registration.PaymentAmount ?? 0m;
        var taxPercent = org?.DefaultTaxPercent ?? 0m;

        var item = new InvoiceItem
        {
            Id = Guid.NewGuid(),
            Description = BuildItemDescription(course),
            Quantity = 1,
            UnitPrice = unitPrice,
            TaxPercent = taxPercent,
            DiscountAmount = 0,
            SortOrder = 0
        };
        var items = new List<InvoiceItem> { item };

        var invoice = new Invoice
        {
            Id = Guid.NewGuid(),
            RegistrationId = registration.Id,
            UserId = registration.UserId,
            InvoiceNumber = $"{prefix}-{year}-{seq:D6}",
            InvoiceYear = year,
            SequenceNumber = seq,
            ReceiptNumber = $"RCPT-{year}-{seq:D6}",
            TransactionId = null,
            OrderId = registration.Id.ToString("N").Substring(0, 12).ToUpperInvariant(),
            ReferenceNumber = registration.Id.ToString(),
            InvoiceDate = now,
            PaymentDate = paymentDate,
            PaymentStatus = registration.PaymentStatus,
            PaymentMethod = "Manual",
            PaymentGateway = null,
            CurrencyCode = org?.CurrencyCode ?? "INR",
            CurrencySymbol = org?.CurrencySymbol ?? "₹",
            ConvenienceFee = 0,
            PlatformFee = 0,
            RefundAmount = string.Equals(registration.PaymentStatus, "Refunded", StringComparison.OrdinalIgnoreCase)
                ? (registration.PaymentAmount ?? 0m) : 0m,
            Notes = org?.DefaultNotes,
            Terms = org?.DefaultTerms,
            // Organization snapshot
            OrgName = org?.OrgName,
            OrgAddressLine1 = org?.AddressLine1,
            OrgAddressLine2 = org?.AddressLine2,
            OrgCity = org?.City,
            OrgState = org?.State,
            OrgPostalCode = org?.PostalCode,
            OrgCountry = org?.Country,
            OrgPhone = org?.Phone,
            OrgEmail = org?.Email,
            OrgWebsite = org?.Website,
            OrgTaxRegNo = org?.TaxRegNo,
            OrgLogoUrl = org?.LogoUrl,
            OrgFooterNote = org?.FooterNote,
            // Customer snapshot
            CustomerName = user != null ? $"{user.FirstName} {user.LastName}".Trim() : null,
            CustomerEmail = user?.Email,
            CustomerPhone = user?.PhoneNumber,
            CustomerBillingAddress = user?.Address,
            CreatedAt = now,
            UpdatedAt = null
        };

        RecomputeTotals(invoice, items, registration.PaymentAmount);

        await _invoiceRepository.CreateAsync(invoice, items);
        _logger.LogInformation("Created invoice {InvoiceNumber} for registration {RegistrationId}",
            invoice.InvoiceNumber, registration.Id);

        return BuildDto(invoice, items);
    }

    // ── Student reads ──────────────────────────────────────────────────────

    public async Task<ApiResponse<IEnumerable<InvoiceListItemDto>>> GetForStudentAsync(Guid userId)
    {
        var invoices = await _invoiceRepository.GetByUserIdAsync(userId);
        var list = new List<InvoiceListItemDto>();
        foreach (var inv in invoices)
        {
            var items = await _invoiceRepository.GetItemsAsync(inv.Id);
            list.Add(ToListItem(inv, items));
        }
        return ApiResponse<IEnumerable<InvoiceListItemDto>>.Ok(list);
    }

    public async Task<ApiResponse<InvoiceDto>> GetByIdForStudentAsync(Guid invoiceId, Guid userId)
    {
        var invoice = await _invoiceRepository.GetByIdAsync(invoiceId);
        if (invoice == null || invoice.UserId != userId)
            return ApiResponse<InvoiceDto>.Fail("Invoice not found.");

        var items = await _invoiceRepository.GetItemsAsync(invoice.Id);
        return ApiResponse<InvoiceDto>.Ok(BuildDto(invoice, items));
    }

    // ── Admin reads / edit ───────────────────────────────────────────────────

    public async Task<ApiResponse<PaginatedResponse<InvoiceListItemDto>>> GetAllAsync(int page, int pageSize)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 20;
        var offset = (page - 1) * pageSize;

        var invoices = await _invoiceRepository.GetAllAsync(offset, pageSize);
        var totalCount = await _invoiceRepository.GetAllCountAsync();

        var items = new List<InvoiceListItemDto>();
        foreach (var inv in invoices)
        {
            var lineItems = await _invoiceRepository.GetItemsAsync(inv.Id);
            items.Add(ToListItem(inv, lineItems));
        }

        var response = new PaginatedResponse<InvoiceListItemDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
        return ApiResponse<PaginatedResponse<InvoiceListItemDto>>.Ok(response);
    }

    public async Task<ApiResponse<InvoiceDto>> GetByIdAsync(Guid invoiceId)
    {
        var invoice = await _invoiceRepository.GetByIdAsync(invoiceId);
        if (invoice == null)
            return ApiResponse<InvoiceDto>.Fail("Invoice not found.");

        var items = await _invoiceRepository.GetItemsAsync(invoice.Id);
        return ApiResponse<InvoiceDto>.Ok(BuildDto(invoice, items));
    }

    public async Task<ApiResponse<InvoiceDto>> UpdateAsync(Guid invoiceId, UpdateInvoiceRequest request)
    {
        var invoice = await _invoiceRepository.GetByIdAsync(invoiceId);
        if (invoice == null)
            return ApiResponse<InvoiceDto>.Fail("Invoice not found.");

        // Scalar fields
        invoice.ReceiptNumber = request.ReceiptNumber;
        invoice.TransactionId = request.TransactionId;
        invoice.OrderId = request.OrderId;
        invoice.ReferenceNumber = request.ReferenceNumber;
        if (request.InvoiceDate.HasValue) invoice.InvoiceDate = request.InvoiceDate.Value;
        invoice.PaymentDate = request.PaymentDate;
        if (!string.IsNullOrWhiteSpace(request.PaymentStatus)) invoice.PaymentStatus = request.PaymentStatus!;
        invoice.PaymentMethod = request.PaymentMethod;
        invoice.PaymentGateway = request.PaymentGateway;
        invoice.ConvenienceFee = request.ConvenienceFee;
        invoice.PlatformFee = request.PlatformFee;
        invoice.RefundAmount = request.RefundAmount;
        invoice.Notes = request.Notes;
        invoice.Terms = request.Terms;

        // Organization override
        if (request.Organization != null)
        {
            var o = request.Organization;
            invoice.OrgName = o.Name;
            invoice.OrgAddressLine1 = o.AddressLine1;
            invoice.OrgAddressLine2 = o.AddressLine2;
            invoice.OrgCity = o.City;
            invoice.OrgState = o.State;
            invoice.OrgPostalCode = o.PostalCode;
            invoice.OrgCountry = o.Country;
            invoice.OrgPhone = o.Phone;
            invoice.OrgEmail = o.Email;
            invoice.OrgWebsite = o.Website;
            invoice.OrgTaxRegNo = o.TaxRegNo;
            invoice.OrgLogoUrl = o.LogoUrl;
            invoice.OrgFooterNote = o.FooterNote;
        }

        // Customer override
        if (request.Customer != null)
        {
            invoice.CustomerName = request.Customer.Name;
            invoice.CustomerEmail = request.Customer.Email;
            invoice.CustomerPhone = request.Customer.Phone;
            invoice.CustomerBillingAddress = request.Customer.BillingAddress;
        }

        // Rebuild line items
        var items = (request.Items ?? new List<UpdateInvoiceItemRequest>())
            .Select((it, idx) => new InvoiceItem
            {
                Id = Guid.NewGuid(),
                InvoiceId = invoice.Id,
                Description = it.Description ?? string.Empty,
                Quantity = it.Quantity,
                UnitPrice = it.UnitPrice,
                TaxPercent = it.TaxPercent,
                DiscountAmount = it.DiscountAmount,
                SortOrder = it.SortOrder != 0 ? it.SortOrder : idx
            })
            .ToList();

        RecomputeTotals(invoice, items, request.AmountPaid);
        invoice.UpdatedAt = DateTime.UtcNow;

        await _invoiceRepository.UpdateAsync(invoice, items);
        _logger.LogInformation("Updated invoice {InvoiceNumber}", invoice.InvoiceNumber);

        return ApiResponse<InvoiceDto>.Ok(BuildDto(invoice, items), "Invoice updated successfully.");
    }

    // ── Organization settings ────────────────────────────────────────────────

    public async Task<ApiResponse<OrganizationSettingsDto>> GetOrganizationSettingsAsync()
    {
        var org = await _orgRepository.GetAsync();
        if (org == null)
            return ApiResponse<OrganizationSettingsDto>.Ok(new OrganizationSettingsDto());
        return ApiResponse<OrganizationSettingsDto>.Ok(ToOrgDto(org));
    }

    public async Task<ApiResponse<OrganizationSettingsDto>> UpdateOrganizationSettingsAsync(UpdateOrganizationSettingsRequest request)
    {
        var org = await _orgRepository.GetAsync();
        if (org == null)
            return ApiResponse<OrganizationSettingsDto>.Fail(
                "Organization settings are not initialized. Apply migration 019 to seed the settings row.");

        org.OrgName = request.OrgName;
        org.AddressLine1 = request.AddressLine1;
        org.AddressLine2 = request.AddressLine2;
        org.City = request.City;
        org.State = request.State;
        org.PostalCode = request.PostalCode;
        org.Country = request.Country;
        org.Phone = request.Phone;
        org.Email = request.Email;
        org.Website = request.Website;
        org.TaxRegNo = request.TaxRegNo;
        org.LogoUrl = request.LogoUrl;
        org.CurrencyCode = string.IsNullOrWhiteSpace(request.CurrencyCode) ? "INR" : request.CurrencyCode;
        org.CurrencySymbol = string.IsNullOrWhiteSpace(request.CurrencySymbol) ? "₹" : request.CurrencySymbol;
        org.DefaultTaxPercent = request.DefaultTaxPercent;
        org.InvoicePrefix = string.IsNullOrWhiteSpace(request.InvoicePrefix) ? "INV" : request.InvoicePrefix;
        org.DefaultNotes = request.DefaultNotes;
        org.DefaultTerms = request.DefaultTerms;
        org.FooterNote = request.FooterNote;
        org.UpdatedAt = DateTime.UtcNow;

        await _orgRepository.UpdateAsync(org);
        return ApiResponse<OrganizationSettingsDto>.Ok(ToOrgDto(org), "Organization settings updated.");
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static bool IsPaidLike(string? status) =>
        string.Equals(status, "Paid", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(status, "Partial", StringComparison.OrdinalIgnoreCase);

    private static string BuildItemDescription(Course? course)
    {
        if (course == null) return "Course Registration";
        var desc = course.Name;
        if (!string.IsNullOrWhiteSpace(course.Duration)) desc += $" ({course.Duration})";
        return desc;
    }

    /// <summary>
    /// Recomputes per-line totals and invoice-level monetary fields from the line items and fees.
    /// The client-supplied totals are never trusted.
    /// </summary>
    private static void RecomputeTotals(Invoice invoice, List<InvoiceItem> items, decimal? amountPaidOverride)
    {
        decimal subtotal = 0, discountTotal = 0, taxTotal = 0;
        foreach (var it in items)
        {
            var lineBase = Math.Round(it.Quantity * it.UnitPrice, 2);
            var afterDiscount = lineBase - it.DiscountAmount;
            if (afterDiscount < 0) afterDiscount = 0;
            var tax = Math.Round(afterDiscount * (it.TaxPercent / 100m), 2);
            it.LineTotal = Math.Round(afterDiscount + tax, 2);

            subtotal += lineBase;
            discountTotal += it.DiscountAmount;
            taxTotal += tax;
        }

        invoice.Subtotal = Math.Round(subtotal, 2);
        invoice.DiscountTotal = Math.Round(discountTotal, 2);
        invoice.TaxTotal = Math.Round(taxTotal, 2);
        invoice.GrandTotal = Math.Round(
            invoice.Subtotal - invoice.DiscountTotal + invoice.TaxTotal
            + invoice.ConvenienceFee + invoice.PlatformFee, 2);

        // Amount paid: honour explicit override, else assume fully paid when status is Paid.
        if (amountPaidOverride.HasValue && amountPaidOverride.Value > 0)
            invoice.AmountPaid = Math.Round(amountPaidOverride.Value, 2);
        else if (string.Equals(invoice.PaymentStatus, "Paid", StringComparison.OrdinalIgnoreCase))
            invoice.AmountPaid = invoice.GrandTotal;
        // else leave AmountPaid as-is (0 for a fresh invoice)

        invoice.BalanceDue = Math.Round(invoice.GrandTotal - invoice.AmountPaid, 2);
    }

    private static InvoiceDto BuildDto(Invoice inv, IEnumerable<InvoiceItem> items) => new()
    {
        Id = inv.Id,
        RegistrationId = inv.RegistrationId,
        InvoiceNumber = inv.InvoiceNumber,
        ReceiptNumber = inv.ReceiptNumber,
        TransactionId = inv.TransactionId,
        OrderId = inv.OrderId,
        ReferenceNumber = inv.ReferenceNumber,
        InvoiceDate = inv.InvoiceDate,
        PaymentDate = inv.PaymentDate,
        PaymentStatus = inv.PaymentStatus,
        PaymentMethod = inv.PaymentMethod,
        PaymentGateway = inv.PaymentGateway,
        CurrencyCode = inv.CurrencyCode,
        CurrencySymbol = inv.CurrencySymbol,
        Subtotal = inv.Subtotal,
        DiscountTotal = inv.DiscountTotal,
        TaxTotal = inv.TaxTotal,
        ConvenienceFee = inv.ConvenienceFee,
        PlatformFee = inv.PlatformFee,
        GrandTotal = inv.GrandTotal,
        AmountPaid = inv.AmountPaid,
        BalanceDue = inv.BalanceDue,
        RefundAmount = inv.RefundAmount,
        Notes = inv.Notes,
        Terms = inv.Terms,
        Organization = new InvoiceOrganizationDto
        {
            Name = inv.OrgName,
            AddressLine1 = inv.OrgAddressLine1,
            AddressLine2 = inv.OrgAddressLine2,
            City = inv.OrgCity,
            State = inv.OrgState,
            PostalCode = inv.OrgPostalCode,
            Country = inv.OrgCountry,
            Phone = inv.OrgPhone,
            Email = inv.OrgEmail,
            Website = inv.OrgWebsite,
            TaxRegNo = inv.OrgTaxRegNo,
            LogoUrl = inv.OrgLogoUrl,
            FooterNote = inv.OrgFooterNote
        },
        Customer = new InvoiceCustomerDto
        {
            CustomerId = inv.UserId,
            Name = inv.CustomerName,
            Email = inv.CustomerEmail,
            Phone = inv.CustomerPhone,
            BillingAddress = inv.CustomerBillingAddress
        },
        Items = items.OrderBy(i => i.SortOrder).Select(i => new InvoiceItemDto
        {
            Id = i.Id,
            Description = i.Description,
            Quantity = i.Quantity,
            UnitPrice = i.UnitPrice,
            TaxPercent = i.TaxPercent,
            DiscountAmount = i.DiscountAmount,
            LineTotal = i.LineTotal,
            SortOrder = i.SortOrder
        }).ToList(),
        CreatedAt = inv.CreatedAt,
        UpdatedAt = inv.UpdatedAt
    };

    private static InvoiceListItemDto ToListItem(Invoice inv, IEnumerable<InvoiceItem> items)
    {
        var summary = string.Join(", ", items.OrderBy(i => i.SortOrder).Select(i => i.Description));
        return new InvoiceListItemDto
        {
            Id = inv.Id,
            RegistrationId = inv.RegistrationId,
            InvoiceNumber = inv.InvoiceNumber,
            CustomerName = inv.CustomerName,
            CustomerEmail = inv.CustomerEmail,
            CourseSummary = summary,
            GrandTotal = inv.GrandTotal,
            AmountPaid = inv.AmountPaid,
            CurrencySymbol = inv.CurrencySymbol,
            PaymentStatus = inv.PaymentStatus,
            InvoiceDate = inv.InvoiceDate,
            PaymentDate = inv.PaymentDate
        };
    }

    private static OrganizationSettingsDto ToOrgDto(OrganizationSettings o) => new()
    {
        Id = o.Id,
        OrgName = o.OrgName,
        AddressLine1 = o.AddressLine1,
        AddressLine2 = o.AddressLine2,
        City = o.City,
        State = o.State,
        PostalCode = o.PostalCode,
        Country = o.Country,
        Phone = o.Phone,
        Email = o.Email,
        Website = o.Website,
        TaxRegNo = o.TaxRegNo,
        LogoUrl = o.LogoUrl,
        CurrencyCode = o.CurrencyCode,
        CurrencySymbol = o.CurrencySymbol,
        DefaultTaxPercent = o.DefaultTaxPercent,
        InvoicePrefix = o.InvoicePrefix,
        DefaultNotes = o.DefaultNotes,
        DefaultTerms = o.DefaultTerms,
        FooterNote = o.FooterNote,
        UpdatedAt = o.UpdatedAt
    };
}
