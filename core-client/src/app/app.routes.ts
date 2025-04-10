import { Routes } from '@angular/router';
import { HomeComponent } from './pages/home/home.component';

// Auth Components
import { LoginComponent } from './pages/auth/login/login.component';
import { RegisterComponent } from './pages/auth/register/register.component';
import { ForgotPasswordComponent } from './pages/auth/forgot-password/forgot-password.component';
import { ResetPasswordComponent } from './pages/auth/reset-password/reset-password.component';
import { VerifyOtpComponent } from './pages/auth/verify-otp/verify-otp.component';

// Account Components
import { ChangePasswordComponent } from './pages/account/change-password/change-password.component';
import { DashboardComponent } from './pages/dashboard/dashboard.component';
import { NotFoundComponent } from './pages/not-found/not-found.component';

export const routes: Routes = [
  { path: '', component: HomeComponent },

  // Auth Routes (Login, Register, External Auth)
  {
    path: 'auth',
    children: [
      { path: 'login', component: LoginComponent },
      { path: 'register', component: RegisterComponent },
      { path: 'verify-otp', component: VerifyOtpComponent },
      { path: 'forgot-password', component: ForgotPasswordComponent },
      { path: 'reset-password', component: ResetPasswordComponent },
      { path: 'callback', component: LoginComponent }, // External login redirect
    ],
  },

  // Account Routes (User Profile, Settings)
  {
    path: 'account',
    children: [
      // { path: 'profile', component: ProfileComponent },
      // { path: 'edit-profile', component: EditProfileComponent },
      { path: 'change-password', component: ChangePasswordComponent },
    ],
  },

  { path: 'dashboard', component: DashboardComponent },
  // 404 Page
  { path: 'notfound', component: NotFoundComponent },
];


// export const routes: Routes = [
//   {
//     path: '',
//     component: HomeComponent,
//   },
//   {
//     path: 'register',
//     component: RegisterComponent,
//   },
//   {
//     path: 'login',
//     component: LoginComponent,
//   },
//   { 
//     path: 'verify-otp', 
//     component: VerifyOtpComponent 
//   },
//   {
//     path: 'forgot-password',
//     component: ForgotPasswordComponent,
//   },
//   {
//     path: 'reset-password',
//     component: ResetPasswordComponent,
//   },
//   {
//     path: 'change-password',
//     component: ChangePasswordComponent,
//   },
//   {
//     path: 'auth/callback',
//     component: LoginComponent,
//   },
//   {
//     path: 'dashboard',
//     component: DashboardComponent,
//   }
// ];