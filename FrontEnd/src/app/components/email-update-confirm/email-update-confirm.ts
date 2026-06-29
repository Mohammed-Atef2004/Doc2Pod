import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { ApiService } from '../../services/api';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-email-update-confirm',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './email-update-confirm.html',
  styleUrl: './email-update-confirm.css'
})
export class EmailUpdateConfirm implements OnInit {
  message: string = 'Verifying your new email address...';
  isLoading: boolean = true;
  isSuccess: boolean = false;
  isError: boolean = false;

  constructor(
    private route: ActivatedRoute,
    private apiService: ApiService,
    private router: Router,
    private cdr: ChangeDetectorRef
  ) {}

ngOnInit(): void {
  const userId = this.route.snapshot.queryParamMap.get('userId');
  const newEmail = this.route.snapshot.queryParamMap.get('newEmail');
  const token = this.route.snapshot.queryParamMap.get('token');

  console.log("Extracted Params:", { userId, newEmail, token });

  if (userId && newEmail && token) {
    this.apiService.confirmEmailChange(userId, newEmail, token).subscribe({
      next: (res) => {
        this.isLoading = false;
        this.isSuccess = true;            
        this.message = 'Email updated! Logging you out...';
        
        localStorage.removeItem('token'); 
        this.apiService.updateAuthStatus(false);
        const authChannel = new BroadcastChannel('auth_status');
        authChannel.postMessage('logout_all_tabs');
        authChannel.close();
        this.cdr.detectChanges();
        
        setTimeout(() => { 
          this.router.navigate(['/login']); 
        }, 3000);
      },
      error: (err) => {
        this.isLoading = false;
        this.isError = true;
        this.message = err.error?.message || 'Link expired.';
        this.cdr.detectChanges();
      }
    });
  } else {
    this.isLoading = false;
    this.isError = true;
    this.message = 'Invalid link format.';
    this.cdr.detectChanges();
  }
}

  goToLogin() {
    this.router.navigate(['/login']);
  }
}