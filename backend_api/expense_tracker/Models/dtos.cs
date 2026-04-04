namespace expense_tracker.Models
{
    public record YearlySummaryDto(int Year, decimal Income,
        decimal Expense
    );
    public record MonthlySummaryDto(int Year, int Month,
        decimal Income, decimal Expense
    );
    public record TypeSummaryDto(string TransactionType, decimal Total);
    
    public record CategorySummaryDto(string Category, decimal Total);
 
    public record CategoryDto(int Id, string Name, int DisplayOrder);
}

