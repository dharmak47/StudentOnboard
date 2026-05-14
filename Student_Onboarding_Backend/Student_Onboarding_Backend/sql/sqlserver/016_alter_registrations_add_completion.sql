-- ============================================
-- Migration: Add IsCompleted to CourseRegistrations
-- Purpose: Track course completion for review eligibility
-- ============================================

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('CourseRegistrations') AND name = 'IsCompleted')
    ALTER TABLE CourseRegistrations ADD IsCompleted BIT NOT NULL DEFAULT 0;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('CourseRegistrations') AND name = 'CompletedAt')
    ALTER TABLE CourseRegistrations ADD CompletedAt DATETIME2 NULL;
