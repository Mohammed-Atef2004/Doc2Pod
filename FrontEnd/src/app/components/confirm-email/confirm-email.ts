import { Component, OnInit,ChangeDetectorRef } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { ApiService } from '../../services/api';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-confirm-email',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './confirm-email.html',
  styleUrl: './confirm-email.css',
})
export class ConfirmEmail implements OnInit {
  message: string = 'Verifying your account...';
  isLoading: boolean = true;
  isError: boolean = false;

  constructor(
    private route: ActivatedRoute,
    private apiService: ApiService,
    private router: Router,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    const userId = this.route.snapshot.queryParamMap.get('userId');
    const token = this.route.snapshot.queryParamMap.get('token');

    if (userId && token) {
      this.apiService.confirmEmail(userId, token).subscribe({
        next: (res) => {
          this.isLoading = false;
          this.isError = false;
          this.message = 'Email confirmed successfully! Redirecting to login...';
          this.cdr.detectChanges();
          setTimeout(() => {
            this.router.navigate(['/login']);
          }, 3000);
        },
        error: (err) => {
          this.isLoading = false;
          this.isError = true;
          this.message = 'Confirmation failed. The link may be invalid or expired.';
          console.error('Confirmation Error:', err);
        }
      });
    } else {
      this.isLoading = false;
      this.isError = true;
      this.message = 'Invalid confirmation parameters.';
    }
  }

  goToLogin() {
  this.router.navigate(['/login']);
}
}