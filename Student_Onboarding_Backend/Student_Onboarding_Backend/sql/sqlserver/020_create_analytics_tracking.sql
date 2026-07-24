-- ============================================
-- Migration: Create MonthlyAnalytics Table
-- Database: SQL Server
-- Date: 2026-07-19
-- Purpose: Store monthly aggregated metrics
-- ============================================

IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MonthlyAnalytics]') AND type in (N'U'))
BEGIN
    CREATE TABLE dbo.MonthlyAnalytics (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),

        -- Period tracking (first day of month)
        YearMonth DATE NOT NULL,

        -- Enrollment metrics
        NewEnrollments INT DEFAULT 0,
        TotalEnrollments INT DEFAULT 0,

        -- Completion metrics
        CompletedCourses INT DEFAULT 0,
        PendingCompletions INT DEFAULT 0,

        -- Payment metrics
        TotalRevenueCollected DECIMAL(12,2) DEFAULT 0,
        PaymentsCompleted INT DEFAULT 0,
        PaymentsPending INT DEFAULT 0,

        -- User metrics
        ActiveStudents INT DEFAULT 0,
        ApprovedStudents INT DEFAULT 0,
        PendingApprovals INT DEFAULT 0,

        -- Performance metrics
        AverageCompletionPercentage DECIMAL(5,2) DEFAULT 0,
        CoursePassRate DECIMAL(5,2) DEFAULT 0,

        -- Audit fields
        CreatedAt DATETIME DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME DEFAULT GETUTCDATE(),
        UpdatedBy UNIQUEIDENTIFIER NULL,

        -- Constraints
        CONSTRAINT fk_analytics_updatedby
            FOREIGN KEY (UpdatedBy) REFERENCES Users(Id),
        CONSTRAINT uq_yearmonth
            UNIQUE(YearMonth)
    );
END;

-- Create indexes for performance
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'idx_analytics_yearmonth')
BEGIN
    CREATE INDEX idx_analytics_yearmonth
    ON MonthlyAnalytics(YearMonth DESC);
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'idx_analytics_updated')
BEGIN
    CREATE INDEX idx_analytics_updated
    ON MonthlyAnalytics(UpdatedAt DESC);
END;

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'idx_analytics_enrollments')
BEGIN
    CREATE INDEX idx_analytics_enrollments
    ON MonthlyAnalytics(NewEnrollments DESC);
END;

-- Initialize monthly records for current and previous 12 months
DECLARE @i INT = 0;
WHILE @i <= 12
BEGIN
    INSERT INTO MonthlyAnalytics (YearMonth, CreatedAt, UpdatedAt)
    SELECT
        CAST(DATEADD(month, -@i, EOMONTH(GETUTCDATE(), -1)) AS DATE) AS YearMonth,
        GETUTCDATE(),
        GETUTCDATE()
    WHERE NOT EXISTS (
        SELECT 1 FROM MonthlyAnalytics
        WHERE YearMonth = CAST(DATEADD(month, -@i, EOMONTH(GETUTCDATE(), -1)) AS DATE)
    );
    SET @i = @i + 1;
END;
