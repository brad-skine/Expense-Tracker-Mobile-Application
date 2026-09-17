-- ============================================================
-- 11: Bank connections (Akahu)
-- Safe to re-run
-- ============================================================

-- One row per linked bank account per user
CREATE TABLE IF NOT EXISTS bank_connections (
    id SERIAL PRIMARY KEY,
    user_id UUID NOT NULL REFERENCES users(id),
    provider TEXT NOT NULL DEFAULT 'akahu',
    external_account_id TEXT NOT NULL,
    name TEXT,
    formatted_account TEXT,
    bank_name TEXT,
    account_type TEXT,
    current_balance NUMERIC(14,2),
    available_balance NUMERIC(14,2),
    is_active BOOLEAN DEFAULT TRUE,
    last_synced_at TIMESTAMPTZ,
    created_at TIMESTAMPTZ DEFAULT NOW(),
    CONSTRAINT uq_bank_connections_user_provider_account
        UNIQUE (user_id, provider, external_account_id)
);

-- Link imported transactions back to Akahu (external_id = Akahu _id)
ALTER TABLE transactions ADD COLUMN IF NOT EXISTS external_id TEXT;
ALTER TABLE transactions ADD COLUMN IF NOT EXISTS bank_connection_id INT REFERENCES bank_connections(id);

-- Dedup key for synced rows; CSV imports leave external_id NULL
CREATE UNIQUE INDEX IF NOT EXISTS uq_transactions_user_external_id
    ON transactions (user_id, external_id)
    WHERE external_id IS NOT NULL;

-- Per-connection listing / sync window queries
CREATE INDEX IF NOT EXISTS idx_transactions_user_connection_date
    ON transactions (user_id, bank_connection_id, transaction_date);
