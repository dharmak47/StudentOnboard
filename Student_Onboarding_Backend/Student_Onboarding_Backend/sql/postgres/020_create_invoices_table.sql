-- ============================================
-- Table: Invoices
-- Purpose: One persisted invoice per course registration (payment receipt)
-- ============================================

CREATE TABLE IF NOT EXISTS Invoices (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    RegistrationId UUID NOT NULL,
    UserId UUID NOT NULL,
    InvoiceNumber VARCHAR(40) NOT NULL,
    InvoiceYear INT NOT NULL,
    SequenceNumber INT NOT NULL,
    ReceiptNumber VARCHAR(40),
    TransactionId VARCHAR(100),
    OrderId VARCHAR(100),
    ReferenceNumber VARCHAR(100),
    InvoiceDate TIMESTAMP NOT NULL DEFAULT NOW(),
    PaymentDate TIMESTAMP,
    PaymentStatus VARCHAR(20) NOT NULL DEFAULT 'Paid',
    PaymentMethod VARCHAR(50),
    PaymentGateway VARCHAR(50),
    CurrencyCode VARCHAR(10) NOT NULL DEFAULT 'INR',
    CurrencySymbol VARCHAR(8) NOT NULL DEFAULT '₹',
    Subtotal DECIMAL(12,2) NOT NULL DEFAULT 0,
    DiscountTotal DECIMAL(12,2) NOT NULL DEFAULT 0,
    TaxTotal DECIMAL(12,2) NOT NULL DEFAULT 0,
    ConvenienceFee DECIMAL(12,2) NOT NULL DEFAULT 0,
    PlatformFee DECIMAL(12,2) NOT NULL DEFAULT 0,
    GrandTotal DECIMAL(12,2) NOT NULL DEFAULT 0,
    AmountPaid DECIMAL(12,2) NOT NULL DEFAULT 0,
    BalanceDue DECIMAL(12,2) NOT NULL DEFAULT 0,
    RefundAmount DECIMAL(12,2) NOT NULL DEFAULT 0,
    Notes TEXT,
    Terms TEXT,
    -- Organization snapshot (frozen at creation)
    OrgName VARCHAR(200),
    OrgAddressLine1 VARCHAR(255),
    OrgAddressLine2 VARCHAR(255),
    OrgCity VARCHAR(100),
    OrgState VARCHAR(100),
    OrgPostalCode VARCHAR(20),
    OrgCountry VARCHAR(100),
    OrgPhone VARCHAR(40),
    OrgEmail VARCHAR(255),
    OrgWebsite VARCHAR(255),
    OrgTaxRegNo VARCHAR(50),
    OrgLogoUrl VARCHAR(1000),
    OrgFooterNote VARCHAR(500),
    -- Customer snapshot (frozen at creation)
    CustomerName VARCHAR(200),
    CustomerEmail VARCHAR(255),
    CustomerPhone VARCHAR(40),
    CustomerBillingAddress VARCHAR(500),
    CreatedAt TIMESTAMP NOT NULL DEFAULT NOW(),
    UpdatedAt TIMESTAMP,
    CONSTRAINT uq_invoices_registration UNIQUE (RegistrationId),
    CONSTRAINT uq_invoices_number UNIQUE (InvoiceNumber),
    CONSTRAINT fk_invoices_registration
        FOREIGN KEY (RegistrationId) REFERENCES CourseRegistrations(Id),
    CONSTRAINT fk_invoices_user
        FOREIGN KEY (UserId) REFERENCES Users(Id)
);

CREATE INDEX IF NOT EXISTS IX_Invoices_UserId ON Invoices (UserId);
CREATE INDEX IF NOT EXISTS IX_Invoices_InvoiceYear ON Invoices (InvoiceYear);
CREATE INDEX IF NOT EXISTS IX_Invoices_CreatedAt ON Invoices (CreatedAt);
