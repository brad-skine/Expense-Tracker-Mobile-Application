import { Injectable } from '@angular/core';
import {BehaviorSubject} from "rxjs";

@Injectable({
  providedIn: 'root',
})
export class RefreshService {
  readonly refresh$ = new BehaviorSubject<void>(undefined);

  triggerRefresh() {
    this.refresh$.next();
  }
}
