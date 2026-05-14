-- ============================================
-- Table: Users
-- Purpose: Stores primary user account information
-- Created: 2026-03-05
-- ============================================

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Users')
CREATE TABLE Users (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    FirstName NVARCHAR(100),
    LastName NVARCHAR(100),
    Email NVARCHAR(255) UNIQUE NOT NULL,
    PhoneNumber NVARCHAR(20) UNIQUE,
    PasswordHash NVARCHAR(MAX) NOT NULL,
    EmailVerified BIT DEFAULT 0,
    PhoneVerified BIT DEFAULT 0,
    IsActive BIT DEFAULT 1,
    IsDeleted BIT DEFAULT 0,
    Role NVARCHAR(20) DEFAULT 'Student',
    PasswordUpdatedAt DATETIME2,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2,
    LastLoginAt DATETIME2
);
