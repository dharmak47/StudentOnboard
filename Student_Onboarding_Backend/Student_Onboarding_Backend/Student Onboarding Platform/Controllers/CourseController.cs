using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Student_Onboarding_Platform.Services.Interfaces;

namespace Student_Onboarding_Platform.Controllers;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
//[Authorize]
public class CourseController : ControllerBase
{
    private readonly ICourseService _courseService;

    public CourseController(ICourseService courseService)
    {
        _courseService = courseService;
    }

    [HttpGet]
    public async Task<IActionResult> GetCourses()
    {
        var result = await _courseService.GetActiveCoursesAsync();
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetCourseById(Guid id)
    {
        var result = await _courseService.GetCourseByIdAsync(id);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
