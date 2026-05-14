-- ============================================
-- Migration: Add IsCompleted to CourseRegistrations
-- Purpose: Track course completion for review eligibility
-- ============================================

DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'courseregistrations' AND column_name = 'iscompleted') THEN
        ALTER TABLE CourseRegistrations ADD COLUMN IsCompleted BOOLEAN NOT NULL DEFAULT FALSE;
    END IF;

    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'courseregistrations' AND column_name = 'completedat') THEN
        ALTER TABLE CourseRegistrations ADD COLUMN CompletedAt TIMESTAMP;
    END IF;
END $$;
