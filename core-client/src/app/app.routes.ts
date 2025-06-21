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
import { InvoiceDashboardComponent } from './pages/invoices/invoice-dashboard.component';
import { CustomerComponent } from './pages/customers/customer.component';
import { UserRoleManagementComponent } from './pages/auth/user-role/user-role-management.component';
import { AdminGuard } from './guards/admin.guard';
import { ProductComponent } from './pages/products/product.component';
import { ProductDetailComponent } from './pages/products/product-detail.component';
import { OtpGuard } from './guards/otp.guard';

export const routes: Routes = [
  { path: '', component: HomeComponent },

  // Auth Routes (Login, Register, External Auth)
  {
    path: 'auth',
    children: [
      { path: 'login', component: LoginComponent },
      { path: 'register', component: RegisterComponent },
      { path: 'verify-otp', component: VerifyOtpComponent, canActivate :[OtpGuard] },
      { path: 'forgot-password', component: ForgotPasswordComponent },
      { path: 'reset-password', component: ResetPasswordComponent },
      { path: 'callback', component: LoginComponent }, // External login redirect
    ],
  },

  // Authenticated routes (wrapped in LayoutComponent)
  {
    path: '',
    component: LayoutComponent,
    canActivate: [AuthGuard], // Protect these routes
    children: [
      { path: 'dashboard', component: DashboardComponent },
      { path: 'customers', component: CustomerComponent },
      { path: 'orders', component: OrdersComponent },
      { path: 'products', component: ProductComponent },
       { path: 'products/:id', component: ProductDetailComponent },
      { path: 'invoices', component: InvoiceDashboardComponent },
      {
        path: 'account',
        children: [
          { path: 'change-password', component: ChangePasswordComponent },
        ],
      },
      {
        path: 'role-management',
        component: UserRoleManagementComponent,
        // canActivate: [AdminGuard],
      },
    ],
  },

  // 404 Page
  { path: 'notfound', component: NotFoundComponent },
];