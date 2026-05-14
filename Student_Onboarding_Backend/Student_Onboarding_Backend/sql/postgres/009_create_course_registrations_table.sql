-- ============================================
-- Table: CourseRegistrations
-- Purpose: Tracks student course enrollments and payment status
-- Created: 2026-03-14
-- ============================================

CREATE TABLE IF NOT EXISTS CourseRegistrations (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    UserId UUID NOT NULL,
    CourseId UUID NOT NULL,
    PaymentStatus VARCHAR(20) DEFAULT 'Pending',
    PaymentAmount DECIMAL(10,2),
    PaymentDate TIMESTAMP,
    Notes TEXT,
    IsActive BOOLEAN DEFAULT TRUE,
    CreatedAt TIMESTAMP DEFAULT NOW(),
    UpdatedAt TIMESTAMP,

    CONSTRAINT fk_courseregistrations_user
        FOREIGN KEY (UserId) REFERENCES Users(Id),
    CONSTRAINT fk_courseregistrations_course
        FOREIGN KEY (CourseId) REFERENCES Courses(Id)
);
