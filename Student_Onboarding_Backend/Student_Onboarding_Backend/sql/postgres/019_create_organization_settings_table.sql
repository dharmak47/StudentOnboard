-- ============================================
-- Table: OrganizationSettings
-- Purpose: Single-row organization branding / invoice defaults (admin-editable)
-- ============================================

CREATE TABLE IF NOT EXISTS OrganizationSettings (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    OrgName VARCHAR(200) NOT NULL DEFAULT 'Student Onboarding Platform',
    AddressLine1 VARCHAR(255),
    AddressLine2 VARCHAR(255),
    City VARCHAR(100),
    State VARCHAR(100),
    PostalCode VARCHAR(20),
    Country VARCHAR(100) DEFAULT 'India',
    Phone VARCHAR(40),
    Email VARCHAR(255),
    Website VARCHAR(255),
    TaxRegNo VARCHAR(50),
    LogoUrl VARCHAR(1000),
    CurrencyCode VARCHAR(10) NOT NULL DEFAULT 'INR',
    CurrencySymbol VARCHAR(8) NOT NULL DEFAULT '₹',
    DefaultTaxPercent DECIMAL(5,2) NOT NULL DEFAULT 0,
    InvoicePrefix VARCHAR(20) NOT NULL DEFAULT 'INV',
    DefaultNotes TEXT,
    DefaultTerms TEXT,
    FooterNote VARCHAR(500),
    UpdatedAt TIMESTAMP
);

-- Seed a single default row (branding is edited by admin from Settings)
INSERT INTO OrganizationSettings (OrgName, City, State, Country, CurrencyCode, CurrencySymbol, DefaultTaxPercent, InvoicePrefix, DefaultNotes, DefaultTerms, FooterNote)
SELECT
    'Student Onboarding Platform',
    'Chennai',
    'Tamil Nadu',
    'India',
    'INR',
    '₹',
    0,
    'INV',
    'Thank you for your payment. This invoice serves as proof of payment. Please retain it for your records. For support, contact our support team.',
    'This is a computer-generated invoice and does not require a physical signature. Goods/services are deemed accepted unless reported otherwise. Refunds are subject to the organization''s refund policy. All disputes are subject to the jurisdiction of the organization''s registered office.',
    'Thank you for choosing us.'
WHERE NOT EXISTS (SELECT 1 FROM OrganizationSettings);
