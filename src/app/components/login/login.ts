import { Component } from '@angular/core';
import { RouterLink, Router } from '@angular/router';
import { ApiService } from '../../services/api'; // Correct path based on your folders
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
  // Login credentials
  loginData = {
    email: '',
    password: ''
  };

  isLoading: boolean = false;
  errorMessage: string = '';

  constructor(private apiService: ApiService, private router: Router) {}

  // Handles the login process
  onLogin(): void {
    if (!this.loginData.email || !this.loginData.password) {
      this.errorMessage = 'Email and password are required.';
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';

    console.log('Attempting login for:', this.loginData.email);

    this.apiService.login(this.loginData).subscribe({
      next: (response: any) => {
        this.isLoading = false;
        console.log('Login Success! Response:', response);

        // Save token if exists (Check your backend response property name)
        const token = response.token || response.accessToken;
        if (token) {
          localStorage.setItem('token', token);
        }

        // Navigate to the profile page as requested
        this.router.navigate(['/profile']);
      },
      error: (err) => {
        this.isLoading = false;
        console.error('Login Failed!', err);

        // Displaying error from backend or generic message
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