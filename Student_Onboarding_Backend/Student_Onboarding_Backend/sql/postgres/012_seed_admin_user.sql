-- ============================================
-- Seed: Default admin user
-- Purpose: Creates initial admin account for the platform
-- Created: 2026-03-14
-- Note: Password is 'Admin@1234' hashed with BCrypt (work factor 12)
-- ============================================

INSERT INTO Users (Id, FirstName, LastName, Email, PhoneNumber, PasswordHash,
    EmailVerified, PhoneVerified, IsActive, IsDeleted, Role, ApprovalStatus, CreatedAt)
SELECT
    gen_random_uuid(),
    'System',
    'Admin',
    'admin@synora.com',
    NULL,
    '$2a$12$LJ3m4ys3Lk0TSwHlvDOzduZ8C/fXMcLMFdVr6FfCKnGMcIhGPqHXa',
    TRUE,
    FALSE,
    TRUE,
    FALSE,
    'Admin',
    'Approved',
    NOW()
WHERE NOT EXISTS (SELECT 1 FROM Users WHERE Email = 'admin@synora.com');
