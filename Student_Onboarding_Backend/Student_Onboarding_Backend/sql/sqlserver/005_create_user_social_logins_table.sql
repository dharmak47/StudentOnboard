-- ============================================
-- Table: UserSocialLogins
-- Purpose: Links users with social login providers (future support)
-- Created: 2026-03-05
-- ============================================

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'UserSocialLogins')
CREATE TABLE UserSocialLogins (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UserId UNIQUEIDENTIFIER NOT NULL,
    Provider NVARCHAR(50) NOT NULL,
    ProviderUserId NVARCHAR(255) NOT NULL,
    Email NVARCHAR(255),
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),

    CONSTRAINT fk_social_user
        FOREIGN KEY (UserId) REFERENCES Users(Id)
);
