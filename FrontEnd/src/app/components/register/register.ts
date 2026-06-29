import { Component, ChangeDetectorRef, NgZone } from '@angular/core';
import { RouterLink, Router } from '@angular/router';
import { ApiService } from '../../services/api'; 
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
  registerData = {
    email: '',
    username: '',
    password: '',
    firstName: '',
    lastName: ''
  };

  isLoading: boolean = false;
  errorMessage: string = '';

  constructor(
    private apiService: ApiService, 
    private router: Router,
    private cdr: ChangeDetectorRef,
    private zone: NgZone
  ) {}

  onSubmit(): void {
    if (!this.registerData.email || !this.registerData.password || !this.registerData.username) {
      this.errorMessage = 'Please fill in all required fields.';
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';

    this.apiService.register(this.registerData).subscribe({
      next: (response: any) => {
        this.zone.run(() => {
          this.isLoading = false;
          console.log('Account created successfully!', response);
          this.router.navigate(['/login']);
          this.cdr.detectChanges();
        });
      },
      error: (err) => {
        this.zone.run(() => {
          this.isLoading = false;
          const errorData = err.error;
          this.errorMessage = errorData?.detail || errorData?.message || (typeof errorData === 'string' ? errorData : 'Registration failed. Check your connection or data.');
          console.error('Registration error details:', err);
          this.cdr.detectChanges();
        });
      }
    });
  }
}