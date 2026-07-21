-- ============================================
-- Migration: Add IsCompleted to CourseRegistrations
-- Purpose: Track course completion for review eligibility
-- ============================================

ALTER TABLE CourseRegistrations ADD COLUMN IF NOT EXISTS IsCompleted BOOLEAN NOT NULL DEFAULT FALSE;
ALTER TABLE CourseRegistrations ADD COLUMN IF NOT EXISTS CompletedAt TIMESTAMP;
