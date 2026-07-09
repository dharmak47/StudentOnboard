using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Student_Onboarding_Platform.Models.DTOs.Auth;
using Student_Onboarding_Platform.Models.Entities;
using Student_Onboarding_Platform.Models.Enums;
using Student_Onboarding_Platform.Models.Settings;
using Student_Onboarding_Platform.Services.Implementations;
using Student_Onboarding_Platform.Services.Interfaces;
using Xunit;

namespace StudentOnboarding.Tests
{
    public class AuthServiceTests
    {
        private readonly Mock<IUserService> _userServiceMock;
        private readonly Mock<ITokenService> _tokenServiceMock;
        private readonly Mock<IOtpService> _otpServiceMock;
        private readonly Mock<IEmailService> _emailServiceMock;
        private readonly Mock<ISessionService> _sessionServiceMock;
        private readonly Mock<ILoginAttemptService> _loginAttemptServiceMock;
        private readonly Mock<IPasswordHasher> _passwordHasherMock;
        private readonly Mock<INotificationService> _notificationServiceMock;
        private readonly Mock<IOptions<JwtSettings>> _jwtSettingsMock;
        private readonly Mock<ILogger<AuthService>> _loggerMock;
        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            _userServiceMock = new Mock<IUserService>();
            _tokenServiceMock = new Mock<ITokenService>();
            _otpServiceMock = new Mock<IOtpService>();
            _emailServiceMock = new Mock<IEmailService>();
            _sessionServiceMock = new Mock<ISessionService>();
            _loginAttemptServiceMock = new Mock<ILoginAttemptService>();
            _passwordHasherMock = new Mock<IPasswordHasher>();
            _notificationServiceMock = new Mock<INotificationService>();
            _jwtSettingsMock = new Mock<IOptions<JwtSettings>>();
            _loggerMock = new Mock<ILogger<AuthService>>();

            var jwtSettings = new JwtSettings
            {
                SecretKey = "super_secret_key_at_least_32_characters_long",
                AccessTokenExpirationMinutes = 15,
                RefreshTokenExpirationDays = 7
            };
            _jwtSettingsMock.Setup(x => x.Value).Returns(jwtSettings);

            _authService = new AuthService(
                _userServiceMock.Object,
                _tokenServiceMock.Object,
                _otpServiceMock.Object,
                _emailServiceMock.Object,
                _sessionServiceMock.Object,
                _loginAttemptServiceMock.Object,
                _passwordHasherMock.Object,
                _notificationServiceMock.Object,
                _jwtSettingsMock.Object,
                _loggerMock.Object
            );
        }

        [Fact]
        public async Task LoginAsync_WithNonExistentUser_ReturnsFailure()
        {
            // Arrange
            var request = new LoginRequest { Email = "nonexistent@example.com", Password = "Password123" };
            _userServiceMock.Setup(x => x.GetByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync((User)null);

            // Act
            var result = await _authService.LoginAsync(request, "127.0.0.1", "TestAgent");

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Invalid email or password.", result.Message);
            _loginAttemptServiceMock.Verify(x => x.LogAttemptAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), false, nameof(FailureReason.UserNotFound)), Times.Once);
        }

        [Fact]
        public async Task LoginAsync_WithIncorrectPassword_ReturnsFailure()
        {
            // Arrange
            var email = "user@example.com";
            var request = new LoginRequest { Email = email, Password = "wrongpassword" };
            var user = new User { Email = email, PasswordHash = "hashedpassword", IsActive = true, IsDeleted = false };

            _userServiceMock.Setup(x => x.GetByEmailAsync(email)).ReturnsAsync(user);
            _passwordHasherMock.Setup(x => x.Verify(request.Password, user.PasswordHash)).Returns(false);

            // Act
            var result = await _authService.LoginAsync(request, "127.0.0.1", "TestAgent");

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Invalid email or password.", result.Message);
            _loginAttemptServiceMock.Verify(x => x.LogAttemptAsync(
                email, It.IsAny<string>(), It.IsAny<string>(), false, nameof(FailureReason.InvalidPassword)), Times.Once);
        }

        [Fact]
        public async Task LoginAsync_WithInactiveUser_ReturnsFailure()
        {
            // Arrange
            var email = "inactive@example.com";
            var request = new LoginRequest { Email = email, Password = "Password123" };
            var user = new User { Email = email, IsActive = false, IsDeleted = false };

            _userServiceMock.Setup(x => x.GetByEmailAsync(email)).ReturnsAsync(user);

            // Act
            var result = await _authService.LoginAsync(request, "127.0.0.1", "TestAgent");

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Your account is inactive. Please contact support.", result.Message);
            _loginAttemptServiceMock.Verify(x => x.LogAttemptAsync(
                email, It.IsAny<string>(), It.IsAny<string>(), false, nameof(FailureReason.AccountInactive)), Times.Once);
        }

        [Fact]
        public async Task LoginAsync_WithValidCredentials_ReturnsSuccess()
        {
            // Arrange
            var email = "valid@example.com";
            var request = new LoginRequest { Email = email, Password = "CorrectPassword123", DeviceType = "Web", DeviceName = "Chrome" };
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = email,
                PasswordHash = "correcthash",
                IsActive = true,
                IsDeleted = false,
                EmailVerified = true,
                ApprovalStatus = "Approved",
                FirstName = "John",
                LastName = "Doe",
                Role = "Student"
            };

            _userServiceMock.Setup(x => x.GetByEmailAsync(email)).ReturnsAsync(user);
            _passwordHasherMock.Setup(x => x.Verify(request.Password, user.PasswordHash)).Returns(true);
            _tokenServiceMock.Setup(x => x.GenerateAccessToken(user)).Returns("mock_access_token");
            _tokenServiceMock.Setup(x => x.GenerateRefreshToken()).Returns("mock_refresh_token");

            // Act
            var result = await _authService.LoginAsync(request, "127.0.0.1", "TestAgent");

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal("mock_access_token", result.Data.AccessToken);
            Assert.Equal("mock_refresh_token", result.Data.RefreshToken);
            _sessionServiceMock.Verify(x => x.CreateSessionAsync(
                user.Id, "mock_refresh_token", request.DeviceType, request.DeviceName, "127.0.0.1", "TestAgent", It.IsAny<DateTime>()), Times.Once);
            _userServiceMock.Verify(x => x.UpdateLastLoginAsync(user.Id), Times.Once);
        }
    }
}
