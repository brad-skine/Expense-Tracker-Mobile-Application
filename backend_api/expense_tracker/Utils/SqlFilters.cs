namespace expense_tracker.Utils
{
    public static class SqlFilters
    {
        // Optional bank account filter for transaction reads.
        // Null/empty => no predicate (CSV rows with NULL bank_connection_id stay included).
        public static (string Sql, int[]? Ids) AccountFilter(IReadOnlyCollection<int>? accountIds)
            => accountIds is { Count: > 0 }
                ? (" AND bank_connection_id = ANY(@AccountIds)", accountIds.ToArray())
                : ("", null);
    }
}
