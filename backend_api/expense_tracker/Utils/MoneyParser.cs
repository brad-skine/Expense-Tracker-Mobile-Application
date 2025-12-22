namespace expense_tracker.Utils
{
    public static class MoneyParser
    {
        public static decimal ParseMoney(string moneyString)
        {

            if (string.IsNullOrWhiteSpace(moneyString))
            {
                throw new ArgumentException("Input string cannot be null or empty", nameof(moneyString));
            }
            // Remove any currency symbols and commas
            var cleanedString = moneyString.Replace("$", "").Replace(",", "").Trim();
            // Parse the cleaned string to decimal
            if (decimal.TryParse(cleanedString, out var result))
            {
                return result;
            }
            else
            {
                throw new FormatException($"Invalid money format: {moneyString}");
            }
        }
    }
}
