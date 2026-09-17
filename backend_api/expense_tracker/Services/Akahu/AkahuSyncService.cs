using Dapper;
using System.Globalization;
using expense_tracker.Models;
using expense_tracker.Models.Akahu;
using expense_tracker.Utils;

namespace expense_tracker.Services.Akahu;

// Pulls Akahu accounts into bank_connections and Akahu transactions into transactions.
public class AkahuSyncService(AkahuClient akahu, DbConnectionFactory db, CategoryClassifierService classifier)
{
    private static readonly TimeZoneInfo NzTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Pacific/Auckland");
    private static readonly TimeSpan DefaultWindow = TimeSpan.FromDays(90);
    private static readonly TimeSpan ResyncOverlap = TimeSpan.FromDays(3);

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
                 account_type, current_balance, available_balance)
            VALUES (@UserId, 'akahu', @ExternalAccountId, @Name, @FormattedAccount, @BankName,
                    @AccountType, @CurrentBalance, @AvailableBalance)
            ON CONFLICT (user_id, provider, external_account_id) DO UPDATE SET
                name              = EXCLUDED.name,
                formatted_account = EXCLUDED.formatted_account,
                bank_name         = EXCLUDED.bank_name,
                account_type      = EXCLUDED.account_type,
                current_balance   = EXCLUDED.current_balance,
                available_balance = EXCLUDED.available_balance
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

    /// <summary>
    /// Fetches transactions for every active Akahu connection and inserts the ones not seen before
    /// (dedup on user_id + external_id). Window per account: since ?? last_synced_at - overlap ?? 90 days ago.
    /// </summary>
    public async Task<TransactionSyncResultDto> SyncTransactionsAsync(
        Guid userId, DateTime? since, CancellationToken ct = default)
    {
        const string connectionsSql = """
            SELECT
                id                  AS Id,
                external_account_id AS ExternalAccountId,
                name                AS Name,
                last_synced_at      AS LastSyncedAt
            FROM bank_connections
            WHERE user_id = @UserId AND provider = 'akahu' AND is_active
            ORDER BY id
            """;

        const string insertSql = """
            INSERT INTO transactions
                (user_id, transaction_date, transaction_type, description, amount, balance, category,
                 external_id, bank_connection_id)
            VALUES (@UserId, @Date, @TransactionType, @Description, @Amount, @Balance, @Category,
                    @ExternalId, @BankConnectionId)
            ON CONFLICT (user_id, external_id) WHERE external_id IS NOT NULL DO NOTHING
            """;

        const string touchSql = """
            UPDATE bank_connections SET last_synced_at = now()
            WHERE user_id = @UserId AND id = ANY(@Ids)
            """;

        await using var conn = db.CreateConnection();
        var connections = (await conn.QueryAsync<SyncConnection>(connectionsSql, new { UserId = userId })).ToList();
        if (connections.Count == 0) return new TransactionSyncResultDto(0, 0, []);

        var now = DateTime.UtcNow;
        var batches = new List<(SyncConnection Conn, List<SyncRow> Rows)>();

        foreach (var c in connections)
        {
            var start = since
                ?? c.LastSyncedAt?.Subtract(ResyncOverlap)
                ?? now.Subtract(DefaultWindow);

            var fetched = await akahu.GetAccountTransactionsAsync(c.ExternalAccountId, start, now, ct);
            batches.Add((c, fetched.Select(t => ToRow(t, userId, c.Id)).ToList()));
        }

        // Classify everything in memory: rules are loaded once, not per row.
        await classifier.ClassifyManyAsync(batches.SelectMany(b => b.Rows).Select(r => r.Txn), userId);

        var perAccount = new List<AccountSyncResultDto>();
        foreach (var (c, rows) in batches)
        {
            var inserted = rows.Count == 0 ? 0 : await conn.ExecuteAsync(insertSql, rows.Select(r => new
            {
                UserId = userId,
                r.Txn.Date,
                r.Txn.TransactionType,
                r.Txn.Description,
                r.Txn.Amount,
                r.Txn.Balance,
                r.Txn.Category,
                r.ExternalId,
                BankConnectionId = c.Id
            }).ToList());

            perAccount.Add(new AccountSyncResultDto(c.Id, c.ExternalAccountId, c.Name, rows.Count, inserted));
        }

        await conn.ExecuteAsync(touchSql, new { UserId = userId, Ids = connections.Select(c => c.Id).ToArray() });

        return new TransactionSyncResultDto(
            perAccount.Sum(a => a.Fetched),
            perAccount.Sum(a => a.Inserted),
            perAccount);
    }

    private static SyncRow ToRow(AkahuTransaction t, Guid userId, int connectionId)
    {
        var utc = DateTime.SpecifyKind(t.Date, DateTimeKind.Utc);
        var nzLocal = TimeZoneInfo.ConvertTimeFromUtc(utc, NzTimeZone);

        var txn = new Transaction
        {
            UserId = userId,
            Date = DateOnly.FromDateTime(nzLocal),
            TransactionType = MapType(t.Type),
            Description = t.Merchant?.Name ?? t.Description,
            Amount = t.Amount,
            Balance = t.Balance ?? 0
        };
        return new SyncRow(txn, t.Id);
    }

    // Akahu types are upper-case (EFTPOS, PAYMENT, DIRECT CREDIT, FEE, ...).
    // Map onto the values CategoryClassifierService already keys on; title-case the rest.
    private static string MapType(string akahuType) => akahuType.ToUpperInvariant() switch
    {
        "FEE" => "Fee",
        "CREDIT" or "DIRECT CREDIT" or "INTEREST" => "Deposit",
        var other => CultureInfo.InvariantCulture.TextInfo.ToTitleCase(other.ToLowerInvariant())
    };

    private record SyncConnection(int Id, string ExternalAccountId, string? Name, DateTime? LastSyncedAt);
    private record SyncRow(Transaction Txn, string ExternalId);
}
