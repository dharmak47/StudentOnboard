using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Student_Onboarding_Platform.Data.Repositories.Interfaces;
using Student_Onboarding_Platform.Models.DTOs.Admin;
using Student_Onboarding_Platform.Models.Entities;
using Student_Onboarding_Platform.Models.Enums;
using Student_Onboarding_Platform.Services.Implementations;
using Student_Onboarding_Platform.Services.Interfaces;
using Xunit;

namespace StudentOnboarding.Tests
{
    public class AdminServiceTests
    {
        private readonly Mock<IUserService>                    _userServiceMock;
        private readonly Mock<ICourseRepository>               _courseRepoMock;
        private readonly Mock<ICourseRegistrationRepository>   _registrationRepoMock;
        private readonly Mock<IEmailService>                   _emailServiceMock;
        private readonly Mock<INotificationService>            _notificationServiceMock;
        private readonly Mock<ISessionService>                 _sessionServiceMock;
        private readonly Mock<IFileStorageService>             _fileStorageMock;
        private readonly Mock<ILogger<AdminService>>           _loggerMock;
        private readonly AdminService                          _adminService;

        public AdminServiceTests()
        {
            _userServiceMock         = new Mock<IUserService>();
            _courseRepoMock          = new Mock<ICourseRepository>();
            _registrationRepoMock    = new Mock<ICourseRegistrationRepository>();
            _emailServiceMock        = new Mock<IEmailService>();
            _notificationServiceMock = new Mock<INotificationService>();
            _sessionServiceMock      = new Mock<ISessionService>();
            _fileStorageMock         = new Mock<IFileStorageService>();
            _loggerMock              = new Mock<ILogger<AdminService>>();

            _adminService = new AdminService(
                _userServiceMock.Object,
                _courseRepoMock.Object,
                _registrationRepoMock.Object,
                _emailServiceMock.Object,
                _notificationServiceMock.Object,
                _sessionServiceMock.Object,
                _fileStorageMock.Object,
                _loggerMock.Object
            );
        }

        // ─── GetDashboard ────────────────────────────────────────────────────

        [Fact]
        public async Task GetDashboardAsync_ReturnsAggregatedStats()
        {
            // Arrange
            _userServiceMock.Setup(u => u.GetStudentsCountAsync(null, null)).ReturnsAsync(10);
            _userServiceMock.Setup(u => u.GetStudentsCountAsync(nameof(ApprovalStatus.Pending),  null)).ReturnsAsync(3);
            _userServiceMock.Setup(u => u.GetStudentsCountAsync(nameof(ApprovalStatus.Approved), null)).ReturnsAsync(5);
            _userServiceMock.Setup(u => u.GetStudentsCountAsync(nameof(ApprovalStatus.Denied),   null)).ReturnsAsync(2);
            _courseRepoMock.Setup(r => r.GetActiveCoursesAsync()).ReturnsAsync(new List<Course>
            {
                new Course { Id = Guid.NewGuid(), Name = "React" },
                new Course { Id = Guid.NewGuid(), Name = "Java"  },
                new Course { Id = Guid.NewGuid(), Name = ".NET"  }
            });
            _registrationRepoMock.Setup(r => r.GetAllCountAsync()).ReturnsAsync(8);

            // Act
            var result = await _adminService.GetDashboardAsync();

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(10, result.Data!.TotalStudents);
            Assert.Equal(3,  result.Data.PendingApprovals);
            Assert.Equal(5,  result.Data.ApprovedStudents);
            Assert.Equal(2,  result.Data.DeniedStudents);
            Assert.Equal(3,  result.Data.TotalCourses);
            Assert.Equal(8,  result.Data.TotalRegistrations);
        }

