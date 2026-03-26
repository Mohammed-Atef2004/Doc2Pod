import { Component } from '@angular/core';
import { RouterLink, Router } from '@angular/router';
import { ApiService } from '../../services/api';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import Swal from 'sweetalert2';

@Component({
  selector: 'app-upload',
  standalone: true,
  imports: [RouterLink, CommonModule, FormsModule],
  templateUrl: './upload.html',
  styleUrl: './upload.css'
})
export class UploadComponent {
  selectedFile: File | null = null;
  selectedVoice: string = 'Friendly Egyptian';

  isProcessing: boolean = false;
  progressStep: 'idle' | 'uploading' | 'generating' | 'done' = 'idle';
  uploadProgress: number = 0;

  selectedMode: number = 1;
  topic: string = '';
  startPage: string = '';
  endPage: string = '';

  constructor(private apiService: ApiService, private router: Router) {}

  onFileSelected(event: any): void {
    const file = event.target.files[0];
    if (file && file.type === 'application/pdf') {
      this.selectedFile = file;
    } else {
      this.showSwal('error', 'Invalid File', 'Please select a valid PDF.');
    }
  }

  onDrop(event: DragEvent): void {
    event.preventDefault();
    const file = event.dataTransfer?.files[0];
    if (file && file.type === 'application/pdf') {
      this.selectedFile = file;
    }
  }

  onDragOver(event: DragEvent): void {
    event.preventDefault();
  }

  startTransformation(): void {
    if (!this.selectedFile) return;

    const mode = Number(this.selectedMode);

    if (mode === 1 && !this.topic.trim()) {
      this.showSwal('warning', 'Missing Topic', 'Please enter a topic for Mode 1.');
      return;
    }
    if (mode === 2 && (!this.topic.trim() || !this.startPage || !this.endPage)) {
      this.showSwal('warning', 'Missing Fields', 'Please fill topic, start page, and end page.');
      return;
    }

    this.isProcessing = true;
    this.progressStep = 'uploading';
    this.uploadProgress = 15;

    const formData = new FormData();
    formData.append('File', this.selectedFile, this.selectedFile.name);

    this.apiService.uploadDocument(formData).subscribe({
      next: (documentId: any) => {
        this.uploadProgress = 50;
        this.progressStep = 'generating';


        let generationData: any = {
          documentId: documentId.toString(),
          mode: mode
        };

        if (mode === 1 || mode === 2) {
          generationData.topic = this.topic.trim();
        }

        if (mode === 2) {
          generationData.startPage = parseInt(this.startPage);
          generationData.endPage = parseInt(this.endPage);
        }

        console.log("Final Payload being sent to API:", generationData);


        this.apiService.generatePodcast(generationData).subscribe({
          next: () => {
            this.handleSuccess();
          },
          error: (err) => {
            this.handleError('Generation Failed', err);
          }
        });
      },
      error: (err) => {
        this.handleError('Upload Failed', err);
      }
    });
  }

  private handleSuccess(): void {
    this.uploadProgress = 100;
    this.progressStep = 'done';
    this.isProcessing = false;

    Swal.fire({
      icon: 'success',
      title: 'Podcast is Being Generated!',
      text: 'Redirecting to Library...',
      confirmButtonColor: '#fbbf24',
      timer: 3000,
      timerProgressBar: true
    }).then(() => this.router.navigate(['/podcast-list']));
  }

  private handleError(title: string, err: any): void {
    this.isProcessing = false;
    this.progressStep = 'idle';
    this.uploadProgress = 0;
    console.error(`${title}:`, err);
    
    const errorMsg = err.error?.errors ? JSON.stringify(err.error.errors) : 'Please try again.';
    
    Swal.fire({
      icon: 'error',
      title: title,
      text: errorMsg,
      footer: `Status: ${err.status}`,
      confirmButtonColor: '#fbbf24'
    });
  }

  private showSwal(icon: any, title: string, text: string): void {
    Swal.fire({ icon, title, text, confirmButtonColor: '#fbbf24' });
  }
}