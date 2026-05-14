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
