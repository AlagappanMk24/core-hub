// export interface PaginatedResult<T> {
//   items: T[];
//   totalCount: number;
//   pageNumber: number;
//   pageSize: number;
//   totalPages: number;
// }

// export interface Customer {
//   id: number;
//   name: string;
//   email: string;
//   phoneNumber: string;
//   taxId?: string;
//   website?: string;
//   creditLimit: number;
//   defaultPaymentTerms?: string;
//   defaultCurrency?: string;
//   customerGroupId?: number;
//   customerGroupName?: string;
//   status: string;
//   activeSince?: Date;
//   lastPurchaseDate?: Date;
//   totalPurchases: number;
//   averagePaymentDays?: number;
//   companyId: number;
//   addressLine1: string;
//   addressLine2?: string | null;
//   city: string;
//   state?: string;
//   countryCode: string;
//   countryName: string;
//   zipCode: string;
//   createdDate: Date;
//   companyName?: string;
// }

// export interface CustomerCreateDto {
//   name: string;
//   email: string;
//   phoneNumber: string;
//   taxId?: string;
//   website?: string;
//   creditLimit?: number;
//   defaultPaymentTerms?: string;
//   defaultCurrency?: string;
//   customerGroupId?: number;
//   addressLine1: string;
//   addressLine2?: string | null;
//   city: string;
//   state?: string;
//   countryCode: string;
//   zipCode: string;
// }

// export interface CustomerUpdateDto extends CustomerCreateDto {
//   id: number;
// }

// export interface CustomerResponseDto {
//   id: number;
//   name: string;
//   email: string;
//   phoneNumber: string;
//   taxId?: string;
//   website?: string;
//   creditLimit: number;
//   defaultPaymentTerms?: string;
//   defaultCurrency?: string;
//   customerGroupId?: number;
//   customerGroupName?: string;
//   status: string;
//   activeSince?: Date;
//   lastPurchaseDate?: Date;
//   totalPurchases: number;
//   averagePaymentDays?: number;
//   companyId: number;
//   addressLine1: string;
//   addressLine2?: string | null;
//   city: string;
//   state?: string;
//   countryCode: string;
//   countryName: string;
//   zipCode: string;
//   createdDate: Date;
//   companyName?: string;
// }

// export interface CustomerStats {
//   allCount: number;
//   allChange: number;
//   activeCount: number;
//   activeChange: number;
//   inactiveCount: number;
//   inactiveChange: number;
// }
// export interface CustomerFilterRequest {
//   pageNumber: number;
//   pageSize: number;
//   search?: string;
//   status?: string | null;
// }
// export interface PaginatedResponse {
//   items: CustomerResponseDto[];
//   totalCount: number;
//   pageNumber: number;
//   pageSize: number;
//   totalPages: number;
// }

// export interface CustomerInvoice {
//   id: number;
//   invoiceNumber: string;
//   issueDate: Date;
//   dueDate: Date;
//   amount: number;
//   status: string;
//   paymentStatus: string;
// }

// export interface CustomerPayment {
//   id: number;
//   paymentNumber: string;
//   invoiceId: number;
//   invoiceNumber: string;
//   amount: number;
//   paymentDate: Date;
//   paymentMethod: string;
//   status: string;
//   isOnTime: boolean;
// }

// export interface CustomerActivity {
//   id: number;
//   icon: string;
//   action: string;
//   description: string;
//   date: Date;
//   color: string;
//   userId?: string;
//   userName?: string;
// }

// export interface SpendingTrend {
//   month: string;
//   year: number;
//   amount: number;
// }

export interface PaginatedResult<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}

export interface Customer {
  id: number;
  name: string;
  email: string;
  phoneNumber: string;
  taxId?: string;
  website?: string;
  creditLimit: number;
  defaultPaymentTerms?: string;
  defaultCurrency?: string;
  customerGroupId?: number;
  customerGroupName?: string;
  status: string;
  activeSince?: Date;
  lastPurchaseDate?: Date;
  totalPurchases: number;
  averagePaymentDays?: number;
  companyId: number;
  addressLine1: string;
  addressLine2?: string | null;
  city: string;
  state?: string;
  countryCode: string;
  countryName: string;
  zipCode: string;
  createdDate: Date;
  companyName?: string;
  notes?: CustomerNote[];
  tags?: string[];
}

export interface CustomerNote {
  id: number;
  customerId: number;
  content: string;
  createdBy: string;
  createdDate: Date;
}

export interface Communication {
  id: number;
  type: 'email' | 'sms' | 'call' | 'meeting';
  subject: string;
  content: string;
  direction: 'inbound' | 'outbound';
  date: Date;
  status: 'sent' | 'delivered' | 'read' | 'failed';
  attachments?: string[];
}

export interface CustomerCreateDto {
  name: string;
  email: string;
  phoneNumber: string;
  taxId?: string;
  website?: string;
  creditLimit?: number;
  defaultPaymentTerms?: string;
  defaultCurrency?: string;
  customerGroupId?: number;
  addressLine1: string;
  addressLine2?: string | null;
  city: string;
  state?: string;
  countryCode: string;
  zipCode: string;
}

export interface CustomerUpdateDto extends CustomerCreateDto {
  id: number;
}

export interface CustomerResponseDto {
  id: number;
  name: string;
  email: string;
  phoneNumber: string;
  taxId?: string;
  website?: string;
  creditLimit: number;
  defaultPaymentTerms?: string;
  defaultCurrency?: string;
  customerGroupId?: number;
  customerGroupName?: string;
  status: string;
  activeSince?: Date;
  lastPurchaseDate?: Date;
  totalPurchases: number;
  averagePaymentDays?: number;
  companyId: number;
  addressLine1: string;
  addressLine2?: string | null;
  city: string;
  state?: string;
  countryCode: string;
  countryName: string;
  zipCode: string;
  createdDate: Date;
  companyName?: string;
}

export interface CustomerStats {
  allCount: number;
  allChange: number;
  activeCount: number;
  activeChange: number;
  inactiveCount: number;
  inactiveChange: number;
}

export interface CustomerFilterRequest {
  pageNumber: number;
  pageSize: number;
  search?: string;
  status?: string | null;
  minTotalPurchases?: number | null;
  maxTotalPurchases?: number | null;
  country?: string;
  customerGroup?: string;
  minCreditLimit?: number | null;
  maxCreditLimit?: number | null;
  dateFrom?: Date | null;
  dateTo?: Date | null;
}

export interface PaginatedResponse {
  items: CustomerResponseDto[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}

export interface CustomerInvoice {
  id: number;
  invoiceNumber: string;
  issueDate: Date;
  dueDate: Date;
  amount: number;
  status: string;
  paymentStatus: string;
}

export interface CustomerPayment {
  id: number;
  paymentNumber: string;
  invoiceId: number;
  invoiceNumber: string;
  amount: number;
  paymentDate: Date;
  paymentMethod: string;
  status: string;
  isOnTime: boolean;
}

export interface CustomerActivity {
  id: number;
  icon: string;
  action: string;
  description: string;
  date: Date;
  color: string;
  userId?: string;
  userName?: string;
}

export interface SpendingTrend {
  month: string;
  year: number;
  amount: number;
}

export interface ColumnVisibility {
  name: boolean;
  company: boolean;
  email: boolean;
  phone: boolean;
  address: boolean;
  status: boolean;
  totalPurchases: boolean;
  creditLimit: boolean;
  lastPurchase: boolean;
  actions: boolean;
}
