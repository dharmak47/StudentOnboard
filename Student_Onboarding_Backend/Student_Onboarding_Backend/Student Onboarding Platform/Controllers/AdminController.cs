using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Student_Onboarding_Platform.Extensions;
using Student_Onboarding_Platform.Models.DTOs.Admin;
using Student_Onboarding_Platform.Models.DTOs.Invoice;
using Student_Onboarding_Platform.Services.Interfaces;

namespace Student_Onboarding_Platform.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;
    private readonly ICourseService _courseService;
    private readonly INotificationService _notificationService;
    private readonly IFaqService _faqService;
    private readonly IInvoiceService _invoiceService;

    public AdminController(
        IAdminService adminService,
        ICourseService courseService,
        INotificationService notificationService,
        IFaqService faqService,
        IInvoiceService invoiceService)
    {
        _adminService = adminService;
        _courseService = courseService;
        _notificationService = notificationService;
        _faqService = faqService;
        _invoiceService = invoiceService;
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        var result = await _adminService.GetDashboardAsync();
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("students")]
    public async Task<IActionResult> GetStudents(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? status = null,
        [FromQuery] string? search = null)
    {
        var result = await _adminService.GetStudentsAsync(page, pageSize, status, search);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("students/{id}")]
    public async Task<IActionResult> GetStudentById(Guid id)
    {
        var result = await _adminService.GetStudentByIdAsync(id);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("students/{id}")]
    public async Task<IActionResult> UpdateStudent(Guid id, [FromBody] UpdateStudentRequest request)
    {
        var result = await _adminService.UpdateStudentAsync(id, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("students/{id}/approve")]
    public async Task<IActionResult> ApproveStudent(Guid id)
    {
        var adminId = User.GetUserId();
        var result = await _adminService.ApproveStudentAsync(id, adminId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("students/{id}/deny")]
    public async Task<IActionResult> DenyStudent(Guid id, [FromBody] DenyStudentRequest request)
    {
        var adminId = User.GetUserId();
        var result = await _adminService.DenyStudentAsync(id, adminId, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("students/stats")]
    public async Task<IActionResult> GetStudentStats()
    {
        var result = await _adminService.GetDashboardAsync();
        if (!result.Success) return BadRequest(result);

        var stats = new
        {
            total = result.Data!.TotalStudents,
            approved = result.Data.ApprovedStudents,
            pending = result.Data.PendingApprovals,
            blocked = result.Data.DeniedStudents
        };
        return Ok(new { success = true, data = stats });
    }

    [HttpPost("notifications/send")]
    public async Task<IActionResult> SendNotification([FromBody] SendNotificationRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.Message))
            return BadRequest(new { success = false, message = "Title and message are required." });

        var result = await _notificationService.SendToStudentsAsync(request.Title, request.Message, request.StudentIds);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("notifications")]
    public async Task<IActionResult> GetNotifications()
    {
        var adminId = User.GetUserId();
        var result = await _notificationService.GetNotificationsAsync(adminId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("notifications/unread-count")]
    public async Task<IActionResult> GetUnreadCount()
    {
        var adminId = User.GetUserId();
        var result = await _notificationService.GetUnreadCountAsync(adminId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("notifications/{id}/read")]
    public async Task<IActionResult> MarkNotificationAsRead(Guid id)
    {
        var adminId = User.GetUserId();
        var result = await _notificationService.MarkAsReadAsync(id, adminId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("courses")]
    public async Task<IActionResult> GetCourses()
    {
        var result = await _courseService.GetActiveCoursesAsync();
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("courses")]
    public async Task<IActionResult> CreateCourse([FromBody] CreateCourseRequest request)
    {
        var adminId = User.GetUserId();
        var result = await _courseService.CreateCourseAsync(request, adminId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("courses/{id}")]
    public async Task<IActionResult> UpdateCourse(Guid id, [FromBody] UpdateCourseRequest request)
    {
        var result = await _courseService.UpdateCourseAsync(id, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("courses/{id}")]
    public async Task<IActionResult> DeleteCourse(Guid id)
    {
        var result = await _courseService.DeleteCourseAsync(id);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("course-registrations")]
    public async Task<IActionResult> GetCourseRegistrations(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await _adminService.GetCourseRegistrationsAsync(page, pageSize);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("course-registrations/{id}/payment")]
    public async Task<IActionResult> UpdatePayment(Guid id, [FromBody] UpdatePaymentRequest request)
    {
        var result = await _adminService.UpdatePaymentAsync(id, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("profile/photo")]
    public async Task<IActionResult> UploadProfilePhoto(IFormFile photo)
    {
        if (photo == null || photo.Length == 0)
            return BadRequest(new { success = false, message = "No photo provided." });

        var adminId = User.GetUserId();
        var result = await _adminService.UploadProfilePhotoAsync(adminId, photo);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // ── FAQ Management ────────────────────────────────────────────────

    [HttpGet("faqs")]
    public async Task<IActionResult> GetFaqs()
    {
        var result = await _faqService.GetAllFaqsAsync();
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("faqs")]
    public async Task<IActionResult> CreateFaq([FromBody] CreateFaqRequest request)
    {
        Guid adminId;
        try { adminId = User.GetUserId(); }
        catch { adminId = Guid.Empty; }
        var result = await _faqService.CreateFaqAsync(request, adminId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("faqs/{id}")]
    public async Task<IActionResult> UpdateFaq(Guid id, [FromBody] UpdateFaqRequest request)
    {
        var result = await _faqService.UpdateFaqAsync(id, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("faqs/{id}")]
    public async Task<IActionResult> DeleteFaq(Guid id)
    {
        var result = await _faqService.DeleteFaqAsync(id);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // ── Invoices ──────────────────────────────────────────────────────

    [HttpGet("invoices")]
    public async Task<IActionResult> GetInvoices([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _invoiceService.GetAllAsync(page, pageSize);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("invoices/{id}")]
    public async Task<IActionResult> GetInvoiceById(Guid id)
    {
        var result = await _invoiceService.GetByIdAsync(id);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("invoices/{id}")]
    public async Task<IActionResult> UpdateInvoice(Guid id, [FromBody] UpdateInvoiceRequest request)
    {
        var result = await _invoiceService.UpdateAsync(id, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("registrations/{registrationId}/invoice")]
    public async Task<IActionResult> GetOrCreateInvoiceForRegistration(Guid registrationId)
    {
        var result = await _invoiceService.GetOrCreateForRegistrationAsync(registrationId, null, isAdmin: true);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("organization-settings")]
    public async Task<IActionResult> GetOrganizationSettings()
    {
        var result = await _invoiceService.GetOrganizationSettingsAsync();
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("organization-settings")]
    public async Task<IActionResult> UpdateOrganizationSettings([FromBody] UpdateOrganizationSettingsRequest request)
    {
        var result = await _invoiceService.UpdateOrganizationSettingsAsync(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("users")]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
    {
        var result = await _adminService.CreateUserAsync(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("users/{id}/password")]
    public async Task<IActionResult> ChangeUserPassword(Guid id, [FromBody] AdminChangePasswordRequest request)
    {
        var result = await _adminService.ChangeUserPasswordAsync(id, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
