using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Student_Onboarding_Platform.Models.DTOs;
using Student_Onboarding_Platform.Services.Interfaces;

namespace Student_Onboarding_Platform.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EnquiriesController : ControllerBase
{
    private readonly IEnquiryService _enquiryService;

    public EnquiriesController(IEnquiryService enquiryService)
    {
        _enquiryService = enquiryService;
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> CreateEnquiry([FromBody] EnquiryRequestDto requestDto)
    {
        var result = await _enquiryService.CreateEnquiryAsync(requestDto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAllEnquiries()
    {
        var result = await _enquiryService.GetAllEnquiriesAsync();
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPatch("{id}/resolve")]
    [Authorize]
    public async Task<IActionResult> ResolveEnquiry(Guid id)
    {
        var result = await _enquiryService.ResolveEnquiryAsync(id);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
