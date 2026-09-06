import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { DailyNutrition, TrendPoint } from '../interfaces/report';

@Injectable({ providedIn: 'root' })
export class ReportApi {
  private readonly url = `${environment.ApiBaseUrl}/reports`;

  constructor(private readonly http: HttpClient) {}

  daily(from: string, to: string): Observable<DailyNutrition[]> {
    return this.http.get<DailyNutrition[]>(`${this.url}/daily`, { params: this.range(from, to) });
  }

  trends(from: string, to: string): Observable<TrendPoint[]> {
    return this.http.get<TrendPoint[]>(`${this.url}/trends`, { params: this.range(from, to) });
  }

  private range(from: string, to: string): HttpParams { return new HttpParams().set('from', from).set('to', to); }
}