        [Fact]
        public async Task GetDashboardAsync_WhenNoCourses_ReturnZeroCoursesCount()
        {
            // Arrange
            _userServiceMock.Setup(u => u.GetStudentsCountAsync(It.IsAny<string?>(), It.IsAny<string?>())).ReturnsAsync(0);
            _courseRepoMock.Setup(r => r.GetActiveCoursesAsync()).ReturnsAsync(new List<Course>());
            _registrationRepoMock.Setup(r => r.GetAllCountAsync()).ReturnsAsync(0);

            // Act
            var result = await _adminService.GetDashboardAsync();

            // Assert
            Assert.True(result.Success);
            Assert.Equal(0, result.Data!.TotalStudents);
            Assert.Equal(0, result.Data.TotalCourses);
        }

        // ─── GetStudents ─────────────────────────────────────────────────────

        [Fact]
        public async Task GetStudentsAsync_ReturnsPaginatedStudents()
        {
            // Arrange
            var students = new List<User>
            {
                new User { Id = Guid.NewGuid(), FirstName = "Malini", LastName = "V",   Email = "vmalini581@gmail.com", ApprovalStatus = "Approved" },
                new User { Id = Guid.NewGuid(), FirstName = "Ravi",   LastName = "K",   Email = "ravi@example.com",     ApprovalStatus = "Pending"  }
            };
            _userServiceMock.Setup(u => u.GetStudentsAsync(0, 10, null, null)).ReturnsAsync(students);
            _userServiceMock.Setup(u => u.GetStudentsCountAsync(null, null)).ReturnsAsync(2);

            // Act
            var result = await _adminService.GetStudentsAsync(1, 10, null, null);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(2, result.Data!.TotalCount);
            Assert.Equal(2, result.Data.Items.Count());
        }

        [Fact]
        public async Task GetStudentsAsync_WithStatusFilter_ReturnsOnlyFilteredStudents()
        {
            // Arrange
            var approvedStudents = new List<User>
            {
                new User { Id = Guid.NewGuid(), Email = "approved@test.com", ApprovalStatus = "Approved" }
            };
            _userServiceMock.Setup(u => u.GetStudentsAsync(0, 10, "Approved", null)).ReturnsAsync(approvedStudents);
            _userServiceMock.Setup(u => u.GetStudentsCountAsync("Approved", null)).ReturnsAsync(1);

            // Act
            var result = await _adminService.GetStudentsAsync(1, 10, "Approved", null);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(1, result.Data!.TotalCount);
            Assert.All(result.Data.Items, s => Assert.Equal("Approved", s.ApprovalStatus));
        }

        // ─── ApproveStudent ──────────────────────────────────────────────────

