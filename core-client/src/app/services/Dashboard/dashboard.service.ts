import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, interval } from 'rxjs';
import { map } from 'rxjs/operators';
import { ChartConfiguration } from 'chart.js';
import { inject } from '@angular/core';
import { AuthService } from '../auth/auth.service';

export interface InvoiceStats {
  totalInvoiceAmount: number;
  pendingInvoices: number;
  paidInvoices: number;
}

export interface Invoice {
  id: string;
  customer: string;
  avatar: string;
  amount: number;
  status: 'PAID' | 'PENDING' | 'OVERDUE';
  lastUpdate: string;
}

export interface InvoiceProgress {
  id: string;
  customer: string;
  dueDate: string;
  paidPercentage: number;
  color: string;
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
  private authService = inject(AuthService);
  private statsSubject = new BehaviorSubject<InvoiceStats>({
    totalInvoiceAmount: 250000,
    pendingInvoices: 120,
    paidInvoices: 350,
  });

  getStats(): Observable<InvoiceStats> {
    const user = this.authService.getUserDetail();
    const isAdmin = user?.roles.includes('Admin');
    return this.statsSubject.pipe(
      map((stats) => ({
        ...stats,
        totalInvoiceAmount: isAdmin ? stats.totalInvoiceAmount : stats.totalInvoiceAmount * 0.5,
        pendingInvoices: isAdmin ? stats.pendingInvoices : stats.pendingInvoices * 0.3,
        paidInvoices: isAdmin ? stats.paidInvoices : stats.paidInvoices * 0.4,
      }))
    );
  }

  startRealTimeUpdates(): void {
    interval(5000).subscribe(() => {
      const currentStats = this.statsSubject.value;
      this.statsSubject.next({
        totalInvoiceAmount: currentStats.totalInvoiceAmount + Math.floor(Math.random() * 2000),
        pendingInvoices: currentStats.pendingInvoices + Math.floor(Math.random() * 5),
        paidInvoices: currentStats.paidInvoices + Math.floor(Math.random() * 10),
      });
    });
  }

  getInvoices(): Invoice[] {
    return [
      {
        id: 'INV-001',
        customer: 'David Grey',
        avatar: 'https://images.unsplash.com/photo-1472099645785-5658abf4ff4e?w=40&h=40&fit=crop&crop=face',
        amount: 1500,
        status: 'PAID',
        lastUpdate: 'Dec 5, 2025',
      },
      {
        id: 'INV-002',
        customer: 'Stella Johnson',
        avatar: 'https://images.unsplash.com/photo-1494790108755-2616b612b786?w=40&h=40&fit=crop&crop=face',
        amount: 2300,
        status: 'PENDING',
        lastUpdate: 'Dec 12, 2025',
      },
      {
        id: 'INV-003',
        customer: 'Marina Michel',
        avatar: 'https://images.unsplash.com/photo-1438761681033-6461ffad8d80?w=40&h=40&fit=crop&crop=face',
        amount: 1800,
        status: 'OVERDUE',
        lastUpdate: 'Dec 16, 2025',
      },
      {
        id: 'INV-004',
        customer: 'John Doe',
        avatar: 'https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?w=40&h=40&fit=crop&crop=face',
        amount: 900,
        status: 'PENDING',
        lastUpdate: 'Dec 3, 2025',
      },
    ];
  }

  getInvoiceProgress(): InvoiceProgress[] {
    return [
      { id: 'INV-001', customer: 'Herman Beck', dueDate: 'May 15, 2025', paidPercentage: 100, color: '#14B8A6' },
      { id: 'INV-002', customer: 'Messy Adam', dueDate: 'Jul 01, 2025', paidPercentage: 80, color: '#F97316' },
      { id: 'INV-003', customer: 'John Richards', dueDate: 'Apr 12, 2025', paidPercentage: 50, color: '#EAB308' },
      { id: 'INV-004', customer: 'Peter Meggik', dueDate: 'May 15, 2025', paidPercentage: 30, color: '#8B5CF6' },
    ];
  }

  getTodos(): TodoItem[] {
    return [
      { id: 1, text: 'Review invoice INV-001', completed: false },
      { id: 2, text: 'Send payment reminder for INV-002', completed: true },
      { id: 3, text: 'Update invoice statuses', completed: false },
      { id: 4, text: 'Generate monthly invoice report', completed: false },
    ];
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

  getInvoiceAmountChartConfig(): ChartConfiguration<'bar'> {
    return {
      type: 'bar',
      data: {
        labels: ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'],
        datasets: [
          { label: 'B2B', data: [5000, 7000, 6000, 9000, 8000, 10000, 7500, 8500, 9500, 6000, 7000, 8000], backgroundColor: '#8B5CF6', borderRadius: 4 },
          { label: 'B2C', data: [3000, 4000, 3500, 5000, 4500, 6000, 4000, 5500, 5000, 4500, 4000, 5000], backgroundColor: '#F472B6', borderRadius: 4 },
          { label: 'Retail', data: [2000, 2500, 3000, 3500, 3000, 4000, 2500, 3000, 3500, 3000, 2500, 3000], backgroundColor: '#60A5FA', borderRadius: 4 },
        ],
      },
      options: { responsive: true, maintainAspectRatio: false, plugins: { legend: { display: false } } },
    };
  }

  getInvoiceStatusChartConfig(): ChartConfiguration<'doughnut'> {
    return {
      type: 'doughnut',
      data: {
        labels: ['Paid', 'Pending', 'Overdue'],
        datasets: [{ data: [350, 120, 30], backgroundColor: ['#60A5FA', '#14B8A6', '#F472B6'], borderWidth: 0 }],
      },
      options: { responsive: true, maintainAspectRatio: false, plugins: { legend: { display: false } } },
    };
  }
}