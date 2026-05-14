-- ============================================
-- Indexes: Performance optimization for authentication queries
-- Created: 2026-03-05
-- ============================================

CREATE INDEX IF NOT EXISTS idx_users_email ON Users(Email);
CREATE INDEX IF NOT EXISTS idx_users_phone ON Users(PhoneNumber);

CREATE INDEX IF NOT EXISTS idx_sessions_userid ON UserSessions(UserId);

CREATE INDEX IF NOT EXISTS idx_otp_email ON OtpVerifications(Email);
CREATE INDEX IF NOT EXISTS idx_otp_phone ON OtpVerifications(PhoneNumber);

CREATE INDEX IF NOT EXISTS idx_login_email ON LoginAttempts(Email);
CREATE INDEX IF NOT EXISTS idx_login_attemptedAt ON LoginAttempts(AttemptedAt);
