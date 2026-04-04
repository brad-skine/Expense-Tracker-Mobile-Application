-- ============================================================
-- 02: Create merchant_rules table
-- Stores keyword patterns that map descriptions to categories
-- Supports global (all users) and per-user rules
-- ============================================================

CREATE TABLE merchant_rules (
    id SERIAL PRIMARY KEY,
    pattern VARCHAR(255) NOT NULL,
    category_id INT NOT NULL REFERENCES categories(id),
    match_type VARCHAR(20) NOT NULL DEFAULT 'contains',
        -- 'contains'     : description contains pattern (case insensitive)
        -- 'starts_with'  : description starts with pattern
        -- 'exact'        : description equals pattern exactly
    priority INT NOT NULL DEFAULT 100,
        -- lower number = higher priority (checked first)
        -- user rules default to 50, global rules default to 100
    is_global BOOLEAN NOT NULL DEFAULT TRUE,
    user_id UUID REFERENCES users(id),
        -- NULL for global rules, set for per-user overrides
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,

    CONSTRAINT chk_user_rule CHECK (
        (is_global = TRUE AND user_id IS NULL) OR
        (is_global = FALSE AND user_id IS NOT NULL)
    )
);

-- Index for fast lookups during classification
CREATE INDEX idx_merchant_rules_global ON merchant_rules (is_global, priority)
    WHERE is_global = TRUE;
CREATE INDEX idx_merchant_rules_user ON merchant_rules (user_id, priority)
    WHERE is_global = FALSE;


-- ============================================================
-- Seed global merchant rules
-- These are generic enough to work for any NZ bank user
-- ============================================================

-- Helper: get category id by name
-- Using a DO block so we can reference category ids by name

DO $$
DECLARE
    cat_groceries       INT;
    cat_rent            INT;
    cat_utilities       INT;
    cat_transport       INT;
    cat_eating_out      INT;
    cat_entertainment   INT;
    cat_sport           INT;
    cat_subscriptions   INT;
    cat_shopping        INT;
    cat_drinks          INT;
    cat_donations       INT;
    cat_income          INT;
    cat_fees            INT;
