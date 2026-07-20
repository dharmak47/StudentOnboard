using Dapper;
using Student_Onboarding_Platform.Data.Repositories.Interfaces;
using Student_Onboarding_Platform.Models.Entities;

namespace Student_Onboarding_Platform.Data.Repositories.Implementations;

public class InvoiceRepository : IInvoiceRepository
{
    private readonly DbConnectionFactory _db;

    public InvoiceRepository(DbConnectionFactory db)
    {
        _db = db;
    }

    private const string InsertInvoiceSql = @"
        INSERT INTO Invoices (
            Id, RegistrationId, UserId, InvoiceNumber, InvoiceYear, SequenceNumber,
            ReceiptNumber, TransactionId, OrderId, ReferenceNumber, InvoiceDate, PaymentDate,
            PaymentStatus, PaymentMethod, PaymentGateway, CurrencyCode, CurrencySymbol,
            Subtotal, DiscountTotal, TaxTotal, ConvenienceFee, PlatformFee, GrandTotal,
            AmountPaid, BalanceDue, RefundAmount, Notes, Terms,
            OrgName, OrgAddressLine1, OrgAddressLine2, OrgCity, OrgState, OrgPostalCode,
            OrgCountry, OrgPhone, OrgEmail, OrgWebsite, OrgTaxRegNo, OrgLogoUrl, OrgFooterNote,
            CustomerName, CustomerEmail, CustomerPhone, CustomerBillingAddress,
            CreatedAt, UpdatedAt)
        VALUES (
            @Id, @RegistrationId, @UserId, @InvoiceNumber, @InvoiceYear, @SequenceNumber,
            @ReceiptNumber, @TransactionId, @OrderId, @ReferenceNumber, @InvoiceDate, @PaymentDate,
            @PaymentStatus, @PaymentMethod, @PaymentGateway, @CurrencyCode, @CurrencySymbol,
            @Subtotal, @DiscountTotal, @TaxTotal, @ConvenienceFee, @PlatformFee, @GrandTotal,
            @AmountPaid, @BalanceDue, @RefundAmount, @Notes, @Terms,
            @OrgName, @OrgAddressLine1, @OrgAddressLine2, @OrgCity, @OrgState, @OrgPostalCode,
            @OrgCountry, @OrgPhone, @OrgEmail, @OrgWebsite, @OrgTaxRegNo, @OrgLogoUrl, @OrgFooterNote,
            @CustomerName, @CustomerEmail, @CustomerPhone, @CustomerBillingAddress,
            @CreatedAt, @UpdatedAt)";

    private const string InsertItemSql = @"
        INSERT INTO InvoiceItems (Id, InvoiceId, Description, Quantity, UnitPrice, TaxPercent,
            DiscountAmount, LineTotal, SortOrder)
        VALUES (@Id, @InvoiceId, @Description, @Quantity, @UnitPrice, @TaxPercent,
            @DiscountAmount, @LineTotal, @SortOrder)";

