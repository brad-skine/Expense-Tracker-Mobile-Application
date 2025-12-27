import { ChangeDetectorRef, Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TransactionModel } from 'src/app/models/transaction.model';
import { TransactionService } from 'src/app/services/transaction.service';
import { Observable } from 'rxjs';


@Component({
  selector: 'app-transaction-list',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './transaction-list.html',
  styleUrls: ['./transaction-list.scss'],
})


export class TransactionListComponent  {
  transactions: TransactionModel[] = [];
  private transactionService = inject(TransactionService); 
  loading = true;
  error: string | null = null; // so can check if error then display it

  transactions$: Observable<TransactionModel[]> = this.transactionService.getAllTransactions();
}
