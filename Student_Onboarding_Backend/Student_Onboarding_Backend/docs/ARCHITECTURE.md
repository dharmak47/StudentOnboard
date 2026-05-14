# Architecture Guide

## Overview

Student Onboarding Platform backend - .NET 8 Web API serving both a React admin website and .NET MAUI mobile app. Supports full authentication, admin approval workflows, student/admin dashboards, course management, and notification system.

## Tech Stack
- **Runtime:** .NET 8
- **ORM:** Dapper (micro-ORM with raw SQL)
- **Dev Database:** PostgreSQL via Supabase
- **Prod Database:** SQL Server
- **Auth:** JWT + Refresh Tokens with role-based authorization
- **Password Hashing:** BCrypt
- **Email:** MailKit (SMTP)
- **Logging:** Serilog
- **Validation:** FluentValidation

## Project Structure

```
Student Onboarding Platform/
  Controllers/
    AuthController.cs          - Authentication endpoints (signup, login, OTP, etc.)
    StudentController.cs       - Student profile, dashboard, course registration
    AdminController.cs         - Admin student management, courses, notifications
    CourseController.cs        - Public course listing (authenticated)
  Models/
    Entities/
      User.cs                  - User account with approval fields
      Course.cs                - Course catalog entry
      CourseRegistration.cs    - Student-course enrollment
      Notification.cs          - Admin notification record
      UserSession.cs           - Refresh token session
      OtpVerification.cs       - OTP verification record
      LoginAttempt.cs          - Login attempt tracking
    DTOs/
      Auth/                    - Auth request/response (signup, login, OTP, check-approval)
      Student/                 - Student profile, dashboard, course registration
      Admin/                   - Admin dashboard, student management, notifications, courses
      Course/                  - Course list/detail responses
      Common/                  - ApiResponse<T>, PaginatedResponse<T>
    Enums/
      OtpType.cs               - EmailVerification, PasswordReset, etc.
      UserRole.cs              - Student, Admin
      FailureReason.cs         - InvalidPassword, PendingApproval, AccountDenied, etc.
      ApprovalStatus.cs        - Pending, Approved, Denied
      PaymentStatus.cs         - Pending, Paid, Partial, Refunded
      NotificationType.cs      - NewRegistration, CourseRegistration, etc.
    Settings/                  - Config POCO classes (AppSettings, JwtSettings, etc.)
  Services/
    Interfaces/
      IAuthService.cs          - Auth orchestration contract
      IUserService.cs          - User CRUD + approval
      IStudentService.cs       - Student profile/dashboard/courses
      IAdminService.cs         - Admin management operations
      ICourseService.cs        - Course CRUD
      INotificationService.cs  - Admin notifications
      ITokenService.cs         - JWT/refresh token management
      IOtpService.cs           - OTP generation/verification
      IEmailService.cs         - Email sending (6 templates)
      ISessionService.cs       - Session management
      ILoginAttemptService.cs  - Login rate limiting
    Implementations/           - Corresponding implementations
  Data/
    DbConnectionFactory.cs     - Database connection toggle (Postgres/SQL Server)
    Repositories/
      Interfaces/
        IUserRepository.cs             - User table queries
        ICourseRepository.cs           - Course table queries
        ICourseRegistrationRepository.cs - Registration table queries
        INotificationRepository.cs     - Notification table queries
        IUserSessionRepository.cs      - Session table queries
        IOtpVerificationRepository.cs  - OTP table queries
        ILoginAttemptRepository.cs     - Login attempt queries
      Implementations/                 - Dapper SQL implementations
  Middleware/                  - ExceptionHandlingMiddleware
  Extensions/                  - DI registration, JWT setup, ClaimsPrincipal helpers
  Helpers/                     - OtpGenerator
  Validators/                  - FluentValidation validators (one per request DTO)
```

## Architecture Layers

```
Controller -> Service (orchestrator) -> Domain Services -> Repositories -> DbConnectionFactory -> Database
```

### Controllers (Thin layer)
- **AuthController**: Authentication flows (signup, login, OTP, password management, check-approval)
- **StudentController**: `[Authorize]` - Student profile, dashboard, course registration
- **AdminController**: `[Authorize(Roles = "Admin")]` - Student management, courses, notifications
- **CourseController**: `[Authorize]` - Course catalog (read-only)

### Services (Business logic)
- **AuthService**: Orchestrates auth flows. Coordinates UserService, TokenService, OtpService, EmailService, SessionService, LoginAttemptService, NotificationService.
- **StudentService**: Student profile management, dashboard data, course registration with duplicate checks.
- **AdminService**: Admin dashboard stats, student approval/denial, course CRUD, payment management.
- **CourseService**: Course catalog CRUD with soft delete.
- **NotificationService**: Creates notifications for all admins on student registration, manages read state.
- **Domain Services**: UserService, TokenService, OtpService, EmailService, SessionService, LoginAttemptService. Each owns a single domain.

### Repositories
- Raw Dapper SQL. One repository per table.
- **DbConnectionFactory**: Returns `NpgsqlConnection` or `SqlConnection` based on `IsProduction` flag.

## Authorization Strategy

- `[Authorize]` = Any authenticated user (Student + Admin) — StudentController, CourseController
- `[Authorize(Roles = "Admin")]` = Admin only — AdminController
- JWT already contains `ClaimTypes.Role` from Phase 1 TokenService
- Admin can access all `[Authorize]` endpoints (since Admin role satisfies the check)

## User Registration Flow

```
Signup -> OTP Verify -> Pending Approval Email -> Admin Notification
                                                          |
                                              Admin Approves/Denies
                                                          |
                                              Welcome/Denial Email
                                                          |
                                              Student can Login (if approved)
```

1. Student signs up → receives OTP email
2. Student verifies OTP → email marked verified, receives "pending approval" email, admin gets notification
3. Admin approves/denies from dashboard → student receives approval/denial email
4. Approved students can login; Pending/Denied students are blocked at login

## Database Toggle

`appsettings.json` contains `AppSettings.IsProduction`:
- `false` -> PostgreSQL (Supabase) via Npgsql
- `true` -> SQL Server via Microsoft.Data.SqlClient

GUIDs are generated in C# (`Guid.NewGuid()`), timestamps use `DateTime.UtcNow` - avoiding database-specific functions in queries.

## Email Service Abstraction

`IEmailService` interface with `SmtpEmailService` implementation (MailKit).
To swap providers: create new implementation (e.g., `SendGridEmailService`), change one line in `ServiceCollectionExtensions.cs`.

**Email Templates:**
- OTP Verification, Welcome (on approval), Pending Approval, Approval Notification, Denial Notification, Course Registration Confirmation

## Security Features
- BCrypt password hashing (work factor 12)
- JWT access tokens (15 min expiry, configurable)
- Refresh tokens (7 day expiry, stored in UserSessions)
- Role-based authorization (Student, Admin)
- Admin approval gate before login access
- OTP brute-force protection (5 attempts max)
- Login rate limiting (5 failed attempts per 15-min window)
- Session revocation on password change/reset
- Email enumeration prevention (check-approval-status returns "Pending" for unknown emails)
- Soft delete (IsDeleted flag instead of hard delete)
