/**
 * Application Routes Configuration
 *
 * This file defines all the routes for the Angular application.
 * Routes are organized by feature modules with proper guard protection.
 *
 * @module AppRoutingModule
 */

import { Routes } from '@angular/router';

// ============================================
// Layout Components
// ============================================
import { HomeComponent } from './layout/pages/home/home.component';
import { DashboardComponent } from './layout/pages/dashboard/dashboard.component';
import { NotFoundComponent } from './layout/pages/not-found/not-found.component';
import { LayoutComponent } from './layout/components/layout/layout.component';

// ============================================
// Auth Module Components
// ============================================
import { RegisterComponent } from './modules/auth/components/register/register.component';
import { LoginComponent } from './modules/auth/components/login/login.component';
import { VerifyOtpComponent } from './modules/auth/components/verify-otp/verify-otp.component';
import { ForgotPasswordComponent } from './modules/auth/components/forgot-password/forgot-password.component';
import { ResetPasswordComponent } from './modules/auth/components/reset-password/reset-password.component';
import { UserRoleManagementComponent } from './modules/auth/components/user-role/user-role-management.component';

// ============================================
// Account Module Components
// ============================================
import { ChangePasswordComponent } from './modules/account/components/change-password/change-password.component';

// ============================================
// Customer Module Components
// ============================================
import { CustomerListComponent } from './modules/customer/components/customer-list/customer-list.component';

// ============================================
// Company Module Components
// ============================================
import { SelectCompanyComponent } from './modules/company/components/select-company/select-company.component';
import { CompanyRequestsComponent } from './modules/company/components/company-requests/company-requests.component';
import { CompanyRequestDetailComponent } from './modules/company/components/company-requests/company-request-detail.component';

// ============================================
// Invoice Module Components
// ============================================
import { InvoiceSettingsComponent } from './modules/settings/components/invoice-settings/invoice-settings.component';
import { CreateInvoiceComponent } from './modules/invoice/standard-invoice/components/invoice-upsert/create-invoice.component';
import { InvoiceComponent } from './modules/invoice/standard-invoice/components/invoice-list/invoice-list.component';
import { EmailSettingsComponent } from './modules/settings/components/email-settings/email-settings.component';
import { InvoiceDisplayComponent } from './modules/invoice/standard-invoice/components/invoice-display/invoice-display.component';

// ============================================
// Tasks Module Components
// ============================================
import { TaskListComponent } from './modules/tasks/components/task-list/task-list.component';

// ============================================
// Features Components
// ============================================
// import { CustomerDashboardComponent } from './features/areas/customer/components/customer-dashboard-stats/customer-dashboard.component';

// ============================================
// Route Guards
// ============================================
import { AuthGuard } from './core/guards/auth.guard';
import { AdminGuard } from './core/guards/admin.guard';
import { OtpGuard } from './core/guards/otp.guard';
import { UnauthGuard } from './core/guards/unauth.guard';
import { CustomerGuard } from './core/guards/customer.guard';
import { UserGuard } from './core/guards/user.guard';
import { InvoiceAccessGuard } from './core/guards/invoice-access.guard';
import { RedirectGuard } from './core/guards/redirect.guard';
import { AdminOrUserGuard } from './core/guards/admin-or-user.guard';
import { TaskDetailComponent } from './modules/tasks/components/task-detail/task-detail.component';
import { RecurringInvoiceListComponent } from './modules/invoice/recurring-invoice/components/recurring-invoice-list/recurring-invoice-list.component';
import { CustomerDetailsComponent } from './modules/customer/components/customer-details/customer-details.component';

/**
 * Main application routes configuration
 *
 * Route organization:
 * 1. Public routes (home, auth)
 * 2. Protected routes (wrapped in LayoutComponent)
 * 3. Redirect routes
 * 4. Fallback routes (404)
 */

