import { Component, OnInit, HostListener,OnDestroy } from '@angular/core';
import { RouterOutlet, RouterLink, RouterLinkActive,Router } from '@angular/router';
import { ApiService } from './services/api'; 
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive, CommonModule],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class AppComponent implements OnInit,OnDestroy {
  
  isLoggedIn: boolean = false;
  isDropdownOpen: boolean = false; 
  userFullName: string = 'User';   
  private authChannel = new BroadcastChannel('auth_status');

  constructor(private apiService: ApiService, private router: Router) {}

  ngOnInit() {
    this.apiService.authStatus$.subscribe((status: boolean) => {
      this.isLoggedIn = status;
      
      if (status) {
        this.loadUserData(); 
      }
    });
    this.apiService.userProfileChanged$.subscribe(() => {
    if (this.isLoggedIn) {
      this.loadUserData();
    }
    });
    this.authChannel.onmessage = (event) => {
      if (event.data === 'logout_all_tabs') {
        if (this.router.url.includes('email-update-confirm')||this.router.url.includes('reset-password'))
          return;        
        localStorage.removeItem('token'); 
        this.apiService.updateAuthStatus(false);
        this.isDropdownOpen = false; 
        
        this.router.navigate(['/']); 
      }
    };
  }

  ngOnDestroy() {
      if (this.authChannel) {
        this.authChannel.close();
      }
    }

  loadUserData() {
    this.apiService.getUserProfile().subscribe({
      next: (response: any) => {
      const data = response.value ?? response;
      if (data.firstname && data.lastname) {
        this.userFullName = `${data.firstname} ${data.lastname}`;
      } else {
        this.userFullName = data.Username || 'User';
      }
      console.log('Header Name Updated to:', this.userFullName);
    },
    error: (err) => console.error('Error loading header data:', err)
    });
  }

  toggleDropdown() {
    this.isDropdownOpen = !this.isDropdownOpen;
  }

  @HostListener('document:click', ['$event'])
    onDocumentClick(event: MouseEvent) {
      const target = event.target as HTMLElement;
      if (!target.closest('.profile-capsule-wrapper')) {
        this.isDropdownOpen = false;
      }
  }
  onLogout() {
    localStorage.removeItem('token'); 
    this.apiService.updateAuthStatus(false);
    this.isDropdownOpen = false; 
    this.router.navigate(['/']);
  }
}