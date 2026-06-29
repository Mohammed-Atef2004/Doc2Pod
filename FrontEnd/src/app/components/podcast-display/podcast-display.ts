import {Component,OnInit,ViewChild,ElementRef, OnDestroy,ChangeDetectorRef,AfterViewInit} from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, ActivatedRoute, Router } from '@angular/router';
import { Subject, forkJoin } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { ApiService } from '../../services/api';
import { AppLogger } from '../../services/logger';

interface Podcast {
  id: string;
  title: string;
  podcastName: string;
  mode: string;          
  topic?: string;
  startPage?: number;
  endPage?: number;
  description?: string;
  imageUrl?: string;
}

interface AudioResponse {
  audioUrl: string;
}

@Component({
  selector: 'app-podcast-display',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './podcast-display.html',
  styleUrl: './podcast-display.css'
})
export class PodcastDisplayComponent implements OnInit, AfterViewInit, OnDestroy {
  @ViewChild('audioPlayer') audioPlayer!: ElementRef<HTMLAudioElement>;

  podcast: Podcast | null = null;
  audioUrl: string | null = null;
  isPlaying = false;
  progress = 0;
  currentTime = '0:00';
  duration = '0:00';
  isLoadingAudio = true;
  hasError = false;

  private destroy$ = new Subject<void>();
  private audioLoaded = false;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private apiService: ApiService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.loadPodcastData(id);
    }
  }

  ngAfterViewInit(): void {
    this.tryLoadAudioIntoPlayer();
  }

  loadPodcastData(id: string): void {
    this.isLoadingAudio = true;
    this.hasError = false;

    forkJoin({
      details: this.apiService.getPodcastDetails(id),
      audio: this.apiService.getPodcast(id)
    })
    .pipe(takeUntil(this.destroy$))
    .subscribe({
      next: ({ details, audio }: { details: Podcast; audio: AudioResponse }) => {
        this.podcast = details;
        this.audioUrl = audio.audioUrl;
        this.isLoadingAudio = false;
        this.cdr.detectChanges();
        this.tryLoadAudioIntoPlayer();
      },
      error: (err) => {
        this.isLoadingAudio = false;
        this.hasError = true;

        if (err.status === 401) {
          this.router.navigate(['/login']);
          return;
        }

        AppLogger.error('Error loading podcast data', err);
        this.cdr.detectChanges();
      }
    });
  }
  private tryLoadAudioIntoPlayer(): void {
    if (!this.audioUrl || this.audioLoaded) return;

    const player = this.audioPlayer?.nativeElement;
    if (!player) return; 
    player.src = this.audioUrl;
    player.load();
    this.audioLoaded = true;
  }

  togglePlay(): void {
    const player = this.audioPlayer?.nativeElement;
    if (!player) return;

    if (this.isPlaying) {
      player.pause();
      this.isPlaying = false;
      this.cdr.detectChanges();
    } else {
      player.play()
        .then(() => {
          this.isPlaying = true;
          this.cdr.detectChanges();
        })
        .catch((err) => {
          AppLogger.error('Playback error', err);
        });
    }
  }

  onMetadataLoaded(): void {
    const player = this.audioPlayer?.nativeElement;
    if (player) {
      this.duration = this.formatTime(player.duration);
      this.cdr.detectChanges();
    }
  }

  updateProgress(): void {
    const player = this.audioPlayer?.nativeElement;
    if (player && player.duration) {
      this.progress = (player.currentTime / player.duration) * 100;
      this.currentTime = this.formatTime(player.currentTime);
      this.cdr.detectChanges();
    }
  }

  onAudioEnded(): void {
    this.isPlaying = false;
    this.progress = 0;
    this.currentTime = '0:00';
    this.cdr.detectChanges();
  }

  skip(seconds: number): void {
    const player = this.audioPlayer?.nativeElement;
    if (player) {
      player.currentTime = Math.max(0, player.currentTime + seconds);
    }
  }

  seek(event: MouseEvent): void {
    const player = this.audioPlayer?.nativeElement;
    const bar = event.currentTarget as HTMLElement;
    const rect = bar.getBoundingClientRect();
    const x = event.clientX - rect.left;
    const percentage = Math.max(0, Math.min(1, x / rect.width));

    if (player && player.duration) {
      player.currentTime = percentage * player.duration;
    }
  }

  private formatTime(time: number): string {
    if (isNaN(time) || time === Infinity) return '0:00';
    const minutes = Math.floor(time / 60);
    const seconds = Math.floor(time % 60);
    return `${minutes}:${seconds < 10 ? '0' : ''}${seconds}`;
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
}