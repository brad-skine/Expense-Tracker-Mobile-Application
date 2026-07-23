using Dapper;
using expense_tracker.Models;
using expense_tracker.Utils;

namespace expense_tracker.Services
{
    public class CategoryManagementService(DbConnectionFactory db)
    {
        private static readonly HashSet<string> ValidMatchTypes = new(StringComparer.OrdinalIgnoreCase)
            { "contains", "starts_with", "exact" };

        // ── Categories ──────────────────────────────────────────────

        public async Task<IEnumerable<CategoryDto>> GetCategoriesAsync(Guid userId)
        {
            const string sql = """
                SELECT id AS Id, name AS Name, display_order AS DisplayOrder,
                       COALESCE(icon_key, '') AS IconKey,
                       COALESCE(color_hex, '') AS ColorHex,
                       (user_id IS NOT NULL) AS IsCustom
                FROM categories
                WHERE is_active = TRUE
                  AND (user_id IS NULL OR user_id = @UserId)
                ORDER BY display_order, name
                """;

            await using var conn = db.CreateConnection();
            return await conn.QueryAsync<CategoryDto>(sql, new { UserId = userId });
        }

        public async Task<CategoryDto> CreateAsync(Guid userId, CreateCategoryRequest req)
        {
            const string checkSql = """
                SELECT COUNT(*) FROM categories
                WHERE LOWER(name) = LOWER(@Name)
                  AND (user_id IS NULL OR user_id = @UserId)
                """;

            const string insertSql = """
                INSERT INTO categories (name, user_id, icon_key, color_hex, display_order, is_active)
                VALUES (@Name, @UserId, @IconKey, @ColorHex,
                        (SELECT COALESCE(MAX(display_order),0)+1 FROM categories), TRUE)
                RETURNING id AS Id, name AS Name, display_order AS DisplayOrder,
                          COALESCE(icon_key, '') AS IconKey,
                          COALESCE(color_hex, '') AS ColorHex,
                          TRUE AS IsCustom
                """;

            await using var conn = db.CreateConnection();
            await conn.OpenAsync();
            await using var tx = await conn.BeginTransactionAsync();

            var exists = await conn.ExecuteScalarAsync<int>(checkSql,
                new { req.Name, UserId = userId }, tx);

            if (exists > 0)
                throw new InvalidOperationException(
                    $"A category named '{req.Name}' already exists.");

            var created = await conn.QuerySingleAsync<CategoryDto>(insertSql,
                new { req.Name, UserId = userId, req.IconKey, req.ColorHex }, tx);

            await tx.CommitAsync();
            return created;
        }

        public async Task UpdateAsync(Guid userId, int id, UpdateCategoryRequest req)
        {
            const string selectSql = """
                SELECT name FROM categories
                WHERE id = @Id AND user_id = @UserId
                """;

            const string checkSql = """
                SELECT COUNT(*) FROM categories
                WHERE LOWER(name) = LOWER(@Name)
                  AND id <> @Id
                  AND (user_id IS NULL OR user_id = @UserId)
                """;

            const string updateSql = """
                UPDATE categories
                SET name = @Name, icon_key = @IconKey, color_hex = @ColorHex,
                    display_order = @DisplayOrder
                WHERE id = @Id AND user_id = @UserId
                """;

            const string renameTxnSql = """
                UPDATE transactions SET category = @NewName
                WHERE user_id = @UserId AND category = @OldName
                """;

            await using var conn = db.CreateConnection();
            await conn.OpenAsync();
            await using var tx = await conn.BeginTransactionAsync();

            var oldName = await conn.QuerySingleOrDefaultAsync<string>(selectSql,
                new { Id = id, UserId = userId }, tx);

            if (oldName is null)
                throw new KeyNotFoundException("Category not found or not owned by you.");

            var exists = await conn.ExecuteScalarAsync<int>(checkSql,
                new { req.Name, Id = id, UserId = userId }, tx);

            if (exists > 0)
                throw new InvalidOperationException(
                    $"A category named '{req.Name}' already exists.");

            await conn.ExecuteAsync(updateSql,
                new { req.Name, req.IconKey, req.ColorHex, req.DisplayOrder, Id = id, UserId = userId }, tx);

            if (!string.Equals(oldName, req.Name, StringComparison.Ordinal))
            {
                await conn.ExecuteAsync(renameTxnSql,
                    new { NewName = req.Name, OldName = oldName, UserId = userId }, tx);
            }

            await tx.CommitAsync();
        }

        public async Task DeleteAsync(Guid userId, int id)
        {
            const string selectSql = """
                SELECT name FROM categories
                WHERE id = @Id AND user_id = @UserId
                """;

            const string deleteRulesSql = """
                DELETE FROM merchant_rules WHERE category_id = @Id AND user_id = @UserId
                """;

            const string updateTxnSql = """
                UPDATE transactions SET category = 'Other'
                WHERE user_id = @UserId AND category = @Name
                """;

            const string deleteCatSql = """
                DELETE FROM categories WHERE id = @Id AND user_id = @UserId
                """;

            await using var conn = db.CreateConnection();
            await conn.OpenAsync();
            await using var tx = await conn.BeginTransactionAsync();

            var name = await conn.QuerySingleOrDefaultAsync<string>(selectSql,
                new { Id = id, UserId = userId }, tx);

            if (name is null)
                throw new KeyNotFoundException("Category not found or not owned by you.");

            await conn.ExecuteAsync(deleteRulesSql, new { Id = id, UserId = userId }, tx);
            await conn.ExecuteAsync(updateTxnSql, new { UserId = userId, Name = name }, tx);
            await conn.ExecuteAsync(deleteCatSql, new { Id = id, UserId = userId }, tx);

            await tx.CommitAsync();
        }