export const routes: Routes = [
  // ============================================
  // Public Routes
  // ============================================

  /**
   * Home/Landing Page Route
   * Accessible to all users (authenticated and unauthenticated)
   */
  { path: '', component: HomeComponent },

  /**
   * Authentication Routes
   * All auth-related routes are prefixed with /auth
   * Protected by UnauthGuard to prevent authenticated users from accessing
   */
  {
    path: 'auth',
    children: [
      {
        path: 'login',
        component: LoginComponent,
        canActivate: [UnauthGuard],
        data: { title: 'Sign In' },
      },
      {
        path: 'register',
        component: RegisterComponent,
        canActivate: [UnauthGuard],
        data: { title: 'Create Account' },
      },
      {
        path: 'verify-otp',
        component: VerifyOtpComponent,
        // canActivate: [OtpGuard],
        data: { title: 'Verify OTP' },
      },
      {
        path: 'forgot-password',
        component: ForgotPasswordComponent,
        canActivate: [UnauthGuard],
        data: { title: 'Forgot Password' },
      },
      {
        path: 'reset-password',
        component: ResetPasswordComponent,
        canActivate: [UnauthGuard],
        data: { title: 'Reset Password' },
      },
      {
        path: 'callback',
        component: LoginComponent,
        canActivate: [UnauthGuard],
        data: { title: 'Authentication Callback' },
      },
      {
        path: 'select-company',
        component: SelectCompanyComponent,
        canActivate: [UnauthGuard],
        data: { title: 'Select Company' },
      },
    ],
  },

  // ============================================
  // Protected Routes (Authenticated Users Only)
  // ============================================
  {
    path: '',
    component: LayoutComponent,
    canActivate: [AuthGuard],
    children: [
      /**
       * Dashboard Routes
       * Role-specific dashboards with appropriate guards
       */
      {
        path: 'dashboard',
        component: DashboardComponent,
        canActivate: [AdminOrUserGuard],
        data: { title: 'Dashboard', roles: ['Admin', 'User', 'Super Admin'] },
      },
      {
        path: 'customer-dashboard',
        component: DashboardComponent,
        canActivate: [CustomerGuard], // Customer only
        data: { title: 'Customer Dashboard', roles: ['Customer'] },
      },

      /**
       * Company Management Routes
       * Admin only routes for company request management
       */
      {
        path: 'company-requests',
        component: CompanyRequestsComponent,
        canActivate: [AdminGuard], // Protect admin routes
        data: { title: 'Company Requests', roles: ['Admin'] },
      },
      {
        path: 'company-requests/:id',
        component: CompanyRequestDetailComponent,
        canActivate: [AdminGuard], // Protect admin routes
        data: { title: 'Company Request Details', roles: ['Admin'] },
      },

      /**
       * Invoice Management Routes
       * Accessible based on user roles and permissions
       */
      {
        path: 'invoices',
        children: [
          {
            path: '',
            component: InvoiceComponent,
            canActivate: [InvoiceAccessGuard], // All authenticated users
            data: { title: 'Invoices' },
          },
          {
            path: 'create',
            component: CreateInvoiceComponent,
            canActivate: [UserGuard], // Super Admin, Admin, User
            data: {
              title: 'Create Invoice',
              roles: ['Admin', 'User', 'Super Admin'],
            },
          },
          {
            path: 'edit/:id',
            component: CreateInvoiceComponent,
            canActivate: [UserGuard], // Super Admin, Admin, User
            data: {
              title: 'Edit Invoice',
              roles: ['Admin', 'User', 'Super Admin'],
            },
          },
          {
            path: 'view/:id',
            component: InvoiceDisplayComponent,
            canActivate: [InvoiceAccessGuard], // All authenticated users
            data: { title: 'View Invoice' },
          },
          // {
          //   path: 'approve/:id',
          //   component: InvoiceApprovalComponent,
          //   canActivate: [AdminGuard], // Super Admin, Admin only
          // },
        ],
      },

      /**
       * Recurring Invoice Management Routes
       * Accessible based on user roles and permissions
       */
       {
        path: 'invoices/recurring',
        children: [
          {
            path: '',
            component: RecurringInvoiceListComponent,
            canActivate: [InvoiceAccessGuard], // All authenticated users
            data: { title: 'Invoices' },
          },
        ]
      },
      /**
       * Customer Management Routes
       * Admin only for full access, view access for users
       */
      {
        path: 'customers',
        children: [
          {
            path: '',
            component: CustomerListComponent,
            canActivate: [AdminGuard], // Super Admin, Admin only
            data: { title: 'Customers', roles: ['Admin'] },
          },
           {
            path: 'view/:id',
            component: CustomerDetailsComponent,
            canActivate: [AdminGuard], // Super Admin, Admin only
            data: { title: 'Customers', roles: ['Admin'] },
          },
        ],
      },

      /**
       * Payments Routes
       * Placeholder for future payment management features
       */
      {
        path: 'payments',
        children: [
          // {
          //   path: '',
          //   component: PaymentsComponent,
          //   canActivate: [InvoiceAccessGuard], // All authenticated users
          // },
          // {
          //   path: 'reconcile',
          //   component: PaymentsComponent,
          //   canActivate: [AdminGuard], // Super Admin, Admin only
          // },
        ],
      },

      /**
       * Task Management Routes
       * Available to all authenticated users
       */
      {
        path: 'tasks',
        children: [
          { path: 'list', component: TaskListComponent },
          { path: 'view/:id', component: TaskDetailComponent },
          { path: '', redirectTo: 'list', pathMatch: 'full' },
        ],
        data: { title: 'Task Management' },
      },

      /**
       * Account Management Routes
       * Available to all authenticated users
       */
      {
        path: 'account',
        children: [
          {
            path: 'change-password',
            component: ChangePasswordComponent,
            data: { title: 'Change Password' },
          },
        ],
      },

      /**
       * User Management Routes
       * Admin only for user administration
       */
      {
        path: 'users',
        children: [],
        data: { title: 'User Management', roles: ['Admin'] },
      },

      /**
       * System Settings Routes
       * Admin only for configuration
       */
      {
        path: 'settings',
        children: [
          {
            path: 'invoice',
            component: InvoiceSettingsComponent,
            canActivate: [AdminGuard], // Super Admin, Admin only
            data: { title: 'Invoice Settings', roles: ['Admin'] },
          },
          {
            path: 'email',
            component: EmailSettingsComponent,
            canActivate: [AdminGuard], // Super Admin, Admin only
            data: { title: 'Email Settings', roles: ['Admin'] },
          },
        ],
      },
    ],
  },

  // ============================================
  // Special Routes
  // ============================================
  /**
   * Post-login redirect route
   * Handles redirection after successful OTP verification
   */
  {
    path: 'redirect-after-login',
    canActivate: [RedirectGuard],
    component: NotFoundComponent,
    data: { title: 'Redirecting...' },
  },

  /**
   * 404 Not Found Route
   * Catch-all for undefined routes
   */
  {
    path: 'notfound',
    component: NotFoundComponent,
    data: { title: 'Page Not Found' },
  },
  /**
   * Wildcard Route - Redirect all unknown paths to 404
   * Must be the last route in the configuration
   */
  { path: '**', redirectTo: '/notfound' },
];
