import { Component, OnInit, AfterViewInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Chart, registerables } from 'chart.js';
import { Subscription } from 'rxjs';
import { inject } from '@angular/core';
import { AuthService } from '../../../services/auth/auth.service';
import { DashboardService, Invoice, InvoiceStats, TodoItem } from '../../../services/Dashboard/dashboard.service';

Chart.register(...registerables);

@Component({
  selector: 'app-user-dashboard-stats',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './user-dashboard-stats.component.html',
  styleUrls: ['./user-dashboard-stats.component.css'],
})
export class UserDashboardStatsComponent implements OnInit, AfterViewInit, OnDestroy {
  stats: InvoiceStats = { totalInvoiceAmount: 0, pendingInvoices: 0, paidInvoices: 0 };
  invoices: Invoice[] = [];
  todos: TodoItem[] = [];
  newTodo = '';
  calendarDays: number[] = [];
  private subscription: Subscription = new Subscription();
  private userEmail: string = '';

  constructor(private dashboardService: DashboardService, private authService: AuthService) {}

  ngOnInit(): void {
    const user = this.authService.getUserDetail();
    this.userEmail = user?.email || '';
    this.invoices = this.dashboardService.getInvoices().filter(invoice => invoice.customer.toLowerCase().includes(this.userEmail.split('@')[0].toLowerCase()));
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