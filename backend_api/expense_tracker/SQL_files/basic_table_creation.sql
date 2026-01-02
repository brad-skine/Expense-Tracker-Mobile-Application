CREATE TABLE transactions (
	id SERIAL PRIMARY KEY,
	transaction_date DATE NOT NULL,
	transaction_type TEXT NOT NULL,
	description TEXT,
    amount NUMERIC(10,2) NOT NULL,
    balance NUMERIC(10,2)
);
