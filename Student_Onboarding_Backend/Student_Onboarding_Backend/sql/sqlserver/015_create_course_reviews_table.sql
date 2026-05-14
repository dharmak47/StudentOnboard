-- ============================================
-- Migration: Create CourseReviews table
-- Purpose: Allow students to rate and review courses
-- ============================================

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'CourseReviews')
BEGIN
    CREATE TABLE CourseReviews (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        CourseId UNIQUEIDENTIFIER NOT NULL,
        UserId UNIQUEIDENTIFIER NOT NULL,
        Rating INT NOT NULL CHECK (Rating >= 1 AND Rating <= 5),
        Remarks NVARCHAR(1000) NULL,
        CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
        FOREIGN KEY (CourseId) REFERENCES Courses(Id),
        FOREIGN KEY (UserId) REFERENCES Users(Id),
        CONSTRAINT UQ_CourseReview_User_Course UNIQUE (CourseId, UserId)
    );
END
