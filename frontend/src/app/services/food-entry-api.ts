import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import {
  FoodEntry,
  FoodEntryQuery,
  FoodEntryRequest,
  ImportFoodEntriesResponse,
} from '../interfaces/food-entry';
import { PagedResponse } from '../interfaces/paged-response';

@Injectable({ providedIn: 'root' })
export class FoodEntryApi {
  private readonly url = `${environment.ApiBaseUrl}/food-entries`;

  constructor(private readonly http: HttpClient) {}

  list(query: FoodEntryQuery = {}): Observable<PagedResponse<FoodEntry>> {
    let params = new HttpParams();
    if (query.from) params = params.set('from', query.from);
    if (query.to) params = params.set('to', query.to);
    if (query.mealType) params = params.set('mealType', query.mealType);
    if (query.minCalories !== undefined) params = params.set('minCalories', query.minCalories);
    if (query.maxCalories !== undefined) params = params.set('maxCalories', query.maxCalories);
    params = params.set('page', query.page ?? 1).set('pageSize', query.pageSize ?? 20);
    return this.http.get<PagedResponse<FoodEntry>>(this.url, { params });
  }

  create(request: FoodEntryRequest): Observable<FoodEntry> {
    return this.http.post<FoodEntry>(this.url, request);
  }

  update(id: string, request: FoodEntryRequest): Observable<FoodEntry> {
    return this.http.put<FoodEntry>(`${this.url}/${id}`, { id, ...request });
  }

  remove(id: string): Observable<void> {
    return this.http.delete<void>(`${this.url}/${id}`);
  }
  
  importPdf(file: File): Observable<ImportFoodEntriesResponse> {
    const formData = new FormData();
    formData.append('file', file, file.name);
    return this.http.post<ImportFoodEntriesResponse>(`${this.url}/import-pdf`, formData);
  }
}
