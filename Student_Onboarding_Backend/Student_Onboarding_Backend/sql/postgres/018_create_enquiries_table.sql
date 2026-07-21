CREATE TABLE IF NOT EXISTS Enquiries (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    Name VARCHAR(100) NOT NULL,
    Email VARCHAR(255) NOT NULL,
    PhoneNumber VARCHAR(20) NULL,
    Message TEXT NOT NULL,
    Status VARCHAR(50) NOT NULL DEFAULT 'New',
    CreatedAt TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    ResolvedAt TIMESTAMP NULL
);

CREATE INDEX IF NOT EXISTS IX_Enquiries_Status ON Enquiries (Status);
CREATE INDEX IF NOT EXISTS IX_Enquiries_CreatedAt ON Enquiries (CreatedAt);
