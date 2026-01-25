
using Dapper;
using expense_tracker.Models;
using Microsoft.AspNetCore.SignalR;
using Npgsql;


namespace expense_tracker.Services
{

    public class TransactionQueryService 
    {
        private readonly string _connectionString;

        public TransactionQueryService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ??
                throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        }

        private Npgsql.NpgsqlConnection GetConnection()
        {
            return new Npgsql.NpgsqlConnection(_connectionString);
        }

        public async Task<IEnumerable<Transaction>> GetAllTansactionsAsync(Guid userId)
        {

            const string sql = $"""
                                SELECT 
                                id,
                                --user_id AS UserId,
                                transaction_date AS Date,
                                transaction_type AS TransactionType,
                                description,
                                amount,
                                balance
                                FROM transactions
                                WHERE user_id = @UserId
                                """;
            using var conn = GetConnection();
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

            using var conn = GetConnection();
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
            using var conn = GetConnection();
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

            using var conn = GetConnection();
            return await conn.QueryAsync<TypeSummaryDto>(sql, new {UserId = userId}); 
        }
    }

}

    




