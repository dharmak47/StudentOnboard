-- ============================================
-- Migration: Create MonthlyAnalytics Table
-- Date: 2026-07-19
-- Purpose: Store monthly aggregated metrics
-- ============================================

CREATE TABLE IF NOT EXISTS MonthlyAnalytics (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),

    -- Period tracking (first day of month)
    YearMonth DATE NOT NULL,

    -- Enrollment metrics
    NewEnrollments INTEGER DEFAULT 0,
    TotalEnrollments INTEGER DEFAULT 0,

    -- Completion metrics
    CompletedCourses INTEGER DEFAULT 0,
    PendingCompletions INTEGER DEFAULT 0,

    -- Payment metrics
    TotalRevenueCollected DECIMAL(12,2) DEFAULT 0,
    PaymentsCompleted INTEGER DEFAULT 0,
    PaymentsPending INTEGER DEFAULT 0,

    -- User metrics
    ActiveStudents INTEGER DEFAULT 0,
    ApprovedStudents INTEGER DEFAULT 0,
    PendingApprovals INTEGER DEFAULT 0,

    -- Performance metrics
    AverageCompletionPercentage DECIMAL(5,2) DEFAULT 0,
    CoursePassRate DECIMAL(5,2) DEFAULT 0,

    -- Audit fields
    CreatedAt TIMESTAMP DEFAULT NOW(),
    UpdatedAt TIMESTAMP DEFAULT NOW(),
    UpdatedBy UUID,

    -- Constraints
    CONSTRAINT fk_analytics_updatedby
        FOREIGN KEY (UpdatedBy) REFERENCES Users(Id),
    CONSTRAINT uq_yearmonth
        UNIQUE(YearMonth)
);

-- Create indexes for performance
CREATE INDEX IF NOT EXISTS idx_analytics_yearmonth
    ON MonthlyAnalytics(YearMonth DESC);

CREATE INDEX IF NOT EXISTS idx_analytics_updated
    ON MonthlyAnalytics(UpdatedAt DESC);

CREATE INDEX IF NOT EXISTS idx_analytics_enrollments
    ON MonthlyAnalytics(NewEnrollments DESC);

-- Initialize monthly records for current and previous 12 months
INSERT INTO MonthlyAnalytics (YearMonth, CreatedAt, UpdatedAt)
SELECT
    (DATE_TRUNC('month', NOW()) - (i || ' months')::INTERVAL)::DATE as YearMonth,
    NOW(),
    NOW()
FROM generate_series(0, 12) AS t(i)
ON CONFLICT (YearMonth) DO NOTHING;
