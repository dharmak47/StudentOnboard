-- ============================================
-- Migration: Add approval columns to Users table
-- Purpose: Support admin approval workflow for student registration
-- Created: 2026-03-14
-- ============================================

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'ApprovalStatus')
    ALTER TABLE Users ADD ApprovalStatus NVARCHAR(20) DEFAULT 'Pending';

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'ApprovedBy')
    ALTER TABLE Users ADD ApprovedBy UNIQUEIDENTIFIER NULL;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'ApprovedAt')
    ALTER TABLE Users ADD ApprovedAt DATETIME2 NULL;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'DenialReason')
    ALTER TABLE Users ADD DenialReason NVARCHAR(500) NULL;
