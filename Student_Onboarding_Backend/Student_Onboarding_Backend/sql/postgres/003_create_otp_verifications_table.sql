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
