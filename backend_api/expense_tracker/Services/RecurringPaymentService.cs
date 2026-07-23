using Dapper;
using expense_tracker.Models;
using expense_tracker.Utils;

namespace expense_tracker.Services
{
    public class RecurringPaymentService(DbConnectionFactory db)
    {
        public async Task<RecurringSummaryDto> GetRecurringAsync(Guid userId)
        {
            const string sql = """
                WITH keyed AS (
                    SELECT transaction_date, description, category, ABS(amount) AS amt,
                           BTRIM(REGEXP_REPLACE(BTRIM(REGEXP_REPLACE(
                               REGEXP_REPLACE(UPPER(description), '[0-9]{3,}', ' ', 'g'),
                               '[^A-Z ]', ' ', 'g')), '\s+', ' ', 'g')) AS merchant_key
                    FROM transactions
                    WHERE user_id = @UserId AND amount < 0
                ),
                daily AS (
                    SELECT merchant_key, transaction_date,
                           MIN(description) AS description,
                           MIN(category)    AS category,
                           SUM(amt)         AS amt
                    FROM keyed
                    WHERE merchant_key <> '' AND LENGTH(merchant_key) >= 3
                    GROUP BY merchant_key, transaction_date
                ),
                gaps AS (
                    SELECT *, transaction_date - LAG(transaction_date)
                                OVER (PARTITION BY merchant_key ORDER BY transaction_date) AS gap_days,
                              LAG(amt) OVER (PARTITION BY merchant_key ORDER BY transaction_date) AS prev_amt,
                              ROW_NUMBER() OVER (PARTITION BY merchant_key ORDER BY transaction_date DESC) AS rn_desc
                    FROM daily
                )
                SELECT
                    MIN(description)                                              AS Description,
                    MODE() WITHIN GROUP (ORDER BY category)                       AS Category,
                    COUNT(*)::int                                                 AS Occurrences,
                    MIN(transaction_date)                                         AS FirstSeen,
                    MAX(transaction_date)                                         AS LastSeen,
                    PERCENTILE_CONT(0.5) WITHIN GROUP (ORDER BY gap_days)::int    AS MedianIntervalDays,
                    PERCENTILE_CONT(0.5) WITHIN GROUP (ORDER BY amt)::numeric      AS TypicalAmount,
                    MAX(amt) FILTER (WHERE rn_desc = 1)                           AS LastAmount,
                    MAX(prev_amt) FILTER (WHERE rn_desc = 1)                      AS PreviousAmount
                FROM gaps
                GROUP BY merchant_key
                HAVING COUNT(*) >= 3
                   AND PERCENTILE_CONT(0.5) WITHIN GROUP (ORDER BY gap_days) BETWEEN 5 AND 400
                   AND COALESCE(STDDEV_POP(gap_days), 0)
                       <= GREATEST(PERCENTILE_CONT(0.5) WITHIN GROUP (ORDER BY gap_days) * 0.25, 3)
                   AND COALESCE(STDDEV_POP(amt), 0) <= GREATEST(AVG(amt) * 0.15, 1)
                ORDER BY PERCENTILE_CONT(0.5) WITHIN GROUP (ORDER BY amt) DESC
                """;

            await using var conn = db.CreateConnection();
            var rows = await conn.QueryAsync<RawRecurringRow>(sql, new { UserId = userId });

            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            var payments = rows.Select(r =>
            {
                var cadence = r.MedianIntervalDays switch
                {
                    >= 6 and <= 8 => "Weekly",
                    >= 13 and <= 15 => "Fortnightly",
                    >= 26 and <= 32 => "Monthly",
                    >= 85 and <= 95 => "Quarterly",
                    >= 355 and <= 375 => "Annual",
                    _ => "Irregular"
                };

                var monthlyEquivalent = Math.Round(r.TypicalAmount * 30.44m / r.MedianIntervalDays, 2);
                var nextExpected = r.LastSeen.AddDays(r.MedianIntervalDays);
                var isActive = r.LastSeen >= today.AddDays(-(int)(r.MedianIntervalDays * 1.6));
                var priceIncreased = r.PreviousAmount > 0 && r.LastAmount > r.PreviousAmount * 1.02m;

                return new RecurringPaymentDto(
                    r.Description, r.Category, cadence,
                    r.TypicalAmount, r.LastAmount, monthlyEquivalent,
                    r.Occurrences, r.MedianIntervalDays,
                    r.FirstSeen, r.LastSeen, nextExpected,
                    isActive, priceIncreased);
            }).ToList();

            var active = payments.Where(p => p.IsActive).ToList();
            var totalMonthly = active.Sum(p => p.MonthlyEquivalent);

            return new RecurringSummaryDto(
                totalMonthly,
                active.Count,
                payments.Count - active.Count,
                payments);
        }

        private record RawRecurringRow(
            string Description, string Category,
            int Occurrences,
            DateOnly FirstSeen, DateOnly LastSeen,
            int MedianIntervalDays,
            decimal TypicalAmount, decimal LastAmount,
            decimal PreviousAmount);
    }
}
