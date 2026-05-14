using Student_Onboarding_Platform.Data.Repositories.Interfaces;
using Student_Onboarding_Platform.Models.DTOs.Admin;
using Student_Onboarding_Platform.Models.DTOs.Common;
using Student_Onboarding_Platform.Models.DTOs.Course;
using Student_Onboarding_Platform.Models.Entities;
using Student_Onboarding_Platform.Services.Interfaces;

namespace Student_Onboarding_Platform.Services.Implementations;

public class CourseService : ICourseService
{
    private readonly ICourseRepository _courseRepository;
    private readonly ICourseRegistrationRepository _registrationRepository;
    private readonly ILogger<CourseService> _logger;

    public CourseService(
        ICourseRepository courseRepository,
        ICourseRegistrationRepository registrationRepository,
        ILogger<CourseService> logger)
    {
        _courseRepository = courseRepository;
        _registrationRepository = registrationRepository;
        _logger = logger;
    }

    public async Task<ApiResponse<List<CourseListResponse>>> GetActiveCoursesAsync()
    {
        var courses = await _courseRepository.GetActiveCoursesAsync();

        var response = courses.Select(c => new CourseListResponse
        {
            Id = c.Id,
            Name = c.Name,
            Description = c.Description,
            Fees = c.Fees,
            OfferPrice = c.OfferPrice,
            Duration = c.Duration,
            Instructor = c.Instructor,
            Category = c.Category,
            Thumbnail = c.Thumbnail,
            IsActive = c.IsActive
        }).ToList();

        return ApiResponse<List<CourseListResponse>>.Ok(response);
    }

    public async Task<ApiResponse<CourseDetailResponse>> GetCourseByIdAsync(Guid courseId)
    {
        var course = await _courseRepository.GetByIdAsync(courseId);
        if (course == null)
            return ApiResponse<CourseDetailResponse>.Fail("Course not found.");

        return ApiResponse<CourseDetailResponse>.Ok(MapToDetail(course));
    }

    public async Task<ApiResponse<CourseDetailResponse>> CreateCourseAsync(CreateCourseRequest request, Guid createdBy)
    {
        _logger.LogInformation("Creating course: {CourseName} by admin {AdminId}", request.Name, createdBy);

        var course = new Course
        {
            Name = request.Name,
            Description = request.Description,
            Fees = request.Fees,
            OfferPrice = request.OfferPrice,
            Syllabus = request.Syllabus,
            Duration = request.Duration,
            Instructor = request.Instructor,
            Category = request.Category,
            Thumbnail = request.Thumbnail,
            IsActive = true,
            IsDeleted = false,
            CreatedBy = createdBy
        };

        await _courseRepository.CreateAsync(course);
        _logger.LogInformation("Course created: {CourseId}", course.Id);

        return ApiResponse<CourseDetailResponse>.Ok(MapToDetail(course), "Course created successfully.");
    }

    public async Task<ApiResponse<CourseDetailResponse>> UpdateCourseAsync(Guid courseId, UpdateCourseRequest request)
    {
        var course = await _courseRepository.GetByIdAsync(courseId);
        if (course == null)
            return ApiResponse<CourseDetailResponse>.Fail("Course not found.");

        course.Name = request.Name;
        course.Description = request.Description;
        course.Fees = request.Fees;
        course.OfferPrice = request.OfferPrice;
        course.Syllabus = request.Syllabus;
        course.Duration = request.Duration;
        course.Instructor = request.Instructor;
        course.Category = request.Category;
        course.Thumbnail = request.Thumbnail;
        course.IsActive = request.IsActive;

        await _courseRepository.UpdateAsync(course);
        _logger.LogInformation("Course updated: {CourseId}", courseId);

        return ApiResponse<CourseDetailResponse>.Ok(MapToDetail(course), "Course updated successfully.");
    }

    public async Task<ApiResponse<string>> DeleteCourseAsync(Guid courseId)
    {
        var course = await _courseRepository.GetByIdAsync(courseId);
        if (course == null)
            return ApiResponse<string>.Fail("Course not found.");

        var activeStudents = await _registrationRepository.GetActiveCountByCourseAsync(courseId);
        if (activeStudents > 0)
            return ApiResponse<string>.Fail($"Cannot delete this course. {activeStudents} active student(s) are enrolled.");

        await _courseRepository.SoftDeleteAsync(courseId);
        _logger.LogInformation("Course soft-deleted: {CourseId}", courseId);

        return ApiResponse<string>.Ok("Course deleted successfully.");
    }

    private static CourseDetailResponse MapToDetail(Course course)
    {
        return new CourseDetailResponse
        {
            Id = course.Id,
            Name = course.Name,
            Description = course.Description,
            Fees = course.Fees,
            OfferPrice = course.OfferPrice,
            Syllabus = course.Syllabus,
            Duration = course.Duration,
            Instructor = course.Instructor,
            Category = course.Category,
            Thumbnail = course.Thumbnail,
            IsActive = course.IsActive,
            CreatedAt = course.CreatedAt,
            UpdatedAt = course.UpdatedAt
        };
    }
}
