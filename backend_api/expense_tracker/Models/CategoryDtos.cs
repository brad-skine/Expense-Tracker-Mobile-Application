namespace expense_tracker.Models;

public record CategoryDto(int Id, string Name, int DisplayOrder, string IconKey, string ColorHex, bool IsCustom);

public record CreateCategoryRequest(string Name, string IconKey, string ColorHex);

public record UpdateCategoryRequest(string Name, string IconKey, string ColorHex, int DisplayOrder);

public record MerchantRuleDto(int Id, string Pattern, int CategoryId, string CategoryName, string MatchType, int Priority, bool IsGlobal);

public record UpsertRuleRequest(string Pattern, int CategoryId, string MatchType);

public record RuleSuggestionDto(string Pattern, int Occurrences, decimal TotalAmount, string SampleDescription);
