-- ============================================
-- Indexes: Phase 2 performance optimization
-- Created: 2026-03-14
-- ============================================

CREATE INDEX IF NOT EXISTS idx_users_approvalstatus ON Users(ApprovalStatus);

CREATE INDEX IF NOT EXISTS idx_courses_isactive ON Courses(IsActive);

CREATE INDEX IF NOT EXISTS idx_courseregistrations_userid ON CourseRegistrations(UserId);
CREATE INDEX IF NOT EXISTS idx_courseregistrations_courseid ON CourseRegistrations(CourseId);

CREATE INDEX IF NOT EXISTS idx_notifications_userid ON Notifications(UserId);
CREATE INDEX IF NOT EXISTS idx_notifications_isread ON Notifications(IsRead);
