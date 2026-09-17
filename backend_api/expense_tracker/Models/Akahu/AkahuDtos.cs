using System.Text.Json.Serialization;

namespace expense_tracker.Models.Akahu;

// Response wrappers
public record AkahuListResponse<T>(
    [property: JsonPropertyName("success")] bool Success,
    [property: JsonPropertyName("items")] List<T> Items,
    [property: JsonPropertyName("cursor")] AkahuCursor? Cursor);

public record AkahuItemResponse<T>(
    [property: JsonPropertyName("success")] bool Success,
    [property: JsonPropertyName("item")] T Item);

public record AkahuCursor(
    [property: JsonPropertyName("next")] string? Next);

// GET /me
public record AkahuMe(
    [property: JsonPropertyName("_id")] string Id,
    [property: JsonPropertyName("access_granted_at")] DateTime AccessGrantedAt,
    [property: JsonPropertyName("email")] string? Email);

// GET /accounts
public record AkahuAccount(
    [property: JsonPropertyName("_id")] string Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("formatted_account")] string? FormattedAccount,
    [property: JsonPropertyName("connection")] AkahuConnection Connection,
    [property: JsonPropertyName("balance")] AkahuBalance Balance,
    [property: JsonPropertyName("attributes")] List<string> Attributes,
    [property: JsonPropertyName("refreshed")] AkahuRefreshed? Refreshed);

public record AkahuConnection(
    [property: JsonPropertyName("_id")] string Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("logo")] string? Logo);

public record AkahuBalance(
    [property: JsonPropertyName("currency")] string Currency,
    [property: JsonPropertyName("current")] decimal Current,
    [property: JsonPropertyName("available")] decimal? Available,
    [property: JsonPropertyName("limit")] decimal? Limit,
    [property: JsonPropertyName("overdrawn")] bool? Overdrawn);

public record AkahuRefreshed(
    [property: JsonPropertyName("balance")] DateTime? Balance,
    [property: JsonPropertyName("transactions")] DateTime? Transactions);

// GET /accounts/{id}/transactions
public record AkahuTransaction(
    [property: JsonPropertyName("_id")] string Id,
    [property: JsonPropertyName("_account")] string AccountId,
    [property: JsonPropertyName("_connection")] string ConnectionId,
    [property: JsonPropertyName("date")] DateTime Date,
    [property: JsonPropertyName("description")] string Description,
    [property: JsonPropertyName("amount")] decimal Amount,
    [property: JsonPropertyName("balance")] decimal? Balance,
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("merchant")] AkahuMerchant? Merchant,
    [property: JsonPropertyName("category")] AkahuCategory? Category,
    [property: JsonPropertyName("meta")] AkahuTransactionMeta? Meta);

public record AkahuMerchant(
    [property: JsonPropertyName("_id")] string Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("website")] string? Website);

public record AkahuCategory(
    [property: JsonPropertyName("_id")] string Id,
    [property: JsonPropertyName("name")] string Name);

public record AkahuTransactionMeta(
    [property: JsonPropertyName("particulars")] string? Particulars,
    [property: JsonPropertyName("code")] string? Code,
    [property: JsonPropertyName("reference")] string? Reference,
    [property: JsonPropertyName("other_account")] string? OtherAccount,
    [property: JsonPropertyName("card_suffix")] string? CardSuffix,
    [property: JsonPropertyName("logo")] string? Logo);
