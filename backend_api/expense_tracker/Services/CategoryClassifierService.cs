using Dapper;
using expense_tracker.Utils;

namespace expense_tracker.Services
{
    public class CategoryClassifierService(DbConnectionFactory db)
    {
        // Cached global rules - loaded once, reused across requests
        private List<MerchantRule>? _globalRulesCache;
        private DateTime _cacheExpiry = DateTime.MinValue;
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(10);

        /// <summary>
        /// Classify a single transaction based on merchant rules.
        /// Priority: transaction_type shortcuts → user rules → global rules → "Other"
        /// </summary>
        public async Task<string> ClassifyAsync(
            string description,
            string transactionType,
            decimal amount,
            Guid userId)
        {
            // 1. Quick classification by transaction type
            if (transactionType.Equals("Fee", StringComparison.OrdinalIgnoreCase))
                return "Fees";

            if (transactionType.Equals("Deposit", StringComparison.OrdinalIgnoreCase) && amount > 0)
                return "Income";

            // 2. Check user-specific rules first (highest priority)
            var userRules = await GetUserRulesAsync(userId);
            var userMatch = MatchAgainstRules(description, userRules);
            if (userMatch != null)
                return userMatch;

            // 3. Check global rules
            var globalRules = await GetGlobalRulesAsync();
            var globalMatch = MatchAgainstRules(description, globalRules);
            if (globalMatch != null)
                return globalMatch;

            // 4. No match
            return "Other";
        }

        private static string? MatchAgainstRules(string description, List<MerchantRule> rules)
        {
            foreach (var rule in rules) // already sorted by priority
            {
                bool matched = rule.MatchType switch
                {
                    "contains" => description.Contains(rule.Pattern, StringComparison.OrdinalIgnoreCase),
                    "starts_with" => description.StartsWith(rule.Pattern, StringComparison.OrdinalIgnoreCase),
                    "exact" => description.Equals(rule.Pattern, StringComparison.OrdinalIgnoreCase),
                    _ => false
                };

                if (matched)
                    return rule.CategoryName;
            }

            return null;
        }

        private async Task<List<MerchantRule>> GetGlobalRulesAsync()
        {
            if (_globalRulesCache != null && DateTime.UtcNow < _cacheExpiry)
                return _globalRulesCache;

            const string sql = """
                               SELECT 
                                   mr.pattern AS Pattern,
                                   c.name AS CategoryName,
                                   mr.match_type AS MatchType,
                                   mr.priority AS Priority
                               FROM merchant_rules mr
                               JOIN categories c ON c.id = mr.category_id
                               WHERE mr.is_global = TRUE
                               ORDER BY mr.priority ASC
                               """;

            await using var conn = db.CreateConnection();
            var rules = (await conn.QueryAsync<MerchantRule>(sql)).ToList();

            _globalRulesCache = rules;
            _cacheExpiry = DateTime.UtcNow.Add(CacheDuration);

            return rules;
        }

        private async Task<List<MerchantRule>> GetUserRulesAsync(Guid userId)
        {
            const string sql = """
                               SELECT 
                                   mr.pattern AS Pattern,
                                   c.name AS CategoryName,
                                   mr.match_type AS MatchType,
                                   mr.priority AS Priority
                               FROM merchant_rules mr
                               JOIN categories c ON c.id = mr.category_id
                               WHERE mr.is_global = FALSE
                                 AND mr.user_id = @UserId
                               ORDER BY mr.priority 
                               """;

            await using var conn = db.CreateConnection();
            return (await conn.QueryAsync<MerchantRule>(sql, new { UserId = userId })).ToList();
        }

        /// <summary>
        /// Set-based reclassify: user rules first, then global rules, all in SQL.
        /// Re-classify all transactions for a user that currently have category = 'Other'
        /// or optionally all transactions (force = true)
        /// </summary>
        public async Task<int> ReclassifyAsync(Guid userId, bool force = false)
        {
            var scopeFilter = force ? "" : "AND t2.category = 'Other'";

            var sql = $"""
                       UPDATE transactions t
                       SET category = matched.cat_name
                       FROM (
                           SELECT DISTINCT ON (t2.id) t2.id AS txn_id, c.name AS cat_name
                           FROM transactions t2
                           JOIN merchant_rules mr ON (mr.is_global = TRUE OR mr.user_id = @UserId) AND (
                                  (mr.match_type = 'contains'    AND t2.description ILIKE '%' || mr.pattern || '%')
                               OR (mr.match_type = 'starts_with' AND t2.description ILIKE mr.pattern || '%')
                               OR (mr.match_type = 'exact'       AND LOWER(t2.description) = LOWER(mr.pattern))
                           )
                           JOIN categories c ON c.id = mr.category_id
                           WHERE t2.user_id = @UserId {scopeFilter}
                           ORDER BY t2.id, mr.priority ASC
                       ) matched
                       WHERE t.id = matched.txn_id;
                       """;

            await using var conn = db.CreateConnection();
            await conn.OpenAsync();

            // type-based rules first (same as before, but set-based)
            var typeUpdates = await conn.ExecuteAsync("""
                                                      UPDATE transactions SET category = 'Fees'
                                                      WHERE user_id = @UserId AND transaction_type = 'Fee'
                                                        AND (category = 'Other' OR @Force);
                                                      """, new { UserId = userId, Force = force });

            typeUpdates += await conn.ExecuteAsync("""
                                                   UPDATE transactions SET category = 'Income'
                                                   WHERE user_id = @UserId AND transaction_type = 'Deposit' AND amount > 0
                                                     AND (category = 'Other' OR @Force);
                                                   """, new { UserId = userId, Force = force });

            return typeUpdates + await conn.ExecuteAsync(sql, new { UserId = userId });
        }

        // Internal DTOs
        private record MerchantRule(
            string Pattern,
            string CategoryName,
            string MatchType,
            int Priority);

        private record TransactionForClassification(
            int Id,
            string Description,
            string TransactionType,
            decimal Amount);
    }
}