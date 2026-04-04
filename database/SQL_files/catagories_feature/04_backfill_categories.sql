-- ============================================================
-- 04: Backfill existing transactions with categories
-- Run AFTER 01, 02, and 03 have been applied
-- 
-- This uses the merchant_rules table to classify all existing
-- transactions that currently have category = 'Other'
--
-- Priority logic:
--   1. transaction_type = 'Fee'       → 'Fees'
--   2. transaction_type = 'Deposit'   → 'Income'  (positive amounts only)
--   3. Merchant rules match           → matched category
--   4. No match                       → stays 'Other'
-- ============================================================


-- Step 1: Classify Fee transactions
UPDATE transactions
SET category = 'Fees'
WHERE transaction_type = 'Fee'
  AND category = 'Other';


-- Step 2: Classify Deposit / Income transactions
UPDATE transactions
SET category = 'Income'
WHERE transaction_type = 'Deposit'
  AND amount > 0
  AND category = 'Other';


-- Step 3: Classify using global merchant rules (contains match)
-- Uses DISTINCT ON to pick the highest priority (lowest number) match
UPDATE transactions t
SET category = matched.cat_name
FROM (
    SELECT DISTINCT ON (t2.id)
        t2.id AS txn_id,
        c.name AS cat_name
    FROM transactions t2
    JOIN merchant_rules mr ON (
        mr.is_global = TRUE
        AND mr.match_type = 'contains'
        AND t2.description ILIKE '%' || mr.pattern || '%'
    )
    JOIN categories c ON c.id = mr.category_id
    WHERE t2.category = 'Other'
    ORDER BY t2.id, mr.priority ASC
) matched
WHERE t.id = matched.txn_id;


-- Step 4: Classify using global merchant rules (starts_with match)
UPDATE transactions t
SET category = matched.cat_name
FROM (
    SELECT DISTINCT ON (t2.id)
        t2.id AS txn_id,
        c.name AS cat_name
    FROM transactions t2
    JOIN merchant_rules mr ON (
        mr.is_global = TRUE
        AND mr.match_type = 'starts_with'
        AND t2.description ILIKE mr.pattern || '%'
    )
    JOIN categories c ON c.id = mr.category_id
    WHERE t2.category = 'Other'
    ORDER BY t2.id, mr.priority ASC
) matched
WHERE t.id = matched.txn_id;


-- Step 5: Classify using global merchant rules (exact match)
UPDATE transactions t
SET category = matched.cat_name
FROM (
    SELECT DISTINCT ON (t2.id)
        t2.id AS txn_id,
        c.name AS cat_name
    FROM transactions t2
    JOIN merchant_rules mr ON (
        mr.is_global = TRUE
        AND mr.match_type = 'exact'
        AND LOWER(t2.description) = LOWER(mr.pattern)
    )
    JOIN categories c ON c.id = mr.category_id
    WHERE t2.category = 'Other'
    ORDER BY t2.id, mr.priority ASC
) matched
WHERE t.id = matched.txn_id;


-- ============================================================
-- Verify: check how well the classification went
-- ============================================================

-- Count by category (see what got classified)
SELECT 
    category, 
    COUNT(*) AS transaction_count,
    SUM(ABS(amount)) AS total_amount
FROM transactions
GROUP BY category
ORDER BY   transaction_count DESC;

-- See what's still in 'Other' (candidates for new rules)
SELECT DISTINCT description, COUNT(*) as occurrences
FROM transactions
WHERE category = 'Other'
  AND amount < 0
GROUP BY description
ORDER BY occurrences DESC
LIMIT 50;
