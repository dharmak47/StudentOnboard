IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Faqs')
CREATE TABLE Faqs (
    Id              UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Question        NVARCHAR(500)    NOT NULL,
    Answer          NVARCHAR(MAX)    NOT NULL,
    SortOrder       INT              NOT NULL DEFAULT 0,
    IsActive        BIT              NOT NULL DEFAULT 1,
    IsDeleted       BIT              NOT NULL DEFAULT 0,
    CreatedBy       UNIQUEIDENTIFIER NULL,
    CreatedAt       DATETIME2        NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt       DATETIME2        NULL
);
