import { Routes } from '@angular/router';
import { HomeComponent } from './components/home/home';
import { LoginComponent } from './components/login/login';
import { RegisterComponent } from './components/register/register';
import { ProfileComponent } from './components/profile/profile';
import { UploadComponent } from './components/upload/upload';
import { PodcastListComponent } from './components/podcast-list/podcast-list';
import { PodcastDisplayComponent } from './components/podcast-display/podcast-display';
import { ConfirmEmail } from './components/confirm-email/confirm-email';
import { EmailUpdateConfirm } from './components/email-update-confirm/email-update-confirm';
import { ResetPassword } from './components/reset-password/reset-password';
import { DocumentListComponent } from './components/document-list/document-list';
export const routes: Routes = [

  { path: '', component: HomeComponent },

  { path: 'login', component: LoginComponent },

  { path: 'register', component: RegisterComponent },

  { path: 'confirm-email', component: ConfirmEmail },
  
  { path: 'email-update-confirm', component: EmailUpdateConfirm },
  
  { path: 'reset-password', component: ResetPassword },

  { path: 'profile', component: ProfileComponent },

  { path: 'upload', component: UploadComponent },

  { path: 'podcasts', component: PodcastListComponent },

  { path: 'documents', component: DocumentListComponent },

  { path: 'display/:id', component: PodcastDisplayComponent }
  // { path: 'display', component: PodcastDisplayComponent }

];