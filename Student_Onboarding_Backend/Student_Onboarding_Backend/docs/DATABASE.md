# Database Documentation

## Tables Overview

| Table | Purpose | FK |
|-------|---------|-----|
| Users | Primary user accounts | - |
| UserSessions | Refresh tokens and device sessions | Users.Id |
| OtpVerifications | OTP records for verification flows | Users.Id |
| LoginAttempts | Login attempt tracking for security | - |
| UserSocialLogins | Social login provider links (future) | Users.Id |
| Courses | Course catalog with fees and syllabus | Users.Id (CreatedBy) |
| CourseRegistrations | Student course enrollments and payments | Users.Id, Courses.Id |
| Notifications | Admin notification records | Users.Id |

## Schema Details

### Users
| Column | Postgres | SQL Server | Notes |
|--------|----------|------------|-------|
| Id | UUID PK | UNIQUEIDENTIFIER PK | Generated in C# |
| FirstName | VARCHAR(100) | NVARCHAR(100) | |
| LastName | VARCHAR(100) | NVARCHAR(100) | |
| Email | VARCHAR(255) UNIQUE | NVARCHAR(255) UNIQUE | Stored lowercase |
| PhoneNumber | VARCHAR(20) UNIQUE | NVARCHAR(20) UNIQUE | Optional |
| PasswordHash | TEXT | NVARCHAR(MAX) | BCrypt hash |
| EmailVerified | BOOLEAN | BIT | Set true after OTP |
| PhoneVerified | BOOLEAN | BIT | Future use |
| IsActive | BOOLEAN | BIT | Admin can deactivate |
| IsDeleted | BOOLEAN | BIT | Soft delete |
| Role | VARCHAR(20) | NVARCHAR(20) | Default: 'Student' |
| ApprovalStatus | VARCHAR(20) | NVARCHAR(20) | Default: 'Pending'. Values: Pending, Approved, Denied |
| ApprovedBy | UUID | UNIQUEIDENTIFIER | FK -> Users.Id (admin who approved) |
| ApprovedAt | TIMESTAMP | DATETIME2 | When approval decision was made |
| DenialReason | VARCHAR(500) | NVARCHAR(500) | Reason if denied |
| PasswordUpdatedAt | TIMESTAMP | DATETIME2 | For session invalidation |
| CreatedAt | TIMESTAMP | DATETIME2 | Auto-set |
| UpdatedAt | TIMESTAMP | DATETIME2 | Set on updates |
| LastLoginAt | TIMESTAMP | DATETIME2 | Updated on login |

### UserSessions
| Column | Type | Notes |
|--------|------|-------|
| Id | UUID/UNIQUEIDENTIFIER | PK |
| UserId | UUID/UNIQUEIDENTIFIER | FK -> Users.Id |
| RefreshToken | TEXT/NVARCHAR(MAX) | Base64 encoded |
| DeviceType | VARCHAR(20) | Web, Android, iOS |
| DeviceName | VARCHAR(200) | Browser/device info |
| IpAddress | VARCHAR(50) | Client IP |
| UserAgent | TEXT | Browser user agent |
| ExpiresAt | TIMESTAMP/DATETIME2 | Token expiration |
| IsRevoked | BOOLEAN/BIT | Revoked on logout/password change |
| CreatedAt | TIMESTAMP/DATETIME2 | Auto-set |
| LastUsedAt | TIMESTAMP/DATETIME2 | Updated on token refresh |

### OtpVerifications
| Column | Type | Notes |
|--------|------|-------|
| Id | UUID/UNIQUEIDENTIFIER | PK |
| UserId | UUID/UNIQUEIDENTIFIER | FK -> Users.Id (nullable) |
| Email | VARCHAR(255) | Target email |
| PhoneNumber | VARCHAR(20) | Target phone (future) |
| OtpCode | VARCHAR(10) | 6-digit code |
| OtpType | VARCHAR(50) | EmailVerification, PasswordReset, etc. |
| AttemptCount | INT | Incremented on each attempt |
| MaxAttempts | INT | Default 5 |
| ExpiresAt | TIMESTAMP/DATETIME2 | 5 min from creation |
| IsUsed | BOOLEAN/BIT | Marked after successful verification |
| CreatedAt | TIMESTAMP/DATETIME2 | Auto-set |

### LoginAttempts
| Column | Type | Notes |
|--------|------|-------|
| Id | UUID/UNIQUEIDENTIFIER | PK |
| Email | VARCHAR(255) | Attempted email |
| IpAddress | VARCHAR(50) | Client IP |
| UserAgent | TEXT | Browser info |
| IsSuccessful | BOOLEAN/BIT | Success/failure flag |
| FailureReason | VARCHAR(100) | InvalidPassword, UserNotFound, PendingApproval, AccountDenied, etc. |
| AttemptedAt | TIMESTAMP/DATETIME2 | Auto-set |

