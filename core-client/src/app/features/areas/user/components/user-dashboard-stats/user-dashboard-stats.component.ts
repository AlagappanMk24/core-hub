// user-dashboard-stats.component.ts
import { Component, OnInit, OnDestroy, AfterViewInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Chart, registerables } from 'chart.js';
import { Subscription } from 'rxjs';
import { Router } from '@angular/router';
import {
  DashboardService,
  DashboardStats,
  UserDashboardSummary,
  InvoiceProgress,
  RecentInvoice,
  TodoItem,
  TeamPerformance,
  RecentActivity,
} from '../../../../../services/Dashboard/dashboard.service';
import { AuthService } from '../../../../../core/services/auth/auth.service';

Chart.register(...registerables);

@Component({
  selector: 'app-user-dashboard-stats',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './user-dashboard-stats.component.html',
  styleUrls: ['./user-dashboard-stats.component.css'],
})
export class UserDashboardStatsComponent
  implements OnInit, AfterViewInit, OnDestroy
{
  public router: Router;

  // Dashboard Stats
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

  // Payment Progress - Partially Paid Invoices
  paymentProgress: InvoiceProgress[] = [];

  // Pending Invoices - Unpaid
  pendingPayments: InvoiceProgress[] = [];

  // Overdue Invoices
  overduePayments: InvoiceProgress[] = [];

  // Recent Invoices
  recentInvoices: RecentInvoice[] = [];

  // Recent Payments Activity
  recentPayments: RecentInvoice[] = [];

  // Team Performance
  teamPerformance: TeamPerformance[] = [];

  // Recent Activities
  recentActivities: RecentActivity[] = [];

  // Monthly Trend Data
  monthlyTrend: { month: string; amount: number }[] = [];

  // Todo List
  todos: TodoItem[] = [];
  newTodo: string = '';

  // Calendar
  calendarDays: number[] = [];
  currentMonth: Date = new Date();
  currentMonthName: string = '';
  currentYear: number = 0;
  today: Date = new Date();

  // Chart references
  private trendChart: Chart | null = null;
  private statusChart: Chart | null = null;

  private subscription: Subscription = new Subscription();
  private refreshInterval: any;

  constructor(
    private dashboardService: DashboardService,
    private authService: AuthService,
    router: Router,
  ) {
    this.router = router;
  }

  ngOnInit(): void {
    this.initCalendar();

    // Subscribe to user dashboard updates
    this.subscription.add(
      this.dashboardService.userDashboard$.subscribe((dashboard) => {
        if (dashboard) {
          this.updateDashboardData(dashboard);
        }
      }),
    );

    // Subscribe to todos
    this.subscription.add(
      this.dashboardService.getTodos().subscribe((todoList) => {
        this.todos = todoList;
      }),
    );

    // Initial load
    this.dashboardService.refreshDashboard();

    // Refresh every 5 minutes
    this.refreshInterval = setInterval(() => {
      this.dashboardService.refreshDashboard();
    }, 300000);
  }

  ngAfterViewInit(): void {}

  ngOnDestroy(): void {
    this.subscription.unsubscribe();
    if (this.refreshInterval) {
      clearInterval(this.refreshInterval);
    }
    this.trendChart?.destroy();
    this.statusChart?.destroy();
  }

  private initCalendar(): void {
    const now = new Date();
    this.currentMonth = now;
    this.currentMonthName = now.toLocaleString('default', { month: 'long' });
    this.currentYear = now.getFullYear();
    this.calendarDays = this.dashboardService.generateCalendar(
      now.getFullYear(),
      now.getMonth(),
    );
  }

  private updateDashboardData(dashboard: UserDashboardSummary): void {
    this.stats = dashboard.stats;
    this.paymentProgress = dashboard.paymentProgress || [];
    this.pendingPayments = dashboard.pendingPayments || [];
    this.overduePayments = dashboard.overduePayments || [];
    this.recentInvoices = dashboard.recentInvoices || [];
    this.recentPayments = dashboard.recentPayments || [];
    this.teamPerformance = dashboard.teamPerformance || [];
    this.recentActivities = dashboard.recentActivities || [];
    this.monthlyTrend = dashboard.monthlyTrend || [];

    this.renderCharts();
  }

  private renderCharts(): void {
    setTimeout(() => {
      this.renderTrendChart();
      this.renderStatusChart();
    }, 100);
  }

  private renderTrendChart(): void {
    const ctx = document.getElementById('userTrendChart') as HTMLCanvasElement;
    if (!ctx || !this.monthlyTrend.length) return;

    this.trendChart?.destroy();

    this.trendChart = new Chart(ctx, {
      type: 'bar',
      data: {
        labels: this.monthlyTrend.map((t) => t.month),
        datasets: [
          {
            label: 'Invoice Amount',
            data: this.monthlyTrend.map((t) => t.amount),
            backgroundColor: '#8B5CF6',
            borderRadius: 4,
          },
        ],
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          legend: { display: false },
          tooltip: {
            callbacks: {
              label: (context: any) => `$${context.raw.toLocaleString()}`,
            },
          },
        },
        scales: {
          y: {
            beginAtZero: true,
            ticks: {
              callback: (value: any) => '$' + value.toLocaleString(),
            },
          },
        },
      },
    });
  }

  private renderStatusChart(): void {
    const ctx = document.getElementById('userStatusChart') as HTMLCanvasElement;
    if (!ctx) return;

    this.statusChart?.destroy();

    this.statusChart = new Chart(
      ctx,
      this.dashboardService.getInvoiceStatusChartConfig(
        this.stats.paidInvoices,
        this.stats.pendingInvoices,
        this.stats.overdueInvoices,
      ),
    );
  }

  // Helper methods for template
  getPaymentProgressPercentage(progress: InvoiceProgress): number {
    return progress.paidPercentage;
  }

  getPaymentProgressColor(progress: InvoiceProgress): string {
    const percentage = progress.paidPercentage / 100;
    if (percentage >= 0.75) return '#10B981';
    if (percentage >= 0.5) return '#F59E0B';
    if (percentage >= 0.25) return '#EF4444';
    return '#8B5CF6';
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

  getCollectionRate(): number {
    const total =
      this.stats.paidInvoices +
      this.stats.pendingInvoices +
      this.stats.overdueInvoices;
    if (total === 0) return 0;
    return Math.round((this.stats.paidInvoices / total) * 100);
  }

  viewInvoice(id: number): void {
    this.router.navigate([`/invoices/view/${id}`]);
  }

  addTodo(): void {
    if (this.newTodo.trim()) {
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

  trackByTeam(index: number, member: TeamPerformance): number {
    return member.userId;
  }

  trackByActivity(index: number, activity: RecentActivity): number {
    return activity.id;
  }

  get Math() {
    return Math;
  }

  getUserName(): string {
    const user = this.authService.getUserDetail();
    return user?.fullName || 'User';
  }

  previousMonth(): void {
    const newMonth = new Date(this.currentMonth);
    newMonth.setMonth(this.currentMonth.getMonth() - 1);
    this.currentMonth = newMonth;
    this.currentMonthName = this.currentMonth.toLocaleString('default', {
      month: 'long',
    });
    this.currentYear = this.currentMonth.getFullYear();
    this.calendarDays = this.dashboardService.generateCalendar(
      this.currentYear,
      this.currentMonth.getMonth(),
    );
  }

  nextMonth(): void {
    const newMonth = new Date(this.currentMonth);
    newMonth.setMonth(this.currentMonth.getMonth() + 1);
    this.currentMonth = newMonth;
    this.currentMonthName = this.currentMonth.toLocaleString('default', {
      month: 'long',
    });
    this.currentYear = this.currentMonth.getFullYear();
    this.calendarDays = this.dashboardService.generateCalendar(
      this.currentYear,
      this.currentMonth.getMonth(),
    );
  }

  isToday(day: number): boolean {
    return (
      day === new Date().getDate() &&
      this.currentMonth.getMonth() === new Date().getMonth() &&
      this.currentYear === new Date().getFullYear()
    );
  }
  // Add this getter method to the UserDashboardStatsComponent class

  get totalForPercentage(): number {
    return (
      (this.stats.paidInvoices || 0) +
      (this.stats.pendingInvoices || 0) +
      (this.stats.overdueInvoices || 0)
    );
  }
}
