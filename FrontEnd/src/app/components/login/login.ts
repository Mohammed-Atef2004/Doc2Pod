import { Component, ChangeDetectorRef, NgZone } from '@angular/core';
import { RouterLink, Router } from '@angular/router';
import { ApiService } from '../../services/api';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import Swal from 'sweetalert2';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [RouterLink, CommonModule, FormsModule],
  templateUrl: './login.html',
  styleUrl: './login.css',
})
export class LoginComponent {
  loginData = { email: '', password: '' };
  twoFactorCode: string = ''; 
  tempUserId: string = '';   
  
  is2FARequired: boolean = false; 
  isLoading: boolean = false;
  errorMessage: string = '';
  showPassword: boolean = false; 

  showForgotModal = false;
  forgotEmail = '';
  forgotError = '';
  isForgotLoading = false;

  constructor(
    public apiService: ApiService, 
    private router: Router,
    private cdr: ChangeDetectorRef,
    private zone: NgZone
  ) {}

  onLogin(form: any): void {
    if (form.invalid) return; 
    
    this.isLoading = true;
    this.errorMessage = '';

    this.apiService.login(this.loginData).subscribe({
      next: (response: any) => {
        this.zone.run(() => {
          this.isLoading = false;
          if (response.value?.requiresTwoFactor) {
            this.is2FARequired = true;
            this.tempUserId = response.value.userId;
          } else {
            this.completeLogin(response.value?.token, response.value?.userId);
          }
          this.cdr.detectChanges();
        });
      },
      error: (err) => {
        this.zone.run(() => {
          this.isLoading = false;
          this.handleError(err);
          this.cdr.detectChanges();
        });
      }
    });
  }

  onVerify2FA(form: any): void {
    if (form.invalid) return; 
    if (!this.twoFactorCode) {
      this.errorMessage = 'Please enter the 2FA code.';
      return;
    }

    this.isLoading = true;
    this.apiService.verify2FALogin(this.tempUserId, this.twoFactorCode).subscribe({
      next: (response: any) => {
        this.zone.run(() => {
          this.isLoading = false;
          this.completeLogin(response.value?.token, response.value?.userId);
          this.cdr.detectChanges();
        });
      },
      error: (err) => {
        this.zone.run(() => {
          this.isLoading = false;
          this.errorMessage = 'Invalid 2FA code. Please try again.';
          this.cdr.detectChanges();
        });
      }
    });
  }

  private completeLogin(token: string, userId: string): void {
    if (token) localStorage.setItem('token', token);
    if (userId) localStorage.setItem('userId', userId);
    this.apiService.updateAuthStatus(true);
    this.router.navigate(['/']);
  }

  private handleError(err: any): void {
    if (err.status === 401) {
      this.errorMessage = 'Invalid email or password.';
    } else {
      const errorData = err.error;
      this.errorMessage = errorData?.detail || errorData?.message || (typeof errorData === 'string' ? errorData : 'Something went wrong.');
    }
  }

  togglePassword() {
    this.showPassword = !this.showPassword;
  }

  openForgotModal() {
    this.showForgotModal = true;
    this.forgotError = '';
    this.forgotEmail = '';
  }

  closeForgotModal() {
    this.showForgotModal = false;
  }

  sendResetLink() {
    if (!this.forgotEmail) {
      this.forgotError = 'Please enter your email.';
      return;
    }
    const payload = { email: this.forgotEmail };
    this.showSuccessAlert();
    this.closeForgotModal();
    this.isForgotLoading = true;
    this.apiService.forgotPassword(payload).subscribe({
      next: (res) => {
        this.zone.run(() => {
          this.isForgotLoading = false;
          this.cdr.detectChanges();
        });
      },
      error: (err) => {
        this.zone.run(() => {
          this.isForgotLoading = false;
          this.cdr.detectChanges();
        });
      }
    }); 
  }

  showSuccessAlert() {
      Swal.fire({
          title: 'Request Received',
          text: "If your email is registered, you'll receive a reset link shortly.",
          icon: 'success',
          background: '#121212',
          color: '#fff',
          showCancelButton: false,    
          confirmButtonText: 'Got it!',   
          confirmButtonColor: '#fbbf24',    
          buttonsStyling: true
      });
  }
}