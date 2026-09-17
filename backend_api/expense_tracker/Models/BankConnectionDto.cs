namespace expense_tracker.Models;

public record BankConnectionDto(
    int Id,
    string Provider,
    string ExternalAccountId,
    string? Name,
    string? FormattedAccount,
    string? BankName,
    string? AccountType,
    decimal? CurrentBalance,
    decimal? AvailableBalance,
    bool IsActive,
    DateTime? LastSyncedAt);

public record AccountSyncResultDto(
    int ConnectionId,
    string ExternalAccountId,
    string? Name,
    int Fetched,
    int Inserted);

public record TransactionSyncResultDto(
    int Fetched,
    int Inserted,
    List<AccountSyncResultDto> PerAccount);
