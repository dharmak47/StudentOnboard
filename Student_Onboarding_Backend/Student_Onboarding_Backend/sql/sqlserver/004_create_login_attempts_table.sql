-- ============================================
-- Table: LoginAttempts
-- Purpose: Tracks login attempts for security monitoring
-- Created: 2026-03-05
-- ============================================

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'LoginAttempts')
CREATE TABLE LoginAttempts (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Email NVARCHAR(255),
    IpAddress NVARCHAR(50),
    UserAgent NVARCHAR(MAX),
    IsSuccessful BIT,
    FailureReason NVARCHAR(100),
    AttemptedAt DATETIME2 DEFAULT GETUTCDATE()
);
