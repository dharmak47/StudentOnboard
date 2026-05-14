CREATE TABLE IF NOT EXISTS Faqs (
    Id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    Question        VARCHAR(500)     NOT NULL,
    Answer          TEXT             NOT NULL,
    SortOrder       INT              NOT NULL DEFAULT 0,
    IsActive        BOOLEAN          NOT NULL DEFAULT TRUE,
    IsDeleted       BOOLEAN          NOT NULL DEFAULT FALSE,
    CreatedBy       UUID             NULL,
    CreatedAt       TIMESTAMP        NOT NULL DEFAULT NOW(),
    UpdatedAt       TIMESTAMP        NULL
);
