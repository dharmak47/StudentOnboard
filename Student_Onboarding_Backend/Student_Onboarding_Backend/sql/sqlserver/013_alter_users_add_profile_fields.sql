-- ============================================
-- Migration: Add profile fields to Users table
-- Purpose: Support full student profile (DOB, Address, Education, Photo)
-- Created: 2026-03-16
-- ============================================

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'DateOfBirth')
    ALTER TABLE Users ADD DateOfBirth DATE NULL;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'Address')
    ALTER TABLE Users ADD Address NVARCHAR(500) NULL;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'Education')
    ALTER TABLE Users ADD Education NVARCHAR(200) NULL;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'ProfilePhotoUrl')
    ALTER TABLE Users ADD ProfilePhotoUrl NVARCHAR(500) NULL;
