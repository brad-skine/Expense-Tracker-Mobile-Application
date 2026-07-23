namespace expense_tracker.Models
{
    // ---- Manual transaction CRUD ----
    public record CreateTransactionRequest(
        DateTime Date,
        string Description,
        decimal Amount,
        string? Category      // null/empty = auto-classify via merchant rules
    );

    public record UpdateTransactionRequest(
        DateTime Date,
        string Description,
        decimal Amount,
        string Category
    );

    // ---- Budgets / expense planning ----
    public record UpsertBudgetRequest(int CategoryId, decimal MonthlyLimit);

    public record BudgetProgressDto(
        int CategoryId,
        string Category,
        decimal MonthlyLimit,   // 0 = no budget set
        decimal Spent           // actual spend for the requested month
    );
}
