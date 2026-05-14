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
