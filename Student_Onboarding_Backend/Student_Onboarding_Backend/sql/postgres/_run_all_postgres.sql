-- =====================================================
-- Combined migration runner (PostgreSQL)
-- Run this whole file once in pgAdmin to apply 001..021 in order.
-- Idempotent: safe to re-run.
-- =====================================================


-- ----- 001_create_users_table.sql -----
-- ============================================
-- Table: Users
-- Purpose: Stores primary user account information
-- Created: 2026-03-05
-- ============================================

CREATE TABLE IF NOT EXISTS Users (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    FirstName VARCHAR(100),
    LastName VARCHAR(100),
    Email VARCHAR(255) UNIQUE NOT NULL,
    PhoneNumber VARCHAR(20) UNIQUE,
    PasswordHash TEXT NOT NULL,
    EmailVerified BOOLEAN DEFAULT FALSE,
    PhoneVerified BOOLEAN DEFAULT FALSE,
    IsActive BOOLEAN DEFAULT TRUE,
    IsDeleted BOOLEAN DEFAULT FALSE,
    Role VARCHAR(20) DEFAULT 'Student',
    PasswordUpdatedAt TIMESTAMP,
    CreatedAt TIMESTAMP DEFAULT NOW(),
    UpdatedAt TIMESTAMP,
    LastLoginAt TIMESTAMP
);

-- Note: the ApprovalStatus column (with a 'Pending' default) is added later by
-- 007_alter_users_add_approval.sql. Do not add SQL Server-style ALTER statements here.


-- ----- 002_create_user_sessions_table.sql -----
-- ============================================
-- Table: UserSessions
-- Purpose: Manages refresh tokens and active login sessions
-- Created: 2026-03-05
-- ============================================

CREATE TABLE IF NOT EXISTS UserSessions (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    UserId UUID NOT NULL,
    RefreshToken TEXT NOT NULL,
    DeviceType VARCHAR(20),
    DeviceName VARCHAR(200),
    IpAddress VARCHAR(50),
    UserAgent TEXT,
    ExpiresAt TIMESTAMP NOT NULL,
    IsRevoked BOOLEAN DEFAULT FALSE,
    CreatedAt TIMESTAMP DEFAULT NOW(),
    LastUsedAt TIMESTAMP,
    CONSTRAINT fk_usersessions_user
        FOREIGN KEY (UserId) REFERENCES Users(Id)
);


-- ----- 003_create_otp_verifications_table.sql -----
-- ============================================
-- Table: OtpVerifications
-- Purpose: Stores OTP records for verification flows
-- Created: 2026-03-05
-- ============================================

CREATE TABLE IF NOT EXISTS OtpVerifications (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    UserId UUID,
    Email VARCHAR(255),
    PhoneNumber VARCHAR(20),
    OtpCode VARCHAR(10) NOT NULL,
    OtpType VARCHAR(50) NOT NULL,
    AttemptCount INT DEFAULT 0,
    MaxAttempts INT DEFAULT 5,
    ExpiresAt TIMESTAMP NOT NULL,
    IsUsed BOOLEAN DEFAULT FALSE,
    CreatedAt TIMESTAMP DEFAULT NOW(),
    CONSTRAINT fk_otp_user
        FOREIGN KEY (UserId) REFERENCES Users(Id)
);


-- ----- 004_create_login_attempts_table.sql -----
-- ============================================
-- Table: LoginAttempts
-- Purpose: Tracks login attempts for security monitoring
-- Created: 2026-03-05
-- ============================================

CREATE TABLE IF NOT EXISTS LoginAttempts (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    Email VARCHAR(255),
    IpAddress VARCHAR(50),
    UserAgent TEXT,
    IsSuccessful BOOLEAN,
    FailureReason VARCHAR(100),
    AttemptedAt TIMESTAMP DEFAULT NOW()
);


-- ----- 005_create_user_social_logins_table.sql -----
-- ============================================
-- Table: UserSocialLogins
-- Purpose: Links users with social login providers (future support)
-- Created: 2026-03-05
-- ============================================

CREATE TABLE IF NOT EXISTS UserSocialLogins (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    UserId UUID NOT NULL,
    Provider VARCHAR(50) NOT NULL,
    ProviderUserId VARCHAR(255) NOT NULL,
    Email VARCHAR(255),
    CreatedAt TIMESTAMP DEFAULT NOW(),
    CONSTRAINT fk_social_user
        FOREIGN KEY (UserId) REFERENCES Users(Id)
);


