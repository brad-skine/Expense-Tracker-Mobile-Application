-- ============================================================
-- 10: Import dedup groundwork
-- Safe to re-run
-- ============================================================

-- Balance should never be NULL — it breaks uniqueness semantics
UPDATE transactions SET balance = 0 WHERE balance IS NULL;
ALTER TABLE transactions ALTER COLUMN balance SET DEFAULT 0;
ALTER TABLE transactions ALTER COLUMN balance SET NOT NULL;

-- The old constraint is too weak on its own but harmless as a backstop.
-- Keep it, but make NULLs behave (Postgres 15+, Supabase is fine).
ALTER TABLE transactions DROP CONSTRAINT IF EXISTS non_unique_row;
ALTER TABLE transactions
    ADD CONSTRAINT uq_transaction_row
        UNIQUE NULLS NOT DISTINCT (user_id, transaction_date, amount, balance);

-- Index the dedup lookup key
CREATE INDEX IF NOT EXISTS idx_transactions_dedup
    ON transactions (user_id, transaction_date, description, amount);
