import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router'; 
import { ApiService } from '../../services/api';
import { SignalrService } from '../../services/signalr.service';
import { FormsModule } from '@angular/forms';
import { Subscription } from 'rxjs'; 
import { AppLogger } from '../../services/logger';

export interface PaginatedResult<T> {
  data: T[];
  currentPage: number;
  totalPages: number;
  totalCount: number;
  pageSize: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

@Component({
  selector: 'app-podcast-list',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './podcast-list.html',
  styleUrl: './podcast-list.css'
})
export class PodcastListComponent implements OnInit, OnDestroy {

  podcasts: any[] = [];
  isLoading: boolean = false;
  errorMessage: string = '';
  currentPage: number = 1;
  pageSize: number = 10;
  totalPages: number = 0;
  sortBy: string = 'Date'; 
  sortDirection: string = 'desc'; 
  searchTerm: string = '';

  private signalrSub!: Subscription;

  constructor(
    private apiService: ApiService,
    private signalrService: SignalrService,
    private cdr: ChangeDetectorRef,
    private router: Router 
  ) {}

  ngOnInit(): void {
    this.loadUserPodcasts();

    this.signalrService.startConnection();

    this.signalrSub = this.signalrService.onStatusChanged().subscribe({
      next: (data) => {
        AppLogger.log('List Component received SignalR update:', data);
       
        if (data.status === '3' || data.status === 'Completed' || data.status === 'done') {
          AppLogger.log(`Podcast ${data.id} is ready! Refreshing list...`);
          this.loadUserPodcasts(this.currentPage); 
        }
      },
      error: (err) => AppLogger.error('SignalR List Subscription Error: ', err)
    });
  }

  ngOnDestroy(): void {
    if (this.signalrSub) {
      this.signalrSub.unsubscribe();
    }
  }

  searchPodcasts(): void {
    this.currentPage = 1;
    this.loadUserPodcasts(1);
  }

  loadUserPodcasts(page: number = 1): void {
    this.isLoading = true;
    this.errorMessage = '';
    this.currentPage = page;

    this.apiService.getUserPodcasts(
      this.currentPage,
      this.pageSize,
      this.sortBy,
      this.sortDirection,
      this.searchTerm
    ).subscribe({
      next: (response: any) => {
        if (response) {
          this.podcasts = response.data ?? (Array.isArray(response) ? response : []);
          this.totalPages = response.totalPages ?? 0;
          this.currentPage = response.currentPage ?? 1;
          
          if (this.podcasts.length === 0) {
            this.errorMessage = "No podcasts found in your library.";
          }
        } else {
          this.podcasts = [];
          this.errorMessage = "No data found.";
        }

        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: (err: any) => {
        this.isLoading = false;

        if (err.status === 401) {
          this.router.navigate(['/login']);
          return;
        }

        this.errorMessage = err.error?.detail || 'Failed to sync your library. Please try again.';
        AppLogger.error('Error fetching podcasts:', err);
        this.cdr.detectChanges();
      }
    });
  }

  toggleSortDirection(): void {
    this.sortDirection = this.sortDirection === 'asc' ? 'desc' : 'asc';
    this.loadUserPodcasts(1); 
  }

  downloadPodcast(id: string): void {
    this.apiService.getPodcast(id).subscribe({
      next: (blob: Blob) => {
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = `podcast-${id}.mp3`;
        link.click();
        window.URL.revokeObjectURL(url);
      },
      error: (err) => {
        if (err.status === 401) {
          this.router.navigate(['/login']);
          return;
        }
        
        const errorMsg = err.error?.detail || 'Download failed. The file might not be ready.';
        alert(errorMsg);
      }
    });
  }

  onPageSizeChange(event: any): void {
    this.pageSize = +event.target.value;
    this.loadUserPodcasts(1); 
  }

  updatePageSize(size: number) {
    this.pageSize = size;
    this.loadUserPodcasts(1); 
  }

  updateSort(direction: string) {
    this.sortDirection = direction;
    this.loadUserPodcasts(1);
  }

  trackByPodcastId(index: number, podcast: any): any {
    return podcast.podcastId;
  }
}