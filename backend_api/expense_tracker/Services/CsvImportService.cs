using CsvHelper;
using Npgsql;
using System.Globalization;

namespace expense_tracker.Services
{
    public class CsvImportService(IConfiguration configuration)
    {
        private readonly string _connectionString = configuration.GetConnectionString("DefaultConnection") ??
                throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");


        public async Task<int> ImportTransactionsAsync(Stream csvStream, Guid userId)
        {

            using var reader = new StreamReader(csvStream);

            var config = new CsvHelper.Configuration.CsvConfiguration(CultureInfo.InvariantCulture)
            {
                PrepareHeaderForMatch = args => args.Header.Trim().ToLower(),
                MissingFieldFound = null // optional: ignore missing columns
            };

            using var csv = new CsvHelper.CsvReader(reader, config);

            var records = csv.GetRecords<Models.transactionCsv>().ToList();
            using var connection = new Npgsql.NpgsqlConnection(_connectionString);

            await connection.OpenAsync(); // Open the database connection
            int inserted = 0; // Counter for inserted records
            int skipped = 0; // Counter for skipped records might not use as slow


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

                using var command = new NpgsqlCommand(
                     """
                    INSERT INTO transactions
                        (user_id, transaction_date, transaction_type, description, amount, balance)
                    VALUES
                        (@UserId, @date, @transaction_type, @description, @amount, @balance)
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

                int new_row = await command.ExecuteNonQueryAsync();
                if (new_row == 1)
                {
                    inserted++;
                }
                else
                {
                    skipped++;
                }

            }

            return inserted;

        }
    }
}
