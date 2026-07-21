-- ============================================
-- Migration: Add approval columns to Users table
-- Purpose: Support admin approval workflow for student registration
-- Created: 2026-03-14
-- ============================================

ALTER TABLE Users ADD COLUMN IF NOT EXISTS ApprovalStatus VARCHAR(20) DEFAULT 'Pending';
ALTER TABLE Users ADD COLUMN IF NOT EXISTS ApprovedBy UUID;
ALTER TABLE Users ADD COLUMN IF NOT EXISTS ApprovedAt TIMESTAMP;
ALTER TABLE Users ADD COLUMN IF NOT EXISTS DenialReason VARCHAR(500);
