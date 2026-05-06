import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, Subscription, interval, tap } from 'rxjs';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { ChartConfiguration } from 'chart.js';
import { inject } from '@angular/core';
import { AuthService } from '../../core/services/auth/auth.service';
import { environment } from '../../environments/environment';

// ============ COMMON INTERFACES ============
export interface DashboardStats {
  totalInvoiceAmount: number;
  totalInvoices: number;
  pendingInvoices: number;
  paidInvoices: number;
  overdueInvoices: number;
  draftInvoices: number;
  percentageChangeTotal: number;
  percentageChangePending: number;
  percentageChangePaid: number;
}

export interface RecentInvoice {
  id: number;
  invoiceNumber: string;
  customerName: string;
  customerEmail: string;
  customerAvatar?: string; // Optional - backend may not send it
  amount: number;
  status: string;
  issueDate: string | Date;
  dueDate: string | Date;
  paymentDate?: string | Date;
}

export interface InvoiceProgress {
  id: number;
  invoiceNumber: string;
  customerName: string;
  dueDate: string | Date;
  totalAmount: number;
  amountPaid: number;
  remainingAmount: number;
  paidPercentage: number;
  color: string;
  status?: string;
  statusColor?: string;
  progressColor?: string;
}

export interface MonthlyTrend {
  month: string;
  year: number;
  amount: number;
  count: number;
}

// ============ ADMIN DASHBOARD INTERFACES ============
export interface AdminDashboardSummary {
  stats: DashboardStats;
  recentInvoices: RecentInvoice[];
  paymentProgress: InvoiceProgress[]; // Partially paid
  pendingPayments: InvoiceProgress[]; // Unpaid but not overdue
  overduePayments: InvoiceProgress[]; // Overdue - needs action
  recentPayments: RecentInvoice[]; // Recently completed
  monthlyTrend: MonthlyTrend[];
  b2BTrend: MonthlyTrend[];
  b2CTrend: MonthlyTrend[];
  retailTrend: MonthlyTrend[];
}

// ============ USER DASHBOARD INTERFACES ============
export interface UserDashboardSummary {
  stats: DashboardStats;
  recentInvoices: RecentInvoice[];
  paymentProgress: InvoiceProgress[];
  pendingPayments: InvoiceProgress[];
  overduePayments: InvoiceProgress[];
  recentPayments: RecentInvoice[];
  monthlyTrend: MonthlyTrend[];
  teamPerformance?: TeamPerformance[];
  recentActivities?: RecentActivity[];
}

export interface TeamPerformance {
  userId: number;
  userName: string;
  userAvatar?: string;
  invoicesCreated: number;
  totalAmount: number;
  collectionRate: number;
}

export interface RecentActivity {
  id: number;
  action: string;
  description: string;
  user: string;
  timestamp: Date;
  icon: string;
  color: string;
}

// ============ CUSTOMER DASHBOARD INTERFACES ============
export interface CustomerDashboardStats {
  totalInvoiced: number;
  totalPaid: number;
  totalDue: number;
  overdueAmount: number;
  invoiceCount: number;
  paidCount: number;
  pendingCount: number;
  overdueCount: number;
  percentageChangeInvoiced: number;
  percentageChangePaid: number;
  percentageChangeDue: number;
}

export interface CustomerInvoiceProgress {
  id: number;
  invoiceNumber: string;
  totalAmount: number;
  amountPaid: number;
  remainingAmount: number;
  paidPercentage: number;
  dueDate: string;
  color?: string;
}

export interface CustomerRecentInvoice {
  id: number;
  invoiceNumber: string;
  amount: number;
  issueDate: string;
  dueDate: string;
  status: string;
}

export interface CustomerPaymentActivity {
  id: number;
  invoiceId: number;
  invoiceNumber: string;
  amount: number;
  paymentDate: string;
  paymentMethod: string;
}

export interface CustomerInvoiceSummary {
  status: string;
  count: number;
  amount: number;
}

