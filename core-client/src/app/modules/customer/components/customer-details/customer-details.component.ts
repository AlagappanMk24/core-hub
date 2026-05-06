import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTabsModule } from '@angular/material/tabs';
import { MatChipsModule } from '@angular/material/chips';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatMenuModule } from '@angular/material/menu';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { animate, style, transition, trigger } from '@angular/animations';
import { AuthService } from '../../../../core/services/auth/auth.service';
import { CustomerService } from '../../services/customer.service';
import {
  Customer,
  CustomerActivity,
  CustomerInvoice,
  CustomerPayment,
} from '../../services/models/customer.model';
import { CustomerDialogComponent } from '../customer-dialog/customer-dialog.component';
import { DeleteConfirmationDialogComponent } from '../../../../features/common/delete-confirmation/delete-confirmation-dialog.component';
import { NotificationDialogComponent } from '../../../../shared/components/notification/notification-dialog.component';
import { Chart, registerables } from 'chart.js';
import { SendEmailDialogComponent } from '../send-email-dialog/send-email-dialog.component';

Chart.register(...registerables);

@Component({
  selector: 'app-customer-details',
  standalone: true,
  imports: [
    CommonModule,
    MatIconModule,
    MatButtonModule,
    MatProgressSpinnerModule,
    MatTabsModule,
    MatChipsModule,
    MatTooltipModule,
    MatMenuModule,
    MatDialogModule,
    RouterModule,
  ],
  templateUrl: './customer-details.component.html',
  styleUrls: ['./customer-details.component.css'],
  animations: [
    trigger('fadeInUp', [
      transition(':enter', [
        style({ opacity: 0, transform: 'translateY(20px)' }),
        animate(
          '400ms ease-out',
          style({ opacity: 1, transform: 'translateY(0)' }),
        ),
      ]),
    ]),
    trigger('slideIn', [
      transition(':enter', [
        style({ opacity: 0, transform: 'translateX(-20px)' }),
        animate(
          '300ms ease-out',
          style({ opacity: 1, transform: 'translateX(0)' }),
        ),
      ]),
    ]),
    trigger('scaleIn', [
      transition(':enter', [
        style({ opacity: 0, transform: 'scale(0.9)' }),
        animate('300ms ease-out', style({ opacity: 1, transform: 'scale(1)' })),
      ]),
    ]),
  ],
})
export class CustomerDetailsComponent implements OnInit, OnDestroy {
  customer: Customer | null = null;
  isLoading = true;
  isAdmin = false;
  isUser = false;
  private destroy$ = new Subject<void>();

  // Dynamic data from API
  invoices: CustomerInvoice[] = [];
  payments: CustomerPayment[] = [];
  activities: CustomerActivity[] = [];

  // Stats data
  totalInvoices = 0;
  totalPaidInvoices = 0;
  totalOverdueInvoices = 0;
  totalPendingInvoices = 0;
  totalPayments = 0;

  // Chart references
  private spendingChart: Chart | null = null;
  private paymentChart: Chart | null = null;

  // Spending trend data
  spendingTrend: { month: string; amount: number }[] = [];

