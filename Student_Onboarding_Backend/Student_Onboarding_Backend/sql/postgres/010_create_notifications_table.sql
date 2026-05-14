-- ============================================
-- Table: Notifications
-- Purpose: Stores admin notifications for events like new registrations
-- Created: 2026-03-14
-- ============================================

CREATE TABLE IF NOT EXISTS Notifications (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    UserId UUID NOT NULL,
    Type VARCHAR(50) NOT NULL,
    Title VARCHAR(200) NOT NULL,
    Message TEXT,
    ReferenceId UUID,
    IsRead BOOLEAN DEFAULT FALSE,
    CreatedAt TIMESTAMP DEFAULT NOW(),

    CONSTRAINT fk_notifications_user
        FOREIGN KEY (UserId) REFERENCES Users(Id)
);
