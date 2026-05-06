// customer-dashboard-stats.component.ts
import { Component, OnInit, OnDestroy, AfterViewInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Chart, registerables } from 'chart.js';
import { Subscription } from 'rxjs';
import { Router } from '@angular/router';
import { AuthService } from '../../../../../core/services/auth/auth.service';
import {
  CustomerDashboardStats,
  CustomerInvoiceProgress,
  CustomerInvoiceSummary,
  CustomerPaymentActivity,
  CustomerRecentInvoice,
  DashboardService,
  MonthlyTrend,
} from '../../../../../services/Dashboard/dashboard.service';

Chart.register(...registerables);

@Component({
  selector: 'app-customer-dashboard-stats',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './customer-dashboard-stats.component.html',
  styleUrls: ['./customer-dashboard-stats.component.css'],
})
export class CustomerDashboardStatsComponent
  implements OnInit, AfterViewInit, OnDestroy
{
  public router: Router;

  // Dashboard Stats - Will be populated from backend
  stats: CustomerDashboardStats = {
    totalInvoiced: 0,
    totalPaid: 0,
    totalDue: 0,
    overdueAmount: 0,
    invoiceCount: 0,
    paidCount: 0,
    pendingCount: 0,
    overdueCount: 0,
    percentageChangeInvoiced: 0,
    percentageChangePaid: 0,
    percentageChangeDue: 0,
  };

  // Payment Progress - Partially Paid Invoices
  paymentProgress: CustomerInvoiceProgress[] = [];

  // Pending Invoices - Unpaid
  pendingInvoices: CustomerInvoiceProgress[] = [];

  // Overdue Invoices
  overdueInvoices: CustomerInvoiceProgress[] = [];

  // Recent Invoices
  recentInvoices: CustomerRecentInvoice[] = [];

  // Recent Payments Activity
  recentPayments: CustomerPaymentActivity[] = [];

  // Invoice Summary by Status
  invoiceSummary: CustomerInvoiceSummary[] = [];

  // Monthly Trend Data from Backend
  monthlyTrend: MonthlyTrend[] = [];

  // Calendar
  calendarDays: number[] = [];
  currentMonth: Date = new Date();
  currentMonthName: string = '';
  currentYear: number = 0;
  today: Date = new Date();

  // Chart references
  private paymentTrendChart: Chart | null = null;
  private statusDistributionChart: Chart | null = null;

  private subscription: Subscription = new Subscription();
  private refreshInterval: any;

  // Animation states
  animatedStats: Record<string, number> = {
    totalInvoiced: 0,
    totalPaid: 0,
    totalDue: 0,
    overdueAmount: 0,
  };
  private animationInterval: any;

  constructor(
    private dashboardService: DashboardService,
    private authService: AuthService,
    router: Router,
  ) {
    this.router = router;
  }

  ngOnInit(): void {
    this.initCalendar();
    this.initAnimatedStats();

    // Subscribe to real-time updates
    this.subscription.add(
      this.dashboardService.customerDashboard$.subscribe((dashboard) => {
        if (dashboard) {
          console.log('Customer Dashboard Raw Data:', dashboard);
          this.updateDashboardData(dashboard);
          this.startNumberAnimation();
        }
      }),
    );

    // Refresh dashboard
    this.dashboardService.refreshDashboard();
    this.refreshInterval = setInterval(() => {
      this.dashboardService.refreshDashboard();
    }, 300000);
  }

  ngAfterViewInit(): void {
    setTimeout(() => {
      this.renderCharts();
    }, 500);
  }

  ngOnDestroy(): void {
    this.subscription.unsubscribe();
    if (this.refreshInterval) {
      clearInterval(this.refreshInterval);
    }
    if (this.animationInterval) {
      clearInterval(this.animationInterval);
    }
    if (this.paymentTrendChart) {
      this.paymentTrendChart.destroy();
      this.paymentTrendChart = null;
    }
    if (this.statusDistributionChart) {
      this.statusDistributionChart.destroy();
      this.statusDistributionChart = null;
    }
  }

  private initAnimatedStats(): void {
    this.animatedStats = {
      totalInvoiced: 0,
      totalPaid: 0,
      totalDue: 0,
      overdueAmount: 0,
    };
  }

  private startNumberAnimation(): void {
    if (this.animationInterval) {
      clearInterval(this.animationInterval);
    }

    const targets = {
      totalInvoiced: this.stats.totalInvoiced,
      totalPaid: this.stats.totalPaid,
      totalDue: this.stats.totalDue,
      overdueAmount: this.stats.overdueAmount,
    };

    const duration = 1000;
    const steps = 60;
    const stepDuration = duration / steps;
    let currentStep = 0;

    const startValues = { ...this.animatedStats };

    this.animationInterval = setInterval(() => {
      currentStep++;
      const progress = Math.min(1, currentStep / steps);

      Object.keys(targets).forEach((key) => {
        const start = startValues[key];
        const end = targets[key as keyof typeof targets];
        this.animatedStats[key] = Math.round(start + (end - start) * progress);
      });

      if (progress >= 1) {
        clearInterval(this.animationInterval);
      }
    }, stepDuration);
  }

  private initCalendar(): void {
    const now = new Date();
    this.currentMonth = now;
    this.currentMonthName = now.toLocaleString('default', { month: 'long' });
    this.currentYear = now.getFullYear();
    this.calendarDays = this.generateCalendar(
      now.getFullYear(),
      now.getMonth(),
    );
  }

  private updateDashboardData(dashboard: any): void {
    const stats = dashboard.stats || {};

    // Calculate totals
    const totalInvoiced = stats.totalInvoiceAmount || 0;
    const totalPaid =
      stats.paidInvoices > 0 ? stats.totalInvoiceAmount || 0 : 0;
    const totalDue =
      stats.pendingInvoices > 0 ? stats.totalInvoiceAmount || 0 : 0;
    const overdueAmount =
      stats.overdueInvoices > 0 ? stats.totalInvoiceAmount || 0 : 0;

    this.stats = {
      totalInvoiced: totalInvoiced,
      totalPaid: totalPaid,
      totalDue: totalDue,
      overdueAmount: overdueAmount,
      invoiceCount: stats.totalInvoices || 0,
      paidCount: stats.paidInvoices || 0,
      pendingCount: stats.pendingInvoices || 0,
      overdueCount: stats.overdueInvoices || 0,
      percentageChangeInvoiced: stats.percentageChangeTotal || 0,
      percentageChangePaid: stats.percentageChangePaid || 0,
      percentageChangeDue: 0,
    };

    // Map recent invoices
    this.recentInvoices = (dashboard.recentInvoices || []).map((inv: any) => ({
      id: inv.id,
      invoiceNumber: inv.invoiceNumber,
      amount: inv.amount,
      issueDate: inv.issueDate,
      dueDate: inv.dueDate,
      status: inv.status,
    }));

    // Map recent payments
    this.recentPayments = (dashboard.recentPayments || []).map(
      (payment: any) => ({
        id: payment.id,
        invoiceId: payment.id,
        invoiceNumber: payment.invoiceNumber,
        amount: payment.amount,
        paymentDate: payment.paymentDate || payment.issueDate,
        paymentMethod: payment.paymentMethod || 'Bank Transfer',
      }),
    );

    // Map payment progress (partially paid)
    this.paymentProgress = (dashboard.paymentProgress || []).map(
      (progress: any) => ({
        id: progress.id,
        invoiceNumber: progress.invoiceNumber,
        totalAmount: progress.totalAmount,
        amountPaid: progress.amountPaid,
        remainingAmount: progress.remainingAmount,
        paidPercentage: progress.paidPercentage,
        dueDate: progress.dueDate,
      }),
    );

    // Map pending invoices
    this.pendingInvoices = (dashboard.pendingPayments || []).map(
      (pending: any) => ({
        id: pending.id,
        invoiceNumber: pending.invoiceNumber,
        totalAmount: pending.totalAmount,
        amountPaid: pending.amountPaid || 0,
        remainingAmount: pending.remainingAmount || pending.totalAmount,
        paidPercentage: pending.paidPercentage || 0,
        dueDate: pending.dueDate,
      }),
    );

    // Map overdue invoices
    this.overdueInvoices = (dashboard.overduePayments || []).map(
      (overdue: any) => ({
        id: overdue.id,
        invoiceNumber: overdue.invoiceNumber,
        totalAmount: overdue.totalAmount,
        amountPaid: overdue.amountPaid || 0,
        remainingAmount: overdue.remainingAmount || overdue.totalAmount,
        paidPercentage: overdue.paidPercentage || 0,
        dueDate: overdue.dueDate,
      }),
    );

    // Monthly trend
    this.monthlyTrend = dashboard.monthlyTrend || [];

    console.log('Mapped Stats:', this.stats);

    this.renderCharts();
  }

  private renderCharts(): void {
    const trendCanvas = document.getElementById('customerPaymentTrendChart');
    const statusCanvas = document.getElementById('customerStatusChart');

    if (trendCanvas && statusCanvas) {
      this.renderPaymentTrendChart();
      this.renderStatusDistributionChart();
    } else {
      setTimeout(() => {
        this.renderPaymentTrendChart();
        this.renderStatusDistributionChart();
      }, 200);
    }
  }

  private generateCalendar(year: number, month: number): number[] {
    const firstDay = new Date(year, month, 1);
    const lastDay = new Date(year, month + 1, 0);
    const daysInMonth = lastDay.getDate();
    const startingDayOfWeek = firstDay.getDay();
    const days: number[] = [];

    for (let i = 0; i < startingDayOfWeek; i++) days.push(0);
    for (let day = 1; day <= daysInMonth; day++) days.push(day);
    while (days.length < 42) days.push(0);

    return days;
  }

  private renderPaymentTrendChart(): void {
    const ctx = document.getElementById(
      'customerPaymentTrendChart',
    ) as HTMLCanvasElement;
    if (!ctx) return;

    if (this.paymentTrendChart) {
      this.paymentTrendChart.destroy();
    }

    let months: string[] = [];
    let invoicedData: number[] = [];
    let paidData: number[] = [];

    if (this.monthlyTrend && this.monthlyTrend.length > 0) {
      // Show last 6 months with data or current year data
      const monthsWithData = this.monthlyTrend.filter(
        (t) => t.amount > 0 || t.month === 'Mar',
      );
      months = monthsWithData.map((t) => t.month);
      invoicedData = monthsWithData.map((t) => t.amount);
      const paidRatio =
        this.stats.totalInvoiced > 0
          ? this.stats.totalPaid / this.stats.totalInvoiced
          : 0.5;
      paidData = monthsWithData.map((t) => Math.round(t.amount * paidRatio));
    } else {
      months = this.getLast6Months();
      invoicedData = this.getMonthlyInvoicedData(months);
      paidData = this.getMonthlyPaidData(months);
    }

    if (months.length === 0) return;

    this.paymentTrendChart = new Chart(ctx, {
      type: 'line',
      data: {
        labels: months,
        datasets: [
          {
            label: 'Invoiced Amount',
            data: invoicedData,
            borderColor: '#8B5CF6',
            backgroundColor: 'rgba(139, 92, 246, 0.1)',
            tension: 0.4,
            fill: true,
            pointBackgroundColor: '#8B5CF6',
            pointBorderColor: '#fff',
            pointRadius: 4,
            pointHoverRadius: 6,
          },
          {
            label: 'Paid Amount',
            data: paidData,
            borderColor: '#10B981',
            backgroundColor: 'rgba(16, 185, 129, 0.1)',
            tension: 0.4,
            fill: true,
            pointBackgroundColor: '#10B981',
            pointBorderColor: '#fff',
            pointRadius: 4,
            pointHoverRadius: 6,
          },
        ],
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        animation: { duration: 1000, easing: 'easeInOutQuart' },
        plugins: {
          legend: {
            position: 'bottom',
            labels: { usePointStyle: true, boxWidth: 10 },
          },
          tooltip: {
            backgroundColor: 'rgba(0, 0, 0, 0.8)',
            callbacks: {
              label: (context: any) =>
                `${context.dataset.label}: $${context.raw.toLocaleString()}`,
            },
          },
        },
        scales: {
          y: {
            beginAtZero: true,
            grid: { color: 'rgba(0, 0, 0, 0.05)' },
            ticks: { callback: (value: any) => '$' + value.toLocaleString() },
          },
          x: { grid: { display: false } },
        },
      },
    });
  }

  private renderStatusDistributionChart(): void {
    const ctx = document.getElementById(
      'customerStatusChart',
    ) as HTMLCanvasElement;
    if (!ctx) return;

    if (this.statusDistributionChart) {
      this.statusDistributionChart.destroy();
    }

    const paidCount = this.stats.paidCount || 0;
    const pendingCount = this.stats.pendingCount || 0;
    const overdueCount = this.stats.overdueCount || 0;
    const partiallyPaidCount = this.paymentProgress.length || 0;

    const chartData = [
      paidCount,
      pendingCount,
      overdueCount,
      partiallyPaidCount,
    ];
    const total = chartData.reduce((a, b) => a + b, 0);

    if (total === 0) {
      this.statusDistributionChart = new Chart(ctx, {
        type: 'doughnut',
        data: {
          labels: ['No Data'],
          datasets: [
            { data: [1], backgroundColor: ['#E5E7EB'], borderWidth: 0 },
          ],
        },
        options: {
          responsive: true,
          maintainAspectRatio: true,
          cutout: '60%',
          plugins: {
            legend: { display: false },
            tooltip: {
              callbacks: { label: () => 'No invoice data available' },
            },
          },
        },
      });
      return;
    }

    // Use the dashboard service's chart configuration for consistency
    this.statusDistributionChart = new Chart(ctx, {
      type: 'doughnut',
      data: {
        labels: ['Paid', 'Pending', 'Overdue', 'Partially Paid'],
        datasets: [
          {
            data: chartData,
            backgroundColor: ['#07cdae', '#fe7096', '#ef4444', , '#047edf'],
            borderWidth: 0,
            hoverOffset: 10,
          },
        ],
      },
      options: {
        responsive: true,
        maintainAspectRatio: true,
        cutout: '65%',
        plugins: {
          legend: { display: false },
          tooltip: {
            backgroundColor: 'rgba(0, 0, 0, 0.8)',
            callbacks: {
              label: (context: any) =>
                `${context.label}: ${context.raw} (${((context.raw / total) * 100).toFixed(1)}%)`,
            },
          },
        },
      },
    });
  }

  private getLast6Months(): string[] {
    const months = [];
    for (let i = 5; i >= 0; i--) {
      const date = new Date();
      date.setMonth(date.getMonth() - i);
      months.push(date.toLocaleString('default', { month: 'short' }));
    }
    return months;
  }

  private getMonthlyInvoicedData(months: string[]): number[] {
    if (this.monthlyTrend && this.monthlyTrend.length > 0) {
      return months.map((month) => {
        const trend = this.monthlyTrend.find((t) => t.month === month);
        return trend ? trend.amount : 0;
      });
    }
    const monthlyAvg = this.stats.totalInvoiced / 6;
    return months.map(() => Math.round(monthlyAvg));
  }

  private getMonthlyPaidData(months: string[]): number[] {
    if (this.monthlyTrend && this.monthlyTrend.length > 0) {
      const paidRatio =
        this.stats.totalInvoiced > 0
          ? this.stats.totalPaid / this.stats.totalInvoiced
          : 0;
      return months.map((month) => {
        const trend = this.monthlyTrend.find((t) => t.month === month);
        return trend ? Math.round(trend.amount * paidRatio) : 0;
      });
    }
    const monthlyAvg = this.stats.totalPaid / 6;
    return months.map(() => Math.round(monthlyAvg));
  }

  // Helper methods for template
  getPaymentProgressPercentage(progress: CustomerInvoiceProgress): number {
    return progress.paidPercentage;
  }

  getPaymentProgressColor(progress: CustomerInvoiceProgress): string {
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

  viewInvoice(id: number): void {
    this.router.navigate([`/invoices/view/${id}`]);
  }

  payNow(invoiceId: number): void {
    this.router.navigate([`/payments/make/${invoiceId}`]);
  }

  getPaymentHealth(): number {
    if (this.stats.totalInvoiced === 0) return 100;
    const health = Math.round(
      (this.stats.totalPaid / this.stats.totalInvoiced) * 100,
    );
    return isNaN(health) ? 0 : health;
  }

  get Math() {
    return Math;
  }

  previousMonth(): void {
    const newMonth = new Date(this.currentMonth);
    newMonth.setMonth(this.currentMonth.getMonth() - 1);
    this.currentMonth = newMonth;
    this.currentMonthName = this.currentMonth.toLocaleString('default', {
      month: 'long',
    });
    this.currentYear = this.currentMonth.getFullYear();
    this.calendarDays = this.generateCalendar(
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
    this.calendarDays = this.generateCalendar(
      this.currentYear,
      this.currentMonth.getMonth(),
    );
  }

  getCustomerName(): string {
    const user = this.authService.getUserDetail();
    return user?.fullName || 'Customer';
  }

  isToday(day: number): boolean {
    return (
      day === new Date().getDate() &&
      this.currentMonth.getMonth() === new Date().getMonth() &&
      this.currentYear === new Date().getFullYear()
    );
  }
}
