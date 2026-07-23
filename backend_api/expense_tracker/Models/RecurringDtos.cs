namespace expense_tracker.Models
{
    public record RecurringPaymentDto(
        string Description, string Category, string Cadence,
        decimal TypicalAmount, decimal LastAmount, decimal MonthlyEquivalent,
        int Occurrences, int MedianIntervalDays,
        DateOnly FirstSeen, DateOnly LastSeen, DateOnly? NextExpected,
        bool IsActive, bool PriceIncreased);

    public record RecurringSummaryDto(
        decimal TotalMonthlyCommitment, int ActiveCount, int InactiveCount,
        IEnumerable<RecurringPaymentDto> Payments);
}
