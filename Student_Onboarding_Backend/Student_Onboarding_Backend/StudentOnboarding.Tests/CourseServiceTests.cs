using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Student_Onboarding_Platform.Data.Repositories.Interfaces;
using Student_Onboarding_Platform.Models.DTOs.Admin;
using Student_Onboarding_Platform.Models.Entities;
using Student_Onboarding_Platform.Services.Implementations;
using Student_Onboarding_Platform.Services.Interfaces;
using Xunit;

namespace StudentOnboarding.Tests
{
    public class CourseServiceTests
    {
        private readonly Mock<ICourseRepository>              _courseRepoMock;
        private readonly Mock<ICourseRegistrationRepository>  _registrationRepoMock;
        private readonly Mock<ILogger<CourseService>>         _loggerMock;
        private readonly CourseService                        _courseService;

        public CourseServiceTests()
        {
            _courseRepoMock       = new Mock<ICourseRepository>();
            _registrationRepoMock = new Mock<ICourseRegistrationRepository>();
            _loggerMock           = new Mock<ILogger<CourseService>>();

            _courseService = new CourseService(
                _courseRepoMock.Object,
                _registrationRepoMock.Object,
                _loggerMock.Object
            );
        }

        // ─── GetActiveCourses ────────────────────────────────────────────────

        [Fact]
        public async Task GetActiveCoursesAsync_ReturnsAllActiveCourses()
        {
            // Arrange
            var courses = new List<Course>
            {
                new Course { Id = Guid.NewGuid(), Name = "React", Category = "Frontend",   Fees = 3998, OfferPrice = 2999, IsActive = true },
                new Course { Id = Guid.NewGuid(), Name = "Java",  Category = "Backend",    Fees = 5999, OfferPrice = 3999, IsActive = true },
                new Course { Id = Guid.NewGuid(), Name = ".NET",  Category = "Full Stack", Fees = 4998, OfferPrice = 3999, IsActive = true }
            };
            _courseRepoMock.Setup(r => r.GetActiveCoursesAsync()).ReturnsAsync(courses);

            // Act
            var result = await _courseService.GetActiveCoursesAsync();

            // Assert
            Assert.True(result.Success);
            Assert.Equal(3, result.Data!.Count);
            Assert.Equal("React", result.Data[0].Name);
        }

        [Fact]
        public async Task GetActiveCoursesAsync_WhenNoCourses_ReturnsEmptyList()
        {
            // Arrange
            _courseRepoMock.Setup(r => r.GetActiveCoursesAsync()).ReturnsAsync(new List<Course>());

            // Act
            var result = await _courseService.GetActiveCoursesAsync();

            // Assert
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        // ─── GetCourseById ───────────────────────────────────────────────────

        [Fact]
        public async Task GetCourseByIdAsync_WithValidId_ReturnsCourse()
        {
            // Arrange
            var courseId = Guid.NewGuid();
            var course   = new Course { Id = courseId, Name = "React", Category = "Frontend", IsActive = true };
            _courseRepoMock.Setup(r => r.GetByIdAsync(courseId)).ReturnsAsync(course);

            // Act
            var result = await _courseService.GetCourseByIdAsync(courseId);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("React", result.Data!.Name);
            Assert.Equal(courseId, result.Data.Id);
        }

        [Fact]
        public async Task GetCourseByIdAsync_WithInvalidId_ReturnsFailure()
        {
            // Arrange
            var courseId = Guid.NewGuid();
            _courseRepoMock.Setup(r => r.GetByIdAsync(courseId)).ReturnsAsync((Course?)null);

            // Act
            var result = await _courseService.GetCourseByIdAsync(courseId);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Course not found.", result.Message);
        }

        // ─── CreateCourse ────────────────────────────────────────────────────

        [Fact]
        public async Task CreateCourseAsync_WithValidData_ReturnsSuccess()
        {
            // Arrange
            var adminId = Guid.NewGuid();
            var request = new CreateCourseRequest
            {
                Name       = "Python",
                Fees       = 4000,
                OfferPrice = 2999,
                Duration   = "8 Weeks",
                Instructor = "John Doe",
                Category   = "Backend"
            };
            // CreateAsync returns Task<Course>
            _courseRepoMock.Setup(r => r.CreateAsync(It.IsAny<Course>()))
                           .ReturnsAsync((Course c) => c);

            // Act
            var result = await _courseService.CreateCourseAsync(request, adminId);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Course created successfully.", result.Message);
            Assert.Equal("Python", result.Data!.Name);
            _courseRepoMock.Verify(r => r.CreateAsync(It.IsAny<Course>()), Times.Once);
        }

        [Fact]
        public async Task CreateCourseAsync_SetsIsActiveToTrue()
        {
            // Arrange
            var adminId = Guid.NewGuid();
            var request = new CreateCourseRequest { Name = "JavaScript", Fees = 3000, OfferPrice = 2000 };
            Course? captured = null;
            _courseRepoMock.Setup(r => r.CreateAsync(It.IsAny<Course>()))
                           .Callback<Course>(c => captured = c)
                           .ReturnsAsync((Course c) => c);

            // Act
            await _courseService.CreateCourseAsync(request, adminId);

            // Assert
            Assert.NotNull(captured);
            Assert.True(captured!.IsActive);
            Assert.False(captured.IsDeleted);
        }

        // ─── UpdateCourse ────────────────────────────────────────────────────

        [Fact]
        public async Task UpdateCourseAsync_WithValidId_UpdatesAndReturnsSuccess()
        {
            // Arrange
            var courseId = Guid.NewGuid();
            var existing = new Course { Id = courseId, Name = "React", Fees = 3998, OfferPrice = 2999 };
            var request  = new UpdateCourseRequest
            {
                Name       = "React Advanced",
                Fees       = 5000,
                OfferPrice = 3999,
                IsActive   = true
            };
            _courseRepoMock.Setup(r => r.GetByIdAsync(courseId)).ReturnsAsync(existing);
            _courseRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Course>())).Returns(Task.CompletedTask);

