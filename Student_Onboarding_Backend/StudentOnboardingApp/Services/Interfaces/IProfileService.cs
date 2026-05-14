using StudentOnboardingApp.Models.Common;
using StudentOnboardingApp.Models.Profile;

namespace StudentOnboardingApp.Services.Interfaces;

public interface IProfileService
{
    Task<ApiResponse<StudentProfileDto>> GetProfileAsync();
    Task<ApiResponse<StudentProfileDto>> UpdateProfileAsync(UpdateProfileRequest request);
    Task<ApiResponse<StudentProfileDto>> UploadPhotoAsync(Stream photoStream, string fileName);
    Task<ApiResponse<string>> UploadDocumentAsync(Stream docStream, string fileName, string documentType);
}