        [Fact]
        public async Task ApproveStudentAsync_WithValidPendingStudent_ApprovesAndSendsEmail()
        {
            // Arrange
            var adminId   = Guid.NewGuid();
            var studentId = Guid.NewGuid();
            var student   = new User
            {
                Id             = studentId,
                FirstName      = "Malini",
                Email          = "vmalini581@gmail.com",
                ApprovalStatus = "Pending"
            };

            _userServiceMock.Setup(u => u.GetByIdAsync(studentId)).ReturnsAsync(student);
            _userServiceMock.Setup(u => u.UpdateApprovalStatusAsync(studentId, nameof(ApprovalStatus.Approved), adminId, null))
                .Returns(Task.CompletedTask);
            _emailServiceMock.Setup(e => e.SendApprovalEmailAsync(student.Email, student.FirstName))
                .Returns(Task.CompletedTask);
            _notificationServiceMock.Setup(n => n.CreateStudentNotificationAsync(
                studentId, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid?>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _adminService.ApproveStudentAsync(studentId, adminId);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Student approved successfully.", result.Data);
            _userServiceMock.Verify(u => u.UpdateApprovalStatusAsync(studentId, nameof(ApprovalStatus.Approved), adminId, null), Times.Once);
            _emailServiceMock.Verify(e => e.SendApprovalEmailAsync(student.Email, student.FirstName), Times.Once);
        }

        [Fact]
        public async Task ApproveStudentAsync_WithNonExistentStudent_ReturnsFailure()
        {
            // Arrange
            var adminId   = Guid.NewGuid();
            var studentId = Guid.NewGuid();
            _userServiceMock.Setup(u => u.GetByIdAsync(studentId)).ReturnsAsync((User?)null);

            // Act
            var result = await _adminService.ApproveStudentAsync(studentId, adminId);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Student not found.", result.Message);
        }

        [Fact]
        public async Task ApproveStudentAsync_WithAlreadyApprovedStudent_ReturnsFailure()
        {
            // Arrange
            var adminId   = Guid.NewGuid();
            var studentId = Guid.NewGuid();
            var student   = new User { Id = studentId, ApprovalStatus = "Approved" };
            _userServiceMock.Setup(u => u.GetByIdAsync(studentId)).ReturnsAsync(student);

            // Act
            var result = await _adminService.ApproveStudentAsync(studentId, adminId);

            // Assert
            Assert.False(result.Success);
            Assert.Contains("already approved", result.Message, StringComparison.OrdinalIgnoreCase);
        }

        // ─── DenyStudent ─────────────────────────────────────────────────────

        [Fact]
        public async Task DenyStudentAsync_WithValidPendingStudent_DeniesAndSendsEmail()
        {
            // Arrange
            var adminId   = Guid.NewGuid();
            var studentId = Guid.NewGuid();
            var student   = new User
            {
                Id             = studentId,
                FirstName      = "Ravi",
                Email          = "ravi@example.com",
                ApprovalStatus = "Pending"
            };
            var request = new DenyStudentRequest { Reason = "Incomplete documents" };

            _userServiceMock.Setup(u => u.GetByIdAsync(studentId)).ReturnsAsync(student);
            _userServiceMock.Setup(u => u.UpdateApprovalStatusAsync(studentId, nameof(ApprovalStatus.Denied), adminId, request.Reason))
                .Returns(Task.CompletedTask);
            _sessionServiceMock.Setup(s => s.RevokeAllUserSessionsAsync(studentId)).Returns(Task.CompletedTask);
            _emailServiceMock.Setup(e => e.SendDenialEmailAsync(student.Email, student.FirstName, request.Reason))
                .Returns(Task.CompletedTask);
            _notificationServiceMock.Setup(n => n.CreateStudentNotificationAsync(
                studentId, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid?>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _adminService.DenyStudentAsync(studentId, adminId, request);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Student denied successfully.", result.Data);
            _userServiceMock.Verify(u => u.UpdateApprovalStatusAsync(studentId, nameof(ApprovalStatus.Denied), adminId, request.Reason), Times.Once);
            _emailServiceMock.Verify(e => e.SendDenialEmailAsync(student.Email, student.FirstName, request.Reason), Times.Once);
        }

        [Fact]
        public async Task DenyStudentAsync_WithNonExistentStudent_ReturnsFailure()
        {
            // Arrange
            var adminId   = Guid.NewGuid();
            var studentId = Guid.NewGuid();
            _userServiceMock.Setup(u => u.GetByIdAsync(studentId)).ReturnsAsync((User?)null);

            // Act
            var result = await _adminService.DenyStudentAsync(studentId, adminId, new DenyStudentRequest());

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Student not found.", result.Message);
        }

        [Fact]
        public async Task DenyStudentAsync_WithAlreadyDeniedStudent_ReturnsFailure()
        {
            // Arrange
            var adminId   = Guid.NewGuid();
            var studentId = Guid.NewGuid();
            var student   = new User { Id = studentId, ApprovalStatus = "Denied" };
            _userServiceMock.Setup(u => u.GetByIdAsync(studentId)).ReturnsAsync(student);

            // Act
            var result = await _adminService.DenyStudentAsync(studentId, adminId, new DenyStudentRequest());

            // Assert
            Assert.False(result.Success);
            Assert.Contains("already denied", result.Message, StringComparison.OrdinalIgnoreCase);
        }
    }
}
