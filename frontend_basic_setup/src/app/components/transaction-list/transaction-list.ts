import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TransactionModel } from 'src/app/models/transaction.model';
import { TransactionService } from 'src/app/services/transaction.service';
import { Observable } from 'rxjs';
import { toSignal } from '@angular/core/rxjs-interop';


@Component({
  selector: 'app-transaction-list',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './transaction-list.html',
  styleUrls: ['./transaction-list.scss'],
})


export class TransactionListComponent  {
  // transactions: TransactionModel[] = [];
  private transactionService = inject(TransactionService); 

  transactions = toSignal(this.transactionService.getAllTransactions(), {initialValue: []});

}
