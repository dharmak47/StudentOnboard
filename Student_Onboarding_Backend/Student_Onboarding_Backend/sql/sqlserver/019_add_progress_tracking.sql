-- ============================================
-- Migration: Add Progress Tracking to CourseRegistrations
-- Database: SQL Server
-- Date: 2026-07-19
-- Purpose: Track module-based course progress
-- ============================================

-- Add progress percentage column
IF NOT EXISTS (SELECT 1 FROM information_schema.COLUMNS
               WHERE TABLE_NAME = 'CourseRegistrations'
               AND COLUMN_NAME = 'ProgressPercentage')
BEGIN
    ALTER TABLE CourseRegistrations
    ADD ProgressPercentage DECIMAL(5,2) DEFAULT 0;
END;

-- Add current module tracking
IF NOT EXISTS (SELECT 1 FROM information_schema.COLUMNS
               WHERE TABLE_NAME = 'CourseRegistrations'
               AND COLUMN_NAME = 'CurrentModule')
BEGIN
    ALTER TABLE CourseRegistrations
    ADD CurrentModule VARCHAR(100) NULL;
END;

-- Add total modules count
IF NOT EXISTS (SELECT 1 FROM information_schema.COLUMNS
               WHERE TABLE_NAME = 'CourseRegistrations'
               AND COLUMN_NAME = 'TotalModules')
BEGIN
    ALTER TABLE CourseRegistrations
    ADD TotalModules INT DEFAULT 0;
END;

-- Add completed modules count
IF NOT EXISTS (SELECT 1 FROM information_schema.COLUMNS
               WHERE TABLE_NAME = 'CourseRegistrations'
               AND COLUMN_NAME = 'CompletedModules')
BEGIN
    ALTER TABLE CourseRegistrations
    ADD CompletedModules INT DEFAULT 0;
END;

-- Add expected completion date
IF NOT EXISTS (SELECT 1 FROM information_schema.COLUMNS
               WHERE TABLE_NAME = 'CourseRegistrations'
               AND COLUMN_NAME = 'ExpectedCompletionDate')
BEGIN
    ALTER TABLE CourseRegistrations
    ADD ExpectedCompletionDate DATE NULL;
END;

-- Add last progress update timestamp
IF NOT EXISTS (SELECT 1 FROM information_schema.COLUMNS
               WHERE TABLE_NAME = 'CourseRegistrations'
               AND COLUMN_NAME = 'LastProgressUpdated')
BEGIN
    ALTER TABLE CourseRegistrations
    ADD LastProgressUpdated DATETIME DEFAULT GETUTCDATE();
END;

-- Create indexes for performance
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'idx_courseregistrations_progress')
BEGIN
    CREATE INDEX idx_courseregistrations_progress
    ON CourseRegistrations(UserId, ProgressPercentage, IsCompleted);
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'idx_courseregistrations_expected_completion')
BEGIN
    CREATE INDEX idx_courseregistrations_expected_completion
    ON CourseRegistrations(ExpectedCompletionDate, IsCompleted);
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'idx_courseregistrations_user_progress')
BEGIN
    CREATE INDEX idx_courseregistrations_user_progress
    ON CourseRegistrations(UserId)
    WHERE IsCompleted = 0;
END;

-- Backfill expected completion dates for existing registrations
UPDATE CourseRegistrations
SET ExpectedCompletionDate = DATEADD(day, 90, CAST(GETUTCDATE() AS DATE))
WHERE ExpectedCompletionDate IS NULL
  AND IsCompleted = 0;
