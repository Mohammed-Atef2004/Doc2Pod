import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-podcast-list',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './podcast-list.html',
  styleUrl: './podcast-list.css' // تأكد إنها list مش display.. دي كانت المصيبة!
})
export class PodcastListComponent {}