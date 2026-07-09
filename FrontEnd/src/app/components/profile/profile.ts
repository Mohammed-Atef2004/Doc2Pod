import { Component, OnInit, ChangeDetectorRef, NgZone } from '@angular/core'; 
import { ApiService } from '../../services/api';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { finalize } from 'rxjs/operators';
import { Router } from '@angular/router';
import { QRCodeComponent } from 'angularx-qrcode';
import Swal from 'sweetalert2';
import { SecurityVerifyComponent } from '../../shared/security-verify/security-verify';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule, FormsModule, SecurityVerifyComponent, QRCodeComponent],
  templateUrl: './profile.html',
  styleUrl: './profile.css'
})
export class ProfileComponent implements OnInit {
  userData: any = null;
  isLoading: boolean = true;
  isEditingName = false;
  tempName = { firstName: '', lastName: '' };
  showSecurityModal = false;
  securityMode: 'email' | 'phone' | 'password' = 'phone';
  isSettingUp2FA = false;
  twoFactorSetupData: any = null;
  totpConfirmationCode: string = '';
  serverError: string | null = null;
  isOtpTouched = false;

  constructor(
    private apiService: ApiService, 
    private cdr: ChangeDetectorRef,
    private router: Router,
    private zone: NgZone 
  ) {}

  ngOnInit(): void {
    this.loadUserProfile();
  }

  private getErrorMessage(err: any): string {
      return (
        err.error?.message || 
        err.error?.error?.message || 
        err.error?.Message || 
        err.error?.detail || 
        'Something went wrong. Please try again.'
      );
    }

  loadUserProfile(): void {
    this.isLoading = true;
    this.apiService.getUserProfile().subscribe({
      next: (res: any) => {
        this.zone.run(() => {
          this.userData = res.value ?? res;
          this.syncTempName();
          this.isLoading = false;
          this.cdr.detectChanges();
        });
      },
      error: (err) => {
        this.zone.run(() => {
          this.serverError = this.getErrorMessage(err);
          this.isLoading = false;
          this.cdr.detectChanges();
        });
      }
    });
  }

  toggleEditName() {
    this.isEditingName = !this.isEditingName;
    if (!this.isEditingName) this.syncTempName();
    this.cdr.detectChanges();
  }

  saveName() {
    if (this.tempName.firstName === this.userData.firstname && 
        this.tempName.lastName === this.userData.lastname) {
      this.isEditingName = false;
      return;
    }

    const payload = { 
      firstName: this.tempName.firstName, 
      lastName: this.tempName.lastName 
    };

    this.isLoading = true; 
    this.apiService.changeName(payload).subscribe({
      next: () => {
        this.zone.run(() => {
          this.userData.firstname = this.tempName.firstName;
          this.userData.lastname = this.tempName.lastName;
          this.apiService.notifyProfileUpdate();
          this.isEditingName = false;
          this.isLoading = false;
          this.cdr.detectChanges();
        });
      },
      error: (err) => {
        this.zone.run(() => {
          this.serverError = this.getErrorMessage(err);
          this.syncTempName();
          this.isEditingName = false;
          this.isLoading = false;
          this.cdr.detectChanges();
        });
      }
    });
  }

  handle2FAAction() {
    if (this.userData?.isTwoFactorEnabled) {
      Swal.fire({
        title: 'Are you sure?',
        text: "Removing this security layer will make your account less secure!",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonText: 'Yes, disable it',
        cancelButtonText: 'Keep it active',
        background: '#121212',
        color: '#fff',
        confirmButtonColor: '#ff4d4d', 
        cancelButtonColor: '#3085d6',
        customClass: {
          confirmButton: 'btn-danger-swal', 
        }
      }).then((result) => {
        if (result.isConfirmed) {
          this.disable2FA();
        }
      });
    } else {
      this.start2FASetup();
    }
  }

