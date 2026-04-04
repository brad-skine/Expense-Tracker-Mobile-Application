import {inject, Injectable} from '@angular/core';
import {HttpClient} from '@angular/common/http';
import {BehaviorSubject, Observable, switchMap} from 'rxjs';
import {CategorySummaryModel} from 'src/app/models/category_summary.model';
import {environment} from 'src/environments/environment';

@Injectable({ providedIn: 'root' })
export class CategorySummaryService {
    private apiUrl = environment.apiUrl + '/api/transactions/summary/category';
    private http = inject(HttpClient);
    private refresh$ = new BehaviorSubject<void>(undefined);

    getAllCategorySummaries(): Observable<CategorySummaryModel[]> {
        return this.refresh$.pipe(
            switchMap(() => this.http.get<CategorySummaryModel[]>(this.apiUrl))
        );
    }

    triggerRefresh() {
        this.refresh$.next();
    }
}