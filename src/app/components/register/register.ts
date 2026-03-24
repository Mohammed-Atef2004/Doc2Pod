import { Component } from '@angular/core';
import { RouterLink, Router } from '@angular/router';
import { ApiService } from '../../services/api'; // Path confirmed
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [RouterLink, CommonModule, FormsModule],
  templateUrl: './register.html',
  styleUrl: './register.css',
})
export class RegisterComponent {
  // Matching the exact Backend DTO requirements
  registerData = {
    email: '',
    username: '',
    password: '',
    firstName: '',
    lastName: ''
  };

  isLoading: boolean = false;
  errorMessage: string = '';

  constructor(private apiService: ApiService, private router: Router) {}

  // Handles the form submission and sends data to the API
  onSubmit(): void {
    // Basic validation to check if fields are filled before calling the API
    if (!this.registerData.email || !this.registerData.password || !this.registerData.username) {
      this.errorMessage = 'Please fill in all required fields.';
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';

    // Calling the register method with the direct data object
    this.apiService.register(this.registerData).subscribe({
      next: (response: any) => {
        this.isLoading = false;
        console.log('Account created successfully!', response);
        // Redirecting to login page on success
        this.router.navigate(['/login']);
      },
      error: (err) => {
        this.isLoading = false;
        // Handling backend error messages if available, otherwise using a generic one
        this.errorMessage = err.error?.message || 'Registration failed. Check your connection or data.';
        console.error('Registration error details:', err);
      }
    });
  }
}