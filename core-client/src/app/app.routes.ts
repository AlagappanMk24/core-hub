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
// import { ProductDetailComponent } from './pages/products/product-detail.component';
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
import { RoleMangementComponent } from './features/areas/admin/role-management/role-management.component';

export const routes: Routes = [
  { path: '', component: HomeComponent },

  // Auth Routes (Login, Register, External Auth)
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
      {
        path: 'dashboard',
        component: DashboardComponent,
        canActivate: [AdminOrUserGuard],
      },
      {
        path: 'customer-dashboard',
        component: CustomerDashboardComponent,
        canActivate: [CustomerGuard],
      },
      {
        path: 'customers',
        component: CustomerListComponent,
        canActivate: [AdminGuard, UserGuard],
      },
      {
        path: 'orders',
        component: OrdersComponent,
        canActivate: [AdminGuard, UserGuard],
      },
      {
        path: 'products',
        component: ProductComponent,
        canActivate: [AdminGuard, UserGuard],
      },
      {
        path: 'cart',
        component: CartComponent,
        canActivate: [AdminGuard, UserGuard],
      },
      {
        path: 'invoices/create',
        component: CreateInvoiceComponent,
        canActivate: [UserGuard],
      },
      {
        path: 'invoices/edit/:id',
        component: CreateInvoiceComponent,
        canActivate: [UserGuard],
      },
      {
        path: 'invoices',
        component: InvoiceComponent,
        canActivate: [AuthGuard, InvoiceAccessGuard],
      },
      {
        path: 'invoices/view/:id',
        component: InvoiceDisplayComponent,
        canActivate: [AuthGuard, CustomerGuard],
      },
      {
        path: 'settings/invoice',
        component: InvoiceSettingsComponent,
        canActivate: [AdminGuard],
      },
      {
        path: 'settings/email',
        component: EmailSettingsComponent,
        canActivate: [AdminGuard],
      },
      {
        path: 'account',
        children: [
          { path: 'change-password', component: ChangePasswordComponent },
        ],
      },
      {
        path: 'role-management',
        component: RoleMangementComponent,
        canActivate: [AdminGuard],
      },
    ],
  },
  // Redirect after OTP verification
  {
    path: 'redirect-after-login',
    canActivate: [RedirectGuard],
    component: NotFoundComponent, // Fallback component (not rendered due to redirect)
  },
  // 404 Page
  { path: 'notfound', component: NotFoundComponent },
];
