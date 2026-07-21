-- ============================================
-- Migration: 021_add_completion_tracking.sql
-- Purpose: Add Grade and AdminNotes columns for admin course completion management
-- Created: 2026-07-19
-- ============================================

-- Add Grade column if it doesn't exist
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='CourseRegistrations' AND COLUMN_NAME='Grade')
ALTER TABLE CourseRegistrations
ADD Grade DECIMAL(5,2) NULL;

-- Add AdminNotes column if it doesn't exist
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='CourseRegistrations' AND COLUMN_NAME='AdminNotes')
ALTER TABLE CourseRegistrations
ADD AdminNotes NVARCHAR(MAX) NULL;

-- Add CompletedByAdminId column to track which admin marked the course complete
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='CourseRegistrations' AND COLUMN_NAME='CompletedByAdminId')
ALTER TABLE CourseRegistrations
ADD CompletedByAdminId UNIQUEIDENTIFIER NULL;

-- Add index for faster lookups on completion status
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IDX_CourseRegistrations_IsCompleted_UserId')
CREATE INDEX IDX_CourseRegistrations_IsCompleted_UserId
ON CourseRegistrations(IsCompleted, UserId);

-- Add index for faster lookups on course completion status
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IDX_CourseRegistrations_CourseId_IsCompleted')
CREATE INDEX IDX_CourseRegistrations_CourseId_IsCompleted
ON CourseRegistrations(CourseId, IsCompleted);

-- Add index for completed date range queries
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IDX_CourseRegistrations_CompletedAt')
CREATE INDEX IDX_CourseRegistrations_CompletedAt
ON CourseRegistrations(CompletedAt);
