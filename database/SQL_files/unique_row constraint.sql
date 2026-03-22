-- This is for when a user imports overlapping date range data and 
-- will ignore duplicate data based of entire row

-- ALTER TABLE transactions
-- DROP CONSTRAINT non_unique_row
ALTER TABLE transactions  
ADD CONSTRAINT non_unique_row 
UNIQUE (
	user_id,
    transaction_date,
    amount,
    balance);