-- ============================================
-- Table: Invoices
-- Purpose: One persisted invoice per course registration (payment receipt)
-- ============================================

CREATE TABLE Invoices (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    RegistrationId UNIQUEIDENTIFIER NOT NULL,
    UserId UNIQUEIDENTIFIER NOT NULL,

    InvoiceNumber NVARCHAR(40) NOT NULL,
    InvoiceYear INT NOT NULL,
    SequenceNumber INT NOT NULL,
    ReceiptNumber NVARCHAR(40) NULL,
    TransactionId NVARCHAR(100) NULL,
    OrderId NVARCHAR(100) NULL,
    ReferenceNumber NVARCHAR(100) NULL,

    InvoiceDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    PaymentDate DATETIME2 NULL,

    PaymentStatus NVARCHAR(20) NOT NULL DEFAULT 'Paid',
    PaymentMethod NVARCHAR(50) NULL,
    PaymentGateway NVARCHAR(50) NULL,

    CurrencyCode NVARCHAR(10) NOT NULL DEFAULT 'INR',
    CurrencySymbol NVARCHAR(8) NOT NULL DEFAULT N'₹',

    Subtotal DECIMAL(12,2) NOT NULL DEFAULT 0,
    DiscountTotal DECIMAL(12,2) NOT NULL DEFAULT 0,
    TaxTotal DECIMAL(12,2) NOT NULL DEFAULT 0,
    ConvenienceFee DECIMAL(12,2) NOT NULL DEFAULT 0,
    PlatformFee DECIMAL(12,2) NOT NULL DEFAULT 0,
    GrandTotal DECIMAL(12,2) NOT NULL DEFAULT 0,
    AmountPaid DECIMAL(12,2) NOT NULL DEFAULT 0,
    BalanceDue DECIMAL(12,2) NOT NULL DEFAULT 0,
    RefundAmount DECIMAL(12,2) NOT NULL DEFAULT 0,

    Notes NVARCHAR(MAX) NULL,
    Terms NVARCHAR(MAX) NULL,

    -- Organization snapshot (frozen at creation)
    OrgName NVARCHAR(200) NULL,
    OrgAddressLine1 NVARCHAR(255) NULL,
    OrgAddressLine2 NVARCHAR(255) NULL,
    OrgCity NVARCHAR(100) NULL,
    OrgState NVARCHAR(100) NULL,
    OrgPostalCode NVARCHAR(20) NULL,
    OrgCountry NVARCHAR(100) NULL,
    OrgPhone NVARCHAR(40) NULL,
    OrgEmail NVARCHAR(255) NULL,
    OrgWebsite NVARCHAR(255) NULL,
    OrgTaxRegNo NVARCHAR(50) NULL,
    OrgLogoUrl NVARCHAR(1000) NULL,
    OrgFooterNote NVARCHAR(500) NULL,

    -- Customer snapshot (frozen at creation)
    CustomerName NVARCHAR(200) NULL,
    CustomerEmail NVARCHAR(255) NULL,
    CustomerPhone NVARCHAR(40) NULL,
    CustomerBillingAddress NVARCHAR(500) NULL,

    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL,

    CONSTRAINT uq_invoices_registration UNIQUE (RegistrationId),
    CONSTRAINT uq_invoices_number UNIQUE (InvoiceNumber),
    CONSTRAINT fk_invoices_registration
        FOREIGN KEY (RegistrationId) REFERENCES CourseRegistrations(Id),
    CONSTRAINT fk_invoices_user
        FOREIGN KEY (UserId) REFERENCES Users(Id)
);

CREATE INDEX IX_Invoices_UserId ON Invoices (UserId);
CREATE INDEX IX_Invoices_InvoiceYear ON Invoices (InvoiceYear);
CREATE INDEX IX_Invoices_CreatedAt ON Invoices (CreatedAt);
