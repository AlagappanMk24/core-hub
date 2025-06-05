// pages/orders/orders.component.ts
import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Order, OrderStats, PaymentStatus, FulfillmentStatus, OrderFilter } from '../../interfaces/orders/order.interface';
import { OrderService } from '../../services/order/order.service';

@Component({
  selector: 'app-orders',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './orders.component.html',
  styleUrls: ['./orders.component.css']
})
export class OrdersComponent implements OnInit {
  orders: Order[] = [];
  filteredOrders: Order[] = [];
  orderStats: OrderStats = {
    totalOrders: 0,
    totalOrdersChange: 0,
    orderItemsOverTime: 0,
    orderItemsOverTimeChange: 0,
    returnsOrders: 0,
    returnsOrdersChange: 0,
    fulfilledOrdersOverTime: 0,
    fulfilledOrdersOverTimeChange: 0
  };

  filters: string[] = ['All', 'Unfulfilled', 'Unpaid', 'Open', 'Closed'];
  activeFilter: string = 'All';
  searchTerm: string = '';
  selectedOrders: string[] = [];

  constructor(private orderService: OrderService) {}

  ngOnInit(): void {
    this.loadOrders();
    this.loadOrderStats();
  }

  loadOrders(): void {
    this.orderService.getOrders().subscribe(orders => {
      this.orders = orders;
      this.filteredOrders = orders;
    });
  }

  loadOrderStats(): void {
    this.orderService.getOrderStats().subscribe(stats => {
      this.orderStats = stats;
    });
  }

  setActiveFilter(filter: string): void {
    this.activeFilter = filter;
    this.applyFilters();
  }

  onSearch(): void {
    this.applyFilters();
  }

  private applyFilters(): void {
    let filtered = [...this.orders];

    // Apply text search
    if (this.searchTerm.trim()) {
      filtered = filtered.filter(order => 
        order.customer.toLowerCase().includes(this.searchTerm.toLowerCase()) ||
        order.id.toLowerCase().includes(this.searchTerm.toLowerCase())
      );
    }

    // Apply status filter
    if (this.activeFilter !== 'All') {
      switch (this.activeFilter.toLowerCase()) {
        case 'unfulfilled':
          filtered = filtered.filter(order => order.fulfillment === FulfillmentStatus.UNFULFILLED);
          break;
        case 'unpaid':
          filtered = filtered.filter(order => order.payment === PaymentStatus.PENDING);
          break;
        case 'open':
          filtered = filtered.filter(order => order.payment === PaymentStatus.PENDING);
          break;
        case 'closed':
          filtered = filtered.filter(order => order.payment === PaymentStatus.SUCCESS);
          break;
      }
    }

    this.filteredOrders = filtered;
  }

  toggleOrderSelection(orderId: string, event: Event): void {
    const target = event.target as HTMLInputElement;
    if (target.checked) {
      if (!this.selectedOrders.includes(orderId)) {
        this.selectedOrders.push(orderId);
      }
    } else {
      this.selectedOrders = this.selectedOrders.filter(id => id !== orderId);
    }
  }

  toggleSelectAll(event: Event): void {
    const target = event.target as HTMLInputElement;
    if (target.checked) {
      this.selectedOrders = this.filteredOrders.map(order => order.id);
    } else {
      this.selectedOrders = [];
    }
  }

  getPaymentStatusClass(status: PaymentStatus): string {
    return status === PaymentStatus.PENDING ? 'pending' : 'success';
  }

  getFulfillmentStatusClass(status: FulfillmentStatus): string {
    return status === FulfillmentStatus.UNFULFILLED ? 'unfulfilled' : 'fulfilled';
  }

  // Helper property to access Math in template
  get Math() {
    return Math;
  }
}