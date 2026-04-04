-- ============================================================
-- 01: Create categories table
-- Stores the list of available spending categories
-- Run this FIRST before other migration scripts
-- ============================================================

CREATE TABLE categories (
    id SERIAL PRIMARY KEY,
    name VARCHAR(100) UNIQUE NOT NULL,
    display_order INT NOT NULL DEFAULT 0,
    is_active BOOLEAN NOT NULL DEFAULT TRUE
);

-- Seed default categories
INSERT INTO categories (name, display_order) VALUES
    ('Groceries',       1),
    ('Rent',            2),
    ('Utilities',       3),
    ('Transport/Fuel',  4),
    ('Eating Out',      5),
    ('Entertainment',   6),
    ('Sport/Fitness',   7),
    ('Subscriptions',   8),
    ('Shopping/Retail', 9),
    ('Drinks/Bars',     10),
    ('Donations/Church',11),
    ('Transfers',       12),
    ('Income',          13),
    ('Fees',            14),
    ('Other',           15);
