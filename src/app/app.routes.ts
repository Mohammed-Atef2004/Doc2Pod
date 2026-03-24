import { Routes } from '@angular/router';
import { HomeComponent } from './components/home/home';
import { LoginComponent } from './components/login/login';
import { RegisterComponent } from './components/register/register';
import { ProfileComponent } from './components/profile/profile';
import { UploadComponent } from './components/upload/upload';
import { PodcastListComponent } from './components/podcast-list/podcast-list';
import { PodcastDisplayComponent } from './components/podcast-display/podcast-display';

export const routes: Routes = [

  { path: '', component: HomeComponent },

  { path: 'login', component: LoginComponent },

  { path: 'register', component: RegisterComponent },

  { path: 'profile', component: ProfileComponent },

  { path: 'upload', component: UploadComponent },

  { path: 'podcasts', component: PodcastListComponent },

  { path: 'display', component: PodcastDisplayComponent }

];