// app.routes.ts
import { Routes } from '@angular/router';
import { HomeComponent } from './pages/home/home.component';

// Auth Components
import { RegisterComponent } from './pages/auth/register/register.component';
import { LoginComponent } from './pages/auth/login/login.component';
import { VerifyOtpComponent } from './pages/auth/verify-otp/verify-otp.component';
import { ForgotPasswordComponent } from './pages/auth/forgot-password/forgot-password.component';
import { ResetPasswordComponent } from './pages/auth/reset-password/reset-password.component';

// Account Components
import { ChangePasswordComponent } from './pages/account/change-password/change-password.component';
import { DashboardComponent } from './pages/dashboard/dashboard.component';
import { NotFoundComponent } from './pages/not-found/not-found.component';
import { LayoutComponent } from './components/layout/layout.component';
import { AuthGuard } from './guards/auth.guard';
import { OrdersComponent } from './pages/orders/orders.component';
import { CustomerComponent } from './pages/customers/customer.component';
import { UserRoleManagementComponent } from './pages/auth/user-role/user-role-management.component';
import { AdminGuard } from './guards/admin.guard';
import { ProductComponent } from './pages/products/product.component';
import { OtpGuard } from './guards/otp.guard';
import { UnauthGuard } from './guards/unauth.guard';
import { CartComponent } from './pages/cart/cart.component';
import { SelectCompanyComponent } from './pages/companies/select-company.component';
import { InvoiceSettingsComponent } from './features/invoices/components/invoice-settings/invoice-settings.component';
import { CreateInvoiceComponent } from './features/invoices/components/upsert-invoice/create-invoice.component';
import { InvoiceComponent } from './features/invoices/components/invoice-list/invoice-list.component';
import { EmailSettingsComponent } from './settings/email/email-settings.component';
import { CustomerGuard } from './guards/customer.guard';
import { UserGuard } from './guards/user.guard';
import { CustomerDashboardComponent } from './features/areas/customer/customer-dashboard.component';
import { InvoiceDisplayComponent } from './features/invoices/components/invoice-display/invoice-display/invoice-display.component';
import { InvoiceAccessGuard } from './guards/invoice-access.guard';
import { RedirectGuard } from './guards/redirect.guard';
import { AdminOrUserGuard } from './guards/admin-or-user.guard';
import { CustomerListComponent } from './pages/customers/customer-list.component';
import { CompanyRequestsComponent } from './pages/company-requests/company-requests.component';
import { CompanyRequestDetailComponent } from './pages/company-requests/company-request-detail.component';

