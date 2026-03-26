import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class ApiService {

  private baseUrl = 'https://localhost:7163/api';

  constructor(private http: HttpClient) {}

  // Helper method to include the JWT token in the headers
  private getHeaders(): HttpHeaders {
  const token = localStorage.getItem('token') || ''; // لو مفيش توكن يبعت سترينج فاضي بدل null
  return new HttpHeaders({
    'Authorization': `Bearer ${token}`,
    'Content-Type': 'application/json'
  });
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

  forgotPassword(data: any) {
    return this.http.post(`${this.baseUrl}/authentication/forgot-password`, data);
  }

  resetPassword(data: any) {
    return this.http.post(`${this.baseUrl}/authentication/reset-password`, data);
  }

  verifyTwoFactor(data: any) {
    return this.http.post(`${this.baseUrl}/authentication/verify-two-factor`, data);
  }

  // =========================
  // Documents
  // =========================

  uploadDocument(formData: FormData) {
    return this.http.post(`${this.baseUrl}/documents/upload`, formData, {
      headers: this.getHeaders()
    });
  }

  // =========================
  // Podcast
  // =========================

  generatePodcast(data: any) {
    return this.http.post(`${this.baseUrl}/podcast/generate`, data, { headers: this.getHeaders() });
  }

  getPodcast(id: string) {
    return this.http.get(`${this.baseUrl}/podcast/${id}`, {
      headers: this.getHeaders(),
      responseType: 'blob'
    });
  }

  // =========================
  // Profile
  // =========================

  // ✅ UPDATED: بتاخد الـ userId من localStorage وتحطه في الـ URL
  getUserProfile() {
    const userId = localStorage.getItem('userId');
    return this.http.get(`${this.baseUrl}/profile`, { headers: this.getHeaders() });
  }

  changeName(data: any) {
    return this.http.put(`${this.baseUrl}/profile/change-name`, data, { headers: this.getHeaders() });
  }

  changeEmail(data: any) {
    return this.http.put(`${this.baseUrl}/profile/change-email`, data, { headers: this.getHeaders() });
  }

  changePassword(data: any) {
    return this.http.put(`${this.baseUrl}/profile/change-password`, data, { headers: this.getHeaders() });
  }

  setPhoneNumber(data: any) {
    return this.http.put(`${this.baseUrl}/profile/set-phone-number`, data, { headers: this.getHeaders() });
  }

  // =========================
  // Security (2FA)
  // =========================

  enable2FA(data: any) {
    return this.http.post(`${this.baseUrl}/security/2fa/enable`, data, { headers: this.getHeaders() });
  }

  disable2FA() {
    return this.http.post(`${this.baseUrl}/security/2fa/disable`, {}, { headers: this.getHeaders() });
  }

}