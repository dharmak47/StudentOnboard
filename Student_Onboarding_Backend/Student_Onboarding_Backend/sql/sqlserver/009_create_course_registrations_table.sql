-- ============================================
-- Table: CourseRegistrations
-- Purpose: Tracks student course enrollments and payment status
-- Created: 2026-03-14
-- ============================================

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'CourseRegistrations')
CREATE TABLE CourseRegistrations (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UserId UNIQUEIDENTIFIER NOT NULL,
    CourseId UNIQUEIDENTIFIER NOT NULL,
    PaymentStatus NVARCHAR(20) DEFAULT 'Pending',
    PaymentAmount DECIMAL(10,2),
    PaymentDate DATETIME2,
    Notes NVARCHAR(MAX),
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2,

    CONSTRAINT fk_courseregistrations_user
        FOREIGN KEY (UserId) REFERENCES Users(Id),
    CONSTRAINT fk_courseregistrations_course
        FOREIGN KEY (CourseId) REFERENCES Courses(Id)
);

-- Add progress tracking columns if they don't exist
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='CourseRegistrations' AND COLUMN_NAME='ExpectedCompletionDate')
ALTER TABLE CourseRegistrations
ADD ExpectedCompletionDate DATE NULL;

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='CourseRegistrations' AND COLUMN_NAME='CurrentModule')
ALTER TABLE CourseRegistrations
ADD CurrentModule NVARCHAR(255) NULL;

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='CourseRegistrations' AND COLUMN_NAME='TotalModules')
ALTER TABLE CourseRegistrations
ADD TotalModules INT DEFAULT 0;

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='CourseRegistrations' AND COLUMN_NAME='CompletedModules')
ALTER TABLE CourseRegistrations
ADD CompletedModules INT DEFAULT 0;

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='CourseRegistrations' AND COLUMN_NAME='ProgressPercentage')
ALTER TABLE CourseRegistrations
ADD ProgressPercentage DECIMAL(5,2) DEFAULT 0.00;

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='CourseRegistrations' AND COLUMN_NAME='LastProgressUpdated')
ALTER TABLE CourseRegistrations
ADD LastProgressUpdated DATETIME2 NULL;

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='CourseRegistrations' AND COLUMN_NAME='IsCompleted')
ALTER TABLE CourseRegistrations
ADD IsCompleted BIT DEFAULT 0;

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='CourseRegistrations' AND COLUMN_NAME='CompletedAt')
ALTER TABLE CourseRegistrations
ADD CompletedAt DATETIME2 NULL;