export interface CustomerDashboardSummary {
  stats: CustomerDashboardStats;
  paymentProgress: CustomerInvoiceProgress[];
  pendingInvoices: CustomerInvoiceProgress[];
  overdueInvoices: CustomerInvoiceProgress[];
  recentInvoices: CustomerRecentInvoice[];
  recentPayments: CustomerPaymentActivity[];
  invoiceSummary: CustomerInvoiceSummary[];
  monthlyTrend: MonthlyTrend[];
}

// ============ TODO INTERFACES ============
export interface TodoItem {
  id: number;
  text: string;
  completed: boolean;
}

// ============ UNIFIED DASHBOARD SERVICE ============
@Injectable({
  providedIn: 'root',
})
export class DashboardService {
  private http = inject(HttpClient);
  private authService = inject(AuthService);

  private apiUrl = `${environment.apiBaseUrl}/dashboard`;

  // Role-based subjects
  private adminDashboardSubject =
    new BehaviorSubject<AdminDashboardSummary | null>(null);
  adminDashboard$ = this.adminDashboardSubject.asObservable();

  private userDashboardSubject =
    new BehaviorSubject<UserDashboardSummary | null>(null);
  userDashboard$ = this.userDashboardSubject.asObservable();

  private customerDashboardSubject =
    new BehaviorSubject<CustomerDashboardSummary | null>(null);
  customerDashboard$ = this.customerDashboardSubject.asObservable();

  private refreshIntervalSub: Subscription | null = null;
  private currentRole: 'admin' | 'user' | 'customer' = 'customer';

  constructor() {
    this.initRoleAndStartUpdates();
  }

  private initRoleAndStartUpdates(): void {
    const user = this.authService.getUserDetail();
    if (user?.roles?.includes('Admin')) {
      this.currentRole = 'admin';
    } else if (user?.roles?.includes('User')) {
      this.currentRole = 'user';
    } else {
      this.currentRole = 'customer';
    }
    this.startRealTimeUpdates();
  }

  // ============ ADMIN DASHBOARD METHODS ============
  getAdminDashboard(): Observable<AdminDashboardSummary> {
    return this.http.get<AdminDashboardSummary>(`${this.apiUrl}/admin/summary`);
  }

  // ============ USER DASHBOARD METHODS ============
  getUserDashboard(): Observable<UserDashboardSummary> {
    return this.http.get<UserDashboardSummary>(`${this.apiUrl}/user/summary`);
  }
  // ============ CUSTOMER DASHBOARD METHODS ============
  getCustomerDashboard(): Observable<CustomerDashboardSummary> {
    return this.http.get<CustomerDashboardSummary>(
      `${this.apiUrl}/customer/summary`,
    );
  }

  // ============ UNIFIED REFRESH LOGIC ============
  // ✅ Single startRealTimeUpdates — polls API every 30 seconds
  startRealTimeUpdates(): void {
    this.refreshIntervalSub = interval(30000).subscribe(() => {
      this.refreshDashboard();
    });
  }

  refreshDashboard(): void {
    if (this.currentRole === 'admin') {
      this.getAdminDashboard().subscribe({
        next: (dashboard) => this.adminDashboardSubject.next(dashboard),
        error: (err) => console.error('Error refreshing admin dashboard:', err),
      });
    } else if (this.currentRole === 'user') {
      this.getUserDashboard().subscribe({
        next: (dashboard) => this.userDashboardSubject.next(dashboard),
        error: (err) => console.error('Error refreshing user dashboard:', err),
      });
    } else {
      this.getCustomerDashboard().subscribe({
        next: (dashboard) => this.customerDashboardSubject.next(dashboard),
        error: (err) =>
          console.error('Error refreshing customer dashboard:', err),
      });
    }
  }

  stopRealTimeUpdates(): void {
    this.refreshIntervalSub?.unsubscribe();
    this.refreshIntervalSub = null;
  }

  // ============ SHARED METHODS ============
  getStats(): Observable<DashboardStats> {
    return this.http
      .get<DashboardStats>(`${this.apiUrl}/stats`);
  }

  getRecentInvoices(count: number = 5): Observable<RecentInvoice[]> {
    return this.http
      .get<RecentInvoice[]>(`${this.apiUrl}/recent-invoices`, {
        params: new HttpParams().set('count', count.toString()),
      });
  }

