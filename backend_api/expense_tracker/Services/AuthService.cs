using Dapper;
using expense_tracker.Models;
using expense_tracker.Services.Interfaces;
namespace expense_tracker.Services
{

    public class AuthService(IConfiguration configuration,
        ITokenService tokenService) : IAuthService
    {   
        
        private readonly string _connectionString = configuration.GetConnectionString("DefaultConnection") ??
                throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        private Npgsql.NpgsqlConnection GetConnection()
        {
            return new Npgsql.NpgsqlConnection(_connectionString);
        }

        public async Task RegisterAsync(string email, string password)
        {
            const string checkSql = """
                SELECT COUNT(1)
                FROM users 
                WHERE email = @Email
                """;

            const string insertSql = """
                INSERT INTO users (id, email, password_hash)
                VALUES (@Id, @Email, @PasswordHash)
                """;
                
            await using var conn = GetConnection();
            var exist = await conn.ExecuteScalarAsync<bool>(
                checkSql, new {Email= email});

            if (exist) {
                throw new Exception("User already exists");
            }
            var hash = BCrypt.Net.BCrypt.HashPassword(password);

            await conn.ExecuteAsync(insertSql, new
            {
                ID = Guid.NewGuid(),
                Email = email,
                PasswordHash = hash
            });

        }


        public async Task<string> LoginAsync(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                throw new UnauthorizedAccessException();
            }
            const string sql = """
                SELECT id, email, password_hash AS PasswordHash
                FROM users
                WHERE email = @Email

                """;
            await using var conn = GetConnection();

            var user = await conn.QuerySingleOrDefaultAsync<User>(
                sql, new { Email = email });
            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash) ) {
                throw new UnauthorizedAccessException();
            }

            //token time
            return tokenService.GenerateToken(user); 

        }
    }
}