  Math = Math;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private customerService: CustomerService,
    private authService: AuthService,
    private dialog: MatDialog,
  ) {}

  ngOnInit(): void {
    this.isAdmin = this.authService.hasRole('Admin');
    this.isUser = this.authService.hasRole('User');
    this.loadCustomer();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
    this.spendingChart?.destroy();
    this.paymentChart?.destroy();
  }

  loadCustomer(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) {
      this.router.navigate(['/customers']);
      return;
    }

    this.isLoading = true;
    const customerId = Number(id);

    // Load customer details
    this.customerService
      .getCustomerById(customerId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (customer) => {
          this.customer = customer;
          this.loadCustomerInvoices(customerId);
          this.loadCustomerPayments(customerId);
          this.loadCustomerActivities(customerId);
          this.loadCustomerSpendingTrend(customerId);
        },
        error: (error) => {
          console.error('Error loading customer:', error);
          this.openDialog(
            'error',
            'Load Failed',
            'Failed to load customer details.',
            error.message,
          );
          this.isLoading = false;
          this.router.navigate(['/customers']);
        },
      });
  }

  loadCustomerInvoices(customerId: number): void {
    this.customerService
      .getCustomerInvoices(customerId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (invoices) => {
          this.invoices = invoices;
          this.calculateInvoiceStats();
          this.isLoading = false;
        },
        error: (error) => {
          console.error('Error loading invoices:', error);
          this.isLoading = false;
        },
      });
  }

  loadCustomerPayments(customerId: number): void {
    this.customerService
      .getCustomerPayments(customerId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (payments) => {
          this.payments = payments;
          this.calculatePaymentStats();
        },
        error: (error) => {
          console.error('Error loading payments:', error);
        },
      });
  }

  loadCustomerActivities(customerId: number): void {
    this.customerService
      .getCustomerActivities(customerId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (activities) => {
          this.activities = activities;
        },
        error: (error) => {
          console.error('Error loading activities:', error);
        },
      });
  }

  loadCustomerSpendingTrend(customerId: number): void {
    this.customerService
      .getCustomerSpendingTrend(customerId)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (trend) => {
          this.spendingTrend = trend;
          setTimeout(() => {
            this.initCharts();
          }, 100);
        },
        error: (error) => {
          console.error('Error loading spending trend:', error);
          setTimeout(() => {
            this.initCharts();
          }, 100);
        },
      });
  }

  calculateInvoiceStats(): void {
    this.totalInvoices = this.invoices.length;
    this.totalPaidInvoices = this.invoices.filter(
      (i) => i.status === 'Paid',
    ).length;
    this.totalOverdueInvoices = this.invoices.filter(
      (i) => i.status === 'Overdue',
    ).length;
    this.totalPendingInvoices = this.invoices.filter(
      (i) => i.status === 'Pending' || i.status === 'Sent',
    ).length;
  }

  calculatePaymentStats(): void {
    this.totalPayments = this.payments.length;
  }

  initCharts(): void {
    this.initSpendingChart();
    this.initPaymentChart();
  }

  initSpendingChart(): void {
    const ctx = document.getElementById('spendingChart') as HTMLCanvasElement;
    if (!ctx) return;

    // Use real spending trend data if available, otherwise use mock data
    let labels: string[] = [];
    let data: number[] = [];

    if (this.spendingTrend && this.spendingTrend.length > 0) {
      labels = this.spendingTrend.map((t) => t.month);
      data = this.spendingTrend.map((t) => t.amount);
    } else {
      // Fallback to last 12 months
      labels = [
        'Jan',
        'Feb',
        'Mar',
        'Apr',
        'May',
        'Jun',
        'Jul',
        'Aug',
        'Sep',
        'Oct',
        'Nov',
        'Dec',
      ];
      data = [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0];
    }
    this.spendingChart = new Chart(ctx, {
      type: 'line',
      data: {
        labels: labels,
        datasets: [
          {
            label: 'Monthly Spending',
            data: data,
            borderColor: '#8B5CF6',
            backgroundColor: 'rgba(139, 92, 246, 0.1)',
            tension: 0.4,
            fill: true,
            pointBackgroundColor: '#8B5CF6',
            pointBorderColor: '#fff',
            pointRadius: 4,
            pointHoverRadius: 6,
          },
        ],
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          legend: { display: false },
          tooltip: {
            backgroundColor: 'rgba(0, 0, 0, 0.8)',
            callbacks: {
              label: (context: any) => `$${context.raw.toLocaleString()}`,
            },
          },
        },
        scales: {
          y: {
            beginAtZero: true,
            grid: { color: 'rgba(0, 0, 0, 0.05)' },
            ticks: { callback: (value) => '$' + value.toLocaleString() },
          },
          x: {
            grid: { display: false },
          },
        },
      },
    });
  }

  initPaymentChart(): void {
    const ctx = document.getElementById('paymentChart') as HTMLCanvasElement;
    if (!ctx) return;

    // Calculate payment behavior from actual data
    const totalCompleted = this.payments.filter(
      (p) => p.status === 'Completed',
    ).length;
    const totalPending = this.payments.filter(
      (p) => p.status === 'Pending',
    ).length;
    const totalFailed = this.payments.filter(
      (p) => p.status === 'Failed',
    ).length;

    const onTimePayments = this.payments.filter((p) => p.isOnTime).length;
    const latePayments = this.payments.filter(
      (p) => !p.isOnTime && p.status === 'Completed',
    ).length;
    const pendingPayments = this.payments.filter(
      (p) => p.status === 'Pending',
    ).length;

    this.paymentChart = new Chart(ctx, {
      type: 'doughnut',
      data: {
        labels: ['On-Time Payments', 'Late Payments', 'Pending'],
        datasets: [
          {
            data: [
              onTimePayments || 85,
              latePayments || 10,
              pendingPayments || 5,
            ],
            backgroundColor: ['#10B981', '#F59E0B', '#94A3B8'],
            borderWidth: 0,
          },
        ],
      },
      options: {
        responsive: true,
        maintainAspectRatio: true,
        cutout: '60%',
        plugins: {
          legend: {
            position: 'bottom',
            labels: { usePointStyle: true, boxWidth: 10 },
          },
        },
      },
    });
  }

  getInitials(name: string): string {
    return name
      .split(' ')
      .map((n) => n.charAt(0).toUpperCase())
      .join('')
      .substring(0, 2);
  }

  getAvatarColor(name: string): string {
    const colors = [
      '#FF2E63',
      '#00D4B9',
      '#FF6B6B',
      '#FFD93D',
      '#1E90FF',
      '#8A2BE2',
      '#4B0082',
    ];
    const index =
      name.split('').reduce((sum, char) => sum + char.charCodeAt(0), 0) %
      colors.length;
    return colors[index];
  }

  getFullAddress(): string {
    if (!this.customer) return '';
    const parts = [this.customer.addressLine1];
    if (this.customer.addressLine2) parts.push(this.customer.addressLine2);
    parts.push(this.customer.city);
    if (this.customer.state) parts.push(this.customer.state);
    parts.push(this.customer.zipCode);
    parts.push(this.customer.countryName);
    return parts.join(', ');
  }

  getPaymentHealthColor(): string {
    const health = this.getPaymentHealth();
    if (health >= 80) return '#10B981';
    if (health >= 50) return '#F59E0B';
    return '#EF4444';
  }

  getPaymentHealth(): number {
    if (!this.customer || this.customer.totalPurchases === 0) return 100;
    if (this.totalPaidInvoices === 0) return 0;
    return Math.round((this.totalPaidInvoices / this.totalInvoices) * 100);
  }

  getTotalInvoicesCount(): number {
    return this.totalInvoices;
  }

  getAverageOrderValue(): number {
    if (
      !this.customer ||
      !this.customer.totalPurchases ||
      this.totalInvoices === 0
    )
      return 0;
    return this.customer.totalPurchases / this.totalInvoices;
  }

  getAvailableCredit(): number {
    if (!this.customer) return 0;
    return (
      (this.customer.creditLimit || 0) - (this.customer.totalPurchases || 0)
    );
  }

  editCustomer(): void {
    const dialogRef = this.dialog.open(CustomerDialogComponent, {
      width: '650px',
      disableClose: true,
      data: { mode: 'edit', customer: this.customer },
    });

    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        this.loadCustomer();
        this.openDialog(
          'success',
          'Customer Updated',
          'Customer details have been updated successfully.',
          '',
        );
      }
    });
  }

  deleteCustomer(): void {
    if (!this.customer) return;

    const dialogRef = this.dialog.open(DeleteConfirmationDialogComponent, {
      width: '450px',
      disableClose: true,
      data: {
        title: 'Delete Customer',
        message: `Are you sure you want to delete customer ${this.customer.name}? This action cannot be undone.`,
        itemName: this.customer.name,
        confirmText: 'Delete',
        cancelText: 'Cancel',
      },
    });

    dialogRef.afterClosed().subscribe((result) => {
      if (result && this.customer) {
        this.customerService.deleteCustomer(this.customer.id).subscribe({
          next: () => {
            this.openDialog(
              'success',
              'Customer Deleted',
              'Customer has been deleted successfully.',
              '',
            );
            this.router.navigate(['/customers']);
          },
          error: (error) => {
            this.openDialog(
              'error',
              'Delete Failed',
              'Failed to delete customer.',
              error.message,
            );
          },
        });
      }
    });
  }

  createInvoice(): void {
    this.router.navigate(['/invoices/create'], {
      queryParams: { customerId: this.customer?.id },
    });
  }

  viewInvoiceDetails(invoiceId: number): void {
    this.router.navigate(['/invoices/view', invoiceId]);
  }

  // Update the sendEmail method
  sendEmail(): void {
    const dialogRef = this.dialog.open(SendEmailDialogComponent, {
      width: '650px',
      disableClose: true,
      data: {
        customerId: this.customer?.id,
        customerName: this.customer?.name,
        customerEmail: this.customer?.email,
        type: 'custom',
      } 
    });

    dialogRef.afterClosed().subscribe((result) => {
      if (result?.sent) {
        // Optionally refresh activities or show success
        this.loadCustomerActivities(this.customer!.id);
      }
    });
  }

  openDialog(
    type: 'success' | 'error',
    title: string,
    message: string,
    submessage: string,
  ): void {
    this.dialog.open(NotificationDialogComponent, {
      width: '400px',
      data: { type, title, message, submessage },
    });
  }

  goBack(): void {
    this.router.navigate(['/customers']);
  }

  formatDate(date?: Date): string {
    if (!date) return 'N/A';
    return new Date(date).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'long',
      day: 'numeric',
    });
  }

  formatCurrency(amount: number): string {
    return `$${amount.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`;
  }
}