-- ----- 006_create_indexes.sql -----
-- ============================================
-- Indexes: Performance optimization for authentication queries
-- Created: 2026-03-05
-- ============================================

CREATE INDEX IF NOT EXISTS idx_users_email ON Users(Email);
CREATE INDEX IF NOT EXISTS idx_users_phone ON Users(PhoneNumber);

CREATE INDEX IF NOT EXISTS idx_sessions_userid ON UserSessions(UserId);

CREATE INDEX IF NOT EXISTS idx_otp_email ON OtpVerifications(Email);
CREATE INDEX IF NOT EXISTS idx_otp_phone ON OtpVerifications(PhoneNumber);

CREATE INDEX IF NOT EXISTS idx_login_email ON LoginAttempts(Email);
CREATE INDEX IF NOT EXISTS idx_login_attemptedAt ON LoginAttempts(AttemptedAt);


-- ----- 007_alter_users_add_approval.sql -----
-- ============================================
-- Migration: Add approval columns to Users table
-- Purpose: Support admin approval workflow for student registration
-- Created: 2026-03-14
-- ============================================

ALTER TABLE Users ADD COLUMN IF NOT EXISTS ApprovalStatus VARCHAR(20) DEFAULT 'Pending';
ALTER TABLE Users ADD COLUMN IF NOT EXISTS ApprovedBy UUID;
ALTER TABLE Users ADD COLUMN IF NOT EXISTS ApprovedAt TIMESTAMP;
ALTER TABLE Users ADD COLUMN IF NOT EXISTS DenialReason VARCHAR(500);


-- ----- 008_create_courses_table.sql -----
-- ============================================
-- Table: Courses
-- Purpose: Stores course catalog for student enrollment
-- Created: 2026-03-14
-- ============================================

CREATE TABLE IF NOT EXISTS Courses (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    Name VARCHAR(200) NOT NULL,
    Description TEXT,
    Fees DECIMAL(10,2) NOT NULL,
    OfferPrice DECIMAL(10,2),
    Syllabus TEXT,
    Duration VARCHAR(100),
    IsActive BOOLEAN DEFAULT TRUE,
    IsDeleted BOOLEAN DEFAULT FALSE,
    CreatedBy UUID NOT NULL,
    CreatedAt TIMESTAMP DEFAULT NOW(),
    UpdatedAt TIMESTAMP,
    CONSTRAINT fk_courses_createdby
        FOREIGN KEY (CreatedBy) REFERENCES Users(Id)
);


-- ----- 009_create_course_registrations_table.sql -----
-- ============================================
-- Table: CourseRegistrations
-- Purpose: Tracks student course enrollments and payment status
-- Created: 2026-03-14
-- ============================================

CREATE TABLE IF NOT EXISTS CourseRegistrations (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    UserId UUID NOT NULL,
    CourseId UUID NOT NULL,
    PaymentStatus VARCHAR(20) DEFAULT 'Pending',
    PaymentAmount DECIMAL(10,2),
    PaymentDate TIMESTAMP,
    Notes TEXT,
    IsActive BOOLEAN DEFAULT TRUE,
    CreatedAt TIMESTAMP DEFAULT NOW(),
    UpdatedAt TIMESTAMP,
    CONSTRAINT fk_courseregistrations_user
        FOREIGN KEY (UserId) REFERENCES Users(Id),
    CONSTRAINT fk_courseregistrations_course
        FOREIGN KEY (CourseId) REFERENCES Courses(Id)
);


-- ----- 010_create_notifications_table.sql -----
-- ============================================
-- Table: Notifications
-- Purpose: Stores admin notifications for events like new registrations
-- Created: 2026-03-14
-- ============================================

CREATE TABLE IF NOT EXISTS Notifications (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    UserId UUID NOT NULL,
    Type VARCHAR(50) NOT NULL,
    Title VARCHAR(200) NOT NULL,
    Message TEXT,
    ReferenceId UUID,
    IsRead BOOLEAN DEFAULT FALSE,
    CreatedAt TIMESTAMP DEFAULT NOW(),
    CONSTRAINT fk_notifications_user
        FOREIGN KEY (UserId) REFERENCES Users(Id)
);


