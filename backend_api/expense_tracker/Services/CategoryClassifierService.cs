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
        /// Re-classify all transactions for a user that currently have category = 'Other'
        /// or optionally all transactions (force = true)
        /// </summary>
        public async Task<int> ReclassifyAsync(Guid userId, bool force = false)
        {
            const string selectSql = """
                SELECT id, description, transaction_type AS TransactionType, amount
                FROM transactions
                WHERE user_id = @UserId
                """;

            const string selectUnclassifiedSql = """
                SELECT id, description, transaction_type AS TransactionType, amount
                FROM transactions
                WHERE user_id = @UserId AND category = 'Other'
                """;

            const string updateSql = """
                UPDATE transactions SET category = @Category WHERE id = @Id
                """;

            await using var conn = db.CreateConnection();
            await conn.OpenAsync();

            var sql = force ? selectSql : selectUnclassifiedSql;
            var transactions = (await conn.QueryAsync<TransactionForClassification>(
                sql, new { UserId = userId })).ToList();

            int updated = 0;
            foreach (var txn in transactions)
            {
                var category = await ClassifyAsync(
                    txn.Description, txn.TransactionType, txn.Amount, userId);

                if (force || category != "Other")
                {
                    await conn.ExecuteAsync(updateSql, new { Category = category, txn.Id });
                    updated++;
                }
            }

            return updated;
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
