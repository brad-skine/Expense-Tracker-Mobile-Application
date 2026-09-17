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
