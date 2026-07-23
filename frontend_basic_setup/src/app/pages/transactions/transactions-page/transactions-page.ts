import {Component, computed, inject, Signal, signal} from '@angular/core';
import {CommonModule} from '@angular/common';
import {toSignal} from '@angular/core/rxjs-interop';
import {TransactionModel} from "../../../models/transaction.model";
import {TransactionFormComponent} from "../../../components/transaction-form/transaction-form";
import {TransactionService} from "../../../components/transaction-list/transaction.service";

type SortKey = 'date' | 'amount' | 'description';
type SortDir = 'asc' | 'desc';

@Component({
  selector: 'app-transactions-page',
  standalone: true,
  imports: [CommonModule, TransactionFormComponent],
  templateUrl: './transactions-page.html',
  styleUrl: './transactions-page.scss',
})
export class TransactionsPageComponent {
  private transactionService = inject(TransactionService);

  sortKey = signal<SortKey>('date');
  sortDir = signal<SortDir>('desc');

  // modal state: closed | adding | editing a transaction
  formOpen = signal(false);
  editingTx = signal<TransactionModel | null>(null);

  private allTransactions:Signal<TransactionModel[]> = toSignal(
      this.transactionService.getAllTransactions(),
      { initialValue: [] as TransactionModel[] }
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

  openAdd() {
    this.editingTx.set(null);
    this.formOpen.set(true);
  }

  openEdit(tx: TransactionModel) {
    this.editingTx.set(tx);
    this.formOpen.set(true);
  }

  closeForm() {
    this.formOpen.set(false);
    this.editingTx.set(null);
  }
}
