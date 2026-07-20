-- ============================================
-- Migration: 021_add_completion_tracking.sql
-- Purpose: Add Grade and AdminNotes columns for admin course completion management
-- Created: 2026-07-19
-- ============================================

-- Add Grade column if it doesn't exist
DO $$
BEGIN
  IF NOT EXISTS (
    SELECT 1 FROM information_schema.columns
    WHERE table_name = 'course_registrations' AND column_name = 'grade'
  ) THEN
    ALTER TABLE course_registrations ADD COLUMN grade NUMERIC(5,2) NULL;
  END IF;
END $$;

-- Add AdminNotes column if it doesn't exist
DO $$
BEGIN
  IF NOT EXISTS (
    SELECT 1 FROM information_schema.columns
    WHERE table_name = 'course_registrations' AND column_name = 'admin_notes'
  ) THEN
    ALTER TABLE course_registrations ADD COLUMN admin_notes TEXT NULL;
  END IF;
END $$;

-- Add CompletedByAdminId column if it doesn't exist
DO $$
BEGIN
  IF NOT EXISTS (
    SELECT 1 FROM information_schema.columns
    WHERE table_name = 'course_registrations' AND column_name = 'completed_by_admin_id'
  ) THEN
    ALTER TABLE course_registrations ADD COLUMN completed_by_admin_id UUID NULL;
  END IF;
END $$;

-- Add index for faster lookups on completion status
CREATE INDEX IF NOT EXISTS idx_course_registrations_is_completed_user_id
ON course_registrations(is_completed, user_id);

-- Add index for faster lookups on course completion status
CREATE INDEX IF NOT EXISTS idx_course_registrations_course_id_is_completed
ON course_registrations(course_id, is_completed);

-- Add index for completed date range queries
CREATE INDEX IF NOT EXISTS idx_course_registrations_completed_at
ON course_registrations(completed_at);
