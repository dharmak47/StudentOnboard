-- ============================================
-- Migration: Add profile fields to Users table
-- Purpose: Support full student profile (DOB, Address, Education, Photo)
-- Created: 2026-03-16
-- ============================================

DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'users' AND column_name = 'dateofbirth') THEN
        ALTER TABLE Users ADD COLUMN DateOfBirth DATE;
    END IF;

    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'users' AND column_name = 'address') THEN
        ALTER TABLE Users ADD COLUMN Address VARCHAR(500);
    END IF;

    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'users' AND column_name = 'education') THEN
        ALTER TABLE Users ADD COLUMN Education VARCHAR(200);
    END IF;

    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'users' AND column_name = 'profilephotourl') THEN
        ALTER TABLE Users ADD COLUMN ProfilePhotoUrl VARCHAR(500);
    END IF;
END $$;
