import { Component, OnInit, AfterViewInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Chart, registerables } from 'chart.js';
import { Subscription } from 'rxjs';
import {
  DashboardService,
  DashboardStats,
  AdminDashboardSummary,
  InvoiceProgress,
  RecentInvoice,
  TodoItem,
} from '../../../../../services/Dashboard/dashboard.service';

Chart.register(...registerables);

@Component({
  selector: 'app-admin-dashboard-stats',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './admin-dashboard-stats.component.html',
  styleUrls: ['./admin-dashboard-stats.component.css'],
})
export class AdminDashboardStatsComponent
  implements OnInit, AfterViewInit, OnDestroy
{
  stats: DashboardStats = {
    totalInvoiceAmount: 0,
    totalInvoices: 0,
    pendingInvoices: 0,
    paidInvoices: 0,
    overdueInvoices: 0,
    draftInvoices: 0,
    percentageChangeTotal: 0,
    percentageChangePending: 0,
    percentageChangePaid: 0,
  };

  paymentProgress: InvoiceProgress[] = []; // Partially paid - shows progress
  pendingPayments: InvoiceProgress[] = []; // Unpaid - shows what's due
  overduePayments: InvoiceProgress[] = []; // Overdue - needs attention
  recentPayments: RecentInvoice[] = []; // Recently completed
  recentInvoices: RecentInvoice[] = [];
  todos: TodoItem[] = [];
  newTodo = '';
  calendarDays: number[] = [];

  private dashboard: AdminDashboardSummary | null = null;
  private subscription: Subscription = new Subscription();
  private amountChart: Chart | null = null;
  private statusChart: Chart | null = null;

  constructor(private dashboardService: DashboardService) {}

  ngOnInit(): void {
    this.subscription.add(
      this.dashboardService.getTodos().subscribe((todoList) => {
        this.todos = todoList;
      }),
    );
    this.calendarDays = this.dashboardService.generateCalendar(
      new Date().getFullYear(),
      new Date().getMonth(),
    );

    // ✅ Subscribe to real-time refresh
    this.subscription.add(
      this.dashboardService.adminDashboard$.subscribe((dashboard) => {
        if (dashboard) {
          this.dashboard = dashboard;
          this.stats = dashboard.stats;
          this.recentInvoices = dashboard.recentInvoices;
          this.paymentProgress = dashboard.paymentProgress;
          this.pendingPayments = dashboard.pendingPayments;
          this.overduePayments = dashboard.overduePayments;
          this.recentPayments = dashboard.recentPayments;
          this.renderCharts(dashboard);
        }
      }),
    );

    this.dashboardService.refreshDashboard();
  }

  ngAfterViewInit(): void {
    // Charts will be rendered once data arrives in ngOnInit subscription
  }

  private renderCharts(dashboard: AdminDashboardSummary): void {
    setTimeout(() => {
      // ✅ Bar chart — dynamic data from backend trends
      const invoiceAmountCtx = document.getElementById(
        'invoiceAmountChart',
      ) as HTMLCanvasElement;
      if (invoiceAmountCtx) {
        this.amountChart?.destroy();
        this.amountChart = new Chart(
          invoiceAmountCtx,
          this.dashboardService.getInvoiceAmountChartConfig(
            dashboard.b2BTrend,
            dashboard.b2CTrend,
            dashboard.retailTrend,
          ),
        );
      }

      // ✅ Doughnut chart — dynamic data from backend stats
      const invoiceStatusCtx = document.getElementById(
        'invoiceStatusChart',
      ) as HTMLCanvasElement;
      if (invoiceStatusCtx) {
        this.statusChart?.destroy();
        this.statusChart = new Chart(
          invoiceStatusCtx,
          this.dashboardService.getInvoiceStatusChartConfig(
            dashboard.stats.paidInvoices,
            dashboard.stats.pendingInvoices,
            dashboard.stats.overdueInvoices,
          ),
        );
      }
    }, 100);
  }

  ngOnDestroy(): void {
    this.subscription.unsubscribe();
    this.dashboardService.stopRealTimeUpdates();
    this.amountChart?.destroy();
    this.statusChart?.destroy();
  }

  get totalForPercentage(): number {
    return (
      (this.stats.paidInvoices || 0) +
      (this.stats.pendingInvoices || 0) +
      (this.stats.overdueInvoices || 0)
    );
  }

  addTodo(): void {
    if (this.newTodo.trim()) {
      // Call the service instead of pushing to local array
      this.dashboardService.addTodo(this.newTodo.trim());
      this.newTodo = '';
    }
  }
  removeTodo(id: number): void {
    this.dashboardService.removeTodo(id);
  }

  trackByTodo(index: number, todo: TodoItem): number {
    return todo.id;
  }
  // Helper methods for template
  getPaymentProgressPercentage(progress: InvoiceProgress): number {
    return progress.paidPercentage;
  }

  getPaymentProgressColor(progress: InvoiceProgress): string {
    return (
      progress.color || this.getProgressColor(progress.paidPercentage / 100)
    );
  }

  getProgressColor(percentage: number): string {
    if (percentage >= 0.75) return '#10B981'; // Green - Almost complete
    if (percentage >= 0.5) return '#F59E0B'; // Orange - Halfway
    if (percentage >= 0.25) return '#EF4444'; // Red - Just started
    return '#8B5CF6'; // Purple - New
  }

  getDaysUntilDue(dueDate: string | Date): number {
    const due = new Date(dueDate);
    const today = new Date();
    today.setHours(0, 0, 0, 0);
    due.setHours(0, 0, 0, 0);
    const diffTime = due.getTime() - today.getTime();
    return Math.ceil(diffTime / (1000 * 60 * 60 * 24));
  }

  isOverdue(dueDate: string | Date): boolean {
    return new Date(dueDate) < new Date();
  }
  get Math() {
    return Math;
  }
}
