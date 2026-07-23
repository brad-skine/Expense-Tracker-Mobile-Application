-- ---------- Step D: set-based reclassify of remaining 'Other' rows ----------
-- Same logic as 04_backfill but repeatable; run after adding rules.

UPDATE transactions t
SET category = matched.cat_name
    FROM (
    SELECT DISTINCT ON (t2.id) t2.id AS txn_id, c.name AS cat_name
    FROM transactions t2
    JOIN merchant_rules mr ON mr.is_global = TRUE AND (
           (mr.match_type = 'contains'    AND t2.description ILIKE '%' || mr.pattern || '%')
        OR (mr.match_type = 'starts_with' AND t2.description ILIKE mr.pattern || '%')
        OR (mr.match_type = 'exact'       AND LOWER(t2.description) = LOWER(mr.pattern))
    )
    JOIN categories c ON c.id = mr.category_id
    WHERE t2.category = 'Other'
    ORDER BY t2.id, mr.priority ASC
) matched
WHERE t.id = matched.txn_id;

-- What's still unmatched (candidates for the next rule batch):
SELECT description, COUNT(*) AS occurrences, SUM(ABS(amount)) AS total
FROM transactions
WHERE category = 'Other' AND amount < 0
GROUP BY description
ORDER BY occurrences DESC
    LIMIT 50;
