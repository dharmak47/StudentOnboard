using System.Net.Http.Json;
using StudentOnboardingApp.Helpers;
using StudentOnboardingApp.Models.Common;
using StudentOnboardingApp.Models.Course;
using StudentOnboardingApp.Services.Interfaces;

namespace StudentOnboardingApp.Services.Implementations;

public class CourseService : ICourseService
{
    private readonly HttpClient _client;

    public CourseService(IHttpClientFactory httpClientFactory)
    {
        _client = httpClientFactory.CreateClient(Constants.AuthenticatedApiClient);
    }

    public async Task<ApiResponse<List<CourseDto>>> GetCoursesAsync()
    {
        try
        {
            var response = await _client.GetAsync("course");
            return await HttpResponseParser.ParseAsync<List<CourseDto>>(response);
        }
        catch
        {
            return new ApiResponse<List<CourseDto>> { Success = false, Message = "Unable to load courses. Please try again." };
        }
    }

    public async Task<ApiResponse<CourseDetailDto>> GetCourseDetailAsync(Guid courseId)
    {
        try
        {
            var response = await _client.GetAsync($"course/{courseId}");
            return await HttpResponseParser.ParseAsync<CourseDetailDto>(response);
        }
        catch
        {
            return new ApiResponse<CourseDetailDto> { Success = false, Message = "Unable to load course details. Please try again." };
        }
    }

    public async Task<ApiResponse<string>> ApplyForCourseAsync(CourseApplicationRequest request)
    {
        try
        {
            var response = await _client.PostAsJsonAsync("student/courses/register", request);
            return await HttpResponseParser.ParseAsync<string>(response);
        }
        catch
        {
            return new ApiResponse<string> { Success = false, Message = "Unable to register for course. Please try again." };
        }
    }

    public async Task<ApiResponse<CourseReviewsResponse>> GetCourseReviewsAsync(Guid courseId)
    {
        try
        {
            var response = await _client.GetAsync($"student/courses/{courseId}/reviews");
            return await HttpResponseParser.ParseAsync<CourseReviewsResponse>(response);
        }
        catch
        {
            return new ApiResponse<CourseReviewsResponse> { Success = false, Message = "Unable to load reviews. Please try again." };
        }
    }

    public async Task<ApiResponse<string>> SubmitReviewAsync(Guid courseId, SubmitReviewRequest request)
    {
        try
        {
            var response = await _client.PostAsJsonAsync($"student/courses/{courseId}/review", request);
            return await HttpResponseParser.ParseAsync<string>(response);
        }
        catch
        {
            return new ApiResponse<string> { Success = false, Message = "Unable to submit review. Please try again." };
        }
    }
}