export const routes: Routes = [
  { path: '', component: HomeComponent },

  // Auth Routes
  {
    path: 'auth',
    children: [
      {
        path: 'login',
        component: LoginComponent,
        canActivate: [UnauthGuard],
      },
      {
        path: 'register',
        component: RegisterComponent,
        canActivate: [UnauthGuard],
      },
      {
        path: 'verify-otp',
        component: VerifyOtpComponent,
        canActivate: [OtpGuard],
      },
      {
        path: 'forgot-password',
        component: ForgotPasswordComponent,
        canActivate: [UnauthGuard],
      },
      {
        path: 'reset-password',
        component: ResetPasswordComponent,
        canActivate: [UnauthGuard],
      },
      {
        path: 'callback',
        component: LoginComponent,
        canActivate: [UnauthGuard],
      },
      {
        path: 'select-company',
        component: SelectCompanyComponent,
        canActivate: [UnauthGuard],
      },
    ],
  },

  // Authenticated routes (wrapped in LayoutComponent)
  {
    path: '',
    component: LayoutComponent,
    canActivate: [AuthGuard],
    children: [
      // Dashboard Routes - Role Specific
      {
        path: 'dashboard',
        component: DashboardComponent,
        canActivate: [AdminOrUserGuard], // Super Admin, Admin, User
      },
      {
        path: 'customer-dashboard',
        component: CustomerDashboardComponent,
        canActivate: [CustomerGuard], // Customer only
      },
      {
        path: 'company-requests',
        component: CompanyRequestsComponent,
        canActivate: [AdminGuard], // Protect admin routes
      },
      {
        path: 'company-requests/:id',
        component: CompanyRequestDetailComponent,
        canActivate: [AdminGuard], // Protect admin routes
      },
      // Invoice Management
      {
        path: 'invoices',
        children: [
          {
            path: '',
            component: InvoiceComponent,
            canActivate: [InvoiceAccessGuard], // All authenticated users
          },
          {
            path: 'create',
            component: CreateInvoiceComponent,
            canActivate: [UserGuard], // Super Admin, Admin, User
          },
          {
            path: 'edit/:id',
            component: CreateInvoiceComponent,
            canActivate: [UserGuard], // Super Admin, Admin, User
          },
          {
            path: 'view/:id',
            component: InvoiceDisplayComponent,
            canActivate: [InvoiceAccessGuard], // All authenticated users
          },
          // {
          //   path: 'approve/:id',
          //   component: InvoiceApprovalComponent,
          //   canActivate: [AdminGuard], // Super Admin, Admin only
          // },
        ],
      },

      // Customer Management
      {
        path: 'customers',
        children: [
          {
            path: '',
            component: CustomerListComponent,
            canActivate: [AdminGuard], // Super Admin, Admin only
          },
          {
            path: 'view/:id',
            component: CustomerComponent,
            canActivate: [AdminOrUserGuard], // Super Admin, Admin, User
          },
          {
            path: 'create',
            component: CustomerComponent,
            canActivate: [AdminGuard], // Super Admin, Admin only
          },
          {
            path: 'edit/:id',
            component: CustomerComponent,
            canActivate: [AdminGuard], // Super Admin, Admin only
          },
        ],
      },

      // Payments
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

      // Products
      {
        path: 'products',
        children: [
          {
            path: '',
            component: ProductComponent,
            canActivate: [AdminOrUserGuard], // Super Admin, Admin, User
          },
          {
            path: 'create',
            component: ProductComponent,
            canActivate: [AdminGuard], // Super Admin, Admin only
          },
          {
            path: 'edit/:id',
            component: ProductComponent,
            canActivate: [AdminGuard], // Super Admin, Admin only
          },
        ],
      },

      // Orders
      {
        path: 'orders',
        children: [
          {
            path: '',
            component: OrdersComponent,
            canActivate: [AdminOrUserGuard], // Super Admin, Admin, User
          },
          {
            path: 'create',
            component: OrdersComponent,
            canActivate: [UserGuard], // Super Admin, Admin, User
          },
          {
            path: 'view/:id',
            component: OrdersComponent,
            canActivate: [InvoiceAccessGuard], // All authenticated users
          },
        ],
      },

      // Reports - Admin only
      // {
      //   path: 'reports',
      //   component: ReportsComponent,
      //   canActivate: [AdminGuard], // Super Admin, Admin only
      // },

      // Cart - Customers only
      {
        path: 'cart',
        component: CartComponent,
        canActivate: [CustomerGuard], // Customer only
      },

      // Account Management - All authenticated users
      {
        path: 'account',
        children: [
          // {
          //   path: 'profile',
          //   component: ProfileComponent,
          // },
          {
            path: 'change-password',
            component: ChangePasswordComponent,
          },
          // {
          //   path: 'settings',
          //   component: CompanySettingsComponent,
          //   canActivate: [AdminGuard], // Super Admin, Admin only
          // },
        ],
      },

      // User Management - Super Admin and Admin only
      {
        path: 'users',
        children: [
          // {
          //   path: '',
          //   component: UserListComponent,
          //   canActivate: [AdminGuard], // Super Admin, Admin only
          // },
          // {
          //   path: 'roles',
          //   component: RoleManagementComponent,
          //   canActivate: [SuperAdminGuard], // Super Admin only
          // },
        ],
      },

      // Settings - Admin only
      {
        path: 'settings',
        children: [
          {
            path: 'invoice',
            component: InvoiceSettingsComponent,
            canActivate: [AdminGuard], // Super Admin, Admin only
          },
          {
            path: 'email',
            component: EmailSettingsComponent,
            canActivate: [AdminGuard], // Super Admin, Admin only
          },
          // {
          //   path: 'company',
          //   component: CompanySettingsComponent,
          //   canActivate: [AdminGuard], // Super Admin, Admin only
          // },
        ],
      },

      // Support - All authenticated users
      // {
      //   path: 'support',
      //   component: SupportComponent,
      // },
    ],
  },

  // Redirect after OTP verification
  {
    path: 'redirect-after-login',
    canActivate: [RedirectGuard],
    component: NotFoundComponent,
  },

  // 404 Page
  { path: 'notfound', component: NotFoundComponent },
  { path: '**', redirectTo: '/notfound' },
];
