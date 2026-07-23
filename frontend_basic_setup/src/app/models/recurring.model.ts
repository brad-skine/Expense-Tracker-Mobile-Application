export interface RecurringPaymentModel {
  description: string;
  category: string;
  cadence: string;
  typicalAmount: number;
  lastAmount: number;
  monthlyEquivalent: number;
  occurrences: number;
  medianIntervalDays: number;
  firstSeen: string;
  lastSeen: string;
  nextExpected: string | null;
  isActive: boolean;
  priceIncreased: boolean;
}

export interface RecurringSummaryModel {
  totalMonthlyCommitment: number;
  activeCount: number;
  inactiveCount: number;
  payments: RecurringPaymentModel[];
}
