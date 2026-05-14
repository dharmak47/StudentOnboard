-- ============================================
-- Migration: Create CourseReviews table
-- Purpose: Allow students to rate and review courses
-- ============================================

CREATE TABLE IF NOT EXISTS CourseReviews (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    CourseId UUID NOT NULL,
    UserId UUID NOT NULL,
    Rating INT NOT NULL CHECK (Rating >= 1 AND Rating <= 5),
    Remarks VARCHAR(1000),
    CreatedAt TIMESTAMP DEFAULT NOW(),
    FOREIGN KEY (CourseId) REFERENCES Courses(Id),
    FOREIGN KEY (UserId) REFERENCES Users(Id),
    CONSTRAINT UQ_CourseReview_User_Course UNIQUE (CourseId, UserId)
);
