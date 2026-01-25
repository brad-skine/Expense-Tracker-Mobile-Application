import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import {Observable, Subject, switchMap} from 'rxjs';
import { MonthlySalesModel } from '../models/monthly_sale.model';
import { environment } from 'src/environments/environment';

@Injectable({providedIn: 'root',})
// TODO: use signals instead of manual trigger refresh
  export class monthlySalesService {
    private apiUrl = environment.apiUrl + "/api/transactions/summary/monthly";
    private http = inject(HttpClient);

    private refresh$ = new Subject<void>

    getAllMonthlySales(): Observable<MonthlySalesModel[]> {
      return this.refresh$.pipe(
          switchMap(() =>this.http.get<MonthlySalesModel[]>(this.apiUrl))
    );

    }

    triggerRefresh() {
      this.refresh$.next();
    }
  }
