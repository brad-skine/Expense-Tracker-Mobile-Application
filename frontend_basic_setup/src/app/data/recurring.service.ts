import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { shareReplay, switchMap } from 'rxjs';
import { toSignal } from '@angular/core/rxjs-interop';
import { environment } from 'src/environments/environment';
import { RefreshService } from './refresh-service';
import { RecurringSummaryModel } from '../models/recurring.model';

@Injectable({ providedIn: 'root' })
export class RecurringService {
  private http = inject(HttpClient);
  private refreshService = inject(RefreshService);
  private apiUrl = environment.apiUrl + '/api/transactions/recurring';

  readonly summary$ = this.refreshService.refresh$.pipe(
    switchMap(() => this.http.get<RecurringSummaryModel>(this.apiUrl)),
    shareReplay(1)
  );

  readonly summary = toSignal(this.summary$, {
    initialValue: {
      totalMonthlyCommitment: 0,
      activeCount: 0,
      inactiveCount: 0,
      payments: []
    } as RecurringSummaryModel
  });
}
