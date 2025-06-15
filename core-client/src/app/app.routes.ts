import { Routes } from '@angular/router';
import { HomeComponent } from './pages/home/home.component';

// Auth Components
import { AuthComponent } from './pages/auth/auth.component';
// import { LoginComponent } from './pages/auth/login/login.component';
// import { RegisterComponent } from './pages/auth/register/register.component';
import { ForgotPasswordComponent } from './pages/auth/forgot-password/forgot-password.component';
import { ResetPasswordComponent } from './pages/auth/reset-password/reset-password.component';
import { VerifyOtpComponent } from './pages/auth/verify-otp/verify-otp.component';

// Account Components
import { ChangePasswordComponent } from './pages/account/change-password/change-password.component';
import { DashboardComponent } from './pages/dashboard/dashboard.component';
import { NotFoundComponent } from './pages/not-found/not-found.component';
import { LayoutComponent } from './components/layout/layout.component';
import { AuthGuard } from './guards/auth.guard';
import { OrdersComponent } from './pages/orders/orders.component';
import { InvoiceDashboardComponent } from './pages/invoices/invoice-dashboard.component';
import { CustomerComponent } from './pages/customers/customer.component';
import { LoginComponent } from './pages/auth/login/login.component';
import { RegisterComponent } from './pages/auth/register/register.component';

export const routes: Routes = [
  { path: '', component: HomeComponent },

  // Auth Routes (Login, Register, External Auth)
  {
    path: 'auth',
    children: [
      // { path: 'login', component: AuthComponent, data: { mode: 'login' } }, // Set mode to login
      // {
      //   path: 'register',
      //   component: AuthComponent,
      //   data: { mode: 'register' },
      // }, // Set mode to register
      { path: 'login', component: LoginComponent },
      { path: 'register', component: RegisterComponent },
      { path: 'verify-otp', component: VerifyOtpComponent },
      { path: 'forgot-password', component: ForgotPasswordComponent },
      { path: 'reset-password', component: ResetPasswordComponent },
      { path: 'callback', component: AuthComponent, data: { mode: 'login' } }, // External login redirect
    ],
  },

  // Authenticated routes (wrapped in LayoutComponent)
  {
    path: '',
    component: LayoutComponent,
    // canActivate: [AuthGuard], // Protect these routes
    children: [
      { path: 'dashboard', component: DashboardComponent },
      { path: 'customers', component: CustomerComponent },
      { path: 'orders', component: OrdersComponent },
      { path: 'invoices', component: InvoiceDashboardComponent },
      {
        path: 'account',
        children: [
          { path: 'change-password', component: ChangePasswordComponent },
        ],
      },
    ],
  },

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