-- ----- 011_create_phase2_indexes.sql -----
-- ============================================
-- Indexes: Phase 2 performance optimization
-- Created: 2026-03-14
-- ============================================

CREATE INDEX IF NOT EXISTS idx_users_approvalstatus ON Users(ApprovalStatus);

CREATE INDEX IF NOT EXISTS idx_courses_isactive ON Courses(IsActive);

CREATE INDEX IF NOT EXISTS idx_courseregistrations_userid ON CourseRegistrations(UserId);
CREATE INDEX IF NOT EXISTS idx_courseregistrations_courseid ON CourseRegistrations(CourseId);

CREATE INDEX IF NOT EXISTS idx_notifications_userid ON Notifications(UserId);
CREATE INDEX IF NOT EXISTS idx_notifications_isread ON Notifications(IsRead);


-- ----- 012_seed_admin_user.sql -----
-- ============================================
-- Seed: Default admin user
-- Purpose: Creates initial admin account for the platform
-- Created: 2026-03-14
-- Note: Password is 'Admin@1234' hashed with BCrypt (work factor 12)
-- ============================================

INSERT INTO Users (Id, FirstName, LastName, Email, PhoneNumber, PasswordHash,
    EmailVerified, PhoneVerified, IsActive, IsDeleted, Role, ApprovalStatus, CreatedAt)
SELECT
    gen_random_uuid(),
    'System',
    'Admin',
    'admin@synora.com',
    NULL,
    '$2a$12$LJ3m4ys3Lk0TSwHlvDOzduZ8C/fXMcLMFdVr6FfCKnGMcIhGPqHXa',
    TRUE,
    FALSE,
    TRUE,
    FALSE,
    'Admin',
    'Approved',
    NOW()
WHERE NOT EXISTS (SELECT 1 FROM Users WHERE Email = 'admin@synora.com');


-- ----- 013_alter_users_add_profile_fields.sql -----
-- ============================================
-- Migration: Add profile fields to Users table
-- Purpose: Support full student profile (DOB, Address, Education, Photo)
-- Created: 2026-03-16
-- ============================================

ALTER TABLE Users ADD COLUMN IF NOT EXISTS DateOfBirth DATE;
ALTER TABLE Users ADD COLUMN IF NOT EXISTS Address VARCHAR(500);
ALTER TABLE Users ADD COLUMN IF NOT EXISTS Education VARCHAR(200);
ALTER TABLE Users ADD COLUMN IF NOT EXISTS ProfilePhotoUrl VARCHAR(500);


-- ----- 014_alter_courses_add_frontend_fields.sql -----
-- ============================================
-- Migration: Add Instructor, Category, Thumbnail to Courses table
-- Purpose: Support admin panel course management UI
-- ============================================

ALTER TABLE Courses ADD COLUMN IF NOT EXISTS Instructor VARCHAR(200);
ALTER TABLE Courses ADD COLUMN IF NOT EXISTS Category VARCHAR(100);
ALTER TABLE Courses ADD COLUMN IF NOT EXISTS Thumbnail VARCHAR(50);


-- ----- 015_create_course_reviews_table.sql -----
-- ============================================
-- Migration: Create CourseReviews table
-- Purpose: Allow students to rate and review courses
-- ============================================

CREATE TABLE IF NOT EXISTS CourseReviews (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    CourseId UUID NOT NULL,
    UserId UUID NOT NULL,
    Rating INT NOT NULL CHECK (Rating >= 1 AND Rating <= 5),
    Remarks VARCHAR(1000),
    CreatedAt TIMESTAMP DEFAULT NOW(),
    FOREIGN KEY (CourseId) REFERENCES Courses(Id),
    FOREIGN KEY (UserId) REFERENCES Users(Id),
    CONSTRAINT UQ_CourseReview_User_Course UNIQUE (CourseId, UserId)
);


-- ----- 016_alter_registrations_add_completion.sql -----
-- ============================================
-- Migration: Add IsCompleted to CourseRegistrations
-- Purpose: Track course completion for review eligibility
-- ============================================

ALTER TABLE CourseRegistrations ADD COLUMN IF NOT EXISTS IsCompleted BOOLEAN NOT NULL DEFAULT FALSE;
ALTER TABLE CourseRegistrations ADD COLUMN IF NOT EXISTS CompletedAt TIMESTAMP;


