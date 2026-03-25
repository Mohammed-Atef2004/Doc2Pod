import { Component, OnInit, ChangeDetectorRef } from '@angular/core'; // 1. أضفنا ChangeDetectorRef
import { ApiService } from '../../services/api';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { finalize } from 'rxjs/operators'; // 2. أضفنا finalize لضمان غلق الـ Loading

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './profile.html',
  styleUrl: './profile.css'
})
export class ProfileComponent implements OnInit {
  userData: any = null;
  isLoading: boolean = true;
  errorMessage: string = '';

  // 3. حقن الـ cdr في الـ constructor
  constructor(
    private apiService: ApiService, 
    private router: Router,
    private cdr: ChangeDetectorRef 
  ) {}

  ngOnInit(): void {
    const token = localStorage.getItem('token');
    const userId = localStorage.getItem('userId');

    if (!token || !userId) {
      this.router.navigate(['/login']);
      return;
    }

    this.loadUserProfile();
  }

  loadUserProfile(): void {
    this.isLoading = true;
    this.errorMessage = '';

    this.apiService.getUserProfile()
      .pipe(
        // 4. الـ finalize بتتنفذ سواء الـ request نجح أو فشل، فبنقفل فيها الـ loading مرة واحدة
        finalize(() => {
          this.isLoading = false;
          this.cdr.detectChanges(); // 5. أهم سطر: بيجبر الـ UI تتحدث فوراً
        })
      )
      .subscribe({
        next: (res: any) => {
          this.userData = res.value ?? res;
          console.log('Profile data loaded successfully!', this.userData);
        },
        error: (err) => {
          console.error('Failed to load profile:', err);
          if (err.status === 401) {
            this.onLogout();
          } else {
            this.errorMessage = 'Could not load your profile. Please check the backend.';
          }
        }
      });
  }

  onLogout(): void {
    localStorage.removeItem('token');
    localStorage.removeItem('userId');
    this.router.navigate(['/login']);
  }
}