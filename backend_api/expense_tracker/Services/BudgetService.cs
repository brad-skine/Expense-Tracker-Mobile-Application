using Dapper;
using expense_tracker.Models;
using expense_tracker.Utils;

namespace expense_tracker.Services
{
    public class BudgetService(DbConnectionFactory db)
    {
        /// <summary>
        /// One round trip: every active expense category, the user's budget
        /// (0 if none set), and actual spend for the given month.
        /// </summary>
        public async Task<IEnumerable<BudgetProgressDto>> GetProgressAsync(Guid userId, int year, int month)
        {
            const string sql = """
                SELECT
                    c.id                              AS CategoryId,
                    c.name                            AS Category,
                    COALESCE(b.monthly_limit, 0)      AS MonthlyLimit,
                    COALESCE(s.spent, 0)              AS Spent
                FROM categories c
                LEFT JOIN budgets b
                    ON b.category_id = c.id AND b.user_id = @UserId
                LEFT JOIN (
                    SELECT category, SUM(ABS(amount)) AS spent
                    FROM transactions
                    WHERE user_id = @UserId
                      AND amount < 0
                      AND EXTRACT(YEAR  FROM transaction_date) = @Year
                      AND EXTRACT(MONTH FROM transaction_date) = @Month
                    GROUP BY category
                ) s ON s.category = c.name
                WHERE c.is_active = TRUE
                  AND c.name NOT IN ('Income', 'Transfers')
                ORDER BY c.display_order
                """;

            await using var conn = db.CreateConnection();
            return await conn.QueryAsync<BudgetProgressDto>(
                sql, new { UserId = userId, Year = year, Month = month });
        }

        public async Task UpsertAsync(Guid userId, UpsertBudgetRequest req)
        {
            const string sql = """
                INSERT INTO budgets (user_id, category_id, monthly_limit)
                VALUES (@UserId, @CategoryId, @MonthlyLimit)
                ON CONFLICT (user_id, category_id)
                DO UPDATE SET monthly_limit = @MonthlyLimit,
                              updated_at = CURRENT_TIMESTAMP
                """;

            await using var conn = db.CreateConnection();
            await conn.ExecuteAsync(sql, new
            {
                UserId = userId,
                req.CategoryId,
                req.MonthlyLimit
            });
        }

        public async Task DeleteAsync(Guid userId, int categoryId)
        {
            const string sql = "DELETE FROM budgets WHERE user_id = @UserId AND category_id = @CategoryId";
            await using var conn = db.CreateConnection();
            await conn.ExecuteAsync(sql, new { UserId = userId, CategoryId = categoryId });
        }
    }
}
