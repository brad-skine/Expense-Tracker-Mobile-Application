-- ============================================================
-- 05: Category queries (for use in backend API)
-- These are reference queries - copy into C# services
-- ============================================================


-- Category spending summary (replacement for type summary pie chart)
-- Use this in TransactionQueryService.GetCategorySummaryAsync
SELECT
    category AS Category,
    SUM(ABS(amount)) AS Total
FROM transactions
WHERE user_id = @UserId
  AND amount < 0
GROUP BY category
ORDER BY Total DESC;


-- Category spending summary filtered by month
SELECT
    category AS Category,
    SUM(ABS(amount)) AS Total
FROM transactions
WHERE user_id = @UserId
  AND amount < 0
  AND EXTRACT(YEAR FROM transaction_date) = @Year
  AND EXTRACT(MONTH FROM transaction_date) = @Month
GROUP BY category
ORDER BY Total DESC;


-- Monthly breakdown per category (for stacked bar chart later)
SELECT
    EXTRACT(YEAR FROM transaction_date)::int AS Year,
    EXTRACT(MONTH FROM transaction_date)::int AS Month,
    category AS Category,
    SUM(ABS(amount)) AS Total
FROM transactions
WHERE user_id = @UserId
  AND amount < 0
GROUP BY Year, Month, category
ORDER BY Year, Month, Total DESC;


-- Get all merchant rules (global + user specific)
-- Used by CategoryClassifierService to load rules into memory
SELECT
    mr.id,
    mr.pattern,
    c.name AS category_name,
    mr.match_type,
    mr.priority,
    mr.is_global,
    mr.user_id
FROM merchant_rules mr
JOIN categories c ON c.id = mr.category_id
WHERE mr.is_global = TRUE
   OR mr.user_id = @UserId
ORDER BY mr.priority ASC;


-- Get all active categories (for frontend dropdown)
SELECT id, name, display_order
FROM categories
WHERE is_active = TRUE
ORDER BY display_order;
