import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';
import { Order, OrderStats, PaymentStatus, FulfillmentStatus } from '../../interfaces/orders/order.interface';

@Injectable({
  providedIn: 'root'
})
export class OrderService {
  private mockOrders: Order[] = [
    {
      id: '#1002',
      date: '11 Feb, 2024',
      customer: 'Wanda Warren',
      payment: PaymentStatus.PENDING,
      total: 20.00,
      delivery: 'N/A',
      items: 2,
      fulfillment: FulfillmentStatus.UNFULFILLED
    },
    {
      id: '#1004',
      date: '13 Feb, 2024',
      customer: 'Esther Howard',
      payment: PaymentStatus.SUCCESS,
      total: 22.00,
      delivery: 'N/A',
      items: 3,
      fulfillment: FulfillmentStatus.FULFILLED
    },
    {
      id: '#1007',
      date: '15 Feb, 2024',
      customer: 'Jenny Wilson',
      payment: PaymentStatus.PENDING,
      total: 25.00,
      delivery: 'N/A',
      items: 1,
      fulfillment: FulfillmentStatus.UNFULFILLED
    },
    {
      id: '#1009',
      date: '17 Feb, 2024',
      customer: 'Guy Hawkins',
      payment: PaymentStatus.SUCCESS,
      total: 27.00,
      delivery: 'N/A',
      items: 5,
      fulfillment: FulfillmentStatus.FULFILLED
    },
    {
      id: '#1011',
      date: '19 Feb, 2024',
      customer: 'Jacob Jones',
      payment: PaymentStatus.PENDING,
      total: 32.00,
      delivery: 'N/A',
      items: 4,
      fulfillment: FulfillmentStatus.UNFULFILLED
    },
    {
      id: '#1013',
      date: '21 Feb, 2024',
      customer: 'Kristin Watson',
      payment: PaymentStatus.SUCCESS,
      total: 25.00,
      delivery: 'N/A',
      items: 3,
      fulfillment: FulfillmentStatus.FULFILLED
    },
    {
      id: '#1015',
      date: '23 Feb, 2024',
      customer: 'Albert Flores',
      payment: PaymentStatus.PENDING,
      total: 28.00,
      delivery: 'N/A',
      items: 2,
      fulfillment: FulfillmentStatus.UNFULFILLED
    },
    {
      id: '#1018',
      date: '25 Feb, 2024',
      customer: 'Eleanor Pena',
      payment: PaymentStatus.SUCCESS,
      total: 35.00,
      delivery: 'N/A',
      items: 1,
      fulfillment: FulfillmentStatus.FULFILLED
    },
    {
      id: '#1019',
      date: '27 Feb, 2024',
      customer: 'Theresa Webb',
      payment: PaymentStatus.PENDING,
      total: 20.00,
      delivery: 'N/A',
      items: 2,
      fulfillment: FulfillmentStatus.UNFULFILLED
    }
  ];

  private mockOrderStats: OrderStats = {
    totalOrders: 21,
    totalOrdersChange: 25.2,
    orderItemsOverTime: 15,
    orderItemsOverTimeChange: 18.2,
    returnsOrders: 0,
    returnsOrdersChange: -1.2,
    fulfilledOrdersOverTime: 12,
    fulfilledOrdersOverTimeChange: 12.2
  };

  getOrders(): Observable<Order[]> {
    return of(this.mockOrders);
  }

  getOrderStats(): Observable<OrderStats> {
    return of(this.mockOrderStats);
  }

  getOrdersByFilter(filter: string): Observable<Order[]> {
    let filteredOrders = this.mockOrders;
    
    switch (filter.toLowerCase()) {
      case 'unfulfilled':
        filteredOrders = this.mockOrders.filter(order => order.fulfillment === FulfillmentStatus.UNFULFILLED);
        break;
      case 'unpaid':
        filteredOrders = this.mockOrders.filter(order => order.payment === PaymentStatus.PENDING);
        break;
      case 'open':
        filteredOrders = this.mockOrders.filter(order => order.payment === PaymentStatus.PENDING);
        break;
      case 'closed':
        filteredOrders = this.mockOrders.filter(order => order.payment === PaymentStatus.SUCCESS);
        break;
      default:
        filteredOrders = this.mockOrders;
    }
    
    return of(filteredOrders);
  }

  searchOrders(searchTerm: string): Observable<Order[]> {
    const filtered = this.mockOrders.filter(order => 
      order.customer.toLowerCase().includes(searchTerm.toLowerCase()) ||
      order.id.toLowerCase().includes(searchTerm.toLowerCase())
    );
    return of(filtered);
  }
}