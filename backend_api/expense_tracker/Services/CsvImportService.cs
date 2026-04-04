using CsvHelper;
using Npgsql;
using System.Globalization;
using expense_tracker.Utils;

namespace expense_tracker.Services
{
    public class CsvImportService(DbConnectionFactory db, CategoryClassifierService classifier)
    {
        public async Task<int> ImportTransactionsAsync(Stream csvStream, Guid userId)
        {
            using var reader = new StreamReader(csvStream);

            var config = new CsvHelper.Configuration.CsvConfiguration(CultureInfo.InvariantCulture)
            {
                PrepareHeaderForMatch = args => args.Header.Trim().ToLower(),
                MissingFieldFound = null
            };

            using var csv = new CsvHelper.CsvReader(reader, config);

            var records = csv.GetRecords<Models.transactionCsv>().ToList();
            await using var connection = db.CreateConnection();
            await connection.OpenAsync();

            int inserted = 0;

            foreach (var csvTransaction in records)
            {
                var transaction = new Models.Transaction
                {
                    UserId = userId,
                    Date = csvTransaction.Date,
                    TransactionType = csvTransaction.TransactionType,
                    Description = csvTransaction.Description,
                    Amount = Utils.MoneyParser.ParseMoney(csvTransaction.Amount),
                    Balance = Utils.MoneyParser.ParseMoney(csvTransaction.Balance)
                };

                // Classify the transaction before inserting
                var category = await classifier.ClassifyAsync(
                    transaction.Description,
                    transaction.TransactionType,
                    transaction.Amount,
                    userId);

                using var command = new NpgsqlCommand(
                    """
                    INSERT INTO transactions
                        (user_id, transaction_date, transaction_type, description, amount, balance, category)
                    VALUES
                        (@UserId, @date, @transaction_type, @description, @amount, @balance, @category)
                    ON CONFLICT (user_id, transaction_date, amount, balance)
                    DO NOTHING;
                    """,
                    connection
                );

                command.Parameters.AddWithValue("UserId", userId);
                command.Parameters.AddWithValue("date", transaction.Date.ToDateTime(new TimeOnly(0, 0)));
                command.Parameters.AddWithValue("transaction_type", transaction.TransactionType);
                command.Parameters.AddWithValue("description", transaction.Description);
                command.Parameters.AddWithValue("amount", transaction.Amount);
                command.Parameters.AddWithValue("balance", transaction.Balance);
                command.Parameters.AddWithValue("category", category);

                int newRow = await command.ExecuteNonQueryAsync();
                if (newRow == 1)
                {
                    inserted++;
                }
            }

            return inserted;
        }
    }
}
