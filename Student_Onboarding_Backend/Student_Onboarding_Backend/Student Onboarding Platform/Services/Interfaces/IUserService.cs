using Student_Onboarding_Platform.Models.Entities;

namespace Student_Onboarding_Platform.Services.Interfaces;

public interface IUserService
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByPhoneNumberAsync(string phoneNumber);
    Task<User?> GetByIdAsync(Guid id);
    Task<User> CreateAsync(User user);
    Task UpdateEmailVerifiedAsync(Guid userId);
    Task UpdatePasswordAsync(Guid userId, string passwordHash);
    Task UpdateLastLoginAsync(Guid userId);
    Task UpdateApprovalStatusAsync(Guid userId, string status, Guid? approvedBy, string? denialReason);
    Task<IEnumerable<User>> GetStudentsAsync(int offset, int pageSize, string? approvalStatus, string? search);
    Task<int> GetStudentsCountAsync(string? approvalStatus, string? search);
    Task<IEnumerable<User>> GetAdminUsersAsync();
    Task UpdateProfileAsync(Guid userId, string firstName, string lastName, string? phoneNumber,
        DateTime? dateOfBirth, string? address, string? education);
    Task UpdateProfilePhotoAsync(Guid userId, string photoUrl);
    Task UpdateIsActiveAsync(Guid userId, bool isActive);
}