  start2FASetup() {
    this.reset2FAState();
    this.isLoading = true;
    this.apiService.setup2FA().subscribe({
      next: (res: any) => {
        this.zone.run(() => {
          this.twoFactorSetupData = res.value; 
          this.isSettingUp2FA = true;
          this.isLoading = false;
          this.cdr.detectChanges();
        });
      },
      error: (err) => {
        this.zone.run(() => {
          this.isLoading = false;
          this.serverError = this.getErrorMessage(err);
          this.cdr.detectChanges();
        });
      }
    });
  }

  confirm2FA() {
    if (!this.totpConfirmationCode || !this.twoFactorSetupData) return;
    
    this.isLoading = true;
    this.serverError = null;

    this.apiService.confirm2FASetup({
      totpCode: this.totpConfirmationCode,
      newSecret: this.twoFactorSetupData.secret
    }).subscribe({
      next: (res: any) => {
        this.zone.run(() => {
          this.isLoading = false;
          this.isSettingUp2FA = false;
          Swal.fire({
            title: 'Activated!',
            text: 'Two-Factor Authentication is now active.',
            icon: 'success',
            background: '#121212',
            color: '#fff',
            confirmButtonText: 'Great!',
            customClass: { confirmButton: 'btn-gold-action' }
          });
          this.loadUserProfile();
          this.cdr.detectChanges();
        });
      },
      error: (err) => {
        this.zone.run(() => {
          this.isLoading = false;
          this.serverError =this.getErrorMessage(err);
          this.cdr.detectChanges();
        });
      }
    });
  }


  disable2FA() {
    this.isLoading = true;
    this.apiService.disable2FA().subscribe({
      next: () => {
        this.zone.run(() => {
          this.isLoading = false;
          this.userData.isTwoFactorEnabled = false;
          Swal.fire({
            title: 'Disabled',
            text: 'Two-Factor Authentication has been removed.',
            icon: 'info',
            background: '#121212',
            color: '#fff',
            confirmButtonText: 'OK',
            customClass: { confirmButton: 'btn-gold-action' }
          });
          this.loadUserProfile();
          this.cdr.detectChanges();
        });
      },
      error: (err) => {
        this.zone.run(() => {
          this.isLoading = false;
          this.serverError = this.getErrorMessage(err);
          this.cdr.detectChanges();
        });
      }
    });
  }
  
  openSecurityCheck(mode: 'email' | 'phone' | 'password') {
    this.securityMode = mode;
    this.showSecurityModal = true;
    this.cdr.detectChanges();
  }

  handleSecuritySuccess() {
    this.showSecurityModal = false;
    if (this.securityMode === 'password' || this.userData?.isLocked) { 
      this.forceLogout();
    } else {
      this.loadUserProfile();
    }
    this.cdr.detectChanges();
  }

  forceLogout() {
    localStorage.removeItem('token');
    this.apiService.updateAuthStatus(false);
    this.router.navigate(['/']); 
  }  

  syncTempName() {
    this.tempName.firstName = this.userData?.firstname || '';
    this.tempName.lastName = this.userData?.lastname || '';
    this.cdr.detectChanges();
  }

  reset2FAState() {
    this.totpConfirmationCode = '';
    this.isOtpTouched = false;
    this.serverError = null;
    this.twoFactorSetupData = null; 
    this.cdr.detectChanges();
  }



  onOtpChange() {
    this.serverError = null; 
    this.isOtpTouched = true;
    this.cdr.detectChanges();
  }

  close2FAModal() {
    this.isSettingUp2FA = false;
    this.reset2FAState();
    this.cdr.detectChanges();
  }

  onlyNumbers(event: any) {
    const pattern = /[0-9]/;
    if (!pattern.test(String.fromCharCode(event.charCode))) event.preventDefault();
  }

  copySecret(text: string) {
    navigator.clipboard.writeText(text);
    const Toast = Swal.mixin({
      toast: true,
      position: 'top-end',
      showConfirmButton: false,
      timer: 2000,
      timerProgressBar: true,
      background: '#1a1a1a',
      color: '#eedca8'
    });
    Toast.fire({ icon: 'success', title: 'Secret copied to clipboard' });
  }
}