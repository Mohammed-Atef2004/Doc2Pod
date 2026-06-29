import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { RouterLink, Router, ActivatedRoute } from '@angular/router';
import { ApiService } from '../../services/api';
import { SignalrService } from '../../services/signalr.service';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Subscription } from 'rxjs';
import { AppLogger } from '../../services/logger';

import Swal from 'sweetalert2';
import * as pdfjsLib from 'pdfjs-dist';

@Component({
  selector: 'app-upload',
  standalone: true,
  imports: [
    RouterLink,
    CommonModule,
    FormsModule
  ],
  templateUrl: './upload.html',
  styleUrl: './upload.css'
})
export class UploadComponent implements OnInit, OnDestroy {

  selectedFile: File | null = null;
  selectedDocumentId: string | null = null;
  selectedDocumentName: string = '';
  selectedVoice: string = 'Friendly Egyptian';

  isProcessing: boolean = false;
  isAlreadyRunning: boolean = false;
  
  totalPages: number = 0;
  selectedMode: number = 1;
  topic: string = '';
  startPage: string = '';
  endPage: string = '';

  uploadProgress: number = 0;

  private signalrSub!: Subscription;
  private reconnectedSub!: Subscription;

  constructor(
    private apiService: ApiService,
    private router: Router,
    private route: ActivatedRoute,
    private signalrService: SignalrService,
    private cdr: ChangeDetectorRef
  ) { }

  ngOnInit(): void {

  pdfjsLib.GlobalWorkerOptions.workerSrc = 'assets/pdf.worker.min.mjs';

  const state = history.state;

  if (state?.documentId) {
    this.selectedDocumentId = state.documentId;
    this.selectedDocumentName = state.fileName || 'Selected PDF';
    this.selectedFile = null;
  }

  this.signalrService.startConnection();

  this.reconnectedSub = this.signalrService.onReconnected().subscribe(() => {
    AppLogger.log('SignalR reconnected');
    this.checkCurrentStatus();
  });

  this.signalrSub = this.signalrService.onStatusChanged().subscribe({
    next: (data) => {

      AppLogger.log('SignalR received:', data);

      if (data?.status) {
        this.handleServerStatus(data.status);
      }
    },
    error: (err) => {
      AppLogger.error('SignalR error:', err);
    }
  });

  this.checkCurrentStatus();
}

  ngOnDestroy(): void {
    this.signalrSub?.unsubscribe();
    this.reconnectedSub?.unsubscribe();
  }

  private checkCurrentStatus(): void {

    this.apiService.getCurrentStatus().subscribe({
      next: (res: any) => {

        if (!res?.status) return;

        const status = res.status.toString().trim().toLowerCase();

        AppLogger.log('API Current Status:', status);

        if (
          status === 'processing' ||
          status === 'pending' ||
          status === '1' ||
          status === '2'
        ) {
          this.isProcessing = true;
          this.isAlreadyRunning = true;
        }

        else if (
          status === 'completed' ||
          status === 'done' ||
          status === '3'
        ) {
          this.showCompletedPopup();
        }
        else if (status === 'failed' ) 
        {
          this.handleServerStatus(res);
        }
        this.cdr.detectChanges();
      },
      error: (err) => {
        AppLogger.error('Status error:', err);
      }
    });
  }
  onFileSelected(event: any): void {

    if (this.isProcessing || this.isAlreadyRunning) {
      return;
    }
    const file = event.target.files[0];
    if (!file) {
      return;
    }
    const maxSize = 50 * 1024 * 1024; 
    if (file.type !== 'application/pdf') {
      this.selectedFile = null;
      this.showSwal(
        'error',
        'Invalid File',
        'Please select a valid PDF.'
      );

      event.target.value = '';
      return;
    }
    if (file.size > maxSize) {
      this.selectedFile = null;
      this.showSwal(
        'error',
        'File Too Large',
        'Maximum allowed size is 50 MB.'
      );
      event.target.value = '';
      return;
    }

    this.selectedFile = file;
    this.selectedDocumentId = null;
    this.selectedDocumentName = '';

    const reader = new FileReader();
    reader.onload = async () => {
      const typedArray = new Uint8Array(
        reader.result as ArrayBuffer
      );
      const pdf = await pdfjsLib
        .getDocument(typedArray)
        .promise;
      this.totalPages = pdf.numPages;
      AppLogger.log('PDF Pages:', this.totalPages);
    };
    reader.readAsArrayBuffer(file);


  }

