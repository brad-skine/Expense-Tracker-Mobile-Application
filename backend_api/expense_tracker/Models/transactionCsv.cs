namespace expense_tracker.Models
{

    using CsvHelper.Configuration.Attributes;
    public class transactionCsv
    {

        [Name("date")]
        [Format("dd/MM/yyyy")]
        public DateOnly Date { get; set; }

        [Name("transaction type")]
        public string TransactionType { get; set; } = "";

        [Name("description")]
        public string Description { get; set; } = "";

        [Name("debit/credit")]
        public string Amount { get; set; } = "";

        [Name("balance")]
        public string Balance { get; set; } = "";


    }
}
