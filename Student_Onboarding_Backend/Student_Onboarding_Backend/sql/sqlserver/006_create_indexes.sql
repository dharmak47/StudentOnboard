-- ============================================
-- Indexes: Performance optimization for authentication queries
-- Created: 2026-03-05
-- ============================================

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'idx_users_email')
    CREATE INDEX idx_users_email ON Users(Email);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'idx_users_phone')
    CREATE INDEX idx_users_phone ON Users(PhoneNumber);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'idx_sessions_userid')
    CREATE INDEX idx_sessions_userid ON UserSessions(UserId);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'idx_otp_email')
    CREATE INDEX idx_otp_email ON OtpVerifications(Email);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'idx_otp_phone')
    CREATE INDEX idx_otp_phone ON OtpVerifications(PhoneNumber);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'idx_login_email')
    CREATE INDEX idx_login_email ON LoginAttempts(Email);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'idx_login_attemptedAt')
    CREATE INDEX idx_login_attemptedAt ON LoginAttempts(AttemptedAt);
