import { HttpClient,HttpParams, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Injectable, signal } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class ApiService {

  private baseUrl = 'https://localhost:7163/api';
  
  // السجنلز اللي الـ Interceptor هيتحكم فيها أوتوماتيك
  isLoading = signal(false);
  errorMessage = signal('');
  
  private authStatus = new BehaviorSubject<boolean>(!!localStorage.getItem('token'));
  authStatus$ = this.authStatus.asObservable();
  private userProfileSubject = new BehaviorSubject<void>(undefined);
  userProfileChanged$ = this.userProfileSubject.asObservable();

  constructor(private http: HttpClient) {}

  updateAuthStatus(status: boolean) {
    this.authStatus.next(status);
  }

  private getHeaders(): HttpHeaders {
    const token = localStorage.getItem('token') || ''; 
    return new HttpHeaders({
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    });
  }

  notifyProfileUpdate() {
    this.userProfileSubject.next();
  }

  // =========================
  // Authentication 
  // =========================

  register(data: any) {
    return this.http.post(`${this.baseUrl}/authentication/register`, data);
  }

  login(data: any) {
    return this.http.post(`${this.baseUrl}/authentication/login`, data);
  }

  logout(data: any) {
    return this.http.post(`${this.baseUrl}/authentication/logout`, data, { headers: this.getHeaders() });
  }

  verify2FALogin(userId: string, totpCode: string) {
    return this.http.post(`${this.baseUrl}/authentication/verify-2fa-login`, { userId, totpCode });
  }

  confirmEmail(userId: string, token: string) {
    return this.http.get(`${this.baseUrl}/authentication/confirm-email?userId=${userId}&token=${token}`);
  }

  confirmEmailChange(userId: string, newEmail: string, token: string) {
    return this.http.get(`${this.baseUrl}/authentication/confirm-email-change?userId=${userId}&newEmail=${newEmail}&token=${token}`);
  }
  
  forgotPassword(data: { email: string }) {
    return this.http.post(`${this.baseUrl}/authentication/forgot-password`, data);
  }

  resetPassword(data: any) {
    return this.http.post(`${this.baseUrl}/authentication/reset-password`, data);
  }


  getUserProfile() {
    return this.http.get(`${this.baseUrl}/profile`, { headers: this.getHeaders() });
  }

  changeName(data: { firstName: string, lastName: string }) {
    return this.http.put(`${this.baseUrl}/profile/change-name`, data, { headers: this.getHeaders() });
  }

  changeEmail(data: { newEmail: string, currentPassword: string, twoFactorCode?: string }) {
    return this.http.post(`${this.baseUrl}/profile/change-email`, data, { headers: this.getHeaders() });
  }

  changePassword(data: { currentPassword: string, newPassword: string, twoFactorCode?: string }) {
    return this.http.put(`${this.baseUrl}/profile/change-password`, data, { headers: this.getHeaders() });
  }

  setPhoneNumber(data: { newPhoneNumber: string, currentPassword: string, twoFactorCode?: string }) {
    return this.http.put(`${this.baseUrl}/profile/set-phone-number`, data, { headers: this.getHeaders() });
  }

  setup2FA() {
    return this.http.get(`${this.baseUrl}/security/setup-2fa`, { headers: this.getHeaders() });
  }

  confirm2FASetup(data: {totpCode: string, newSecret: string}) {
    return this.http.post(`${this.baseUrl}/security/confirm-2fa-setup`,data, { headers: this.getHeaders() });
  }

  disable2FA() {
    return this.http.post(`${this.baseUrl}/security/2fa/disable`, {}, { headers: this.getHeaders() });
  }

  
  // =========================
  // Podcast
  // =========================


    generatePodcast(data: any) {
      return this.http.post(`${this.baseUrl}/podcast/generate`, data, { 
        headers: this.getHeaders() 
      });
    }

  getUserPodcasts(
  pageNumber: number,
  pageSize: number,
  sortBy: string,
  sortDirection: string,
  searchTerm: string = ''
): Observable<any> {

  let params = new HttpParams()
    .set('pageNumber', pageNumber.toString())
    .set('pageSize', pageSize.toString())
    .set('sortBy', sortBy)
    .set('sortDirection', sortDirection);


  if (searchTerm && searchTerm.trim()) {

    params = params.set(
      'searchTerm',
      searchTerm.trim()
    );

  }
  const token =
    localStorage.getItem('token');
  const headers =
    new HttpHeaders({
      Authorization: `Bearer ${token}`
    });
  return this.http.get<any>(
    `${this.baseUrl}/podcast/my-podcasts`,
    {
      params,
      headers
    });
  }

    getCurrentStatus(): Observable<any> {
      return this.http.get<any>(`${this.baseUrl}/podcast/current-status`, { 
        headers: this.getHeaders() 
      });
    }

    getPodcast(id: string) {
  return this.http.get<any>(`${this.baseUrl}/podcast/${id}`, {
    headers: this.getHeaders()
  });
}
    getPodcastDetails(id: string): Observable<any> {
      return this.http.get<any>(`${this.baseUrl}/podcast/details/${id}`, {
        headers: this.getHeaders()
      });
    }


     // =========================
      // Document
      // =========================

  
    uploadDocument(formData: FormData) {
      const token = localStorage.getItem('token');
      const headers = new HttpHeaders({
        'Authorization': `Bearer ${token}`
      });

      return this.http.post(`${this.baseUrl}/documents/upload`, formData, { headers });
    } 

    
getMyDocuments(
  pageNumber: number = 1,
  pageSize: number = 20,
  sortBy: string = 'date',
  sortDirection: string = 'desc',
  searchTerm: string = ''
): Observable<any> {

  let params = new HttpParams()
    .set('pageNumber', pageNumber.toString())
    .set('pageSize', pageSize.toString())
    .set('sortBy', sortBy)
    .set('sortDirection', sortDirection);


  if (searchTerm && searchTerm.trim()) {
    params = params.set(
      'searchTerm',
      searchTerm.trim()
    );
  }
  const token = localStorage.getItem('token');
  const headers = new HttpHeaders({
    Authorization: `Bearer ${token}`
  });
  return this.http.get<any>(
    `${this.baseUrl}/documents/my-document`,
    {
      params,
      headers
    }
  );
}
}