  startTransformation(): void {

    if (this.isProcessing || this.isAlreadyRunning) {
      this.showSwal('info', 'Wait', 'Podcast is already generating.');
      return;
    }

    if (!this.selectedFile && !this.selectedDocumentId) {
      this.showSwal('warning', 'No Document', 'Upload or choose a PDF.');
      return;
    }

    const mode = Number(this.selectedMode);

    if (mode === 1 && !this.topic.trim()) {
      this.showSwal('warning', 'Missing Topic', 'Please enter topic.');
      return;
    }

    if (mode === 2) {

      if (!this.topic.trim() || !this.startPage || !this.endPage) {
        this.showSwal('warning', 'Missing Fields', 'Please fill all fields.');
        return;
      }

      const start = parseInt(this.startPage, 10);
      const end = parseInt(this.endPage, 10);

      if (isNaN(start) || isNaN(end)) {
        this.showSwal(
          'warning',
          'Invalid Pages',
          'Please enter valid page numbers.'
        );
        return;
      }

      if (start < 1) {
        this.showSwal(
          'warning',
          'Invalid Start Page',
          'Start page must be greater than 0.'
        );
        return;
      }

      if (this.selectedFile && end > this.totalPages) {
        this.showSwal(
          'warning',
          'Invalid End Page',
          `This PDF contains only ${this.totalPages} pages.`
        );
        return;
      }

      if (start > end) {
        this.showSwal(
          'warning',
          'Invalid Range',
          'Start page cannot be greater than end page.'
        );
        return;
      }
    }

    if (this.selectedDocumentId) {
      this.generatePodcast(this.selectedDocumentId, mode);
      return;
    }

    this.uploadNewDocument(mode);
}

  private uploadNewDocument(mode: number): void {

    this.isProcessing = true;

    const formData = new FormData();
    formData.append('File', this.selectedFile!, this.selectedFile!.name);

    this.apiService.uploadDocument(formData).subscribe({
      next: (response: any) => {

        const documentId =
          response?.documentId ??
          response?.value ??
          response;

        this.generatePodcast(documentId.toString(), mode);
      },
      error: (err) => {
      this.handleError('Upload Failed', err);
      }});
  }

  private generatePodcast(documentId: string, mode: number): void {
    const body: any = {
      documentId,
      mode
    };

    if (mode === 1 || mode === 2) {
      body.topic = this.topic.trim();
    }
    if ( mode === 2) {
      body.startPage = parseInt(this.startPage, 10);
      body.endPage = parseInt(this.endPage, 10);
    }
    this.isProcessing = true;
    this.apiService.generatePodcast(body).subscribe({
      next: () => {

        this.isAlreadyRunning = true;

        Swal.fire({
          icon: 'success',
          title: 'Started',
          text: 'Podcast generation started.',
          confirmButtonColor: '#fbbf24'
        });
      },
      error: (err) => {
        this.handleError('Generation Failed', err);
      }
    });
  }

  public handleServerStatus(data: any): void {
  if (!data) return;
  const s = (data.status ?? data).toString().trim().toLowerCase();
    AppLogger.log('SignalR status:', s);
    switch (s) {
      case 'processing':
      case 'pending':
      case '1':
      case '2':
        this.isProcessing = true;
        this.isAlreadyRunning = true;
        break;

      case 'completed':
      case 'done':
      case '3':
        this.showCompletedPopup();
        break;

      case 'failed':
        this.showSwal(
          'error',
          'Generation Failed',
          'Something went wrong, please try again.'
        );
      this.resetStateAfterError();
      break;
          }
    this.cdr.detectChanges();
  }

  private showCompletedPopup(): void {

    Swal.fire({
  icon: 'success',
  title: 'Your Podcast is Ready! 🎧',
  text: 'The document has been successfully transformed into a podcast.',
  confirmButtonColor: '#fbbf24',
  confirmButtonText: 'Go to Library 🎧',
  allowOutsideClick: true
    }).then((result) => {
      this.resetUpload(); 
      if (result.isConfirmed) {
        this.goToLibrary(); 
      }
    });
  }

  resetUpload(): void {

    this.selectedFile = null;
    this.selectedDocumentId = null;
    this.selectedDocumentName = '';
    this.topic = '';
    this.startPage = '';
    this.endPage = '';

    this.resetStateAfterError();
  }

  private resetStateAfterError(): void {

    this.isProcessing = false;
    this.isAlreadyRunning = false;
    this.uploadProgress = 0;

    this.cdr.detectChanges();
  }

  private handleError(title: string, err: any): void {

    this.resetStateAfterError();

    if (err.status === 401) {

      Swal.fire({
        icon: 'warning',
        title: 'Session Expired',
        text: 'Please login again.',
        confirmButtonColor: '#fbbf24'
      }).then(() => {
        this.router.navigate(['/login']);
      });

      return;
    }
    if (err.status === 413) {

      Swal.fire({
        icon: 'error',
        title: 'File Too Large',
        text: 'Maximum allowed file size is 50 MB.',
        confirmButtonColor: '#fbbf24'
      });

      return;
    }
    const msg =
      err.error?.detail ||
      err.error?.message ||
      'Something went wrong';

    this.showSwal('error', title, msg);
  }

  private showSwal(icon: any, title: string, text: string): void {

    Swal.fire({
      icon,
      title,
      text,
      confirmButtonColor: '#fbbf24'
    });
  }

  goToDocuments(): void {
    this.router.navigate(['/documents']);
  }

  goToLibrary(): void {
    this.router.navigate(['/podcasts']);
  }
}