-- ============================================
-- Migration: Add approval columns to Users table
-- Purpose: Support admin approval workflow for student registration
-- Created: 2026-03-14
-- ============================================

DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'users' AND column_name = 'approvalstatus') THEN
        ALTER TABLE Users ADD COLUMN ApprovalStatus VARCHAR(20) DEFAULT 'Pending';
    END IF;

    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'users' AND column_name = 'approvedby') THEN
        ALTER TABLE Users ADD COLUMN ApprovedBy UUID;
    END IF;

    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'users' AND column_name = 'approvedat') THEN
        ALTER TABLE Users ADD COLUMN ApprovedAt TIMESTAMP;
    END IF;

    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name = 'users' AND column_name = 'denialreason') THEN
        ALTER TABLE Users ADD COLUMN DenialReason VARCHAR(500);
    END IF;
END $$;