            // Act
            var result = await _courseService.UpdateCourseAsync(courseId, request);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("React Advanced", result.Data!.Name);
            Assert.Equal("Course updated successfully.", result.Message);
        }

        [Fact]
        public async Task UpdateCourseAsync_WithInvalidId_ReturnsFailure()
        {
            // Arrange
            var courseId = Guid.NewGuid();
            _courseRepoMock.Setup(r => r.GetByIdAsync(courseId)).ReturnsAsync((Course?)null);

            // Act
            var result = await _courseService.UpdateCourseAsync(courseId, new UpdateCourseRequest());

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Course not found.", result.Message);
        }

        // ─── DeleteCourse ────────────────────────────────────────────────────

        [Fact]
        public async Task DeleteCourseAsync_WithNoEnrolledStudents_DeletesSuccessfully()
        {
            // Arrange
            var courseId = Guid.NewGuid();
            var course   = new Course { Id = courseId, Name = "Old Course" };
            _courseRepoMock.Setup(r => r.GetByIdAsync(courseId)).ReturnsAsync(course);
            _registrationRepoMock.Setup(r => r.GetActiveCountByCourseAsync(courseId)).ReturnsAsync(0);
            _courseRepoMock.Setup(r => r.SoftDeleteAsync(courseId)).Returns(Task.CompletedTask);

            // Act
            var result = await _courseService.DeleteCourseAsync(courseId);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Course deleted successfully.", result.Data);
            _courseRepoMock.Verify(r => r.SoftDeleteAsync(courseId), Times.Once);
        }

        [Fact]
        public async Task DeleteCourseAsync_WithEnrolledStudents_BlocksDeletion()
        {
            // Arrange
            var courseId = Guid.NewGuid();
            var course   = new Course { Id = courseId, Name = "React" };
            _courseRepoMock.Setup(r => r.GetByIdAsync(courseId)).ReturnsAsync(course);
            _registrationRepoMock.Setup(r => r.GetActiveCountByCourseAsync(courseId)).ReturnsAsync(5);

            // Act
            var result = await _courseService.DeleteCourseAsync(courseId);

            // Assert
            Assert.False(result.Success);
            Assert.Contains("5 active student(s) are enrolled", result.Message);
            _courseRepoMock.Verify(r => r.SoftDeleteAsync(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task DeleteCourseAsync_WithInvalidId_ReturnsFailure()
        {
            // Arrange
            var courseId = Guid.NewGuid();
            _courseRepoMock.Setup(r => r.GetByIdAsync(courseId)).ReturnsAsync((Course?)null);

            // Act
            var result = await _courseService.DeleteCourseAsync(courseId);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Course not found.", result.Message);
        }
    }
}
