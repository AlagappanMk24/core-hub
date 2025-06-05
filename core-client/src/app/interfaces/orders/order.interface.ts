// interfaces/order.interface.ts
export interface Order {
  id: string;
  date: string;
  customer: string;
  payment: PaymentStatus;
  total: number;
  delivery: string;
  items: number;
  fulfillment: FulfillmentStatus;
}

export interface OrderStats {
  totalOrders: number;
  totalOrdersChange: number;
  orderItemsOverTime: number;
  orderItemsOverTimeChange: number;
  returnsOrders: number;
  returnsOrdersChange: number;
  fulfilledOrdersOverTime: number;
  fulfilledOrdersOverTimeChange: number;
}

export enum PaymentStatus {
  PENDING = 'Pending',
  SUCCESS = 'Success'
}

export enum FulfillmentStatus {
  UNFULFILLED = 'Unfulfilled',
  FULFILLED = 'Fulfilled'
}

export enum OrderFilter {
  ALL = 'All',
  UNFULFILLED = 'Unfulfilled',
  UNPAID = 'Unpaid',
  OPEN = 'Open',
  CLOSED = 'Closed'
}