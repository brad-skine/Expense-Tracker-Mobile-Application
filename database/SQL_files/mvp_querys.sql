SELECT -- income and expense for each year
	EXTRACT (YEAR FROM transaction_Date) :: int AS year,
	COALESCE(SUM(amount) FILTER (WHERE amount > 0) ,0) AS income,
	COALESCE(SUM(amount) FILTER (WHERE amount < 0), 0) AS expense
FROM transactions
GROUP by year
ORDER by year

SELECT -- income and expense for each month
	EXTRACT (YEAR FROM transaction_Date) :: int AS year,
	EXTRACT (Month FROM transaction_Date) :: int AS month,
	COALESCE(SUM(amount) FILTER (WHERE amount > 0) ,0) AS income,
	COALESCE(SUM(amount) FILTER (WHERE amount < 0), 0) AS expense
FROM transactions
group by Year, Month
ORDER by Year, Month


SELECT -- results for transaction type
	transaction_type AS TransactionType,
	SUM(ABS(amount)) AS Total
FROM transactions
WHERE amount < 0
GROUP BY transaction_type
ORDER BY Total DESC;


