// dashboard.component.ts
import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { inject } from '@angular/core';
import { AuthService } from '../../../core/services/auth/auth.service';
import { AdminDashboardStatsComponent } from '../../../features/areas/admin/components/admin-dashboard-stats/admin-dashboard-stats.component';
import { UserDashboardStatsComponent } from '../../../features/areas/user/components/user-dashboard-stats/user-dashboard-stats.component';
import { CustomerDashboardStatsComponent } from "../../../features/areas/customer/components/customer-dashboard-stats/customer-dashboard-stats.component";

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    AdminDashboardStatsComponent,
    UserDashboardStatsComponent,
    CustomerDashboardStatsComponent
],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.css'],
})
export class DashboardComponent implements OnInit {
  private authService = inject(AuthService);
 userRole: 'admin' | 'user' | 'customer' = 'customer';

  ngOnInit(): void {
    const user = this.authService.getUserDetail();
    if (user?.roles?.includes('Admin')) {
      this.userRole = 'admin';
    } else if (user?.roles?.includes('User')) {
      this.userRole = 'user';
    } else {
      this.userRole = 'customer';
    }
  }
}