-- ----- 017_create_faqs_table.sql -----
CREATE TABLE IF NOT EXISTS Faqs (
    Id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    Question        VARCHAR(500)     NOT NULL,
    Answer          TEXT             NOT NULL,
    SortOrder       INT              NOT NULL DEFAULT 0,
    IsActive        BOOLEAN          NOT NULL DEFAULT TRUE,
    IsDeleted       BOOLEAN          NOT NULL DEFAULT FALSE,
    CreatedBy       UUID             NULL,
    CreatedAt       TIMESTAMP        NOT NULL DEFAULT NOW(),
    UpdatedAt       TIMESTAMP        NULL
);


-- ----- 018_create_enquiries_table.sql -----
CREATE TABLE IF NOT EXISTS Enquiries (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    Name VARCHAR(100) NOT NULL,
    Email VARCHAR(255) NOT NULL,
    PhoneNumber VARCHAR(20) NULL,
    Message TEXT NOT NULL,
    Status VARCHAR(50) NOT NULL DEFAULT 'New',
    CreatedAt TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    ResolvedAt TIMESTAMP NULL
);

CREATE INDEX IF NOT EXISTS IX_Enquiries_Status ON Enquiries (Status);
CREATE INDEX IF NOT EXISTS IX_Enquiries_CreatedAt ON Enquiries (CreatedAt);


-- ----- 019_create_organization_settings_table.sql -----
-- ============================================
-- Table: OrganizationSettings
-- Purpose: Single-row organization branding / invoice defaults (admin-editable)
-- ============================================

CREATE TABLE IF NOT EXISTS OrganizationSettings (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    OrgName VARCHAR(200) NOT NULL DEFAULT 'Student Onboarding Platform',
    AddressLine1 VARCHAR(255),
    AddressLine2 VARCHAR(255),
    City VARCHAR(100),
    State VARCHAR(100),
    PostalCode VARCHAR(20),
    Country VARCHAR(100) DEFAULT 'India',
    Phone VARCHAR(40),
    Email VARCHAR(255),
    Website VARCHAR(255),
    TaxRegNo VARCHAR(50),
    LogoUrl VARCHAR(1000),
    CurrencyCode VARCHAR(10) NOT NULL DEFAULT 'INR',
    CurrencySymbol VARCHAR(8) NOT NULL DEFAULT '₹',
    DefaultTaxPercent DECIMAL(5,2) NOT NULL DEFAULT 0,
    InvoicePrefix VARCHAR(20) NOT NULL DEFAULT 'INV',
    DefaultNotes TEXT,
    DefaultTerms TEXT,
    FooterNote VARCHAR(500),
    UpdatedAt TIMESTAMP
);

-- Seed a single default row (branding is edited by admin from Settings)
INSERT INTO OrganizationSettings (OrgName, City, State, Country, CurrencyCode, CurrencySymbol, DefaultTaxPercent, InvoicePrefix, DefaultNotes, DefaultTerms, FooterNote)
SELECT
    'Student Onboarding Platform',
    'Chennai',
    'Tamil Nadu',
    'India',
    'INR',
    '₹',
    0,
    'INV',
    'Thank you for your payment. This invoice serves as proof of payment. Please retain it for your records. For support, contact our support team.',
    'This is a computer-generated invoice and does not require a physical signature. Goods/services are deemed accepted unless reported otherwise. Refunds are subject to the organization''s refund policy. All disputes are subject to the jurisdiction of the organization''s registered office.',
    'Thank you for choosing us.'
WHERE NOT EXISTS (SELECT 1 FROM OrganizationSettings);


-- ----- 020_create_invoices_table.sql -----
-- ============================================
-- Table: Invoices
-- Purpose: One persisted invoice per course registration (payment receipt)
-- ============================================

