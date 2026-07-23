import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, shareReplay, switchMap } from 'rxjs';
import { TransactionModel } from '../../models/transaction.model';
import { environment } from 'src/environments/environment';

export interface CategoryModel {
    id: number;
    name: string;
    displayOrder: number;
}

export interface TransactionUpsert {
    date: string;        // yyyy-MM-dd
    description: string;
    amount: number;
    category: string | null;
}

@Injectable({ providedIn: 'root' })
export class TransactionService {
    private baseUrl = environment.apiUrl + '/api/transactions';

    private http = inject(HttpClient);
    private refresh$ = new BehaviorSubject<void>(undefined);

    // ONE shared stream for the whole app. shareReplay(1) means every
    // component (list, D3 chart, planning...) gets the cached result
    // instantly instead of each firing its own HTTP request.
    private readonly transactions$: Observable<TransactionModel[]> = this.refresh$.pipe(
        switchMap(() => this.http.get<TransactionModel[]>(`${this.baseUrl}/all`)),
        shareReplay({ bufferSize: 1, refCount: false })
    );

    // Categories basically never change -> fetch once, cache forever
    readonly categories$: Observable<CategoryModel[]> = this.http
        .get<CategoryModel[]>(`${this.baseUrl}/categories`)
        .pipe(shareReplay({ bufferSize: 1, refCount: false }));

    getAllTransactions(): Observable<TransactionModel[]> {
        return this.transactions$;
    }

    // ---- CRUD ----
    addTransaction(tx: TransactionUpsert): Observable<{ id: number }> {
        return this.http.post<{ id: number }>(this.baseUrl, tx);
    }

    updateTransaction(id: number, tx: TransactionUpsert): Observable<void> {
        return this.http.put<void>(`${this.baseUrl}/${id}`, tx);
    }

    deleteTransaction(id: number): Observable<void> {
        return this.http.delete<void>(`${this.baseUrl}/${id}`);
    }

    triggerRefresh() {
        this.refresh$.next();
    }
}
