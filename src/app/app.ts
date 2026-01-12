import { Component } from '@angular/core';
import { RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive], // لازم يكونوا هنا
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class AppComponent {}