CREATE TABLE IF NOT EXISTS Invoices (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    RegistrationId UUID NOT NULL,
    UserId UUID NOT NULL,
    InvoiceNumber VARCHAR(40) NOT NULL,
    InvoiceYear INT NOT NULL,
    SequenceNumber INT NOT NULL,
    ReceiptNumber VARCHAR(40),
    TransactionId VARCHAR(100),
    OrderId VARCHAR(100),
    ReferenceNumber VARCHAR(100),
    InvoiceDate TIMESTAMP NOT NULL DEFAULT NOW(),
    PaymentDate TIMESTAMP,
    PaymentStatus VARCHAR(20) NOT NULL DEFAULT 'Paid',
    PaymentMethod VARCHAR(50),
    PaymentGateway VARCHAR(50),
    CurrencyCode VARCHAR(10) NOT NULL DEFAULT 'INR',
    CurrencySymbol VARCHAR(8) NOT NULL DEFAULT '₹',
    Subtotal DECIMAL(12,2) NOT NULL DEFAULT 0,
    DiscountTotal DECIMAL(12,2) NOT NULL DEFAULT 0,
    TaxTotal DECIMAL(12,2) NOT NULL DEFAULT 0,
    ConvenienceFee DECIMAL(12,2) NOT NULL DEFAULT 0,
    PlatformFee DECIMAL(12,2) NOT NULL DEFAULT 0,
    GrandTotal DECIMAL(12,2) NOT NULL DEFAULT 0,
    AmountPaid DECIMAL(12,2) NOT NULL DEFAULT 0,
    BalanceDue DECIMAL(12,2) NOT NULL DEFAULT 0,
    RefundAmount DECIMAL(12,2) NOT NULL DEFAULT 0,
    Notes TEXT,
    Terms TEXT,
    -- Organization snapshot (frozen at creation)
    OrgName VARCHAR(200),
    OrgAddressLine1 VARCHAR(255),
    OrgAddressLine2 VARCHAR(255),
    OrgCity VARCHAR(100),
    OrgState VARCHAR(100),
    OrgPostalCode VARCHAR(20),
    OrgCountry VARCHAR(100),
    OrgPhone VARCHAR(40),
    OrgEmail VARCHAR(255),
    OrgWebsite VARCHAR(255),
    OrgTaxRegNo VARCHAR(50),
    OrgLogoUrl VARCHAR(1000),
    OrgFooterNote VARCHAR(500),
    -- Customer snapshot (frozen at creation)
    CustomerName VARCHAR(200),
    CustomerEmail VARCHAR(255),
    CustomerPhone VARCHAR(40),
    CustomerBillingAddress VARCHAR(500),
    CreatedAt TIMESTAMP NOT NULL DEFAULT NOW(),
    UpdatedAt TIMESTAMP,
    CONSTRAINT uq_invoices_registration UNIQUE (RegistrationId),
    CONSTRAINT uq_invoices_number UNIQUE (InvoiceNumber),
    CONSTRAINT fk_invoices_registration
        FOREIGN KEY (RegistrationId) REFERENCES CourseRegistrations(Id),
    CONSTRAINT fk_invoices_user
        FOREIGN KEY (UserId) REFERENCES Users(Id)
);

CREATE INDEX IF NOT EXISTS IX_Invoices_UserId ON Invoices (UserId);
CREATE INDEX IF NOT EXISTS IX_Invoices_InvoiceYear ON Invoices (InvoiceYear);
CREATE INDEX IF NOT EXISTS IX_Invoices_CreatedAt ON Invoices (CreatedAt);


-- ----- 021_create_invoice_items_table.sql -----
-- ============================================
-- Table: InvoiceItems
-- Purpose: Line items for an invoice (Description, Qty, Unit Price, Tax, Discount, Total)
-- ============================================

CREATE TABLE IF NOT EXISTS InvoiceItems (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    InvoiceId UUID NOT NULL,
    Description VARCHAR(500) NOT NULL,
    Quantity DECIMAL(10,2) NOT NULL DEFAULT 1,
    UnitPrice DECIMAL(12,2) NOT NULL DEFAULT 0,
    TaxPercent DECIMAL(5,2) NOT NULL DEFAULT 0,
    DiscountAmount DECIMAL(12,2) NOT NULL DEFAULT 0,
    LineTotal DECIMAL(12,2) NOT NULL DEFAULT 0,
    SortOrder INT NOT NULL DEFAULT 0,
    CONSTRAINT fk_invoiceitems_invoice
        FOREIGN KEY (InvoiceId) REFERENCES Invoices(Id) ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS IX_InvoiceItems_InvoiceId ON InvoiceItems (InvoiceId);

