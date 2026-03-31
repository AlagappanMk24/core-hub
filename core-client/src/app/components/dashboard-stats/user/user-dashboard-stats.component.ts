import { Component, OnInit, AfterViewInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Chart, registerables } from 'chart.js';
import { Subscription } from 'rxjs';
import { AuthService } from '../../../services/auth/auth.service';
import {
  DashboardService,
  DashboardStats,
  DashboardSummary,
  InvoiceProgress,
  RecentInvoice,
  TodoItem,
} from '../../../services/Dashboard/dashboard.service';

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

  paymentProgress: InvoiceProgress[] = []; // Partially paid
  pendingPayments: InvoiceProgress[] = []; // Unpaid but not overdue
  overduePayments: InvoiceProgress[] = []; // Overdue - needs action
  recentPayments: RecentInvoice[] = []; // Recently completed
  recentInvoices: RecentInvoice[] = [];
  invoiceProgress: InvoiceProgress[] = [];
  todos: TodoItem[] = [];
  newTodo = '';
  calendarDays: number[] = [];

  private dashboard: DashboardSummary | null = null;
  private subscription: Subscription = new Subscription();
  private statusChart: Chart | null = null;
  private trendChart: Chart | null = null;

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
    // ✅ Load customer dashboard from API — backend filters by customerId from JWT claim
    this.subscription.add(
      this.dashboardService.getCustomerDashboard().subscribe({
        next: (dashboard) => {
          this.dashboard = dashboard;
          this.stats = dashboard.stats;
          this.recentInvoices = dashboard.recentInvoices;
          this.paymentProgress = dashboard.paymentProgress;
          this.pendingPayments = dashboard.pendingPayments;
          this.overduePayments = dashboard.overduePayments;
          this.recentPayments = dashboard.recentPayments;
          this.renderCharts(dashboard);
        },
        error: (err) => console.error('Error loading customer dashboard:', err),
      }),
    );
    // ✅ Subscribe to real-time refresh
    this.subscription.add(
      this.dashboardService.dashboard$.subscribe((dashboard) => {
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

    this.dashboardService.startRealTimeUpdates();
  }

  ngAfterViewInit(): void {
    // Charts rendered once data arrives in subscription
  }

  private renderCharts(dashboard: DashboardSummary): void {
    setTimeout(() => {
      // ✅ Doughnut chart with dynamic backend stats
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

      // ✅ Monthly trend chart for customer (backend returns monthlyTrend)
      const trendCtx = document.getElementById(
        'placeholderChart',
      ) as HTMLCanvasElement;
      if (trendCtx && dashboard.monthlyTrend?.length) {
        this.trendChart?.destroy();
        this.trendChart = new Chart(trendCtx, {
          type: 'bar',
          data: {
            labels: dashboard.monthlyTrend.map((t) => t.month),
            datasets: [
              {
                label: 'Monthly Total',
                data: dashboard.monthlyTrend.map((t) => t.amount),
                backgroundColor: '#8B5CF6',
                borderRadius: 4,
              },
            ],
          },
          options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: { legend: { display: false } },
          },
        });
      }
    }, 100);
  }

  ngOnDestroy(): void {
    this.subscription.unsubscribe();
    this.dashboardService.stopRealTimeUpdates();
    this.statusChart?.destroy();
    this.trendChart?.destroy();
  }

  get totalForPercentage(): number {
    return (
      (this.stats.paidInvoices || 0) +
      (this.stats.pendingInvoices || 0) +
      (this.stats.overdueInvoices || 0)
    );
  }

  get today(): number {
    return new Date().getDate();
  }

  get currentMonthLabel(): string {
    return new Date().toLocaleString('default', {
      month: 'long',
      year: 'numeric',
    });
  }

  addTodo(): void {
    if (this.newTodo.trim()) {
      // ✅ Use the service method
      this.dashboardService.addTodo(this.newTodo.trim());
      this.newTodo = '';
    }
  }

  removeTodo(id: number): void {
    // ✅ Use the service method
    this.dashboardService.removeTodo(id);
  }

  trackByTodo(index: number, todo: TodoItem): number {
    return todo.id;
  }
   getDaysUntilDue(dueDate: string | Date): number {
    const due = new Date(dueDate);
    const today = new Date();
    today.setHours(0, 0, 0, 0);
    due.setHours(0, 0, 0, 0);
    const diffTime = due.getTime() - today.getTime();
    return Math.ceil(diffTime / (1000 * 60 * 60 * 24));
  }

  getPaymentProgressColor(progress: InvoiceProgress): string {
    return progress.progressColor || this.getProgressColor(progress.paidPercentage / 100);
  }

  getProgressColor(percentage: number): string {
    if (percentage >= 0.75) return '#10B981';
    if (percentage >= 0.5) return '#F59E0B';
    if (percentage >= 0.25) return '#EF4444';
    return '#8B5CF6';
  }

  // Add Math property for template access
  get Math() {
    return Math;
  }
}
