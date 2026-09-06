import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { FoodAnalysisResponse } from '../interfaces/food-entry';

@Injectable({ providedIn: 'root' })
export class FoodAnalysisApi {
  private readonly url = `${environment.ApiBaseUrl}/image-analysis/food`;

  constructor(private readonly http: HttpClient) {}

  analyze(image: File): Observable<FoodAnalysisResponse> {
    const form = new FormData();
    form.append('Image', image, image.name);
    return this.http.post<FoodAnalysisResponse>(this.url, form);
  }
}
