import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, Subscription, interval, tap } from 'rxjs';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { ChartConfiguration } from 'chart.js';
import { inject } from '@angular/core';
import { AuthService } from '../auth/auth.service';
import { environment } from '../../environments/environment';

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

export interface DashboardSummary {
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
export interface TodoItem {
  id: number;
  text: string;
  completed: boolean;
}

@Injectable({
  providedIn: 'root',
})
export class DashboardService {
  private http = inject(HttpClient);
  private authService = inject(AuthService);

  private apiUrl = `${environment.apiBaseUrl}/dashboard`;
  private dashboardSubject = new BehaviorSubject<DashboardSummary | null>(null);
  dashboard$ = this.dashboardSubject.asObservable();

  private refreshIntervalSub: Subscription | null = null;

  constructor() {
    this.startRealTimeUpdates();
  }

  private getHeaders(): HttpHeaders {
    const token = this.authService.getAuthToken();
    if (!token) {
      throw new Error('No authentication token found');
    }
    return new HttpHeaders({
      Authorization: `Bearer ${token}`,
      'Content-Type': 'application/json',
    });
  }

  getAdminDashboard(): Observable<DashboardSummary> {
    return this.http
      .get<DashboardSummary>(`${this.apiUrl}/admin/summary`, {
        headers: this.getHeaders(),
      })
      .pipe(
        // ✅ The correct way to log the response data
        tap((data) => console.log('Admin Dashboard API Response:', data)),
      );
  }

  getCustomerDashboard(): Observable<DashboardSummary> {
    return this.http
      .get<DashboardSummary>(`${this.apiUrl}/customer/summary`, {
        headers: this.getHeaders(),
      })
      .pipe(
        tap((data) => console.log('Customer Dashboard API Response:', data)),
      );
  }

  getStats(): Observable<DashboardStats> {
    return this.http
      .get<DashboardStats>(`${this.apiUrl}/stats`, {
        headers: this.getHeaders(),
      })
      .pipe(tap((data) => console.log('Stats API Response:', data)));
  }

  getRecentInvoices(count: number = 5): Observable<RecentInvoice[]> {
    return this.http
      .get<RecentInvoice[]>(`${this.apiUrl}/recent-invoices`, {
        params: new HttpParams().set('count', count.toString()),
        headers: this.getHeaders(),
      })
      .pipe(tap((data) => console.log('Recent Invoices API Response:', data)));
  }

  // ✅ Single startRealTimeUpdates — polls API every 30 seconds
  startRealTimeUpdates(): void {
    this.refreshIntervalSub = interval(30000).subscribe(() => {
      this.refreshDashboard();
    });
  }

  refreshDashboard(): void {
    const user = this.authService.getUserDetail();

    const isAdminOrUser =
      user?.roles?.includes('Admin') || user?.roles?.includes('User');

    const dashboardObservable = isAdminOrUser
      ? this.getAdminDashboard()
      : this.getCustomerDashboard();

    dashboardObservable.subscribe({
      next: (dashboard) => this.dashboardSubject.next(dashboard),
      error: (err) => console.error('Error refreshing dashboard:', err),
    });
  }

  stopRealTimeUpdates(): void {
    this.refreshIntervalSub?.unsubscribe();
    this.refreshIntervalSub = null;
  }

  private todosSubject = new BehaviorSubject<TodoItem[]>([
    { id: 1, text: 'Review invoice INV-001', completed: false },
    { id: 2, text: 'Send payment reminder for INV-002', completed: true },
    { id: 3, text: 'Update invoice statuses', completed: false },
    { id: 4, text: 'Generate monthly invoice report', completed: false },
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

  generateCalendar(year: number, month: number): number[] {
    const days: number[] = [];
    const daysInMonth = new Date(year, month + 1, 0).getDate();
    const firstDayOfWeek = new Date(year, month, 1).getDay();
    for (let i = 0; i < firstDayOfWeek; i++) {
      days.push(0);
    }
    for (let day = 1; day <= daysInMonth; day++) {
      days.push(day);
    }
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
