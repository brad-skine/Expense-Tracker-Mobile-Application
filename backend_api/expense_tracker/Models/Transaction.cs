namespace expense_tracker.Models
{
    public class Transaction 
    {
        public DateOnly Date { get; set; }
        public string TransactionType { get; set; } = "";
        public string Description { get; set; } = "";
        public decimal Amount { get; set; }
        public decimal Balance { get; set; }
    }
}
