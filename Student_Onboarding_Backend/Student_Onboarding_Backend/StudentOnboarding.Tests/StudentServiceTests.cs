using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Student_Onboarding_Platform.Data.Repositories.Interfaces;
using Student_Onboarding_Platform.Models.DTOs.Student;
using Student_Onboarding_Platform.Models.Entities;
using Student_Onboarding_Platform.Services.Implementations;
using Student_Onboarding_Platform.Services.Interfaces;
using Xunit;

namespace StudentOnboarding.Tests
{
    public class StudentServiceTests
    {
        private readonly Mock<IUserService> _userServiceMock;
        private readonly Mock<ICourseRepository> _courseRepoMock;
        private readonly Mock<ICourseRegistrationRepository> _registrationRepoMock;
        private readonly Mock<IEmailService> _emailServiceMock;
        private readonly Mock<IFileStorageService> _fileStorageMock;
        private readonly Mock<ILogger<StudentService>> _loggerMock;
        private readonly StudentService _studentService;

        public StudentServiceTests()
        {
            _userServiceMock       = new Mock<IUserService>();
            _courseRepoMock        = new Mock<ICourseRepository>();
            _registrationRepoMock  = new Mock<ICourseRegistrationRepository>();
            _emailServiceMock      = new Mock<IEmailService>();
            _fileStorageMock       = new Mock<IFileStorageService>();
            _loggerMock            = new Mock<ILogger<StudentService>>();

            _studentService = new StudentService(
                _userServiceMock.Object,
                _courseRepoMock.Object,
                _registrationRepoMock.Object,
                _emailServiceMock.Object,
                _fileStorageMock.Object,
                _loggerMock.Object
            );
        }

        // ─── GetProfile ──────────────────────────────────────────────────────

        [Fact]
        public async Task GetProfileAsync_WithValidUserId_ReturnsStudentProfile()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new User
            {
                Id        = userId,
                FirstName = "Malini",
                LastName  = "V",
                Email     = "vmalini581@gmail.com",
                Role      = "Student",
                IsActive  = true
            };
            _userServiceMock.Setup(u => u.GetByIdAsync(userId)).ReturnsAsync(user);

            // Act
            var result = await _studentService.GetProfileAsync(userId);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal("Malini", result.Data!.FirstName);
            Assert.Equal("vmalini581@gmail.com", result.Data.Email);
        }

        [Fact]
        public async Task GetProfileAsync_WithInvalidUserId_ReturnsFailure()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _userServiceMock.Setup(u => u.GetByIdAsync(userId)).ReturnsAsync((User?)null);

            // Act
            var result = await _studentService.GetProfileAsync(userId);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("User not found.", result.Message);
        }

        // ─── UpdateProfile ───────────────────────────────────────────────────

        [Fact]
        public async Task UpdateProfileAsync_WithValidData_UpdatesSuccessfully()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new User { Id = userId, FirstName = "Malini", LastName = "V", Email = "vmalini581@gmail.com" };
            var request = new UpdateProfileRequest
            {
                FirstName   = "Malini",
                LastName    = "Updated",
                PhoneNumber = "9876543210",
                Address     = "Chennai, India",
                Education   = "B.Tech"
            };

            _userServiceMock.Setup(u => u.GetByIdAsync(userId)).ReturnsAsync(user);
            _userServiceMock.Setup(u => u.UpdateProfileAsync(
                userId, request.FirstName, request.LastName,
                request.PhoneNumber, request.DateOfBirth,
                request.Address, request.Education))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _studentService.UpdateProfileAsync(userId, request);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Profile updated successfully.", result.Message);
            Assert.Equal("Updated", result.Data!.LastName);
            _userServiceMock.Verify(u => u.UpdateProfileAsync(
                userId, It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string?>(), It.IsAny<DateTime?>(),
                It.IsAny<string?>(), It.IsAny<string?>()), Times.Once);
        }

        [Fact]
        public async Task UpdateProfileAsync_WithNonExistentUser_ReturnsFailure()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _userServiceMock.Setup(u => u.GetByIdAsync(userId)).ReturnsAsync((User?)null);

            // Act
            var result = await _studentService.UpdateProfileAsync(userId, new UpdateProfileRequest());

            // Assert
            Assert.False(result.Success);
            Assert.Equal("User not found.", result.Message);
        }

        // ─── UploadProfilePhoto ──────────────────────────────────────────────

        [Fact]
        public async Task UploadProfilePhotoAsync_WithValidUser_UploadsAndUpdatesPhotoUrl()
        {
            // Arrange
            var userId   = Guid.NewGuid();
            var user     = new User { Id = userId, Email = "vmalini581@gmail.com" };
            var photoUrl = "https://cdn.example.com/photo.jpg";

            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns("photo.jpg");
            fileMock.Setup(f => f.ContentType).Returns("image/jpeg");
            fileMock.Setup(f => f.OpenReadStream()).Returns(new System.IO.MemoryStream());

            _userServiceMock.Setup(u => u.GetByIdAsync(userId)).ReturnsAsync(user);
            _fileStorageMock.Setup(f => f.UploadAsync(
                It.IsAny<System.IO.Stream>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(photoUrl);
            _userServiceMock.Setup(u => u.UpdateProfilePhotoAsync(userId, photoUrl))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _studentService.UploadProfilePhotoAsync(userId, fileMock.Object);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Photo uploaded successfully.", result.Message);
            Assert.Equal(photoUrl, result.Data!.ProfilePhotoUrl);
        }

        [Fact]
        public async Task UploadProfilePhotoAsync_WithNonExistentUser_ReturnsFailure()
        {
            // Arrange
            var userId   = Guid.NewGuid();
            var fileMock = new Mock<IFormFile>();
            _userServiceMock.Setup(u => u.GetByIdAsync(userId)).ReturnsAsync((User?)null);

            // Act
            var result = await _studentService.UploadProfilePhotoAsync(userId, fileMock.Object);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("User not found.", result.Message);
            _fileStorageMock.Verify(f => f.UploadAsync(
                It.IsAny<System.IO.Stream>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }
    }
}
