-- ============================================
-- Table: OrganizationSettings
-- Purpose: Single-row organization branding / invoice defaults (admin-editable)
-- ============================================

CREATE TABLE OrganizationSettings (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    OrgName NVARCHAR(200) NOT NULL DEFAULT 'Student Onboarding Platform',
    AddressLine1 NVARCHAR(255) NULL,
    AddressLine2 NVARCHAR(255) NULL,
    City NVARCHAR(100) NULL,
    State NVARCHAR(100) NULL,
    PostalCode NVARCHAR(20) NULL,
    Country NVARCHAR(100) NULL DEFAULT 'India',
    Phone NVARCHAR(40) NULL,
    Email NVARCHAR(255) NULL,
    Website NVARCHAR(255) NULL,
    TaxRegNo NVARCHAR(50) NULL,
    LogoUrl NVARCHAR(1000) NULL,
    CurrencyCode NVARCHAR(10) NOT NULL DEFAULT 'INR',
    CurrencySymbol NVARCHAR(8) NOT NULL DEFAULT N'₹',
    DefaultTaxPercent DECIMAL(5,2) NOT NULL DEFAULT 0,
    InvoicePrefix NVARCHAR(20) NOT NULL DEFAULT 'INV',
    DefaultNotes NVARCHAR(MAX) NULL,
    DefaultTerms NVARCHAR(MAX) NULL,
    FooterNote NVARCHAR(500) NULL,
    UpdatedAt DATETIME2 NULL
);

-- Seed a single default row (branding is edited by admin from Settings)
INSERT INTO OrganizationSettings (OrgName, City, State, Country, CurrencyCode, CurrencySymbol, DefaultTaxPercent, InvoicePrefix, DefaultNotes, DefaultTerms, FooterNote)
SELECT
    'Student Onboarding Platform',
    'Chennai',
    'Tamil Nadu',
    'India',
    'INR',
    N'₹',
    0,
    'INV',
    'Thank you for your payment. This invoice serves as proof of payment. Please retain it for your records. For support, contact our support team.',
    'This is a computer-generated invoice and does not require a physical signature. Goods/services are deemed accepted unless reported otherwise. Refunds are subject to the organization''s refund policy. All disputes are subject to the jurisdiction of the organization''s registered office.',
    'Thank you for choosing us.'
WHERE NOT EXISTS (SELECT 1 FROM OrganizationSettings);
