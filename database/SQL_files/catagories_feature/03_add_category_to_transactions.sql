-- ============================================================
-- 03: Add category column to transactions table
-- Defaults to 'Other' so existing rows get a value
-- ============================================================

ALTER TABLE transactions
ADD COLUMN category VARCHAR(100) NOT NULL DEFAULT 'Other';
