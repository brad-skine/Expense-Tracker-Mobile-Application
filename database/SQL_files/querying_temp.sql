-- SELECT * FROM transactions;
-- SELECT Sum(amount) as tot,  transaction_type as tt 
-- FROM transactions
-- WHERE amount < 0
-- GROUP BY tt
-- ORDER BY tot;






SELECT 
	EXTRACT (YEAR FROM transaction_Date) :: int AS year,
	EXTRACT (Month FROM transaction_Date) :: int AS month,
	COALESCE(SUM(amount) FILTER (WHERE amount > 0) ,0) AS income,
	COALESCE(SUM(amount) FILTER (WHERE amount < 0), 0) AS expense
FROM transactions
group by Year, Month
ORDER by Year, Month


-- DELETE FROM transactions 
-- WHERE  ID IN( 
-- 	SELECT ID 
-- 	FROM transactions
-- 	ORDER BY ID
-- 	LIMIT 5
-- );