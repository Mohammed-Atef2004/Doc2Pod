import { Component, Input, Output, EventEmitter, OnInit,NgZone,ChangeDetectorRef } from '@angular/core';
import { CommonModule, TitleCasePipe } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ApiService } from '../../services/api'; 
import Swal from 'sweetalert2';
import { Router } from '@angular/router';

@Component({
  selector: 'app-security-verify',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, TitleCasePipe],
  templateUrl: './security-verify.html',
  styleUrl: './security-verify.css'
})
export class SecurityVerifyComponent implements OnInit {
  @Input() mode: 'email' | 'phone' | 'password' = 'phone';
  @Input() is2FAEnabled: boolean = false; 
  @Input() failedAttempts: number = 0;
  
  @Output() onClose = new EventEmitter<void>();
  @Output() onSuccess = new EventEmitter<void>();

  securityForm!: FormGroup;
  isLoading = false;
  serverError: string | null = null;

  constructor(private fb: FormBuilder, private apiService: ApiService,private zone: NgZone,private cdr: ChangeDetectorRef,private router: Router) {}

  ngOnInit() {
    this.initForm();
  }

  initForm() {
    this.securityForm = this.fb.group({
      currentPassword: ['', [Validators.required, Validators.minLength(8)]],
      otpCode: ['', this.is2FAEnabled ? [Validators.required, Validators.pattern('^[0-9]{6}$')] : []],
      newValue: ['', this.getModeValidators()]
    });

    this.securityForm.valueChanges.subscribe(() => {
      if (this.serverError) this.serverError = null;
      this.cdr.detectChanges();
    });
  }

  getModeValidators() {
    if (this.mode === 'email') return [Validators.required, Validators.email];
    if (this.mode === 'phone') return [Validators.required, Validators.pattern('^\\+[1-9]\\d{1,14}$')];
    if (this.mode === 'password') return [
      Validators.required, 
      Validators.pattern('(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9])(?=.*[!@#$%^&*.]).{8,}')
    ];
    return [Validators.required];
  }

  submit() {
    if (this.securityForm.invalid) {
      this.securityForm.markAllAsTouched();
      return;
    }

    this.isLoading = true;
    this.serverError = null;
    const val = this.securityForm.value;
    const authData = { currentPassword: val.currentPassword, twoFactorCode: this.is2FAEnabled ? val.otpCode : null };
    
    let request;
    if (this.mode === 'email') request = this.apiService.changeEmail({ newEmail: val.newValue, ...authData });
    else if (this.mode === 'phone') request = this.apiService.setPhoneNumber({ newPhoneNumber: val.newValue, ...authData });
    else request = this.apiService.changePassword({ newPassword: val.newValue, ...authData });

    request.subscribe({
      next: () => {
        this.zone.run(() => {
          this.isLoading = false;
          this.handleSuccess();
          this.cdr.detectChanges();
        });
      },
      error: (err) => {
        this.zone.run(() => {
        this.isLoading = false;
          this.handleBackendError(err);
          this.cdr.detectChanges();
      });
      }
    });
  }

    handleBackendError(err: any) {
      this.zone.run(() => {
        this.serverError =
          err.error?.message ||           
          err.error?.Message ||   
          err.error?.error?.message || 
          err.error?.detail ||          
          'Update failed. Please check your data.';
        if (err.status === 401 || err.status === 400) {
          this.failedAttempts++;
          if (this.failedAttempts >= 5) {
            this.lockAndLogout();
          }
        }
        this.cdr.detectChanges(); 
      });
  }

  handleSuccess() {
    let msg = 'Changes saved successfully!';
    if (this.mode === 'email') msg = 'A confirmation link has been sent to your new email.';
    if (this.mode === 'password') msg = 'Password changed! You will be logged out now.';

    Swal.fire({
      title: 'Success!',
      text: msg,
      icon: 'success',
      confirmButtonText: 'OK',
      background: '#121212',
      color: '#fff',
      customClass: {
      confirmButton: 'btn-gold-action' 
      }
    }).then(() => {
      this.onSuccess.emit();
      this.onClose.emit();
    });
  }

  lockAndLogout() {
    Swal.fire({
      title: 'Account Locked!',
      text: 'Too many attempts. For security, you will be logged out.',
      icon: 'error',
      confirmButtonText: 'Logout',
      allowOutsideClick: false,
      background: '#121212',
      color: '#fff',
      customClass: { confirmButton: 'btn-gold-action' }
    }).then(() => {
    this.zone.run(() => {
      localStorage.removeItem('token');
      localStorage.removeItem('userId');
      this.apiService.updateAuthStatus(false);
      this.router.navigate(['/']);
      this.onClose.emit();
      this.cdr.detectChanges();
    });
  });
}

  onlyNumbers(event: any) {
    const pattern = /[0-9]/;
    if (!pattern.test(String.fromCharCode(event.charCode))) event.preventDefault();
  }
}