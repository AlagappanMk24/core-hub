// import { Component, OnInit, AfterViewInit, OnDestroy } from '@angular/core';
// import { CommonModule } from '@angular/common';
// import { FormsModule } from '@angular/forms';
// import { Chart, registerables, ChartConfiguration } from 'chart.js';
// import { Subscription } from 'rxjs';
// import { DashboardService, DashboardStats } from '../../../services/Dashboard/dashboard.service';

// Chart.register(...registerables);

// interface Ticket {
//   assignee: string;
//   avatar: string;
//   subject: string;
//   status: 'DONE' | 'PROGRESS' | 'ON HOLD' | 'REJECTED';
//   lastUpdate: string;
//   trackingId: string;
// }

// interface Project {
//   id: number;
//   name: string;
//   dueDate: string;
//   progress: number;
//   color: string;
// }

// @Component({
//   selector: 'app-admin-dashboard-stats',
//   standalone: true,
//   imports: [CommonModule, FormsModule],
//   templateUrl: './admin-dashboard-stats.component.html',
//   styleUrls: ['./admin-dashboard-stats.component.css'],
// })
// export class AdminDashboardStatsComponent implements OnInit, AfterViewInit, OnDestroy {
//   stats: DashboardStats = { weeklySales: 0, weeklyOrders: 0, visitorsOnline: 0, pendingTasks: 0 };
//   private subscription: Subscription = new Subscription();
//   tickets: Ticket[] = [
//     {
//       assignee: 'David Grey',
//       avatar: 'https://images.unsplash.com/photo-1472099645785-5658abf4ff4e?w=40&h=40&fit=crop&crop=face',
//       subject: 'Fund is not received',
//       status: 'DONE',
//       lastUpdate: 'Dec 5, 2025',
//       trackingId: 'WD-12345',
//     },
//     // ... other tickets ...
//   ];
//   projects: Project[] = [
//     { id: 1, name: 'Herman Beck', dueDate: 'May 15, 2025', progress: 25, color: '#14B8A6' },
//     // ... other projects ...
//   ];

//   constructor(private dashboardService: DashboardService) {}

//   ngOnInit(): void {
//     this.subscription.add(
//       this.dashboardService.getStats().subscribe((stats) => {
//         this.stats = stats;
//       })
//     );
//     this.dashboardService.startRealTimeUpdates();
//   }

//   ngAfterViewInit(): void {
//     setTimeout(() => {
//       this.createRevenueChart();
//       this.createUserActivityChart();
//     }, 100);
//   }

//   ngOnDestroy(): void {
//     this.subscription.unsubscribe();
//   }

//   private createRevenueChart(): void {
//     const canvas = document.getElementById('revenueChart') as HTMLCanvasElement;
//     if (!canvas) return;
//     const ctx = canvas.getContext('2d');
//     if (!ctx) return;

//     const config: ChartConfiguration<'line'> = {
//       type: 'line',
//       data: {
//         labels: ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun'],
//         datasets: [
//           {
//             label: 'Revenue',
//             data: [120000, 150000, 130000, 170000, 160000, this.stats.weeklySales],
//             borderColor: '#8B5CF6',
//             backgroundColor: 'rgba(139, 92, 246, 0.2)',
//             fill: true,
//             tension: 0.4,
//           },
//         ],
//       },
//       options: {
//         responsive: true,
//         maintainAspectRatio: false,
//         plugins: { legend: { display: false } },
//         scales: {
//           y: { beginAtZero: true, ticks: { color: '#6B7280' } },
//           x: { ticks: { color: '#6B7280' } },
//         },
//       },
//     };
//     new Chart(ctx, config);
//   }

//   private createUserActivityChart(): void {
//     const canvas = document.getElementById('userActivityChart') as HTMLCanvasElement;
//     if (!canvas) return;
//     const ctx = canvas.getContext('2d');
//     if (!ctx) return;

//     const config: ChartConfiguration<'doughnut'> = {
//       type: 'doughnut',
//       data: {
//         labels: ['Active Users', 'New Users', 'Inactive Users'],
//         datasets: [
//           {
//             data: [this.stats.visitorsOnline, 25000, 15000],
//             backgroundColor: ['#60A5FA', '#14B8A6', '#F472B6'],
//             borderWidth: 0,
//           },
//         ],
//       },
//       options: {
//         responsive: true,
//         maintainAspectRatio: false,
//         plugins: { legend: { display: false } },
//       },
//     };
//     new Chart(ctx, config);
//   }
// }

// import { Component, OnInit, AfterViewInit, OnDestroy } from '@angular/core';
// import { CommonModule } from '@angular/common';
// import { FormsModule } from '@angular/forms';
// import { Chart, registerables, ChartConfiguration } from 'chart.js';
// import { Subscription } from 'rxjs';
// import { DashboardService, Invoice, InvoiceStats } from '../../../services/Dashboard/dashboard.service';

// Chart.register(...registerables);

// @Component({
//   selector: 'app-admin-dashboard-stats',
//   standalone: true,
//   imports: [CommonModule, FormsModule],
//   templateUrl: './admin-dashboard-stats.component.html',
//   styleUrls: ['./admin-dashboard-stats.component.css'],
// })
// export class AdminDashboardStatsComponent implements OnInit, AfterViewInit, OnDestroy {
//   stats: InvoiceStats = { totalInvoiced: 0, pendingInvoices: 0, paidInvoices: 0, overdueInvoices: 0 };
//   invoices: Invoice[] = [];
//   private subscription: Subscription = new Subscription();

