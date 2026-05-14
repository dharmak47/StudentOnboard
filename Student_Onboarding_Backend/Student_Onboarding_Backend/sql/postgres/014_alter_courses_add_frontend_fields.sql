-- ============================================
-- Migration: Add Instructor, Category, Thumbnail to Courses table
-- Purpose: Support admin panel course management UI
-- ============================================

DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'courses' AND column_name = 'instructor') THEN
        ALTER TABLE Courses ADD COLUMN Instructor VARCHAR(200);
    END IF;

    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'courses' AND column_name = 'category') THEN
        ALTER TABLE Courses ADD COLUMN Category VARCHAR(100);
    END IF;

    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'courses' AND column_name = 'thumbnail') THEN
        ALTER TABLE Courses ADD COLUMN Thumbnail VARCHAR(50);
    END IF;
END $$;
