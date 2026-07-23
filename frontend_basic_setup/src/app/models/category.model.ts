export interface CategoryModel {
  id: number;
  name: string;
  displayOrder: number;
  iconKey: string;
  colorHex: string;
  isCustom: boolean;
}

export interface MerchantRuleModel {
  id: number;
  pattern: string;
  categoryId: number;
  categoryName: string;
  matchType: 'contains' | 'starts_with' | 'exact';
  priority: number;
  isGlobal: boolean;
}

export interface RuleSuggestionModel {
  pattern: string;
  occurrences: number;
  totalAmount: number;
  sampleDescription: string;
}
