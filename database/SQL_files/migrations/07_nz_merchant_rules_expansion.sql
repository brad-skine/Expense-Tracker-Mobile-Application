-- ============================================================
-- 07: NZ merchant rules expansion
-- Safe to re-run (unique index + ON CONFLICT DO NOTHING)
--
-- Step A: remove case-duplicate global rules (ILIKE makes
--         'PAK N SAVE' vs 'Pak n Save' redundant)
-- Step B: unique index so ON CONFLICT works on future runs
-- Step C: large NZ merchant rule set
-- Step D: set-based reclassify of remaining 'Other' rows
-- ============================================================

-- ---------- Step A: dedupe (keep lowest id per lower(pattern)) ----------
DELETE FROM merchant_rules a
USING merchant_rules b
WHERE a.is_global = TRUE
  AND b.is_global = TRUE
  AND LOWER(a.pattern) = LOWER(b.pattern)
  AND a.id > b.id;

-- ---------- Step B: uniqueness for global patterns ----------
CREATE UNIQUE INDEX IF NOT EXISTS uq_merchant_rules_global_pattern
    ON merchant_rules (LOWER(pattern))
    WHERE is_global = TRUE;

-- ---------- Step C: NZ merchant rules ----------
DO $$
DECLARE
    cat_groceries       INT;
    cat_utilities       INT;
    cat_transport       INT;
    cat_eating_out      INT;
    cat_entertainment   INT;
    cat_sport           INT;
    cat_subscriptions   INT;
    cat_shopping        INT;
    cat_drinks          INT;
    cat_income          INT;
    cat_fees            INT;
