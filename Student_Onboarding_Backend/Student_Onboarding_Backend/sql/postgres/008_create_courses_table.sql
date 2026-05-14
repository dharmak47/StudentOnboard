-- ============================================
-- Table: Courses
-- Purpose: Stores course catalog for student enrollment
-- Created: 2026-03-14
-- ============================================

CREATE TABLE IF NOT EXISTS Courses (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    Name VARCHAR(200) NOT NULL,
    Description TEXT,
    Fees DECIMAL(10,2) NOT NULL,
    OfferPrice DECIMAL(10,2),
    Syllabus TEXT,
    Duration VARCHAR(100),
    IsActive BOOLEAN DEFAULT TRUE,
    IsDeleted BOOLEAN DEFAULT FALSE,
    CreatedBy UUID NOT NULL,
    CreatedAt TIMESTAMP DEFAULT NOW(),
    UpdatedAt TIMESTAMP,

    CONSTRAINT fk_courses_createdby
        FOREIGN KEY (CreatedBy) REFERENCES Users(Id)
);
