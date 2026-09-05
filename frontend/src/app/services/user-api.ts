import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { RegisterRequest, UpdateUserRequest, User } from '../interfaces/user';
import { LoginResponse } from '../interfaces/auth';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root',
})
export class UserApi {
  private readonly baseUrl = environment.ApiBaseUrl;

  constructor(private readonly http: HttpClient) {}

  register(request: RegisterRequest): Observable<User> {
    return this.http.post<User>(`${this.baseUrl}/users`, request);
  }

  login(email: string, password: string): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.baseUrl}/auth/login`, { email, password });
  }

  currentUser(): Observable<User> {
    return this.http.get<User>(`${this.baseUrl}/users/me`);
  }

  update(request: UpdateUserRequest): Observable<User> {
    return this.http.put<User>(`${this.baseUrl}/users/me`, request);
  }

  delete(): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/users/me`);
  }
}