### Courses
| Column | Postgres | SQL Server | Notes |
|--------|----------|------------|-------|
| Id | UUID PK | UNIQUEIDENTIFIER PK | Generated in C# |
| Name | VARCHAR(200) | NVARCHAR(200) | Required |
| Description | TEXT | NVARCHAR(MAX) | Optional |
| Fees | DECIMAL(10,2) | DECIMAL(10,2) | Required, >= 0 |
| OfferPrice | DECIMAL(10,2) | DECIMAL(10,2) | Optional, <= Fees |
| Syllabus | TEXT | NVARCHAR(MAX) | JSON structured: `[{"section":"Module 1","topics":["Intro"]}]` |
| Duration | VARCHAR(100) | NVARCHAR(100) | e.g., "6 months", "1 year" |
| IsActive | BOOLEAN | BIT | Default: true |
| IsDeleted | BOOLEAN | BIT | Soft delete |
| CreatedBy | UUID | UNIQUEIDENTIFIER | FK -> Users.Id (admin) |
| CreatedAt | TIMESTAMP | DATETIME2 | Auto-set |
| UpdatedAt | TIMESTAMP | DATETIME2 | Set on updates |

### CourseRegistrations
| Column | Postgres | SQL Server | Notes |
|--------|----------|------------|-------|
| Id | UUID PK | UNIQUEIDENTIFIER PK | Generated in C# |
| UserId | UUID | UNIQUEIDENTIFIER | FK -> Users.Id |
| CourseId | UUID | UNIQUEIDENTIFIER | FK -> Courses.Id |
| PaymentStatus | VARCHAR(20) | NVARCHAR(20) | Default: 'Pending'. Values: Pending, Paid, Partial, Refunded |
| PaymentAmount | DECIMAL(10,2) | DECIMAL(10,2) | Amount paid |
| PaymentDate | TIMESTAMP | DATETIME2 | When payment was recorded |
| Notes | TEXT | NVARCHAR(MAX) | Admin notes |
| IsActive | BOOLEAN | BIT | Default: true |
| CreatedAt | TIMESTAMP | DATETIME2 | Auto-set |
| UpdatedAt | TIMESTAMP | DATETIME2 | Set on updates |

### Notifications
| Column | Postgres | SQL Server | Notes |
|--------|----------|------------|-------|
| Id | UUID PK | UNIQUEIDENTIFIER PK | Generated in C# |
| UserId | UUID | UNIQUEIDENTIFIER | FK -> Users.Id (admin recipient) |
| Type | VARCHAR(50) | NVARCHAR(50) | NewRegistration, CourseRegistration, etc. |
| Title | VARCHAR(200) | NVARCHAR(200) | Notification title |
| Message | TEXT | NVARCHAR(MAX) | Full message |
| ReferenceId | UUID | UNIQUEIDENTIFIER | e.g., student userId or registration id |
| IsRead | BOOLEAN | BIT | Default: false |
| CreatedAt | TIMESTAMP | DATETIME2 | Auto-set |

## Indexes

### Phase 1
- `idx_users_email` - Users(Email)
- `idx_users_phone` - Users(PhoneNumber)
- `idx_sessions_userid` - UserSessions(UserId)
- `idx_sessions_refreshtoken` - UserSessions(RefreshToken)
- `idx_otp_email` - OtpVerifications(Email)
- `idx_otp_phone` - OtpVerifications(PhoneNumber)
- `idx_login_email` - LoginAttempts(Email)
- `idx_login_attemptedAt` - LoginAttempts(AttemptedAt)

### Phase 2
- `idx_users_approvalstatus` - Users(ApprovalStatus)
- `idx_courses_isactive` - Courses(IsActive)
- `idx_courseregistrations_userid` - CourseRegistrations(UserId)
- `idx_courseregistrations_courseid` - CourseRegistrations(CourseId)
- `idx_notifications_userid` - Notifications(UserId)
- `idx_notifications_isread` - Notifications(IsRead)

## SQL Scripts Location
- PostgreSQL: `sql/postgres/` (numbered 001-012)
- SQL Server: `sql/sqlserver/` (numbered 001-012)

Run scripts in numerical order. Each script is idempotent (uses IF NOT EXISTS / CREATE IF NOT EXISTS).

### Phase 1 Scripts (001-006)
- `001_create_users_table.sql`
- `002_create_user_sessions_table.sql`
- `003_create_otp_verifications_table.sql`
- `004_create_login_attempts_table.sql`
- `005_create_user_social_logins_table.sql`
- `006_create_indexes.sql`

### Phase 2 Scripts (007-012)
- `007_alter_users_add_approval.sql` - Adds ApprovalStatus, ApprovedBy, ApprovedAt, DenialReason
- `008_create_courses_table.sql` - Course catalog
- `009_create_course_registrations_table.sql` - Student enrollments
- `010_create_notifications_table.sql` - Admin notifications
- `011_create_phase2_indexes.sql` - Performance indexes for new tables
- `012_seed_admin_user.sql` - Default admin user (admin@synora.com, password: Admin@1234)
