using Dapper;
using expense_tracker.Models;
using expense_tracker.Utils;

namespace expense_tracker.Services
{
    public class TransactionCrudService(DbConnectionFactory db, CategoryClassifierService classifier)
    {
        public async Task<int> CreateAsync(Guid userId, CreateTransactionRequest req)
        {
            var category = string.IsNullOrWhiteSpace(req.Category)
                ? await classifier.ClassifyAsync(
                    req.Description,
                    req.Amount > 0 ? "Deposit" : "Manual",
                    req.Amount,
                    userId)
                : req.Category;

            const string sql = """
                INSERT INTO transactions
                    (user_id, transaction_date, transaction_type, description, amount, balance, category)
                VALUES
                    (@UserId, @Date, 'Manual', @Description, @Amount, 0, @Category)
                RETURNING id
                """;

            await using var conn = db.CreateConnection();
            return await conn.ExecuteScalarAsync<int>(sql, new
            {
                UserId = userId,
                req.Date,
                Description = req.Description.Trim(),
                req.Amount,
                Category = category
            });
        }

        public async Task<bool> UpdateAsync(Guid userId, int id, UpdateTransactionRequest req)
        {
            // user_id in WHERE prevents editing anyone else's rows
            const string sql = """
                UPDATE transactions
                SET transaction_date = @Date,
                    description      = @Description,
                    amount           = @Amount,
                    category         = @Category
                WHERE id = @Id AND user_id = @UserId
                """;

            await using var conn = db.CreateConnection();
            var rows = await conn.ExecuteAsync(sql, new
            {
                Id = id,
                UserId = userId,
                req.Date,
                Description = req.Description.Trim(),
                req.Amount,
                req.Category
            });
            return rows == 1;
        }

        public async Task<bool> DeleteAsync(Guid userId, int id)
        {
            const string sql = "DELETE FROM transactions WHERE id = @Id AND user_id = @UserId";
            await using var conn = db.CreateConnection();
            return await conn.ExecuteAsync(sql, new { Id = id, UserId = userId }) == 1;
        }
    }
}