    public async Task<Invoice?> GetByIdAsync(Guid id)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<Invoice>(
            "SELECT * FROM Invoices WHERE Id = @Id", new { Id = id });
    }

    public async Task<Invoice?> GetByRegistrationIdAsync(Guid registrationId)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<Invoice>(
            "SELECT * FROM Invoices WHERE RegistrationId = @RegistrationId",
            new { RegistrationId = registrationId });
    }

    public async Task<IEnumerable<Invoice>> GetByUserIdAsync(Guid userId)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<Invoice>(
            "SELECT * FROM Invoices WHERE UserId = @UserId ORDER BY CreatedAt DESC",
            new { UserId = userId });
    }

    public async Task<IEnumerable<Invoice>> GetAllAsync(int offset, int pageSize)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<Invoice>(@"
            SELECT * FROM Invoices
            ORDER BY CreatedAt DESC OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY",
            new { Offset = offset, PageSize = pageSize });
    }

    public async Task<int> GetAllCountAsync()
    {
        using var conn = _db.CreateConnection();
        return await conn.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Invoices");
    }

    public async Task<IEnumerable<InvoiceItem>> GetItemsAsync(Guid invoiceId)
    {
        using var conn = _db.CreateConnection();
        return await conn.QueryAsync<InvoiceItem>(
            "SELECT * FROM InvoiceItems WHERE InvoiceId = @InvoiceId ORDER BY SortOrder",
            new { InvoiceId = invoiceId });
    }

    public async Task<int> GetMaxSequenceForYearAsync(int year)
    {
        using var conn = _db.CreateConnection();
        return await conn.ExecuteScalarAsync<int>(
            "SELECT COALESCE(MAX(SequenceNumber), 0) FROM Invoices WHERE InvoiceYear = @Year",
            new { Year = year });
    }

    public async Task CreateAsync(Invoice invoice, IEnumerable<InvoiceItem> items)
    {
        using var conn = _db.CreateConnection();
        conn.Open();
        using var tx = conn.BeginTransaction();
        try
        {
            await conn.ExecuteAsync(InsertInvoiceSql, invoice, tx);
            foreach (var item in items)
            {
                item.Id = item.Id == Guid.Empty ? Guid.NewGuid() : item.Id;
                item.InvoiceId = invoice.Id;
                await conn.ExecuteAsync(InsertItemSql, item, tx);
            }
            tx.Commit();
        }
        catch
        {
            tx.Rollback();
            throw;
        }
    }

    public async Task UpdateAsync(Invoice invoice, IEnumerable<InvoiceItem> items)
    {
        using var conn = _db.CreateConnection();
        conn.Open();
        using var tx = conn.BeginTransaction();
        try
        {
            await conn.ExecuteAsync(@"
                UPDATE Invoices SET
                    ReceiptNumber = @ReceiptNumber, TransactionId = @TransactionId,
                    OrderId = @OrderId, ReferenceNumber = @ReferenceNumber,
                    InvoiceDate = @InvoiceDate, PaymentDate = @PaymentDate,
                    PaymentStatus = @PaymentStatus, PaymentMethod = @PaymentMethod,
                    PaymentGateway = @PaymentGateway,
                    Subtotal = @Subtotal, DiscountTotal = @DiscountTotal, TaxTotal = @TaxTotal,
                    ConvenienceFee = @ConvenienceFee, PlatformFee = @PlatformFee,
                    GrandTotal = @GrandTotal, AmountPaid = @AmountPaid, BalanceDue = @BalanceDue,
                    RefundAmount = @RefundAmount, Notes = @Notes, Terms = @Terms,
                    OrgName = @OrgName, OrgAddressLine1 = @OrgAddressLine1, OrgAddressLine2 = @OrgAddressLine2,
                    OrgCity = @OrgCity, OrgState = @OrgState, OrgPostalCode = @OrgPostalCode,
                    OrgCountry = @OrgCountry, OrgPhone = @OrgPhone, OrgEmail = @OrgEmail,
                    OrgWebsite = @OrgWebsite, OrgTaxRegNo = @OrgTaxRegNo, OrgLogoUrl = @OrgLogoUrl,
                    OrgFooterNote = @OrgFooterNote,
                    CustomerName = @CustomerName, CustomerEmail = @CustomerEmail,
                    CustomerPhone = @CustomerPhone, CustomerBillingAddress = @CustomerBillingAddress,
                    UpdatedAt = @UpdatedAt
                WHERE Id = @Id", invoice, tx);

            await conn.ExecuteAsync("DELETE FROM InvoiceItems WHERE InvoiceId = @InvoiceId",
                new { InvoiceId = invoice.Id }, tx);

            foreach (var item in items)
            {
                item.Id = item.Id == Guid.Empty ? Guid.NewGuid() : item.Id;
                item.InvoiceId = invoice.Id;
                await conn.ExecuteAsync(InsertItemSql, item, tx);
            }
            tx.Commit();
        }
        catch
        {
            tx.Rollback();
            throw;
        }
    }
}
