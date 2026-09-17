-- ============================================================
-- 12: Scope CSV dedup key to CSV rows
-- Akahu-synced rows dedupe on (user_id, external_id) (see 11);
-- CSV rows (external_id IS NULL) keep the date/amount/balance key.
-- Safe to re-run
-- ============================================================

ALTER TABLE transactions DROP CONSTRAINT IF EXISTS uq_transaction_row;

CREATE UNIQUE INDEX IF NOT EXISTS uq_transactions_csv_row
    ON transactions (user_id, transaction_date, amount, balance) NULLS NOT DISTINCT
    WHERE external_id IS NULL;
