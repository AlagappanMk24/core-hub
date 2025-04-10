import { Component, OnInit } from '@angular/core';
import { AuthService } from '../../services/auth.service';
import { Router } from '@angular/router';
// Import Angular Material Modules
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatDividerModule } from '@angular/material/divider';
import { MatCardModule } from '@angular/material/card';

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.css'],
  imports: [
    MatSidenavModule,
    MatIconModule,
    MatButtonModule,
    MatDividerModule,
    MatCardModule
  ],
})
export class DashboardComponent implements OnInit {
  user: any;
  isDarkMode = false;

  // Chart Data
  chartData = [30, 50, 20];
  chartLabels = ['Sales', 'Profit', 'Expenses'];

  constructor(private authService: AuthService, private router: Router) {}

  ngOnInit(): void {
    this.user = this.authService.getLoggedInUser();
  }

  toggleDarkMode() {
    this.isDarkMode = !this.isDarkMode;
    document.body.classList.toggle('dark-mode');
  }

  logout() {
    this.authService.logout();
    this.router.navigate(['/auth/login']);
  }
}
