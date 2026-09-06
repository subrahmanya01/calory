import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { FoodEntry, FoodEntryRequest } from '../interfaces/food-entry';

@Injectable({ providedIn: 'root' })
export class FoodEntryApi {
  private readonly url = `${environment.ApiBaseUrl}/food-entries`;

  constructor(private readonly http: HttpClient) {}

  list(from?: string, to?: string): Observable<FoodEntry[]> {
    let params = new HttpParams();
    if (from) params = params.set('from', from);
    if (to) params = params.set('to', to);
    return this.http.get<FoodEntry[]>(this.url, { params });
  }

  create(request: FoodEntryRequest): Observable<FoodEntry> { return this.http.post<FoodEntry>(this.url, request); }
  update(id: string, request: FoodEntryRequest): Observable<FoodEntry> {
    return this.http.put<FoodEntry>(`${this.url}/${id}`, { id, ...request });
  }
  remove(id: string): Observable<void> { return this.http.delete<void>(`${this.url}/${id}`); }
}
