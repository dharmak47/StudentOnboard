using Student_Onboarding_Platform.Data.Repositories.Interfaces;
using Student_Onboarding_Platform.Models.DTOs.Common;
using Student_Onboarding_Platform.Models.DTOs.Student;
using Student_Onboarding_Platform.Services.Interfaces;

namespace Student_Onboarding_Platform.Services.Implementations;

public class StudentProgressService : IStudentProgressService
{
    private readonly ICourseRegistrationRepository _registrationRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly ILogger<StudentProgressService> _logger;

    public StudentProgressService(
        ICourseRegistrationRepository registrationRepository,
        ICourseRepository courseRepository,
        ILogger<StudentProgressService> logger)
    {
        _registrationRepository = registrationRepository;
        _courseRepository = courseRepository;
        _logger = logger;
    }

    public async Task<ApiResponse<CourseProgressDto>> GetProgressAsync(Guid registrationId)
    {
        try
        {
            var registration = await _registrationRepository.GetByIdAsync(registrationId);
            if (registration == null)
            {
                return ApiResponse<CourseProgressDto>.Fail("Course registration not found.");
            }

            var course = await _courseRepository.GetByIdAsync(registration.CourseId);
            if (course == null)
            {
                return ApiResponse<CourseProgressDto>.Fail("Course not found.");
            }

            var progressPercentage = await CalculateProgressPercentageAsync(registrationId);
            var daysRemaining = registration.ExpectedCompletionDate.HasValue
                ? (registration.ExpectedCompletionDate.Value.Date - DateTime.UtcNow.Date).Days
                : 0;

            var progress = new CourseProgressDto
            {
                RegistrationId = registration.Id,
                CourseId = registration.CourseId,
                CourseName = course.Name,
                EnrolledDate = registration.CreatedAt,
                ExpectedCompletionDate = registration.ExpectedCompletionDate,
                DaysRemaining = Math.Max(daysRemaining, 0),
                ProgressPercentage = progressPercentage,
                CurrentModule = registration.CurrentModule ?? string.Empty,
                TotalModules = registration.TotalModules,
                CompletedModules = registration.CompletedModules,
                PaymentStatus = registration.PaymentStatus,
                IsCompleted = registration.IsCompleted,
                Modules = BuildModuleList(registration)
            };

            _logger.LogInformation("Retrieved progress for registration {RegistrationId}, Progress: {Progress}%",
                registrationId, progressPercentage);

            return ApiResponse<CourseProgressDto>.Ok(progress);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving progress for registration {RegistrationId}", registrationId);
            return ApiResponse<CourseProgressDto>.Fail($"Error retrieving progress: {ex.Message}");
        }
    }

    public async Task<ApiResponse<CourseProgressDto>> UpdateProgressAsync(
        Guid registrationId,
        UpdateProgressRequest request)
    {
        try
        {
            var registration = await _registrationRepository.GetByIdAsync(registrationId);
            if (registration == null)
            {
                return ApiResponse<CourseProgressDto>.Fail("Course registration not found.");
            }

            // Validate input
            if (request.CompletedModules < 0 || registration.TotalModules > 0 && request.CompletedModules > registration.TotalModules)
            {
                return ApiResponse<CourseProgressDto>.Fail("Invalid completed modules count.");
            }

            // Update progress fields
            registration.CompletedModules = request.CompletedModules;
            if (!string.IsNullOrWhiteSpace(request.CurrentModule))
            {
                registration.CurrentModule = request.CurrentModule;
            }
            registration.LastProgressUpdated = DateTime.UtcNow;

            // Calculate new progress percentage
            var progressPercentage = registration.TotalModules > 0
                ? Math.Round((decimal)registration.CompletedModules / registration.TotalModules * 100, 2)
                : 0;

            registration.ProgressPercentage = progressPercentage;

            // Mark as completed if all modules done
            if (registration.TotalModules > 0 && registration.CompletedModules >= registration.TotalModules)
            {
                registration.IsCompleted = true;
                registration.CompletedAt = DateTime.UtcNow;
                _logger.LogInformation("Course marked as completed for registration {RegistrationId}", registrationId);
            }

            await _registrationRepository.UpdateAsync(registration);

            _logger.LogInformation("Progress updated for registration {RegistrationId}, New Progress: {Progress}%",
                registrationId, progressPercentage);

            return await GetProgressAsync(registrationId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating progress for registration {RegistrationId}", registrationId);
            return ApiResponse<CourseProgressDto>.Fail($"Error updating progress: {ex.Message}");
        }
    }

    public async Task<ApiResponse<StudentProgressSummaryDto>> GetStudentProgressSummaryAsync(Guid studentId)
    {
        try
        {
            var registrations = await _registrationRepository.GetByUserIdAsync(studentId);

            var summary = new StudentProgressSummaryDto
            {
                StudentId = studentId,
                EnrolledCourses = registrations.Count(),
                ActiveCourses = registrations.Count(r => !r.IsCompleted),
                CompletedCourses = registrations.Count(r => r.IsCompleted),
                AverageProgress = registrations.Any()
                    ? registrations.Average(r => r.ProgressPercentage)
                    : 0m,
                AtRiskCourses = registrations.Count(r => r.ProgressPercentage < 30 && !r.IsCompleted)
            };

            _logger.LogInformation("Retrieved progress summary for student {StudentId}", studentId);

            return ApiResponse<StudentProgressSummaryDto>.Ok(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving progress summary for student {StudentId}", studentId);
            return ApiResponse<StudentProgressSummaryDto>.Fail($"Error retrieving progress summary: {ex.Message}");
        }
    }

    public async Task<decimal> CalculateProgressPercentageAsync(Guid registrationId)
    {
        try
        {
            var registration = await _registrationRepository.GetByIdAsync(registrationId);
            if (registration == null || registration.TotalModules == 0)
                return 0m;

            return Math.Round(
                (decimal)registration.CompletedModules / registration.TotalModules * 100,
                2);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating progress percentage for registration {RegistrationId}", registrationId);
            return 0m;
        }
    }

    private List<ModuleProgressDto> BuildModuleList(dynamic registration)
    {
        var modules = new List<ModuleProgressDto>();

        for (int i = 1; i <= registration.TotalModules; i++)
        {
            var isCompleted = i <= registration.CompletedModules;
            modules.Add(new ModuleProgressDto
            {
                ModuleNumber = i,
                ModuleName = $"Module {i}",
                IsCompleted = isCompleted,
                CompletedDate = isCompleted ? DateTime.UtcNow : null
            });
        }

        return modules;
    }
}
