using Dapper;
using expense_tracker.Models;
using expense_tracker.Utils;

namespace expense_tracker.Services.Akahu;

// Pulls Akahu accounts into bank_connections. Transaction sync will build on this.
public class AkahuSyncService(AkahuClient akahu, DbConnectionFactory db)
{
    /// <summary>
    /// Fetches every Akahu account and upserts it into bank_connections.
    /// Returns the number of accounts received from Akahu.
    /// </summary>
    public async Task<int> SyncAccountsAsync(Guid userId, CancellationToken ct = default)
    {
        var accounts = await akahu.GetAccountsAsync(ct);
        if (accounts.Count == 0) return 0;

        const string sql = """
            INSERT INTO bank_connections
                (user_id, provider, external_account_id, name, formatted_account, bank_name,
                 account_type, current_balance, available_balance, last_synced_at)
            VALUES (@UserId, 'akahu', @ExternalAccountId, @Name, @FormattedAccount, @BankName,
                    @AccountType, @CurrentBalance, @AvailableBalance, now())
            ON CONFLICT (user_id, provider, external_account_id) DO UPDATE SET
                name              = EXCLUDED.name,
                formatted_account = EXCLUDED.formatted_account,
                bank_name         = EXCLUDED.bank_name,
                account_type      = EXCLUDED.account_type,
                current_balance   = EXCLUDED.current_balance,
                available_balance = EXCLUDED.available_balance,
                last_synced_at    = now()
            """;

        var rows = accounts.Select(a => new
        {
            UserId = userId,
            ExternalAccountId = a.Id,
            a.Name,
            a.FormattedAccount,
            BankName = a.Connection.Name,
            AccountType = a.Type,
            CurrentBalance = a.Balance.Current,
            AvailableBalance = a.Balance.Available
        }).ToList();

        await using var conn = db.CreateConnection();
        await conn.ExecuteAsync(sql, rows);
        return accounts.Count;
    }

    public async Task<IEnumerable<BankConnectionDto>> GetConnectionsAsync(Guid userId)
    {
        const string sql = """
            SELECT
                id                  AS Id,
                provider            AS Provider,
                external_account_id AS ExternalAccountId,
                name                AS Name,
                formatted_account   AS FormattedAccount,
                bank_name           AS BankName,
                account_type        AS AccountType,
                current_balance     AS CurrentBalance,
                available_balance   AS AvailableBalance,
                is_active           AS IsActive,
                last_synced_at      AS LastSyncedAt
            FROM bank_connections
            WHERE user_id = @UserId
            ORDER BY bank_name, name
            """;

        await using var conn = db.CreateConnection();
        return await conn.QueryAsync<BankConnectionDto>(sql, new { UserId = userId });
    }
}
