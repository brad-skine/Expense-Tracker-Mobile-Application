-- ============================================================
-- 08: Expense planning (budgets) + performance indexes
-- Safe to re-run
-- ============================================================

-- Monthly budget per category, one row per (user, category)
CREATE TABLE IF NOT EXISTS budgets (
    id SERIAL PRIMARY KEY,
    user_id UUID NOT NULL REFERENCES users(id),
    category_id INT NOT NULL REFERENCES categories(id),
    monthly_limit NUMERIC(12,2) NOT NULL CHECK (monthly_limit >= 0),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT uq_budgets_user_category UNIQUE (user_id, category_id)
);

CREATE INDEX IF NOT EXISTS idx_budgets_user ON budgets (user_id);

-- ============================================================
-- Performance indexes for the hottest queries
-- ============================================================

-- Every summary + list query filters user_id, most also filter/sort dates
CREATE INDEX IF NOT EXISTS idx_transactions_user_date
    ON transactions (user_id, transaction_date DESC);

-- Category summaries only look at expenses; partial index keeps it tiny
CREATE INDEX IF NOT EXISTS idx_transactions_user_category_expense
    ON transactions (user_id, category)
    WHERE amount < 0;

ANALYZE transactions;
ANALYZE merchant_rules;

