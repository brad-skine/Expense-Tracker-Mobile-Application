-- ============================================================
-- Adding new global merchant rules
-- Run this ANYTIME after 01 and 02 have been applied
--
-- Pattern matching is CASE-INSENSITIVE (uses ILIKE in backfill
-- and case-insensitive compare in C# classifier)
-- so 'mcdonalds' matches 'McDonalds', 'MCDONALDS', etc.
--
-- After adding new rules, re-run the backfill on uncategorised:
--   See the "Re-classify" section at the bottom of this file
-- ============================================================

DO $$
DECLARE
    cat_groceries       INT;
    cat_rent            INT;
    cat_eating_out      INT;
    cat_transport       INT;
    cat_entertainment   INT;
    cat_sport           INT;
    cat_subscriptions   INT;
    cat_shopping        INT;
    cat_drinks          INT;
    cat_donations       INT;
BEGIN
    SELECT id INTO cat_groceries     FROM categories WHERE name = 'Groceries';
    SELECT id INTO cat_rent          FROM categories WHERE name = 'Rent';
    SELECT id INTO cat_eating_out    FROM categories WHERE name = 'Eating Out';
    SELECT id INTO cat_transport     FROM categories WHERE name = 'Transport/Fuel';
    SELECT id INTO cat_entertainment FROM categories WHERE name = 'Entertainment';
    SELECT id INTO cat_sport         FROM categories WHERE name = 'Sport/Fitness';
    SELECT id INTO cat_subscriptions FROM categories WHERE name = 'Subscriptions';
    SELECT id INTO cat_shopping      FROM categories WHERE name = 'Shopping/Retail';
    SELECT id INTO cat_drinks        FROM categories WHERE name = 'Drinks/Bars';
    SELECT id INTO cat_donations     FROM categories WHERE name = 'Donations/Church';

    -- =======================================================
    -- EATING OUT  - fast food, cafes, restaurants
    -- =======================================================
    INSERT INTO merchant_rules (pattern, category_id, match_type, priority) VALUES
        -- Fast food / chains
        ('Wendys',              cat_eating_out, 'contains', 100),
        ('Carls Jr',            cat_eating_out, 'contains', 100),
        ('Taco Bell',           cat_eating_out, 'contains', 100),
        ('Nandos',              cat_eating_out, 'contains', 100),
        ('Zambrero',            cat_eating_out, 'contains', 100),
        ('Mexicali Fresh',      cat_eating_out, 'contains', 100),
        ('Mad Mex',             cat_eating_out, 'contains', 100),
        ('Hells Pizza',         cat_eating_out, 'contains', 100),
        ('Sal''s Pizza',        cat_eating_out, 'contains', 100),
        ('Pita Pit',            cat_eating_out, 'contains', 100),
        ('Tank Juice',          cat_eating_out, 'contains', 100),
        ('Boost Juice',         cat_eating_out, 'contains', 100),
        ('Chatime',             cat_eating_out, 'contains', 100),
        ('Starbucks',           cat_eating_out, 'contains', 100),
        ('Robert Harris',       cat_eating_out, 'contains', 100),
        ('Columbus Coffee',     cat_eating_out, 'contains', 100),
        ('Muffin Break',        cat_eating_out, 'contains', 100),
        ('Burger Fuel',         cat_eating_out, 'contains', 100),
        ('BurgerFuel',          cat_eating_out, 'contains', 100),
        ('Habitual Fix',        cat_eating_out, 'contains', 100),
        ('Uber Eats',           cat_eating_out, 'contains', 100),
        ('UBER *EATS',          cat_eating_out, 'contains', 100),
        ('DoorDash',            cat_eating_out, 'contains', 100),
        ('Menulog',             cat_eating_out, 'contains', 100)

    -- Avoid ON CONFLICT errors if rule already exists
    ON CONFLICT DO NOTHING;



    -- =======================================================
    -- GROCERIES  - supermarkets & food stores
    -- =======================================================
    INSERT INTO merchant_rules (pattern, category_id, match_type, priority) VALUES
        ('rent',           cat_rent, 'contains', 100)
    ON CONFLICT DO NOTHING;
    -- =======================================================
    -- GROCERIES  - supermarkets & food stores
    -- =======================================================
    INSERT INTO merchant_rules (pattern, category_id, match_type, priority) VALUES
        ('Countdown',           cat_groceries, 'contains', 100),
        ('FreshChoice',         cat_groceries, 'contains', 100),
        ('SuperValue',          cat_groceries, 'contains', 100),
        ('Woolworths',          cat_groceries, 'contains', 100),
        ('Bin Inn',             cat_groceries, 'contains', 100),
        ('Farro Fresh',         cat_groceries, 'contains', 100),
        ('Commonsense Organics',cat_groceries, 'contains', 100),
        ('Asian Supermarket',   cat_groceries, 'contains', 100),
        ('Tai Ping',            cat_groceries, 'contains', 100),
        ('Kosco',               cat_groceries, 'contains', 100)
    ON CONFLICT DO NOTHING;


    -- =======================================================
    -- TRANSPORT / FUEL
    -- =======================================================
    INSERT INTO merchant_rules (pattern, category_id, match_type, priority) VALUES
        ('Mobil',               cat_transport, 'contains', 100),
        ('Allied Petroleum',    cat_transport, 'contains', 100),
        ('Waitomo',             cat_transport, 'contains', 100),
        ('UBER *TRIP',          cat_transport, 'contains', 100),
        ('Uber Trip',           cat_transport, 'contains', 100),
        ('Lime Scooter',        cat_transport, 'contains', 100),
        ('Beam Scooter',        cat_transport, 'contains', 100),
        ('AT HOP',              cat_transport, 'contains', 100),
        ('Snapper',             cat_transport, 'contains', 100)
    ON CONFLICT DO NOTHING;


    -- =======================================================
    -- ENTERTAINMENT
    -- =======================================================
    INSERT INTO merchant_rules (pattern, category_id, match_type, priority) VALUES
        ('Netflix',             cat_entertainment, 'contains', 100),
        ('Disney Plus',         cat_entertainment, 'contains', 100),
        ('NEON',                cat_entertainment, 'contains', 100),
        ('Spark Sport',         cat_entertainment, 'contains', 100),
        ('Event Cinemas',       cat_entertainment, 'contains', 100),
        ('Reading Cinemas',     cat_entertainment, 'contains', 100),
        ('Timezone',            cat_entertainment, 'contains', 100),
        ('Xbox',                cat_entertainment, 'contains', 100),
        ('Nintendo',            cat_entertainment, 'contains', 100),
        ('EPIC GAMES',          cat_entertainment, 'contains', 100),
        ('Spotify',             cat_entertainment, 'contains', 100),
        ('Apple Music',         cat_entertainment, 'contains', 100),
        ('Ticketmaster',        cat_entertainment, 'contains', 100),
        ('Ticketek',            cat_entertainment, 'contains', 100),
        ('iTicket',             cat_entertainment, 'contains', 100)
    ON CONFLICT DO NOTHING;


    -- =======================================================
    -- SUBSCRIPTIONS
    -- =======================================================
    INSERT INTO merchant_rules (pattern, category_id, match_type, priority) VALUES
        ('Spark NZ',            cat_subscriptions, 'contains', 100),
        ('Vodafone',            cat_subscriptions, 'contains', 100),
        ('2degrees',            cat_subscriptions, 'contains', 100),
        ('One NZ',              cat_subscriptions, 'contains', 100),
        ('Apple.com/bill',      cat_subscriptions, 'contains', 100),
        ('APPLE.COM',           cat_subscriptions, 'contains', 100),
        ('Google Storage',      cat_subscriptions, 'contains', 100),
        ('Amazon Prime',        cat_subscriptions, 'contains', 100),
        ('ChatGPT',             cat_subscriptions, 'contains', 100),
        ('OPENAI',              cat_subscriptions, 'contains', 100),
        ('GitHub',              cat_subscriptions, 'contains', 100),
        ('Adobe',               cat_subscriptions, 'contains', 100),
        ('Microsoft 365',       cat_subscriptions, 'contains', 100),
        ('iCloud',              cat_subscriptions, 'contains', 100)
    ON CONFLICT DO NOTHING;


    -- =======================================================
    -- SHOPPING / RETAIL
    -- =======================================================
    INSERT INTO merchant_rules (pattern, category_id, match_type, priority) VALUES
        ('Kmart',               cat_shopping, 'contains', 100),
        ('Briscoes',            cat_shopping, 'contains', 100),
        ('Rebel Sport',         cat_shopping, 'contains', 100),
        ('Cotton On',           cat_shopping, 'contains', 100),
        ('H&M',                 cat_shopping, 'contains', 100),
        ('Hallensteins',        cat_shopping, 'contains', 100),
        ('JB Hi-Fi',            cat_shopping, 'contains', 100),
        ('PB Tech',             cat_shopping, 'contains', 100),
        ('Mitre 10',            cat_shopping, 'contains', 100),
        ('Bunnings',            cat_shopping, 'contains', 100),
        ('Farmers',             cat_shopping, 'contains', 100),
        ('Amazon',              cat_shopping, 'contains', 110),
        ('SHEIN',               cat_shopping, 'contains', 100),
        ('TEMU',                cat_shopping, 'contains', 100),
        ('AliExpress',          cat_shopping, 'contains', 100)
    ON CONFLICT DO NOTHING;


    -- =======================================================
    -- SPORT / FITNESS
    -- =======================================================
    INSERT INTO merchant_rules (pattern, category_id, match_type, priority) VALUES
        ('Les Mills',           cat_sport, 'contains', 100),
        ('Snap Fitness',        cat_sport, 'contains', 100),
        ('Anytime Fitness',     cat_sport, 'contains', 100),
        ('City Fitness',        cat_sport, 'contains', 100),
        ('Jetts Fitness',       cat_sport, 'contains', 100),
        ('Club Physical',       cat_sport, 'contains', 100),
        ('GYM',                 cat_sport, 'contains', 120)
    ON CONFLICT DO NOTHING;


    -- =======================================================
    -- DRINKS / BARS
    -- =======================================================
    INSERT INTO merchant_rules (pattern, category_id, match_type, priority) VALUES
        ('Liquorland',          cat_drinks, 'contains', 100),
        ('Super Liquor',        cat_drinks, 'contains', 100),
        ('Henry''s',            cat_drinks, 'contains', 100),
        ('Bottle-O',            cat_drinks, 'contains', 100),
        ('Tavern',              cat_drinks, 'contains', 100),
        ('Countdown Liquor',    cat_drinks, 'contains', 100)
    ON CONFLICT DO NOTHING;

END $$;


-- ============================================================
-- After adding rules: re-classify anything still in 'Other'
-- ============================================================

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


-- Check what's still uncategorised
SELECT DISTINCT description, COUNT(*) AS occurrences
FROM transactions
WHERE category = 'Other'
  AND amount < 0
GROUP BY description
ORDER BY occurrences DESC
LIMIT 30;

-- See what's still in 'Other' (candidates for new rules)
SELECT DISTINCT description, COUNT(*) as occurrences
FROM transactions
WHERE category = 'Other'
  AND amount < 0
GROUP BY description
ORDER BY occurrences DESC
LIMIT 50;
