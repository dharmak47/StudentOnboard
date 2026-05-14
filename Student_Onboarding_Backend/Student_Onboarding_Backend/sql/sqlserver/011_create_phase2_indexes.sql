-- ============================================
-- Indexes: Phase 2 performance optimization
-- Created: 2026-03-14
-- ============================================

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'idx_users_approvalstatus')
    CREATE INDEX idx_users_approvalstatus ON Users(ApprovalStatus);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'idx_courses_isactive')
    CREATE INDEX idx_courses_isactive ON Courses(IsActive);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'idx_courseregistrations_userid')
    CREATE INDEX idx_courseregistrations_userid ON CourseRegistrations(UserId);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'idx_courseregistrations_courseid')
    CREATE INDEX idx_courseregistrations_courseid ON CourseRegistrations(CourseId);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'idx_notifications_userid')
    CREATE INDEX idx_notifications_userid ON Notifications(UserId);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'idx_notifications_isread')
    CREATE INDEX idx_notifications_isread ON Notifications(IsRead);
