import { Component, OnInit } from '@angular/core';
import { ApiService } from '../../services/api'; // Path confirmed
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './profile.html',
  styleUrl: './profile.css'
})
export class ProfileComponent implements OnInit {
  // Object to hold user data retrieved from the backend
  userData: any = null;
  isLoading: boolean = true;
  errorMessage: string = '';

  constructor(private apiService: ApiService, private router: Router) {}

  ngOnInit(): void {
    this.loadUserProfile();
  }

  // Fetches the profile data using the new getUserProfile service method
  loadUserProfile(): void {
    this.isLoading = true;
    this.errorMessage = '';

    this.apiService.getUserProfile().subscribe({
      next: (res: any) => {
        this.userData = res;
        this.isLoading = false;
        console.log('Profile data loaded successfully!', res);
      },
      error: (err) => {
        this.isLoading = false;
        console.error('Failed to load profile:', err);
        
        // If unauthorized (no token or expired), redirect to login
        if (err.status === 401) {
          this.router.navigate(['/login']);
        } else {
          this.errorMessage = 'Could not load your profile. Please check if the backend is running.';
        }
      }
    });
  }

  // Signs out the user by clearing the local storage
  onLogout(): void {
    localStorage.removeItem('token'); // Clears the saved JWT
    this.router.navigate(['/login']);
  }
}