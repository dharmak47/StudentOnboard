-- ============================================
-- Table: Notifications
-- Purpose: Stores admin notifications for events like new registrations
-- Created: 2026-03-14
-- ============================================

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Notifications')
CREATE TABLE Notifications (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UserId UNIQUEIDENTIFIER NOT NULL,
    Type NVARCHAR(50) NOT NULL,
    Title NVARCHAR(200) NOT NULL,
    Message NVARCHAR(MAX),
    ReferenceId UNIQUEIDENTIFIER,
    IsRead BIT DEFAULT 0,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),

    CONSTRAINT fk_notifications_user
        FOREIGN KEY (UserId) REFERENCES Users(Id)
);