BEGIN
    SELECT id INTO cat_groceries     FROM categories WHERE name = 'Groceries';
    SELECT id INTO cat_rent          FROM categories WHERE name = 'Rent';
    SELECT id INTO cat_utilities     FROM categories WHERE name = 'Utilities';
    SELECT id INTO cat_transport     FROM categories WHERE name = 'Transport/Fuel';
    SELECT id INTO cat_eating_out    FROM categories WHERE name = 'Eating Out';
    SELECT id INTO cat_entertainment FROM categories WHERE name = 'Entertainment';
    SELECT id INTO cat_sport         FROM categories WHERE name = 'Sport/Fitness';
    SELECT id INTO cat_subscriptions FROM categories WHERE name = 'Subscriptions';
    SELECT id INTO cat_shopping      FROM categories WHERE name = 'Shopping/Retail';
    SELECT id INTO cat_drinks        FROM categories WHERE name = 'Drinks/Bars';
    SELECT id INTO cat_donations     FROM categories WHERE name = 'Donations/Church';
    SELECT id INTO cat_income        FROM categories WHERE name = 'Income';
    SELECT id INTO cat_fees          FROM categories WHERE name = 'Fees';

    -- ==================== GROCERIES ====================
    INSERT INTO merchant_rules (pattern, category_id, match_type, priority) VALUES
        ('PAK N SAVE',      cat_groceries, 'contains', 100),
        ('Pak n Save',      cat_groceries, 'contains', 100),
        ('NEW WORLD',       cat_groceries, 'contains', 100),
        ('New World',       cat_groceries, 'contains', 100),
        ('COUNTDOWN',       cat_groceries, 'contains', 100),
        ('FOUR SQUARE',     cat_groceries, 'contains', 100),
        ('WOOLWORTHS',      cat_groceries, 'contains', 100),
        ('FRESHCHOICE',     cat_groceries, 'contains', 100),
        ('SUPERVALUE',      cat_groceries, 'contains', 100);

    -- ==================== EATING OUT ====================
    INSERT INTO merchant_rules (pattern, category_id, match_type, priority) VALUES
        ('Dominos',         cat_eating_out, 'contains', 100),
        ('PIZZA HUT',       cat_eating_out, 'contains', 100),
        ('KFC',             cat_eating_out, 'contains', 100),
        ('McDonalds',       cat_eating_out, 'contains', 100),
        ('BURGER KING',     cat_eating_out, 'contains', 100),
        ('Subway',          cat_eating_out, 'contains', 100),
        ('Gong Cha',        cat_eating_out, 'contains', 100),
        ('Sals Pizza',      cat_eating_out, 'contains', 100),
        ('Base Wood Fired', cat_eating_out, 'contains', 100),
        ('Guzman y Gomez',  cat_eating_out, 'contains', 100),
        ('ARMADILLO',       cat_eating_out, 'contains', 100),
        ('Fern & Co',       cat_eating_out, 'contains', 100),
        ('Central Park Little', cat_eating_out, 'contains', 100),
        ('Caribe',          cat_eating_out, 'contains', 100),
        ('ZAKS LIMITED',    cat_eating_out, 'contains', 100);

    -- ==================== TRANSPORT / FUEL ====================
    INSERT INTO merchant_rules (pattern, category_id, match_type, priority) VALUES
        ('NPD',             cat_transport, 'contains', 100),
        ('Z ENERGY',        cat_transport, 'contains', 100),
        ('BP 2GO',          cat_transport, 'contains', 100),
        ('BP Connect',      cat_transport, 'contains', 100),
        ('CALTEX',          cat_transport, 'contains', 100),
        ('GULL',            cat_transport, 'contains', 100),
        ('MetroCard',       cat_transport, 'contains', 100),
        ('WILSON PARKING',  cat_transport, 'contains', 100);

    -- ==================== ENTERTAINMENT ====================
    INSERT INTO merchant_rules (pattern, category_id, match_type, priority) VALUES
        ('HOYTS',           cat_entertainment, 'contains', 100),
        ('Hoyts',           cat_entertainment, 'contains', 100),
        ('PlaystationNetwork', cat_entertainment, 'contains', 100),
        ('PLAYSTATION',     cat_entertainment, 'contains', 100),
        ('STEAM PURCHASE',  cat_entertainment, 'contains', 100),
        ('MYLOTTO',         cat_entertainment, 'contains', 100),
        ('CHRISTCHURCHCASINO', cat_entertainment, 'contains', 100),
        ('Skycity Malta',   cat_entertainment, 'contains', 100),
        ('Pay2Play',        cat_entertainment, 'contains', 100),
        ('NIGHT CREW GAMES',cat_entertainment, 'contains', 100);

    -- ==================== SPORT / FITNESS ====================
    INSERT INTO merchant_rules (pattern, category_id, match_type, priority) VALUES
        ('BADMINTON',       cat_sport, 'contains', 100),
        ('Badminton',       cat_sport, 'contains', 100),
        ('UPRISING',        cat_sport, 'contains', 100),
        ('Xplosiv Supplements', cat_sport, 'contains', 100),
        ('Players Sports',  cat_sport, 'contains', 100);

    -- ==================== SUBSCRIPTIONS ====================
    INSERT INTO merchant_rules (pattern, category_id, match_type, priority) VALUES
        ('Skinny Mobile',   cat_subscriptions, 'contains', 100),
        ('Canva',           cat_subscriptions, 'contains', 100),
        ('Google After Inc',cat_subscriptions, 'contains', 100),
        ('CocaColaEPP',     cat_subscriptions, 'contains', 100);

    -- ==================== SHOPPING / RETAIL ====================
    INSERT INTO merchant_rules (pattern, category_id, match_type, priority) VALUES
        ('Noel Leeming',    cat_shopping, 'contains', 100),
        ('The Warehouse',   cat_shopping, 'contains', 100),
        ('Samsung',         cat_shopping, 'contains', 100),
        ('Look Sharp',      cat_shopping, 'contains', 100),
        ('SUNSON GIFT',     cat_shopping, 'contains', 100),
        ('ALIEXPRESS',      cat_shopping, 'contains', 100),
        ('TB BUSH INN',     cat_shopping, 'contains', 100),
        ('POP STOP',        cat_shopping, 'contains', 100),
        ('PAYPAL',          cat_shopping, 'contains', 110);

    -- ==================== DRINKS / BARS ====================
    INSERT INTO merchant_rules (pattern, category_id, match_type, priority) VALUES
        ('ROSE AND THISTLE',cat_drinks, 'contains', 100),
        ('CHERRY BAR',      cat_drinks, 'contains', 100),
        ('SQ *MIAMI MARKETTA', cat_drinks, 'contains', 100);

    -- ==================== FEES ====================
    -- (these also get caught by transaction_type logic in the 
    --  classifier, but having rules here is a safety net)
    INSERT INTO merchant_rules (pattern, category_id, match_type, priority) VALUES
        ('Foreign Currency Transaction', cat_fees, 'contains', 100);

END $$;
