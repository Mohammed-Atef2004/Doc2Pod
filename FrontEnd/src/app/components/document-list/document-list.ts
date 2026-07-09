import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../services/api';
import { ChangeDetectorRef } from '@angular/core';
import { AppLogger } from '../../services/logger';

interface UserDocument {
  documentId: string;
  documentName: string;
  createdAt: string;
}

@Component({
  selector: 'app-document-list',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule
  ],
  templateUrl: './document-list.html',
  styleUrl: './document-list.css'
})
export class DocumentListComponent implements OnInit {

  documents: UserDocument[] = [];
  isLoading = true;

  currentPage = 1;
  totalPages = 1;
  pageSize = 5;
  sortBy = 'date';
  sortDirection = 'desc';
  searchTerm = '';

  constructor(
    private apiService: ApiService,
    private router: Router,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.loadDocuments();
  }

  loadDocuments(page: number = this.currentPage): void {
    AppLogger.log('loadDocuments started');
    this.currentPage = page;
    this.isLoading = true;

    this.apiService.getMyDocuments(
      this.currentPage,
      this.pageSize,
      this.sortBy,
      this.sortDirection,
      this.searchTerm
    ).subscribe({
      next: (response: any) => {
        if (response) {
          this.documents = response.data ?? (Array.isArray(response) ? response : []);
          this.currentPage = response.currentPage ?? 1;
          this.totalPages = Math.ceil(response.totalItems / response.pageSize) || 1;
        }

        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        this.isLoading = false;
    
        if (err.status === 401) {
          this.router.navigate(['/login']);
          return;
        }

        AppLogger.error('Error loading documents:', err);
        this.cdr.detectChanges();
      }
    });
  }

  updatePageSize(size: number): void {
    this.pageSize = size;
    this.currentPage = 1;
    this.loadDocuments(1);
  }

  updateSort(direction: string): void {
    this.sortDirection = direction;
    this.currentPage = 1;
    this.loadDocuments(1);
  }

  onSearch(): void {
    this.currentPage = 1;
    this.loadDocuments(1);
  }

  selectDocument(doc: UserDocument): void {
  this.router.navigate(['/upload'], {
    state: {
      documentId: doc.documentId,
      fileName: doc.documentName
    }
  });
}
}