-- ============================================================
-- 09: User-created categories + icons/colours
-- Safe to re-run
-- ============================================================

-- Ownership: NULL user_id = global preset category
ALTER TABLE categories ADD COLUMN IF NOT EXISTS user_id UUID REFERENCES users(id);
ALTER TABLE categories ADD COLUMN IF NOT EXISTS icon_key  VARCHAR(50) NOT NULL DEFAULT 'tag';
ALTER TABLE categories ADD COLUMN IF NOT EXISTS color_hex VARCHAR(9)  NOT NULL DEFAULT '#94a3b8';

-- name was globally UNIQUE; now it's unique per scope instead
ALTER TABLE categories DROP CONSTRAINT IF EXISTS categories_name_key;

CREATE UNIQUE INDEX IF NOT EXISTS uq_categories_global_name
    ON categories (LOWER(name)) WHERE user_id IS NULL;

CREATE UNIQUE INDEX IF NOT EXISTS uq_categories_user_name
    ON categories (user_id, LOWER(name)) WHERE user_id IS NOT NULL;

CREATE INDEX IF NOT EXISTS idx_categories_user ON categories (user_id);

-- Stop duplicate user rules
CREATE UNIQUE INDEX IF NOT EXISTS uq_merchant_rules_user_pattern
    ON merchant_rules (user_id, LOWER(pattern), category_id) WHERE user_id IS NOT NULL;

-- Seed icons/colours on the presets so the UI isn't all grey tags
UPDATE categories SET icon_key = v.icon, color_hex = v.col
    FROM (VALUES
    ('Groceries',        'cart',        '#4ade80'),
    ('Rent',             'home',        '#93b4f8'),
    ('Utilities',        'bolt',        '#fbbf24'),
    ('Transport/Fuel',   'fuel',        '#38bdf8'),
    ('Eating Out',       'utensils',    '#fb923c'),
    ('Entertainment',    'film',        '#c084fc'),
    ('Sport/Fitness',    'dumbbell',    '#a3e635'),
    ('Subscriptions',    'repeat',      '#f472b6'),
    ('Shopping/Retail',  'bag',         '#f87171'),
    ('Drinks/Bars',      'glass',       '#e879f9'),
    ('Donations/Church', 'heart',       '#fda4af'),
    ('Transfers',        'arrows',      '#94a3b8'),
    ('Income',           'trending-up', '#22c55e'),
    ('Fees',             'receipt',     '#64748b'),
    ('Other',            'tag',         '#94a3b8')
) AS v(name, icon, col)
WHERE categories.name = v.name AND categories.user_id IS NULL;