BEGIN
    SELECT id INTO cat_groceries     FROM categories WHERE name = 'Groceries';
    SELECT id INTO cat_utilities     FROM categories WHERE name = 'Utilities';
    SELECT id INTO cat_transport     FROM categories WHERE name = 'Transport/Fuel';
    SELECT id INTO cat_eating_out    FROM categories WHERE name = 'Eating Out';
    SELECT id INTO cat_entertainment FROM categories WHERE name = 'Entertainment';
    SELECT id INTO cat_sport         FROM categories WHERE name = 'Sport/Fitness';
    SELECT id INTO cat_subscriptions FROM categories WHERE name = 'Subscriptions';
    SELECT id INTO cat_shopping      FROM categories WHERE name = 'Shopping/Retail';
    SELECT id INTO cat_drinks        FROM categories WHERE name = 'Drinks/Bars';
    SELECT id INTO cat_income        FROM categories WHERE name = 'Income';
    SELECT id INTO cat_fees          FROM categories WHERE name = 'Fees';

    -- ==================== GROCERIES ====================
    -- Woolworths finished rebranding Countdown -> Woolworths (2023-2025),
    -- statements can contain either name depending on transaction age.
    INSERT INTO merchant_rules (pattern, category_id, match_type, priority) VALUES
        ('WOOLWORTHS',      cat_groceries, 'contains', 100),
        ('COUNTDOWN',       cat_groceries, 'contains', 100),
        ('NEW WORLD',       cat_groceries, 'contains', 100),
        ('PAKNSAVE',        cat_groceries, 'contains', 100),
        ('PAK''NSAVE',      cat_groceries, 'contains', 100),
        ('FOUR SQUARE',     cat_groceries, 'contains', 100),
        ('FRESH CHOICE',    cat_groceries, 'contains', 100),
        ('FRESHCHOICE',     cat_groceries, 'contains', 100),
        ('SUPERVALUE',      cat_groceries, 'contains', 100),
        ('SUPER VALUE',     cat_groceries, 'contains', 100),
        ('NIGHT N DAY',     cat_groceries, 'contains', 100),
        ('NIGHT ''N DAY',   cat_groceries, 'contains', 100),
        ('ON THE SPOT',     cat_groceries, 'contains', 105),
        ('MAD BUTCHER',     cat_groceries, 'contains', 100),
        ('BIN INN',         cat_groceries, 'contains', 100),
        ('VEGGIE BOYS',     cat_groceries, 'contains', 100),
        ('MOORE WILSON',    cat_groceries, 'contains', 100),
        ('FARRO FRESH',     cat_groceries, 'contains', 100),
        ('COSTCO',          cat_groceries, 'contains', 100),
        ('MY FOOD BAG',     cat_groceries, 'contains', 100),
        ('HELLOFRESH',      cat_groceries, 'contains', 100),
        ('HELLO FRESH',     cat_groceries, 'contains', 100),
        ('BUTCHERY',        cat_groceries, 'contains', 115),
        ('GREENGROCER',     cat_groceries, 'contains', 115)
    ON CONFLICT DO NOTHING;

    -- ==================== TRANSPORT / FUEL ====================
    INSERT INTO merchant_rules (pattern, category_id, match_type, priority) VALUES
        ('Z ENERGY',        cat_transport, 'contains', 100),
        ('Z PETROL',        cat_transport, 'contains', 100),
        ('CALTEX',          cat_transport, 'contains', 100),
        ('BP CONNECT',      cat_transport, 'contains', 100),
        ('BP 2GO',          cat_transport, 'contains', 100),
        ('BP ',             cat_transport, 'starts_with', 105),
        ('MOBIL',           cat_transport, 'contains', 100),
        ('GULL ',           cat_transport, 'contains', 100),
        ('WAITOMO FUEL',    cat_transport, 'contains', 100),
        ('WAITOMO GROUP',   cat_transport, 'contains', 100),
        ('NPD ',            cat_transport, 'starts_with', 100),
        ('ALLIED PETROLEUM',cat_transport, 'contains', 100),
        ('RD PETROLEUM',cat_transport, 'starts_with', 105),
        ('RD',cat_transport, 'starts_with', 100),
        ('CHALLENGE FUEL',  cat_transport, 'contains', 100),
        ('GAS ALLEY',       cat_transport, 'contains', 100),
        ('GASOLINE ALLEY',  cat_transport, 'contains', 100),
        ('G.A.S',           cat_transport, 'contains', 105),
        -- public transport / rideshare
        ('AT HOP',          cat_transport, 'contains', 100),
        ('AT METRO',        cat_transport, 'contains', 100),
        ('METLINK',         cat_transport, 'contains', 100),
        ('BEE CARD',        cat_transport, 'contains', 100),
        ('SNAPPER SERVICES',cat_transport, 'contains', 100),
        ('INTERCITY',       cat_transport, 'contains', 100),
        ('UBER TRIP',       cat_transport, 'contains', 100),
        ('UBER* TRIP',      cat_transport, 'contains', 100),
        ('UBER',            cat_transport, 'contains', 115),  -- Uber Eats rule below outranks this
        ('OLA ',            cat_transport, 'starts_with', 105),
        ('ZOOMY',           cat_transport, 'contains', 100),
        ('LIME RIDE',       cat_transport, 'contains', 100),
        ('LIME*RIDE',       cat_transport, 'contains', 100),
        ('BEAM MOBILITY',   cat_transport, 'contains', 100),
        ('FLAMINGO SCOOTER',cat_transport, 'contains', 100),
        ('MEVO',            cat_transport, 'contains', 100),
        ('CITYHOP',         cat_transport, 'contains', 100),
        -- vehicle running costs
        ('VTNZ',            cat_transport, 'contains', 100),
        ('VINZ',            cat_transport, 'contains', 100),
        ('WAKA KOTAHI',     cat_transport, 'contains', 100),
        ('NZTA',            cat_transport, 'contains', 100),
        ('NZ TRANSPORT AGENCY', cat_transport, 'contains', 100),
        ('BEAUREPAIRES',    cat_transport, 'contains', 100),
        ('BRIDGESTONE',     cat_transport, 'contains', 100),
        ('TONY''S TYRE',    cat_transport, 'contains', 100),
        ('PIT STOP',        cat_transport, 'contains', 100),
        ('MIDAS',           cat_transport, 'contains', 100),
        ('WILSON PARKING',  cat_transport, 'contains', 100),
        ('CARE PARK',       cat_transport, 'contains', 100),
        ('SECURE PARKING',  cat_transport, 'contains', 100),
        ('PARKING',         cat_transport, 'contains', 120),
        -- travel
        ('AIR NEW ZEALAND', cat_transport, 'contains', 100),
        ('AIR NZ',          cat_transport, 'contains', 100),
        ('JETSTAR',         cat_transport, 'contains', 100),
        ('INTERISLANDER',   cat_transport, 'contains', 100),
        ('BLUEBRIDGE',      cat_transport, 'contains', 100)
    ON CONFLICT DO NOTHING;

    -- ==================== UTILITIES (power, telco, internet) ====================
    INSERT INTO merchant_rules (pattern, category_id, match_type, priority) VALUES
        ('CONTACT ENERGY',  cat_utilities, 'contains', 100),
        ('GENESIS ENERGY',  cat_utilities, 'contains', 100),
        ('MERCURY ENERGY',  cat_utilities, 'contains', 100),
        ('MERCURY NZ',      cat_utilities, 'contains', 100),
        ('MERIDIAN',        cat_utilities, 'contains', 100),
        ('ELECTRIC KIWI',   cat_utilities, 'contains', 100),
        ('POWERSHOP',       cat_utilities, 'contains', 100),
        ('FRANK ENERGY',    cat_utilities, 'contains', 100),
        ('NOVA ENERGY',     cat_utilities, 'contains', 100),
        ('PULSE ENERGY',    cat_utilities, 'contains', 100),
        ('FLICK ELECTRIC',  cat_utilities, 'contains', 100),
        ('OCTOPUS ENERGY',  cat_utilities, 'contains', 100),
        ('GLOBUG',          cat_utilities, 'contains', 100),
        ('AURORA ENERGY',   cat_utilities, 'contains', 100),
        ('WATERCARE',       cat_utilities, 'contains', 100),
        ('SPARK NZ',        cat_utilities, 'contains', 100),
        ('SPARK NEW ZEALAND', cat_utilities, 'contains', 100),
        ('SPARK PREPAY',    cat_utilities, 'contains', 100),
        ('ONE NZ',          cat_utilities, 'contains', 100),
        ('ONE.NZ',          cat_utilities, 'contains', 100),
        ('VODAFONE',        cat_utilities, 'contains', 100),
        ('2DEGREES',        cat_utilities, 'contains', 100),
        ('2 DEGREES',       cat_utilities, 'contains', 100),
        ('SKINNY MOBILE',   cat_utilities, 'contains', 100),
        ('SKINNY BROADBAND',cat_utilities, 'contains', 100),
        ('SLINGSHOT',       cat_utilities, 'contains', 100),
        ('ORCON',           cat_utilities, 'contains', 100),
        ('MYREPUBLIC',      cat_utilities, 'contains', 100),
        ('VOYAGER INTERNET',cat_utilities, 'contains', 100),
        ('CONTACT ROCKGAS', cat_utilities, 'contains', 100),
        ('ROCKGAS',         cat_utilities, 'contains', 100),
        ('GENESIS BOTTLED GAS', cat_utilities, 'contains', 100)
    ON CONFLICT DO NOTHING;

    -- ==================== EATING OUT ====================
    INSERT INTO merchant_rules (pattern, category_id, match_type, priority) VALUES
        ('MCDONALD',        cat_eating_out, 'contains', 100),  -- covers McDonalds / McDonald's / MCD
        ('BURGER KING',     cat_eating_out, 'contains', 100),
        ('KFC ',            cat_eating_out, 'starts_with', 100),
        ('KFC',             cat_eating_out, 'contains', 110),
        ('PIZZA HUT',       cat_eating_out, 'contains', 100),
        ('DOMINOS',         cat_eating_out, 'contains', 100),
        ('DOMINO''S',       cat_eating_out, 'contains', 100),
        ('SUBWAY',          cat_eating_out, 'contains', 100),
        ('HELL PIZZA',      cat_eating_out, 'contains', 100),
        ('BURGERFUEL',      cat_eating_out, 'contains', 100),
        ('BURGER FUEL',     cat_eating_out, 'contains', 100),
        ('BURGER WISCONSIN',cat_eating_out, 'contains', 100),
        ('ST PIERRE',       cat_eating_out, 'contains', 100),
        ('MUFFIN BREAK',    cat_eating_out, 'contains', 100),
        ('COLUMBUS COFFEE', cat_eating_out, 'contains', 100),
        ('ROBERT HARRIS',   cat_eating_out, 'contains', 100),
        ('COFFEE CLUB',     cat_eating_out, 'contains', 100),
        ('STARBUCKS',       cat_eating_out, 'contains', 100),
        ('MOJO COFFEE',     cat_eating_out, 'contains', 100),
        ('LONE STAR',       cat_eating_out, 'contains', 100),
        ('COBB & CO',       cat_eating_out, 'contains', 100),
        ('COBB AND CO',     cat_eating_out, 'contains', 100),
        ('DENNYS',          cat_eating_out, 'contains', 100),
        ('DENNY''S',        cat_eating_out, 'contains', 100),
        ('VALENTINES',      cat_eating_out, 'contains', 100),
        ('NOODLE CANTEEN',  cat_eating_out, 'contains', 100),
        ('SUSHI',           cat_eating_out, 'contains', 105),
        ('KEBAB',           cat_eating_out, 'contains', 105),
        ('UBER EATS',       cat_eating_out, 'contains', 90),   -- must beat generic UBER (115)
        ('UBER *EATS',      cat_eating_out, 'contains', 90),
        ('UBER* EATS',      cat_eating_out, 'contains', 90),
        ('DELIVEREASY',     cat_eating_out, 'contains', 100),
        ('MENULOG',         cat_eating_out, 'contains', 100),
        ('DOORDASH',        cat_eating_out, 'contains', 100),
        -- generic catch-alls, low priority so branded rules win first
        ('BAKERY',          cat_eating_out, 'contains', 125),
        ('CAFE',            cat_eating_out, 'contains', 130),
        ('COFFEE',          cat_eating_out, 'contains', 130),
        ('RESTAURANT',      cat_eating_out, 'contains', 130),
        ('TAKEAWAY',        cat_eating_out, 'contains', 130),
        ('EATERY',          cat_eating_out, 'contains', 130)
    ON CONFLICT DO NOTHING;

    -- ==================== ENTERTAINMENT ====================
    INSERT INTO merchant_rules (pattern, category_id, match_type, priority) VALUES
        ('EVENT CINEMAS',   cat_entertainment, 'contains', 100),
        ('HOYTS',           cat_entertainment, 'contains', 100),
        ('READING CINEMA',  cat_entertainment, 'contains', 100),
        ('RIALTO',          cat_entertainment, 'contains', 100),
        ('TICKETEK',        cat_entertainment, 'contains', 100),
        ('TICKETMASTER',    cat_entertainment, 'contains', 100),
        ('EVENTBRITE',      cat_entertainment, 'contains', 100),
        ('SKYCITY',         cat_entertainment, 'contains', 100),
        ('TIMEZONE',        cat_entertainment, 'contains', 100),
        ('STEAMGAMES',      cat_entertainment, 'contains', 100),
        ('STEAM PURCHASE',  cat_entertainment, 'contains', 100),
        ('PLAYSTATION',     cat_entertainment, 'contains', 100),
        ('NINTENDO',        cat_entertainment, 'contains', 100),
        ('XBOX',            cat_entertainment, 'contains', 100),
        ('TWITCH',          cat_entertainment, 'contains', 100),
        ('CINEMA',          cat_entertainment, 'contains', 125)
    ON CONFLICT DO NOTHING;

    -- ==================== SPORT / FITNESS ====================
    INSERT INTO merchant_rules (pattern, category_id, match_type, priority) VALUES
        ('LES MILLS',       cat_sport, 'contains', 100),
        ('CITYFITNESS',     cat_sport, 'contains', 100),
        ('CITY FITNESS',    cat_sport, 'contains', 100),
        ('ANYTIME FITNESS', cat_sport, 'contains', 100),
        ('SNAP FITNESS',    cat_sport, 'contains', 100),
        ('JETTS',           cat_sport, 'contains', 100),
        ('F45',             cat_sport, 'contains', 100),
        ('YMCA',            cat_sport, 'contains', 100),
        ('AQUATIC CENTRE',  cat_sport, 'contains', 110),
        ('SWIM',            cat_sport, 'contains', 125),
        ('GYM',             cat_sport, 'contains', 125)
    ON CONFLICT DO NOTHING;

    -- ==================== SUBSCRIPTIONS ====================
    INSERT INTO merchant_rules (pattern, category_id, match_type, priority) VALUES
        ('NETFLIX',         cat_subscriptions, 'contains', 100),
        ('SPOTIFY',         cat_subscriptions, 'contains', 100),
        ('NEON ',           cat_subscriptions, 'starts_with', 100),
        ('DISNEY PLUS',     cat_subscriptions, 'contains', 100),
        ('DISNEYPLUS',      cat_subscriptions, 'contains', 100),
        ('AMAZON PRIME',    cat_subscriptions, 'contains', 95),  -- beats generic AMAZON shopping rule
        ('PRIME VIDEO',     cat_subscriptions, 'contains', 95),
        ('YOUTUBE PREMIUM', cat_subscriptions, 'contains', 100),
        ('GOOGLE YOUTUBE',  cat_subscriptions, 'contains', 100),
        ('APPLE.COM/BILL',  cat_subscriptions, 'contains', 100),
        ('APPLE.COM BILL',  cat_subscriptions, 'contains', 100),
        ('MICROSOFT 365',   cat_subscriptions, 'contains', 100),
        ('MICROSOFT*',      cat_subscriptions, 'contains', 110),
        ('ADOBE',           cat_subscriptions, 'contains', 100),
        ('SKY TELEVISION',  cat_subscriptions, 'contains', 100),
        ('SKY NETWORK',     cat_subscriptions, 'contains', 100),
        ('OPENAI',          cat_subscriptions, 'contains', 100),
        ('CHATGPT',         cat_subscriptions, 'contains', 100),
        ('ANTHROPIC',       cat_subscriptions, 'contains', 100),
        ('CLAUDE.AI',       cat_subscriptions, 'contains', 100),
        ('GITHUB',          cat_subscriptions, 'contains', 100),
        ('JETBRAINS',       cat_subscriptions, 'contains', 100),
        ('AUDIBLE',         cat_subscriptions, 'contains', 100),
        ('PATREON',         cat_subscriptions, 'contains', 100),
        ('CRUNCHYROLL',     cat_subscriptions, 'contains', 100)
    ON CONFLICT DO NOTHING;

    -- ==================== SHOPPING / RETAIL ====================
    INSERT INTO merchant_rules (pattern, category_id, match_type, priority) VALUES
        ('THE WAREHOUSE',   cat_shopping, 'contains', 100),
        ('WAREHOUSE STATIONERY', cat_shopping, 'contains', 100),
        ('KMART',           cat_shopping, 'contains', 100),
        ('FARMERS',         cat_shopping, 'contains', 105),
        ('BRISCOES',        cat_shopping, 'contains', 100),
        ('REBEL SPORT',     cat_shopping, 'contains', 100),
        ('TORPEDO7',        cat_shopping, 'contains', 100),
        ('MITRE 10',        cat_shopping, 'contains', 100),
        ('MITRE10',         cat_shopping, 'contains', 100),
        ('BUNNINGS',        cat_shopping, 'contains', 100),
        ('PLACEMAKERS',     cat_shopping, 'contains', 100),
        ('NOEL LEEMING',    cat_shopping, 'contains', 100),
        ('HARVEY NORMAN',   cat_shopping, 'contains', 100),
        ('JB HI-FI',        cat_shopping, 'contains', 100),
        ('JB HIFI',         cat_shopping, 'contains', 100),
        ('PB TECH',         cat_shopping, 'contains', 100),
        ('PBTECH',          cat_shopping, 'contains', 100),
        ('MIGHTY APE',      cat_shopping, 'contains', 100),
        ('TRADE ME',        cat_shopping, 'contains', 100),
        ('TRADEME',         cat_shopping, 'contains', 100),
        ('TEMU',            cat_shopping, 'contains', 100),
        ('SHEIN',           cat_shopping, 'contains', 100),
        ('AMAZON',          cat_shopping, 'contains', 110),
        ('EBAY',            cat_shopping, 'contains', 100),
        ('COTTON ON',       cat_shopping, 'contains', 100),
        ('HALLENSTEIN',     cat_shopping, 'contains', 100),
        ('GLASSONS',        cat_shopping, 'contains', 100),
        ('POSTIE',          cat_shopping, 'contains', 100),
        ('NUMBER ONE SHOES',cat_shopping, 'contains', 100),
        ('HANNAHS',         cat_shopping, 'contains', 100),
        ('CHEMIST WAREHOUSE', cat_shopping, 'contains', 100),
        ('UNICHEM',         cat_shopping, 'contains', 100),
        ('LIFE PHARMACY',   cat_shopping, 'contains', 100),
        ('PHARMACY',        cat_shopping, 'contains', 120),
        ('ANIMATES',        cat_shopping, 'contains', 100),
        ('PAPER PLUS',      cat_shopping, 'contains', 100),
        ('WHITCOULLS',      cat_shopping, 'contains', 100),
        ('SPOTLIGHT',       cat_shopping, 'contains', 100),
        ('STIRLING SPORTS', cat_shopping, 'contains', 100),
        ('MACPAC',          cat_shopping, 'contains', 100),
        ('KATHMANDU',       cat_shopping, 'contains', 100),
        ('HUNTING & FISHING', cat_shopping, 'contains', 100),
        ('HUNTING AND FISHING', cat_shopping, 'contains', 100),
        ('FARMLANDS',       cat_shopping, 'contains', 100),
        ('IKEA',            cat_shopping, 'contains', 100)
    ON CONFLICT DO NOTHING;

    -- ==================== DRINKS / BARS ====================
    INSERT INTO merchant_rules (pattern, category_id, match_type, priority) VALUES
        ('LIQUORLAND',      cat_drinks, 'contains', 100),
        ('SUPER LIQUOR',    cat_drinks, 'contains', 100),
        ('THIRSTY LIQUOR',  cat_drinks, 'contains', 100),
        ('BLACK BULL LIQUOR', cat_drinks, 'contains', 100),
        ('BIG BARREL',      cat_drinks, 'contains', 100),
        ('THE BOTTLE-O',    cat_drinks, 'contains', 100),
        ('BOTTLE-O',        cat_drinks, 'contains', 105),
        ('LIQUOR',          cat_drinks, 'contains', 120),
        ('TAVERN',          cat_drinks, 'contains', 120),
        ('BREWERY',         cat_drinks, 'contains', 120),
        ('BREWING',         cat_drinks, 'contains', 120),
        ('EMERSON''S',      cat_drinks, 'contains', 100),
        ('EMERSONS',        cat_drinks, 'contains', 100)
    ON CONFLICT DO NOTHING;

    -- ==================== INCOME ====================
    INSERT INTO merchant_rules (pattern, category_id, match_type, priority) VALUES
        ('SALARY',          cat_income, 'contains', 100),
        ('WAGES',           cat_income, 'contains', 100)
    ON CONFLICT DO NOTHING;

    -- ==================== FEES ====================
    INSERT INTO merchant_rules (pattern, category_id, match_type, priority) VALUES
        ('MONTHLY ACCOUNT FEE', cat_fees, 'contains', 100),
        ('ACCOUNT FEE',     cat_fees, 'contains', 105),
        ('OVERDRAWN',       cat_fees, 'contains', 100),
        ('UNARRANGED OVERDRAFT', cat_fees, 'contains', 100)
    ON CONFLICT DO NOTHING;

END $$;


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
