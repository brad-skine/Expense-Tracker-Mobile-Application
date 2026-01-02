
using Dapper;
using expense_tracker.Models;
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

        public async Task<IEnumerable<Transaction>> GetAllTansactionsAsync()
        {

            const string sql = """
                SELECT 
                id,
                transaction_date AS Date,
                transaction_type AS TransactionType,
                description,
                amount,
                balance
                FROM transactions
                """;
            using var conn = GetConnection();
            return await conn.QueryAsync<Transaction>(sql);
        }


        public async Task<IEnumerable<MonthlySummaryDto>> GetMonthlySummaryAsync()
        {
            const string sql = """
                SELECT
                    EXTRACT (YEAR FROM transaction_Date) :: int AS year,
                    EXTRACT (Month FROM transaction_Date) :: int AS month,
                    COALESCE(SUM(amount) FILTER (WHERE amount > 0) ,0) AS income,
                    COALESCE(SUM(amount) FILTER (WHERE amount < 0), 0) AS expense
                FROM transactions
                group by Year, Month
                ORDER by Year, Month
            
            """;

            using var conn = GetConnection();
            return await conn.QueryAsync<MonthlySummaryDto>(sql);
        }

        public async Task<IEnumerable<YearlySummaryDto>> GetYearlySummaryAsync()
        {
            const string sql = """
                SELECT 
                	EXTRACT (YEAR FROM transaction_Date) :: int AS year,
                	COALESCE(SUM(amount) FILTER (WHERE amount > 0) ,0) AS income,
                	COALESCE(SUM(amount) FILTER (WHERE amount < 0), 0) AS expense
                FROM transactions
                GROUP by year
                ORDER by year
                """;
            using var conn = GetConnection();
            return await conn.QueryAsync<YearlySummaryDto>(sql);
        }


        public async Task<IEnumerable<TypeSummaryDto>> GetTypeSummaryAsync()
        {
           
            const string sql = """
                SELECT -- results for transaction type
                	transaction_type AS TransactionType,
                	SUM(ABS(amount)) AS Total
                FROM transactions
                WHERE amount < 0
                GROUP BY transaction_type
                ORDER BY Total DESC;
                """;

            using var conn = GetConnection();
            return await conn.QueryAsync<TypeSummaryDto>(sql); 
        }
    }

}

    




