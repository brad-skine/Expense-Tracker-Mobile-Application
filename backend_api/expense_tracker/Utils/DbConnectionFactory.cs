using Npgsql;

namespace expense_tracker.Utils;
public class DbConnectionFactory(IConfiguration configuration)
{
    private readonly string _connectionString = 
        configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException(
            "Connection string 'DefaultConnection' not found.");

    public NpgsqlConnection CreateConnection() => new(_connectionString);
}
