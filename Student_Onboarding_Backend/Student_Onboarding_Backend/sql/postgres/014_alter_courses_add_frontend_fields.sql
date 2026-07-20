-- ============================================
-- Migration: Add Instructor, Category, Thumbnail to Courses table
-- Purpose: Support admin panel course management UI
-- ============================================

ALTER TABLE Courses ADD COLUMN IF NOT EXISTS Instructor VARCHAR(200);
ALTER TABLE Courses ADD COLUMN IF NOT EXISTS Category VARCHAR(100);
ALTER TABLE Courses ADD COLUMN IF NOT EXISTS Thumbnail VARCHAR(50);
