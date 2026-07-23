import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { BudgetProgressModel } from '../models/budget.model';
import { environment } from 'src/environments/environment';

@Injectable({ providedIn: 'root' })
export class BudgetService {
    private baseUrl = environment.apiUrl + '/api/budgets';
    private http = inject(HttpClient);

    getProgress(year: number, month: number): Observable<BudgetProgressModel[]> {
        return this.http.get<BudgetProgressModel[]>(
            `${this.baseUrl}/progress`, { params: { year, month } });
    }

    // monthlyLimit 0 clears the budget
    setBudget(categoryId: number, monthlyLimit: number): Observable<void> {
        return this.http.put<void>(this.baseUrl, { categoryId, monthlyLimit });
    }
}