//   constructor(private dashboardService: DashboardService) {}

//   ngOnInit(): void {
//     this.subscription.add(
//       this.dashboardService.getInvoiceStats().subscribe((stats) => {
//         this.stats = stats;
//       })
//     );
//     this.subscription.add(
//       this.dashboardService.getInvoices().subscribe((invoices) => {
//         this.invoices = invoices;
//       })
//     );
//     this.dashboardService.startRealTimeUpdates();
//   }

//   ngAfterViewInit(): void {
//     setTimeout(() => {
//       this.createInvoiceTrendChart();
//       this.createInvoiceStatusChart();
//     }, 100);
//   }

//   ngOnDestroy(): void {
//     this.subscription.unsubscribe();
//   }

//   private createInvoiceTrendChart(): void {
//     const canvas = document.getElementById('invoiceTrendChart') as HTMLCanvasElement;
//     if (!canvas) return;
//     const ctx = canvas.getContext('2d');
//     if (!ctx) return;

//     const config: ChartConfiguration<'bar'> = {
//       type: 'bar',
//       data: {
//         labels: ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun'],
//         datasets: [
//           {
//             label: 'Total Invoiced',
//             data: [50000, 60000, 55000, 70000, 65000, this.stats.totalInvoiced / 6],
//             backgroundColor: '#8B5CF6',
//             borderRadius: 4,
//             borderSkipped: false,
//           },
//           {
//             label: 'Paid',
//             data: [40000, 50000, 45000, 60000, 55000, this.stats.paidInvoices * 1000],
//             backgroundColor: '#60A5FA',
//             borderRadius: 4,
//             borderSkipped: false,
//           },
//         ],
//       },
//       options: {
//         responsive: true,
//         maintainAspectRatio: false,
//         plugins: { legend: { display: false } },
//         scales: {
//           y: { beginAtZero: true, ticks: { color: '#6B7280' } },
//           x: { ticks: { color: '#6B7280' } },
//         },
//       },
//     };
//     new Chart(ctx, config);
//   }

//   private createInvoiceStatusChart(): void {
//     const canvas = document.getElementById('invoiceStatusChart') as HTMLCanvasElement;
//     if (!canvas) return;
//     const ctx = canvas.getContext('2d');
//     if (!ctx) return;

//     const config: ChartConfiguration<'doughnut'> = {
//       type: 'doughnut',
//       data: {
//         labels: ['Paid', 'Pending', 'Overdue'],
//         datasets: [
//           {
//             data: [this.stats.paidInvoices, this.stats.pendingInvoices, this.stats.overdueInvoices],
//             backgroundColor: ['#60A5FA', '#F472B6', '#A78BFA'],
//             borderWidth: 0,
//           },
//         ],
//       },
//       options: {
//         responsive: true,
//         maintainAspectRatio: false,
//         plugins: { legend: { display: false } },
//       },
//     };
//     new Chart(ctx, config);
//   }
// }

import { Component, OnInit, AfterViewInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Chart, registerables } from 'chart.js';
import { Subscription } from 'rxjs';
import { DashboardService, Invoice, InvoiceProgress, InvoiceStats, TodoItem } from '../../../services/Dashboard/dashboard.service';

Chart.register(...registerables);

@Component({
  selector: 'app-admin-dashboard-stats',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './admin-dashboard-stats.component.html',
  styleUrls: ['./admin-dashboard-stats.component.css'],
})
export class AdminDashboardStatsComponent implements OnInit, AfterViewInit, OnDestroy {
  stats: InvoiceStats = { totalInvoiceAmount: 0, pendingInvoices: 0, paidInvoices: 0 };
  invoices: Invoice[] = [];
  invoiceProgress: InvoiceProgress[] = [];
  todos: TodoItem[] = [];
  newTodo = '';
  calendarDays: number[] = [];
  private subscription: Subscription = new Subscription();

  constructor(private dashboardService: DashboardService) {}

  ngOnInit(): void {
    this.invoices = this.dashboardService.getInvoices();
    this.invoiceProgress = this.dashboardService.getInvoiceProgress();
    this.todos = this.dashboardService.getTodos();
    this.calendarDays = this.dashboardService.generateCalendar(2025, 5);
    this.subscription.add(
      this.dashboardService.getStats().subscribe((stats) => {
        this.stats = stats;
      })
    );
    this.dashboardService.startRealTimeUpdates();
  }

  ngAfterViewInit(): void {
    setTimeout(() => {
      const invoiceAmountCtx = document.getElementById('invoiceAmountChart') as HTMLCanvasElement;
      if (invoiceAmountCtx) {
        new Chart(invoiceAmountCtx, this.dashboardService.getInvoiceAmountChartConfig());
      }
      const invoiceStatusCtx = document.getElementById('invoiceStatusChart') as HTMLCanvasElement;
      if (invoiceStatusCtx) {
        new Chart(invoiceStatusCtx, this.dashboardService.getInvoiceStatusChartConfig());
      }
    }, 100);
  }

  ngOnDestroy(): void {
    this.subscription.unsubscribe();
  }

  addTodo(): void {
    if (this.newTodo.trim()) {
      const newId = this.todos.length > 0 ? Math.max(...this.todos.map(t => t.id)) + 1 : 1;
      this.todos.push({ id: newId, text: this.newTodo.trim(), completed: false });
      this.newTodo = '';
    }
  }

  removeTodo(id: number): void {
    this.todos = this.todos.filter(todo => todo.id !== id);
  }

  trackByTodo(index: number, todo: TodoItem): number {
    return todo.id;
  }
}