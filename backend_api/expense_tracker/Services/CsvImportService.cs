using Npgsql;
using System.Globalization;

namespace expense_tracker.Services
{
    public class CsvImportService
    {
        private readonly string _connectionString;
        public CsvImportService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("localConnection") ?? 
                throw new InvalidOperationException("Connection string 'localConnection' not found.");
        }


        public async Task<int> ImportTransactionsAsync(Stream csvStream) 
        {
          
            using var reader = new StreamReader(csvStream);
            using var csv = new CsvHelper.CsvReader(reader, CultureInfo.InvariantCulture);

            var records = csv.GetRecords<Models.transactionCsv>().ToList();
            using var connection = new Npgsql.NpgsqlConnection(_connectionString);

            await connection.OpenAsync(); // Open the database connection
            int inserted = 0; // Counter for inserted records
            foreach (var csvTransaction in records)
            {
                var transaction = new Models.Transaction
                {
                    Date = csvTransaction.Date,
                    TransactionType = csvTransaction.TransactionType,
                    Description = csvTransaction.Description,
                    Amount = Utils.MoneyParser.ParseMoney(csvTransaction.Amount),
                    Balance = Utils.MoneyParser.ParseMoney(csvTransaction.Balance)
                };
                
                using var command = new NpgsqlCommand( 
                     """
                    INSERT INTO transactions
                        (date, transaction_type, description, amount, balance)
                    VALUES
                        (@date, @t_type, @description, @amount, @balance)
                    """,
                     connection
                 );
                command.Parameters.AddWithValue("date", transaction.Date.ToDateTime(new TimeOnly(0, 0)));
                command.Parameters.AddWithValue("t_type", transaction.TransactionType);
                command.Parameters.AddWithValue("description", transaction.Description);
                command.Parameters.AddWithValue("amount", transaction.Amount);
                command.Parameters.AddWithValue("balance", transaction.Balance);
        
                inserted += await command.ExecuteNonQueryAsync(); 
            }

            return inserted;
        }

    }
}