  // ============ TODO METHODS (Shared) ============
  private todosSubject = new BehaviorSubject<TodoItem[]>([
    {
      id: 1,
      text: 'Invoice Creation, Updation and Deletion',
      completed: false,
    },
    { id: 2, text: 'Send Invoice', completed: false },
    { id: 3, text: 'Update invoice statuses', completed: false },
    { id: 4, text: 'Generate monthly invoice report', completed: false },
    { id: 5, text: 'Download invoice', completed: false },
    {
      id: 6,
      text: 'To Add the features on Update Invoice Status in the edit invoice.. When click three dots, we need to show the options as Mark as sent, Mark as paid',
      completed: false,
    },
  ]);

  getTodos(): Observable<TodoItem[]> {
    return this.todosSubject.asObservable();
  }

  addTodo(text: string): void {
    const current = this.todosSubject.value;
    const newId =
      current.length > 0 ? Math.max(...current.map((t) => t.id)) + 1 : 1;
    this.todosSubject.next([...current, { id: newId, text, completed: false }]);
  }

  removeTodo(id: number): void {
    const current = this.todosSubject.value;
    this.todosSubject.next(current.filter((t) => t.id !== id));
  }

  // ============ UTILITY METHODS ============
  generateCalendar(year: number, month: number): number[] {
    const days: number[] = [];
    const daysInMonth = new Date(year, month + 1, 0).getDate();
    const firstDayOfWeek = new Date(year, month, 1).getDay();
    for (let i = 0; i < firstDayOfWeek; i++) days.push(0);
    for (let day = 1; day <= daysInMonth; day++) days.push(day);
    return days;
  }

  getInvoiceAmountChartConfig(
    b2BTrend: MonthlyTrend[],
    b2CTrend: MonthlyTrend[],
    retailTrend: MonthlyTrend[],
  ): ChartConfiguration<'bar'> {
    const labels = b2BTrend.map((t) => t.month);
    return {
      type: 'bar',
      data: {
        labels,
        datasets: [
          {
            label: 'B2B',
            data: b2BTrend.map((t) => t.amount),
            backgroundColor: '#8B5CF6',
            borderRadius: 4,
          },
          {
            label: 'B2C',
            data: b2CTrend.map((t) => t.amount),
            backgroundColor: '#F472B6',
            borderRadius: 4,
          },
          {
            label: 'Retail',
            data: retailTrend.map((t) => t.amount),
            backgroundColor: '#60A5FA',
            borderRadius: 4,
          },
        ],
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: { legend: { display: false } },
      },
    };
  }

  getInvoiceStatusChartConfig(
    paid: number,
    pending: number,
    overdue: number,
  ): ChartConfiguration<'doughnut'> {
    return {
      type: 'doughnut',
      data: {
        labels: ['Paid', 'Pending', 'Overdue'],
        datasets: [
          {
            data: [paid, pending, overdue],
            backgroundColor: ['#60A5FA', '#14B8A6', '#F472B6'],
            borderWidth: 0,
          },
        ],
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: { legend: { display: false } },
      },
    };
  }

  // getInvoiceProgress(): InvoiceProgress[] {
  //   return [
  //     {
  //       id: 'INV-001',
  //       customer: 'Herman Beck',
  //       dueDate: 'May 15, 2025',
  //       paidPercentage: 100,
  //       color: '#14B8A6',
  //     },
  //     {
  //       id: 'INV-002',
  //       customer: 'Messy Adam',
  //       dueDate: 'Jul 01, 2025',
  //       paidPercentage: 80,
  //       color: '#F97316',
  //     },
  //     {
  //       id: 'INV-003',
  //       customer: 'John Richards',
  //       dueDate: 'Apr 12, 2025',
  //       paidPercentage: 50,
  //       color: '#EAB308',
  //     },
  //     {
  //       id: 'INV-004',
  //       customer: 'Peter Meggik',
  //       dueDate: 'May 15, 2025',
  //       paidPercentage: 30,
  //       color: '#8B5CF6',
  //     },
  //   ];
  // }
}
