-- ============================================
-- Table: InvoiceItems
-- Purpose: Line items for an invoice (Description, Qty, Unit Price, Tax, Discount, Total)
-- ============================================

CREATE TABLE IF NOT EXISTS InvoiceItems (
    Id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    InvoiceId UUID NOT NULL,
    Description VARCHAR(500) NOT NULL,
    Quantity DECIMAL(10,2) NOT NULL DEFAULT 1,
    UnitPrice DECIMAL(12,2) NOT NULL DEFAULT 0,
    TaxPercent DECIMAL(5,2) NOT NULL DEFAULT 0,
    DiscountAmount DECIMAL(12,2) NOT NULL DEFAULT 0,
    LineTotal DECIMAL(12,2) NOT NULL DEFAULT 0,
    SortOrder INT NOT NULL DEFAULT 0,
    CONSTRAINT fk_invoiceitems_invoice
        FOREIGN KEY (InvoiceId) REFERENCES Invoices(Id) ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS IX_InvoiceItems_InvoiceId ON InvoiceItems (InvoiceId);
