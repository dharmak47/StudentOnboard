-- ============================================
-- Table: OtpVerifications
-- Purpose: Stores OTP records for verification flows
-- Created: 2026-03-05
-- ============================================

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'OtpVerifications')
CREATE TABLE OtpVerifications (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UserId UNIQUEIDENTIFIER,
    Email NVARCHAR(255),
    PhoneNumber NVARCHAR(20),
    OtpCode NVARCHAR(10) NOT NULL,
    OtpType NVARCHAR(50) NOT NULL,
    AttemptCount INT DEFAULT 0,
    MaxAttempts INT DEFAULT 5,
    ExpiresAt DATETIME2 NOT NULL,
    IsUsed BIT DEFAULT 0,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),

    CONSTRAINT fk_otp_user
        FOREIGN KEY (UserId) REFERENCES Users(Id)
);
