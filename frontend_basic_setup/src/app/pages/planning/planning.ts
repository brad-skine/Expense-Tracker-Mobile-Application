import {Component, computed, inject, signal} from '@angular/core';
import {CommonModule} from '@angular/common';
import {RouterLink} from '@angular/router';
import {FormsModule} from '@angular/forms';
import {toObservable, toSignal} from '@angular/core/rxjs-interop';
import {combineLatest, switchMap} from 'rxjs';
import {RefreshService} from '../../data/refresh-service';
import {BudgetProgressModel} from '../../models/budget.model';
import {BudgetService} from "../../services/budget.service";
import {RecurringService} from '../../data/recurring.service';
import {CategoryIconComponent} from '../../components/category-icon/category-icon';

export type PlanningTab = 'budgets' | 'recurring';

@Component({
  selector: 'app-planning',
  standalone: true,
  imports: [CommonModule, FormsModule, CategoryIconComponent, RouterLink],
  templateUrl: './planning.html',
  styleUrl: './planning.scss',
})
export class PlanningComponent {
  private budgetService = inject(BudgetService);
  private refreshService = inject(RefreshService);
  private recurringService = inject(RecurringService);

  activeTab = signal<PlanningTab>('budgets');
  expandedRecurring = signal<string | null>(null); // description as key?

  readonly planningTabs: { key: PlanningTab; label: string }[] = [
    { key: 'budgets', label: 'Budgets' },
    { key: 'recurring', label: 'Recurring' },
  ];

  // month being viewed
  viewDate = signal(new Date());
  monthLabel = computed(() =>
      this.viewDate().toLocaleDateString('en-NZ', { month: 'long', year: 'numeric' }));

  // refetch whenever the month changes OR data refreshes (import / CRUD)
  budgets = toSignal(
      combineLatest([toObservable(this.viewDate), this.refreshService.refresh$]).pipe(
          switchMap(([date]) =>
              this.budgetService.getProgress(date.getFullYear(), date.getMonth() + 1))
      ),
      { initialValue: [] as BudgetProgressModel[] }
  );

  totalBudget = computed(() =>
      this.budgets().reduce((sum, b) => sum + b.monthlyLimit, 0));
  totalSpent = computed(() =>
      this.budgets().reduce((sum, b) => sum + b.spent, 0));

  recurringSummary = this.recurringService.summary;

  activeRecurring = computed(() =>
      [...this.recurringSummary().payments]
          .filter(p => p.isActive)
          .sort((a, b) => b.monthlyEquivalent - a.monthlyEquivalent)
  );

  inactiveRecurring = computed(() =>
      [...this.recurringSummary().payments]
          .filter(p => !p.isActive)
          .sort((a, b) => new Date(b.lastSeen).getTime() - new Date(a.lastSeen).getTime())
  );

  // which category row is in edit mode, and its draft value
  editingId = signal<number | null>(null);
  draftLimit = signal<number | null>(null);

  changeMonth(delta: number) {
    const d = new Date(this.viewDate());
    d.setDate(1);                       // avoid 31st -> skips-a-month issues
    d.setMonth(d.getMonth() + delta);
    this.viewDate.set(d);
  }

  startEdit(b: BudgetProgressModel) {
    this.editingId.set(b.categoryId);
    this.draftLimit.set(b.monthlyLimit || null);
  }

  saveEdit(b: BudgetProgressModel) {
    const limit = this.draftLimit() ?? 0;
    this.budgetService.setBudget(b.categoryId, limit).subscribe({
      next: () => {
        this.editingId.set(null);
        this.refreshService.triggerRefresh();
      },
    });
  }

  cancelEdit() {
    this.editingId.set(null);
  }

  setTab(tab: PlanningTab) {
    this.activeTab.set(tab);
  }

  toggleExpand(desc: string) {
    this.expandedRecurring.update(current => current === desc ? null : desc);
  }

  getRelativeTime(dateStr: string): string {
    const date = new Date(dateStr);
    const now = new Date();
    const diffMs = now.getTime() - date.getTime();
    const diffMonths = Math.floor(diffMs / (1000 * 60 * 60 * 24 * 30.44));
    if (diffMonths <= 0) return 'this month';
    if (diffMonths === 1) return '1 month ago';
    return `${diffMonths} months ago`;
  }

  percent(b: BudgetProgressModel): number {
    if (b.monthlyLimit <= 0) return 0;
    return Math.min(100, (b.spent / b.monthlyLimit) * 100);
  }

  barClass(b: BudgetProgressModel): string {
    if (b.monthlyLimit <= 0) return 'none';
    const pct = b.spent / b.monthlyLimit;
    if (pct >= 1) return 'over';
    if (pct >= 0.8) return 'warn';
    return 'ok';
  }
}
