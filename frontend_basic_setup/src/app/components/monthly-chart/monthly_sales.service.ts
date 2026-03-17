import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import {BehaviorSubject, Observable, Subject, switchMap} from 'rxjs';
import { MonthlySalesModel } from '../../models/monthly_sale.model';
import { environment } from 'src/environments/environment';
import {RefreshService} from "../../data/refresh-service";

@Injectable({providedIn: 'root',})
// TODO: use signals instead of manual trigger refresh
  export class monthlySalesService {
    private apiUrl = "/api/transactions/summary/monthly";
    private http = inject(HttpClient);

    // private refresh$ = new BehaviorSubject<void>(undefined);
    private refreshService = inject(RefreshService);

    getAllMonthlySales(): Observable<MonthlySalesModel[]> {
      return this.refreshService.refresh$.pipe(
          switchMap(() =>this.http.get<MonthlySalesModel[]>(this.apiUrl))
    );

    }

  }