        // ── Rules ───────────────────────────────────────────────────

        public async Task<IEnumerable<MerchantRuleDto>> GetRulesAsync(Guid userId)
        {
            const string sql = """
                SELECT mr.id AS Id, mr.pattern AS Pattern, mr.category_id AS CategoryId,
                       c.name AS CategoryName, mr.match_type AS MatchType,
                       mr.priority AS Priority, mr.is_global AS IsGlobal
                FROM merchant_rules mr
                JOIN categories c ON c.id = mr.category_id
                WHERE mr.is_global = TRUE OR mr.user_id = @UserId
                ORDER BY mr.priority
                """;

            await using var conn = db.CreateConnection();
            return await conn.QueryAsync<MerchantRuleDto>(sql, new { UserId = userId });
        }

        public async Task<MerchantRuleDto> CreateRuleAsync(Guid userId, UpsertRuleRequest req)
        {
            ValidateRule(req);

            const string sql = """
                INSERT INTO merchant_rules (pattern, category_id, match_type, priority, is_global, user_id)
                VALUES (@Pattern, @CategoryId, @MatchType, 50, FALSE, @UserId)
                RETURNING id AS Id, pattern AS Pattern, category_id AS CategoryId,
                          match_type AS MatchType, priority AS Priority, FALSE AS IsGlobal
                """;

            await using var conn = db.CreateConnection();
            var rule = await conn.QuerySingleAsync<MerchantRuleDto>(sql,
                new { req.Pattern, req.CategoryId, MatchType = req.MatchType.ToLowerInvariant(), UserId = userId });

            return rule;
        }

        public async Task UpdateRuleAsync(Guid userId, int id, UpsertRuleRequest req)
        {
            ValidateRule(req);

            const string sql = """
                UPDATE merchant_rules
                SET pattern = @Pattern, category_id = @CategoryId, match_type = @MatchType
                WHERE id = @Id AND user_id = @UserId AND is_global = FALSE
                """;

            await using var conn = db.CreateConnection();
            var rows = await conn.ExecuteAsync(sql,
                new { req.Pattern, req.CategoryId, MatchType = req.MatchType.ToLowerInvariant(), Id = id, UserId = userId });

            if (rows == 0)
                throw new KeyNotFoundException("Rule not found or not owned by you.");
        }

        public async Task DeleteRuleAsync(Guid userId, int id)
        {
            const string sql = """
                DELETE FROM merchant_rules
                WHERE id = @Id AND user_id = @UserId AND is_global = FALSE
                """;

            await using var conn = db.CreateConnection();
            var rows = await conn.ExecuteAsync(sql, new { Id = id, UserId = userId });

            if (rows == 0)
                throw new KeyNotFoundException("Rule not found or not owned by you.");
        }

        // ── Suggestions ─────────────────────────────────────────────

        public async Task<IEnumerable<RuleSuggestionDto>> GetRuleSuggestionsAsync(Guid userId)
        {
            const string sql = """
                WITH cleaned AS (
                    SELECT description,
                           ABS(amount) AS amt,
                           BTRIM(REGEXP_REPLACE(
                               REGEXP_REPLACE(UPPER(description), '[0-9]{3,}', ' ', 'g'),
                               '[^A-Z ]', ' ', 'g')) AS clean
                    FROM transactions
                    WHERE user_id = @UserId AND category = 'Other' AND amount < 0
                ),
                stems AS (
                    SELECT description, amt,
                           (string_to_array(BTRIM(REGEXP_REPLACE(clean, '\s+', ' ', 'g')), ' '))[1]
                             || COALESCE(' ' || (string_to_array(BTRIM(REGEXP_REPLACE(clean, '\s+', ' ', 'g')), ' '))[2], '') AS stem
                    FROM cleaned
                    WHERE clean <> ''
                )
                SELECT stem AS Pattern,
                       COUNT(*)::int AS Occurrences,
                       SUM(amt) AS TotalAmount,
                       MIN(description) AS SampleDescription
                FROM stems
                WHERE LENGTH(stem) >= 3
                GROUP BY stem
                HAVING COUNT(*) >= 2
                ORDER BY COUNT(*) DESC, SUM(amt) DESC
                LIMIT 25
                """;

            await using var conn = db.CreateConnection();
            return await conn.QueryAsync<RuleSuggestionDto>(sql, new { UserId = userId });
        }

        // ── Validation ──────────────────────────────────────────────

        private static void ValidateRule(UpsertRuleRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Pattern))
                throw new InvalidOperationException("Pattern must not be empty.");

            if (!ValidMatchTypes.Contains(req.MatchType))
                throw new InvalidOperationException(
                    "MatchType must be one of: contains, starts_with, exact.");

            if (req.MatchType.Equals("contains", StringComparison.OrdinalIgnoreCase)
                && req.Pattern.Trim().Length < 3)
                throw new InvalidOperationException(
                    "A 'contains' pattern must be at least 3 characters.");
        }
    }
}
