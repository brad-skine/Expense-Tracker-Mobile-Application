using Dapper;
using expense_tracker.Models;
using expense_tracker.Utils;

namespace expense_tracker.Services
{

    public class TransactionQueryService (DbConnectionFactory db)
    {

        public async Task<IEnumerable<Transaction>> GetAllTransactionsAsync(Guid userId)
        {

            const string sql = $"""
                                SELECT 
                                id,
                                --user_id AS UserId,
                                transaction_date AS Date,
                                category,
                                description,
                                amount,
                                balance
                                FROM transactions
                                WHERE user_id = @UserId;
                                """;
            await using var conn = db.CreateConnection();
            return await conn.QueryAsync<Transaction>(sql, new {UserId= userId});
        }


        public async Task<IEnumerable<MonthlySummaryDto>> GetMonthlySummaryAsync(Guid userId)
        {
            const string sql = """
                SELECT
                    EXTRACT (YEAR FROM transaction_Date) :: int AS year,
                    EXTRACT (Month FROM transaction_Date) :: int AS month,
                    COALESCE(SUM(amount) FILTER (WHERE amount > 0) ,0) AS income,
                    COALESCE(SUM(amount) FILTER (WHERE amount < 0), 0) AS expense
                FROM transactions
                WHERE user_id = @UserID
                group by Year, Month
                ORDER by Year, Month
            
            """;

            await using var conn = db.CreateConnection();
            return await conn.QueryAsync<MonthlySummaryDto>(sql, new {UserId = userId});
        }

        public async Task<IEnumerable<YearlySummaryDto>> GetYearlySummaryAsync(Guid userId)
        {
            const string sql = """
                SELECT 
                	EXTRACT (YEAR FROM transaction_Date) :: int AS year,
                	COALESCE(SUM(amount) FILTER (WHERE amount > 0) ,0) AS income,
                	COALESCE(SUM(amount) FILTER (WHERE amount < 0), 0) AS expense
                FROM transactions
                WHERE user_id = @UserId
                GROUP by year
                ORDER by year
                """;
            await using var conn = db.CreateConnection();
            return await conn.QueryAsync<YearlySummaryDto>(sql, new {UserId = userId});
        }


        public async Task<IEnumerable<TypeSummaryDto>> GetTypeSummaryAsync(Guid userId)
        {
           
            const string sql = """
                SELECT -- results for transaction type
                	transaction_type AS TransactionType,
                	SUM(ABS(amount)) AS Total
                FROM transactions
                WHERE user_id = @UserID 
                AND amount < 0
                GROUP BY transaction_type
                ORDER BY Total DESC;
                """;

            await using var conn = db.CreateConnection();
            return await conn.QueryAsync<TypeSummaryDto>(sql, new {UserId = userId}); 
        }
        // NEW: Category-based spending summary (replaces type summary for pie chart)
        public async Task<IEnumerable<CategorySummaryDto>> GetCategorySummaryAsync(Guid userId)
        {
            const string sql = """
                SELECT
                    category AS Category,
                    SUM(ABS(amount)) AS Total
                FROM transactions
                WHERE user_id = @UserId AND amount < 0
                GROUP BY category
                ORDER BY Total DESC
                """;
 
            await using var conn = db.CreateConnection();
            return await conn.QueryAsync<CategorySummaryDto>(sql, new { UserId = userId });
        }
 
        // NEW: Get all available categories (for frontend dropdowns)
        public async Task<IEnumerable<CategoryDto>> GetCategoriesAsync()
        {
            const string sql = """
                SELECT id AS Id, name AS Name, display_order AS DisplayOrder,
                       COALESCE(icon_key, '') AS IconKey,
                       COALESCE(color_hex, '') AS ColorHex,
                       (user_id IS NOT NULL) AS IsCustom
                FROM categories
                WHERE is_active = TRUE
                ORDER BY display_order
                """;
 
            await using var conn = db.CreateConnection();
            return await conn.QueryAsync<CategoryDto>(sql);
        }
    }
}



