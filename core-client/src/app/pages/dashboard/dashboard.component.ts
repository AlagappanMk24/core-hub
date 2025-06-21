// dashboard.component.ts 
import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DashboardStatsComponent } from '../../components/dashboard-stats/dashboard-stats.component';
@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, DashboardStatsComponent],
  templateUrl : './dashboard.component.html',
  styleUrls: ['./dashboard.component.css']
})
export class DashboardComponent {
  constructor() {
  }
}