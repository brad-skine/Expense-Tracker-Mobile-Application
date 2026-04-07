import {Component, computed, inject, signal} from '@angular/core';
import {CommonModule} from '@angular/common';
import {toSignal} from '@angular/core/rxjs-interop';
import {TransactionService} from "../../../components/transaction-list/transaction.service";

type SortKey = 'date' | 'amount' | 'description';
type SortDir = 'asc' | 'desc';

@Component({
  selector: 'app-transactions-page',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './transactions-page.html',
  styleUrl: './transactions-page.scss',
})
export class TransactionsPageComponent {
  private transactionService = inject(TransactionService);

  sortKey = signal<SortKey>('date');
  sortDir = signal<SortDir>('desc');

  private allTransactions = toSignal(
      this.transactionService.getAllTransactions(),
      { initialValue: [] }
  );

  transactions = computed(() => {
    const key = this.sortKey();
    const dir = this.sortDir();
    return [...this.allTransactions()].sort((a, b) => {
      let cmp = 0;
      if (key === 'date') {
        cmp = new Date(a.date).getTime() - new Date(b.date).getTime();
      } else if (key === 'amount') {
        cmp = Math.abs(a.amount) - Math.abs(b.amount);
      } else if (key === 'description') {
        cmp = a.description.localeCompare(b.description);
      }
      return dir === 'asc' ? cmp : -cmp;
    });
  });

  setSort(key: SortKey) {
    if (this.sortKey() === key) {
      this.sortDir.update(d => d === 'asc' ? 'desc' : 'asc');
    } else {
      this.sortKey.set(key);
      this.sortDir.set('desc');
    }
  }
}
