# Changelog

All notable changes to the Student Onboarding Platform will be documented here.

## [0.2.0] - 2026-03-14

### Added
- Admin approval workflow for student registration
  - Students are now "Pending" after OTP verification until admin approves
  - POST /api/auth/check-approval-status - Check approval status by email
- Student management APIs
  - GET /api/student/profile - View own profile
  - PUT /api/student/profile - Update profile (name, phone)
  - GET /api/student/dashboard - Student dashboard
  - GET /api/student/courses - View registered courses
  - POST /api/student/courses/register - Register for a course
- Admin management APIs
  - GET /api/admin/dashboard - Admin dashboard with stats
  - GET /api/admin/students - Paginated student list with filters
  - GET /api/admin/students/{id} - Student detail with courses
  - PUT /api/admin/students/{id} - Activate/deactivate student
  - POST /api/admin/students/{id}/approve - Approve student registration
  - POST /api/admin/students/{id}/deny - Deny student registration
  - GET /api/admin/notifications - Admin notifications
  - GET /api/admin/notifications/unread-count - Unread notification count
  - PUT /api/admin/notifications/{id}/read - Mark notification as read
  - POST /api/admin/courses - Create course
  - PUT /api/admin/courses/{id} - Update course
  - DELETE /api/admin/courses/{id} - Soft-delete course
  - GET /api/admin/course-registrations - All course registrations
  - PUT /api/admin/course-registrations/{id}/payment - Update payment status
- Course catalog system
  - GET /api/courses - List active courses
  - GET /api/courses/{id} - Course detail with syllabus (JSON structured)
- Notification system for admin (bell icon badge with unread count)
- Role-based authorization ([Authorize(Roles = "Admin")] for admin endpoints)
- Email notifications for approval, denial, pending approval, and course registration
- Paginated responses with PaginatedResponse<T> wrapper
- Admin seed script (admin@synora.com)

### Modified
- AuthService.LoginAsync: Added approval status check (Pending/Denied blocks login)
- AuthService.VerifyOtpAsync: Sends "pending approval" email instead of welcome email, notifies admins
- User entity: Added ApprovalStatus, ApprovedBy, ApprovedAt, DenialReason columns
- UserDto: Added ApprovalStatus field
- FailureReason enum: Added PendingApproval, AccountDenied
- IEmailService: Added 4 new email template methods
- ServiceCollectionExtensions: Registered 7 new services/repositories

### Database Tables (New)
- Courses - Course catalog with fees, syllabus (JSON), duration
- CourseRegistrations - Student enrollments with payment tracking
- Notifications - Admin notification records

### Database Changes (Existing)
- Users table: Added ApprovalStatus, ApprovedBy, ApprovedAt, DenialReason columns

### SQL Scripts Added
- 007_alter_users_add_approval.sql
- 008_create_courses_table.sql
- 009_create_course_registrations_table.sql
- 010_create_notifications_table.sql
- 011_create_phase2_indexes.sql
- 012_seed_admin_user.sql

---

## [0.1.0] - 2026-03-05

### Added
- Project scaffolding with .NET 8 Web API
- Authentication system with full signup/login flow
  - POST /api/auth/signup - User registration
  - POST /api/auth/login - Email/password login
  - POST /api/auth/verify-otp - OTP verification
  - POST /api/auth/resend-otp - Resend OTP
  - POST /api/auth/forgot-password - Password reset request
  - POST /api/auth/reset-password - Reset password with OTP
  - POST /api/auth/change-password - Change password (authenticated)
  - POST /api/auth/refresh-token - JWT token refresh
  - POST /api/auth/logout - Session revocation
- JWT + Refresh Token authentication
- BCrypt password hashing
- OTP verification system with brute-force protection
- Login attempt tracking and rate limiting (5 attempts per 15 min)
- Dual database support via DbConnectionFactory
  - PostgreSQL (Supabase) for development
  - SQL Server for production
  - Toggled by AppSettings.IsProduction boolean
- Dapper micro-ORM for data access
- Serilog structured logging (Console + File sinks)
- SMTP email service via MailKit with IEmailService abstraction
- FluentValidation for request validation
- Global exception handling middleware
- CORS policy for frontend/mobile access
- SQL scripts for both PostgreSQL and SQL Server
- Project documentation (SETUP, API_REFERENCE, ARCHITECTURE, DATABASE)

### Database Tables
- Users
- UserSessions
- OtpVerifications
- LoginAttempts
- UserSocialLogins (future use)
