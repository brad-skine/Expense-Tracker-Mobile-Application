import { computed, inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, shareReplay, switchMap, tap } from 'rxjs';
import { toSignal } from '@angular/core/rxjs-interop';
import { environment } from 'src/environments/environment';
import { CategoryModel, MerchantRuleModel, RuleSuggestionModel } from '../models/category.model';
import { CATEGORY_ICONS } from './category-icons';
import { RefreshService } from './refresh-service';

@Injectable({ providedIn: 'root' })
export class CategoryService {
  private http = inject(HttpClient);
  private refreshService = inject(RefreshService);
  private apiUrl = environment.apiUrl + '/api/categories';
  
  private refreshTrigger$ = new BehaviorSubject<void>(undefined);
  
  readonly categories$ = this.refreshTrigger$.pipe(
    switchMap(() => this.http.get<CategoryModel[]>(this.apiUrl)),
    shareReplay(1)
  );
  
  readonly categories = toSignal(this.categories$, { initialValue: [] as CategoryModel[] });
  
  readonly styleByName = computed(() => {
    const map = new Map<string, { color: string; iconPath: string }>();
    this.categories().forEach(c => {
      map.set(c.name, {
        color: c.colorHex,
        iconPath: CATEGORY_ICONS[c.iconKey] || CATEGORY_ICONS['tag']
      });
    });
    return map;
  });

  refresh() {
    this.refreshTrigger$.next();
  }

  private notifyAndRefresh<T>(obs: Observable<T>): Observable<T> {
    return obs.pipe(
      tap(() => {
        this.refresh();
        this.refreshService.refresh$.next();
      })
    );
  }

  createCategory(category: Partial<CategoryModel>): Observable<CategoryModel> {
    return this.notifyAndRefresh(this.http.post<CategoryModel>(this.apiUrl, category));
  }

  updateCategory(id: number, category: Partial<CategoryModel>): Observable<void> {
    return this.notifyAndRefresh(this.http.put<void>(`${this.apiUrl}/${id}`, category));
  }

  deleteCategory(id: number): Observable<void> {
    return this.notifyAndRefresh(this.http.delete<void>(`${this.apiUrl}/${id}`));
  }

  getRules(): Observable<MerchantRuleModel[]> {
    return this.http.get<MerchantRuleModel[]>(`${this.apiUrl}/rules`);
  }

  createRule(rule: Partial<MerchantRuleModel>): Observable<MerchantRuleModel> {
    return this.notifyAndRefresh(this.http.post<MerchantRuleModel>(`${this.apiUrl}/rules`, rule));
  }

  updateRule(id: number, rule: Partial<MerchantRuleModel>): Observable<void> {
    return this.notifyAndRefresh(this.http.put<void>(`${this.apiUrl}/rules/${id}`, rule));
  }

  deleteRule(id: number): Observable<void> {
    return this.notifyAndRefresh(this.http.delete<void>(`${this.apiUrl}/rules/${id}`));
  }

  getSuggestions(): Observable<RuleSuggestionModel[]> {
    return this.http.get<RuleSuggestionModel[]>(`${this.apiUrl}/suggestions`);
  }

  reclassify(): Observable<void> {
    return this.notifyAndRefresh(this.http.post<void>(`${environment.apiUrl}/api/transactions/reclassify`, {}));
  }
}
