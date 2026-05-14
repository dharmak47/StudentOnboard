using Student_Onboarding_Platform.Data.Repositories.Interfaces;
using Student_Onboarding_Platform.Models.Entities;
using Student_Onboarding_Platform.Services.Interfaces;

namespace Student_Onboarding_Platform.Services.Implementations;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _userRepository.GetByEmailAsync(email);
    }

    public async Task<User?> GetByPhoneNumberAsync(string phoneNumber)
    {
        return await _userRepository.GetByPhoneNumberAsync(phoneNumber);
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        return await _userRepository.GetByIdAsync(id);
    }

    public async Task<User> CreateAsync(User user)
    {
        return await _userRepository.CreateAsync(user);
    }

    public async Task UpdateEmailVerifiedAsync(Guid userId)
    {
        await _userRepository.UpdateEmailVerifiedAsync(userId);
    }

    public async Task UpdatePasswordAsync(Guid userId, string passwordHash)
    {
        await _userRepository.UpdatePasswordAsync(userId, passwordHash, DateTime.UtcNow);
    }

    public async Task UpdateLastLoginAsync(Guid userId)
    {
        await _userRepository.UpdateLastLoginAsync(userId, DateTime.UtcNow);
    }

    public async Task UpdateApprovalStatusAsync(Guid userId, string status, Guid? approvedBy, string? denialReason)
    {
        await _userRepository.UpdateApprovalStatusAsync(userId, status, approvedBy, denialReason);
    }

    public async Task<IEnumerable<User>> GetStudentsAsync(int offset, int pageSize, string? approvalStatus, string? search)
    {
        return await _userRepository.GetStudentsAsync(offset, pageSize, approvalStatus, search);
    }

    public async Task<int> GetStudentsCountAsync(string? approvalStatus, string? search)
    {
        return await _userRepository.GetStudentsCountAsync(approvalStatus, search);
    }

    public async Task<IEnumerable<User>> GetAdminUsersAsync()
    {
        return await _userRepository.GetAdminUsersAsync();
    }

    public async Task UpdateProfileAsync(Guid userId, string firstName, string lastName, string? phoneNumber,
        DateTime? dateOfBirth, string? address, string? education)
    {
        await _userRepository.UpdateProfileAsync(userId, firstName, lastName, phoneNumber, dateOfBirth, address, education);
    }

    public async Task UpdateProfilePhotoAsync(Guid userId, string photoUrl)
    {
        await _userRepository.UpdateProfilePhotoAsync(userId, photoUrl);
    }

    public async Task UpdateIsActiveAsync(Guid userId, bool isActive)
    {
        await _userRepository.UpdateIsActiveAsync(userId, isActive);
    }
}
