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
