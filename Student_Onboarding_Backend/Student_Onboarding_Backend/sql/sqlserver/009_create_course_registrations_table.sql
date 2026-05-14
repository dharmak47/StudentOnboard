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
