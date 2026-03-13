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