-- ============================================
-- Migration: Add profile fields to Users table
-- Purpose: Support full student profile (DOB, Address, Education, Photo)
-- Created: 2026-03-16
-- ============================================

ALTER TABLE Users ADD COLUMN IF NOT EXISTS DateOfBirth DATE;
ALTER TABLE Users ADD COLUMN IF NOT EXISTS Address VARCHAR(500);
ALTER TABLE Users ADD COLUMN IF NOT EXISTS Education VARCHAR(200);
ALTER TABLE Users ADD COLUMN IF NOT EXISTS ProfilePhotoUrl VARCHAR(500);
