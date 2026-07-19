using System;
using System.Threading.Tasks;
using Moq;
using Student_Onboarding_Platform.Data.Repositories.Interfaces;
using Student_Onboarding_Platform.Models.Entities;
using Student_Onboarding_Platform.Services.Implementations;
using Xunit;

namespace StudentOnboarding.Tests
{
    public class UserServiceTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly UserService _userService;

        public UserServiceTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _userService = new UserService(_userRepositoryMock.Object);
        }

        [Fact]
        public async Task GetByEmailAsync_WithValidEmail_ReturnsUser()
        {
            // Arrange
            var email = "test@example.com";
            var expectedUser = new User { Email = email, FirstName = "Test", LastName = "User" };
            _userRepositoryMock.Setup(x => x.GetByEmailAsync(email)).ReturnsAsync(expectedUser);

            // Act
            var result = await _userService.GetByEmailAsync(email);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(email, result.Email);
            _userRepositoryMock.Verify(x => x.GetByEmailAsync(email), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_WithNewUser_SavesToDatabase()
        {
            // Arrange
            var user = new User { Email = "new@example.com", FirstName = "New", LastName = "User" };
            _userRepositoryMock.Setup(x => x.CreateAsync(user)).ReturnsAsync(user);

            // Act
            var result = await _userService.CreateAsync(user);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(user.Email, result.Email);
            _userRepositoryMock.Verify(x => x.CreateAsync(user), Times.Once);
        }

        [Fact]
        public async Task UpdateLastLoginAsync_CallsRepositoryWithCorrectParameters()
        {
            // Arrange
            var userId = Guid.NewGuid();

            // Act
            await _userService.UpdateLastLoginAsync(userId);

            // Assert
            _userRepositoryMock.Verify(x => x.UpdateLastLoginAsync(userId, It.IsAny<DateTime>()), Times.Once);
        }
    }
}
