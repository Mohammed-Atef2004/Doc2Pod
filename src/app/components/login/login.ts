import { Component } from '@angular/core';
import { RouterLink, Router } from '@angular/router';
import { ApiService } from '../../services/api';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [RouterLink, CommonModule, FormsModule],
  templateUrl: './login.html',
  styleUrl: './login.css',
})
export class LoginComponent {
  loginData = {
    email: '',
    password: ''
  };

  isLoading: boolean = false;
  errorMessage: string = '';

  constructor(private apiService: ApiService, private router: Router) {}

  onLogin(): void {
    if (!this.loginData.email || !this.loginData.password) {
      this.errorMessage = 'Email and password are required.';
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';

    this.apiService.login(this.loginData).subscribe({
      next: (response: any) => {
        this.isLoading = false;
        console.log('Login Success! Response:', response);

        // ✅ الـ token والـ userId جوه response.value
        const token = response.value?.token;
        const userId = response.value?.userId;

        if (token) {
          localStorage.setItem('token', token);
        }

        // ✅ بنحفظ الـ userId عشان نبعته في endpoint بتاع الـ profile
        if (userId) {
          localStorage.setItem('userId', userId);
        }

        this.router.navigate(['/profile']);
      },
      error: (err) => {
        this.isLoading = false;
        console.error('Login Failed!', err);

        if (err.status === 401) {
          this.errorMessage = 'Invalid email or password.';
        } else if (err.status === 0) {
          this.errorMessage = 'Cannot reach the server. Check if the Backend is running.';
        } else {
          this.errorMessage = err.error?.message || 'Something went wrong. Please try again.';
        }
      }
    });
  }
}