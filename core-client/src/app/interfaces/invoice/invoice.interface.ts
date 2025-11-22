import { Company } from '../company/company.interface';

export interface InvoiceUpsert {
  id?: number;
  invoiceNumber: string;
  poNumber: string;
  projectDetail: string;
  issueDate: Date;
  dueDate: Date;
  type: string;
  currency: string;
  customerId: number;
  notes: string;
  paymentMethod: string;
  items: {
    description: string;
    quantity: number;
    unitPrice: number;
    taxType: string;
    taxAmount: number;
    amount: number;
  }[];
  taxDetails: { taxType: string; rate: number; amount: number }[];
  discounts: { description: string; amount: number; isPercentage: boolean }[];
  invoiceStatus?: string;
  paymentStatus?: string;
  attachments: Attachment[]; 
}

export interface Invoice {
  id: number;
  customerName: string;
  invoiceNumber: string;
  invoiceStatus: 'Draft' | 'Sent' | 'Approved' | 'Cancelled';
  paymentStatus:
    | 'Pending'
    | 'Processing'
    | 'Completed'
    | 'PartiallyPaid'
    | 'Overdue'
    | 'Failed'
    | 'Refunded';
  totalAmount: number;
  issueDate: Date;
  dueDate: Date;
  poNumber: string;
  projectDetail: string;
  items: {
    description: string;
    quantity: number;
    unitPrice: number;
    taxType: string;
    taxAmount: number;
    amount: number;
  }[];
  taxDetails: { taxType: string; rate: number; amount: number }[];
  discounts: { description: string; amount: number; isPercentage: boolean }[];
  paymentMethod: string;
  notes: string;
  customerId: number;
  currency: string;
  isAutomated: boolean;
  customer: Customer;
  company?: Company; // Added for company info
  subtotal: number;
  invoiceAttachments: Attachment[];
}

export interface Address {
  address1: string;
  address2: string;
  city: string;
  state: string;
  country: string;
  zipCode: string;
}

// Define the Customer interface, based on CustomerResponseDto
export interface Customer {
  id: number;
  name: string;
  email: string;
  phoneNumber: string;
  address: Address; // Use the Address interface here
  companyId: number;
}
// export interface Company {
//   id : number;
//   name: string;
//   email: string;
//   address: Address;
// }
export interface InvoiceApiResponse {
  id: number;
  invoiceNumber: string;
  invoiceStatus: 'Draft' | 'Sent' | 'Approved' | 'Cancelled';
  paymentStatus:
    | 'Pending'
    | 'Processing'
    | 'Completed'
    | 'PartiallyPaid'
    | 'Overdue'
    | 'Failed'
    | 'Refunded';
  totalAmount: number;
  issueDate: string | Date;
  dueDate: string | Date;
  poNumber: string;
  projectDetail: string;
  items: {
    description: string;
    quantity: number;
    unitPrice: number;
    taxType: string;
    taxAmount: number;
    amount: number;
  }[];
  taxDetails: { taxType: string; rate: number; amount: number }[];
  discounts: { description: string; amount: number; isPercentage: boolean }[];
  paymentMethod: string;
  notes: string;
  customerId: number;
  currency: string;
  isAutomated: boolean;
  customer: Customer;
  subtotal: number;
  invoiceAttachments: Attachment[];
}

export interface TaxType {
  id: number;
  name: string;
  rate: number;
}

export interface TaxTypeCreate {
  name: string;
  rate: number;
}

export interface PaginatedResult<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}

export interface InvoiceStats {
  all: { count: number; amount: number; change: number };
  draft: { count: number; amount: number; change: number };
  sent: { count: number; amount: number; change: number };
  approved: { count: number; amount: number; change: number };
  cancelled: { count: number; amount: number; change: number };
  pending: { count: number; amount: number; change: number };
  completed: { count: number; amount: number; change: number };
  partiallyPaid: { count: number; amount: number; change: number };
  overdue: { count: number; amount: number; change: number };
  refunded: { count: number; amount: number; change: number };
}

export interface InvoiceSettings {
  isAutomated: boolean;
  invoicePrefix: string;
  invoiceStartingNumber: number;
}

export interface MoreFiltersDialogData {
  customers: { id: number; name: string }[];
  formData: {
    customerId: number | null;
    minAmount: number | null;
    maxAmount: number | null;
    invoiceNumberFrom: string;
    invoiceNumberTo: string;
    invoiceStatus: string | null;
    paymentStatus: string | null;
    issueDateFrom: Date | null;
    issueDateTo: Date | null;
    dueDateFrom: Date | null;
    dueDateTo: Date | null;
  };
}

export interface InvoiceFilter {
  pageNumber: number;
  pageSize: number;
  search?: string;
  invoiceStatus?: string;
  paymentStatus?: string;
  customerId?: number;
  taxType?: number;
  minAmount?: number;
  maxAmount?: number;
  invoiceNumberFrom?: string;
  invoiceNumberTo?: string;
  issueDateFrom?: string;
  issueDateTo?: string;
  dueDateFrom?: string;
  dueDateTo?: string;
}
export interface Attachment {
  id?: number; // Optional, as new attachments may not have an ID yet
  fileName: string;
  fileUrl: string;
  file?: File; // Optional, used for client-side file handling
}