import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { inject } from '@angular/core';
import { AuthService } from '../../services/auth/auth.service';
import { AdminDashboardStatsComponent } from '../../components/dashboard-stats/admin/admin-dashboard-stats.component';
import { UserDashboardStatsComponent } from '../../components/dashboard-stats/user/user-dashboard-stats.component';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    AdminDashboardStatsComponent,
    UserDashboardStatsComponent,
  ],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.css'],
})
export class DashboardComponent implements OnInit {
  private authService = inject(AuthService);
  isAdmin: boolean = false;

  ngOnInit(): void {
    const user = this.authService.getUserDetail();
    this.isAdmin = user?.roles.includes('Admin') || false;
  }
}
