import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { UserSignIn } from '../../interfaces/SignIn.interface';
import { StorageService } from './storage.service';
import { UserSignUp } from '../../interfaces/SignUp.interface';

const HttpOptions = {
  headers: new HttpHeaders({
    'Content-Type': 'application/json',
    'Access-Control-Allow-Origin': '*',
  }),
};

@Injectable({
  providedIn: 'root',
})
export class UserManagementService {
  constructor(private httpClient: HttpClient) {}
  private URLBase = environment.apiUrl;

  public signIn(user: UserSignIn): Observable<{ token: string }> {
    const url = this.URLBase + '/api/UserManagement/SignIn';
    return this.httpClient.post<{ token: string }>(url, user, HttpOptions);
  }
  public signUp(user: UserSignUp): Observable<any> {
    const url = this.URLBase + '/api/UserManagement/SignUp';
    return this.httpClient.post(url, user, HttpOptions);
  }
  public refreshToken(): Observable<{ token: string }> {
    const url = this.URLBase + '/api/UserManagement/RefreshToken';
    const token = localStorage.getItem('token');
    const headers = new HttpHeaders({
      'Content-Type': 'application/json',
      Authorization: `Bearer ${token}`,
    });
    return this.httpClient.post<{ token: string }>(url, {}, { headers });
  }
}
