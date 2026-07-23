export interface BudgetProgressModel {
    categoryId: number;
    category: string;
    monthlyLimit: number;   // 0 = no budget set
    spent: number;
}
