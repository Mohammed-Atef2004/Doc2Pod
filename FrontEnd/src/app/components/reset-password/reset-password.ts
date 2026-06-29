import { Component, OnInit, ChangeDetectorRef, NgZone } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { ApiService } from '../../services/api';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import Swal from 'sweetalert2';

@Component({
  selector: 'app-reset-password',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './reset-password.html',
  styleUrl: './reset-password.css',
})
export class ResetPassword implements OnInit {
  resetForm!: FormGroup;
  userId: string | null = null;
  token: string | null = null;
  
  isLoading = false;
  serverError: string | null = null;
  isLinkValid = false;
  isSuccess = false; 
  isError = false;   
  message = '';      

  constructor(
    private fb: FormBuilder,
    private apiService: ApiService,
    private route: ActivatedRoute,
    public router: Router,
    private cdr: ChangeDetectorRef,
    private zone: NgZone
  ) {}

  ngOnInit() {
    this.userId = this.route.snapshot.queryParamMap.get('userId');
    this.token = this.route.snapshot.queryParamMap.get('token');

    if (this.userId && this.token) {
      this.isLinkValid = true;
      this.initForm();
    } else {
      this.isLinkValid = false;
      this.serverError = 'Invalid or expired reset link.';
    }
  }

  initForm() {
    this.resetForm = this.fb.group({
      newPassword: ['', [
        Validators.required, 
        Validators.pattern('(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9])(?=.*[!@#$%^&*.]).{8,}')
      ]],
      confirmPassword: ['', [Validators.required]]
    }, { validator: this.passwordMatchValidator });

    this.resetForm.valueChanges.subscribe(() => {
      if (this.serverError) this.serverError = null;
      this.cdr.detectChanges();
    });
  }

  passwordMatchValidator(g: FormGroup) {
    return g.get('newPassword')?.value === g.get('confirmPassword')?.value
      ? null : { 'mismatch': true };
  }

  submit() {
    if (this.resetForm.invalid) return;
    this.isLoading = true;
    this.isError = false;

    const resetModel = {
      userId: this.userId,
      token: this.token,
      newPassword: this.resetForm.value.newPassword
    };

    this.apiService.resetPassword(resetModel).subscribe({
      next: (res) => {
        this.isLoading = false;
        this.isSuccess = true;
        this.message = 'Password updated successfully!';
        const authChannel = new BroadcastChannel('auth_status');
        authChannel.postMessage('logout_all_tabs');
        authChannel.close();
        setTimeout(() => {
          if (this.isSuccess) this.router.navigate(['/login']);
        }, 3000);

        this.cdr.detectChanges();
      },
      error: (err) => {
        this.isLoading = false;
        this.isError = true;
        this.message = err.error?.message || 'Link expired or invalid.';
        this.cdr.detectChanges();
      }
    });
  }

  goToLogin() {
    this.router.navigate(['/login']);
  }
  
}