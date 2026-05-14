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
