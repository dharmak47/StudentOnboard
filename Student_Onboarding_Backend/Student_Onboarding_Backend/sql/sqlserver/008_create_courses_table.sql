-- ============================================
-- Table: Courses
-- Purpose: Stores course catalog for student enrollment
-- Created: 2026-03-14
-- ============================================

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Courses')
CREATE TABLE Courses (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Name NVARCHAR(200) NOT NULL,
    Description NVARCHAR(MAX),
    Fees DECIMAL(10,2) NOT NULL,
    OfferPrice DECIMAL(10,2),
    Syllabus NVARCHAR(MAX),
    Duration NVARCHAR(100),
    IsActive BIT DEFAULT 1,
    IsDeleted BIT DEFAULT 0,
    CreatedBy UNIQUEIDENTIFIER NOT NULL,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2,

    CONSTRAINT fk_courses_createdby
        FOREIGN KEY (CreatedBy) REFERENCES Users(Id)
);
