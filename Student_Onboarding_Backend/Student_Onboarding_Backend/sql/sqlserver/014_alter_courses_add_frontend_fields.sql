-- ============================================
-- Migration: Add Instructor, Category, Thumbnail to Courses table
-- Purpose: Support admin panel course management UI
-- ============================================

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Courses') AND name = 'Instructor')
    ALTER TABLE Courses ADD Instructor NVARCHAR(200) NULL;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Courses') AND name = 'Category')
    ALTER TABLE Courses ADD Category NVARCHAR(100) NULL;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Courses') AND name = 'Thumbnail')
    ALTER TABLE Courses ADD Thumbnail NVARCHAR(50) NULL;
