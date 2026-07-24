-- ============================================
-- Migration: Add Progress Tracking to CourseRegistrations
-- Date: 2026-07-19
-- Purpose: Track module-based course progress
-- ============================================

DO $$
BEGIN
    -- Add progress percentage column
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns
                   WHERE table_name = 'courseregistrations'
                   AND column_name = 'progresspercentage') THEN
        ALTER TABLE CourseRegistrations
        ADD COLUMN ProgressPercentage DECIMAL(5,2) DEFAULT 0;
    END IF;

    -- Add current module tracking
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns
                   WHERE table_name = 'courseregistrations'
                   AND column_name = 'currentmodule') THEN
        ALTER TABLE CourseRegistrations
        ADD COLUMN CurrentModule VARCHAR(100);
    END IF;

    -- Add total modules count
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns
                   WHERE table_name = 'courseregistrations'
                   AND column_name = 'totalmodules') THEN
        ALTER TABLE CourseRegistrations
        ADD COLUMN TotalModules INTEGER DEFAULT 0;
    END IF;

    -- Add completed modules count
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns
                   WHERE table_name = 'courseregistrations'
                   AND column_name = 'completedmodules') THEN
        ALTER TABLE CourseRegistrations
        ADD COLUMN CompletedModules INTEGER DEFAULT 0;
    END IF;

    -- Add expected completion date
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns
                   WHERE table_name = 'courseregistrations'
                   AND column_name = 'expectedcompletiondate') THEN
        ALTER TABLE CourseRegistrations
        ADD COLUMN ExpectedCompletionDate DATE;
    END IF;

    -- Add last progress update timestamp
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns
                   WHERE table_name = 'courseregistrations'
                   AND column_name = 'lastprogressupdated') THEN
        ALTER TABLE CourseRegistrations
        ADD COLUMN LastProgressUpdated TIMESTAMP DEFAULT NOW();
    END IF;

END $$;

-- Create indexes for performance
CREATE INDEX IF NOT EXISTS idx_courseregistrations_progress
    ON CourseRegistrations(UserId, ProgressPercentage, IsCompleted);

CREATE INDEX IF NOT EXISTS idx_courseregistrations_expected_completion
    ON CourseRegistrations(ExpectedCompletionDate, IsCompleted);

CREATE INDEX IF NOT EXISTS idx_courseregistrations_user_progress
    ON CourseRegistrations(UserId) WHERE IsCompleted = FALSE;

-- Backfill expected completion dates for existing registrations
UPDATE CourseRegistrations
SET ExpectedCompletionDate = CURRENT_DATE + INTERVAL '90 days'
WHERE ExpectedCompletionDate IS NULL
  AND IsCompleted = FALSE;
