import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, switchMap } from 'rxjs';
import { TransactionModel } from '../../models/transaction.model';
import { environment } from 'src/environments/environment';


@Injectable({providedIn: 'root',})
  export class TransactionService {
    private apiUrl = environment.apiUrl + "/api/transactions/all";

    private http = inject(HttpClient)
    private  refresh$ = new BehaviorSubject<void>(undefined)

    getAllTransactions():Observable<TransactionModel[]> {
      return this.refresh$.pipe(
        switchMap(() =>
          this.http.get<TransactionModel[]>(this.apiUrl)
      ));
    }



    triggerRefresh() {
      this.refresh$.next();
    }
  }
