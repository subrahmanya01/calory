import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { HealthGoal, HealthGoalRequest } from '../interfaces/health-goal';
import { PagedResponse } from '../interfaces/paged-response';

@Injectable({ providedIn: 'root' })
export class HealthGoalApi {
  private readonly url = `${environment.ApiBaseUrl}/goals`;

  constructor(private readonly http: HttpClient) {}

  getAll(page = 1, pageSize = 20): Observable<PagedResponse<HealthGoal>> {
    return this.http.get<PagedResponse<HealthGoal>>(this.url, { params: { page, pageSize } });
  }
  create(request: HealthGoalRequest): Observable<HealthGoal> { return this.http.post<HealthGoal>(this.url, request); }
  update(id: string, request: HealthGoalRequest): Observable<HealthGoal> {
    return this.http.put<HealthGoal>(`${this.url}/${id}`, { id, ...request });
  }
}
