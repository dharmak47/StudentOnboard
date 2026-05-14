-- ============================================
-- Table: UserSessions
-- Purpose: Manages refresh tokens and active login sessions
-- Created: 2026-03-05
-- ============================================

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'UserSessions')
CREATE TABLE UserSessions (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UserId UNIQUEIDENTIFIER NOT NULL,
    RefreshToken NVARCHAR(MAX) NOT NULL,
    DeviceType NVARCHAR(20),
    DeviceName NVARCHAR(200),
    IpAddress NVARCHAR(50),
    UserAgent NVARCHAR(MAX),
    ExpiresAt DATETIME2 NOT NULL,
    IsRevoked BIT DEFAULT 0,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    LastUsedAt DATETIME2,

    CONSTRAINT fk_usersessions_user
        FOREIGN KEY (UserId) REFERENCES Users(Id)
);
