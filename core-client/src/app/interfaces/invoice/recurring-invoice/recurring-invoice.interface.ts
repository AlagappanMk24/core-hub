// recurring-invoice.interface.ts
export interface RecurringInvoiceFilter {
  pageNumber: number;
  pageSize: number;
  search?: string;
  status?: string;
  frequency?: string;
  customerId?: number;
  nextDateFrom?: string;
  nextDateTo?: string;
  startDateFrom?: string;
  startDateTo?: string;
  endDateFrom?: string;
  endDateTo?: string;
  minAmount?: number;
  maxAmount?: number;
  autoSend?: boolean;
  sortBy?: string;
  sortOrder?: string;
}

export interface RecurringInvoiceStats {
  draft: { count: number; amount: number; change: number };
  active: { count: number; amount: number; change: number };
  paused: { count: number; amount: number; change: number };
  completed: { count: number; amount: number; change: number };
  cancelled: { count: number; amount: number; change: number };
  expired: { count: number; amount: number; change: number };
  totalActiveAmount: number;
  totalMonthlyRecurring: number;
  totalYearlyRecurring: number;
}

export interface RecurringInvoiceInstance {
  id: number;
  recurringInvoiceId: number;
  invoiceId: number;
  generatedDate: Date;
  scheduledGenerationDate: Date;
  sequenceNumber: number;
  generatedInvoiceNumber: string;
  amount: number;
  notes?: string;
  generationStatus: 'Success' | 'Failed' | 'RetryPending' | 'Skipped';
  errorMessage?: string;
  retryCount: number;
  invoice?: {
    id: number;
    invoiceNumber: string;
    invoiceStatus: string;
    paymentStatus: string;
  };
}

export interface RecurringInvoice {
  id: number;
  name: string;
  description?: string;
  
  // Frequency & Interval
  frequency: 'Daily' | 'Weekly' | 'Monthly' | 'Quarterly' | 'HalfYearly' | 'Annually';
  frequencyInterval: number;
  
  // Schedule specifics
  dayOfMonth?: number;
  dayOfWeek?: number;
  weekOfMonth?: number;
  monthOfYear?: number;
  
  // Lifecycle dates
  startDate: Date;
  endDate?: Date;
  pausedDate?: Date;
  cancelledDate?: Date;
  
  // Occurrence tracking
  maxOccurrences?: number;
  occurrencesGenerated: number;
  nextInvoiceDate: Date;
  lastInvoiceDate?: Date;
  
  // Status
  status: 'Draft' | 'Active' | 'Paused' | 'Completed' | 'Cancelled' | 'Expired';
  
  // Generation settings
  generateInAdvanceDays: number;
  
  // Automation flags
  autoSend: boolean;
  autoEmail: boolean;
  autoCharge: boolean;
  reminderBeforeDue: boolean;
  reminderDaysBefore: number;
  
  // Template overrides
  overridePONumber?: string;
  overrideCustomerNotes?: string;
  overrideTermsAndConditions?: string;
  overrideFooterNote?: string;
  overrideProjectDetail?: string;
  overridePaymentMethod?: string;
  overridePaymentTerms?: number;
  overrideShippingAmount?: number;
  overrideAdjustmentAmount?: number;
  overrideAdjustmentDescription?: string;
  
  // Source template
  sourceInvoiceId?: number;
  sourceInvoice?: any;
  
  // Invoice header fields (inherited)
  customerId: number;
  customerName?: string;
  companyId: number;
  currency: string;
  poNumber?: string;
  subtotal: number;
  discountTotal: number;
  taxTotal: number;
  shippingAmount: number;
  adjustmentAmount: number;
  adjustmentDescription?: string;
  totalAmount: number;
  
  // Collections
  generatedInvoices?: RecurringInvoiceInstance[];
  
  // Audit
  createdDate: Date;
  createdBy?: string;
  updatedDate?: Date;
  updatedBy?: string;
}

export interface PaginatedRecurringInvoices {
  items: RecurringInvoice[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}

export interface StatusCounts {
  draft: number;
  active: number;
  paused: number;
  completed: number;
  cancelled: number;
  expired: number;
}

export interface RecurringInvoiceInstanceFilter {
  recurringInvoiceId: number;
  pageNumber: number;
  pageSize: number;
}

export interface MoreFiltersDialogData {
  customers: { id: number; name: string }[];
  frequencies: { value: string; label: string }[];
  statuses: { value: string; label: string }[];
  formData: any;
}

export type RecurringFrequency = 
  | 'Daily' 
  | 'Weekly' 
  | 'Monthly' 
  | 'Quarterly' 
  | 'HalfYearly' 
  | 'Annually';

export type RecurringStatus = 
  | 'Draft' 
  | 'Active' 
  | 'Paused' 
  | 'Completed' 
  | 'Cancelled' 
  | 'Expired';