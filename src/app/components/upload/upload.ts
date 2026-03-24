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
      Swal.fire({ icon: 'error', title: 'Invalid File', text: 'Please select a valid PDF.', confirmButtonColor: '#fbbf24' });
    }
  }

  onDrop(event: DragEvent): void {
    event.preventDefault();
    const file = event.dataTransfer?.files[0];
    if (file && file.type === 'application/pdf') this.selectedFile = file;
  }

  onDragOver(event: DragEvent): void { event.preventDefault(); }

  // زرار واحد بيعمل كل حاجة
  startTransformation(): void {
    if (!this.selectedFile) return;

    // Validate قبل ما نبدأ
    if (this.selectedMode === 1 && !this.topic.trim()) {
      Swal.fire({ icon: 'warning', title: 'Missing Topic', text: 'Please enter a topic for Mode 1.', confirmButtonColor: '#fbbf24' });
      return;
    }
    if (this.selectedMode === 2 && (!this.topic.trim() || !this.startPage || !this.endPage)) {
      Swal.fire({ icon: 'warning', title: 'Missing Fields', text: 'Please fill topic, start page, and end page.', confirmButtonColor: '#fbbf24' });
      return;
    }

    this.isProcessing = true;
    this.progressStep = 'uploading';
    this.uploadProgress = 15;

    const formData = new FormData();
    formData.append('file', this.selectedFile, this.selectedFile.name);

    // Step 1: Upload
    this.apiService.uploadDocument(formData).subscribe({
      next: (documentId: any) => {
        this.uploadProgress = 50;
        this.progressStep = 'generating';
        console.log('Document ID:', documentId);

        // Step 2: Build generation object
        let generationData: any = {
          documentId: documentId as string,
          mode: this.selectedMode
        };

        if (this.selectedMode === 1) {
          generationData['topic'] = this.topic.trim();
        }
        if (this.selectedMode === 2) {
          generationData['topic'] = this.topic.trim();
          generationData['startPage'] = parseInt(this.startPage);
          generationData['endPage'] = parseInt(this.endPage);
        }

        console.log('Generation Payload:', generationData);

        // Step 2: Generate
        this.apiService.generatePodcast(generationData).subscribe({
          next: () => {
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
          },
          error: (err) => {
            this.isProcessing = false;
            this.progressStep = 'idle';
            console.error('Generation Error:', err);
            Swal.fire({ icon: 'warning', title: 'Generation Delayed', text: 'Document uploaded but AI service is busy.', confirmButtonColor: '#fbbf24' });
          }
        });
      },
      error: (err) => {
        this.isProcessing = false;
        this.progressStep = 'idle';
        this.uploadProgress = 0;
        console.error('Upload Error:', err);
        Swal.fire({ icon: 'error', title: 'Upload Failed', text: 'Please try again.', footer: `HTTP ${err.status}`, confirmButtonColor: '#fbbf24' });
      }
    });
  }
}