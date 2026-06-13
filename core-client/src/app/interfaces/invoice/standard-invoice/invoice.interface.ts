export interface InvoiceUpsert {
  id?: number;
  invoiceNumber: string;
  poNumber: string;
  projectDetail: string;
  issueDate: Date;
  dueDate: Date;
  type: string;
  currency: string;
  currencyRate?: number;
  customerId: number;
  notes: string;
  customerNotes?: string;
  internalNotes?: string;
  termsAndConditions?: string;
  footerNote?: string;
  paymentMethod: string;
  paymentTerms?: string;
  shippingAmount?: number;
  adjustmentAmount?: number;
  adjustmentDescription?: string;
  isAutomated: boolean;
  items: InvoiceItem[];
  taxDetails: TaxDetail[];
  discounts: Discount[];
  attachments: Attachment[];
  invoiceStatus?: string;
  paymentStatus?: string;
}

export interface Invoice {
  id: number;
  invoiceNumber: string;
  poNumber?: string;
  issueDate: Date;
  dueDate: Date;
  sentDate?: Date;
  paidDate?: Date;
  invoiceStatus: InvoiceStatusType;
  paymentStatus: PaymentStatusType;
  type: InvoiceTypeType;
  customerId: number;
  companyId: number;
  currency: string;
  currencyRate: number;
  subtotal: number;
  discountTotal: number;
  taxTotal: number;
  shippingAmount: number;
  adjustmentAmount: number;
  adjustmentDescription?: string;
  totalAmount: number;
  amountPaid: number;
  amountDue: number;
  amountRefunded: number;
  paymentMethod?: string;
  paymentGateway?: string;
  paymentTerms?: string;
  paymentTransactionId?: string;
  notes: string;
  customerNotes?: string;
  internalNotes?: string;
  termsAndConditions?: string;
  footerNote?: string;
  projectDetail?: string;
  isAutomated: boolean;
  isRecurring: boolean;
  recurringInvoiceId?: number;
  sourceSystem?: string;
  customer: Customer;
  customerName?: string;
  items: InvoiceItem[];
  taxDetails: TaxDetail[];
  discounts: Discount[];
  invoiceAttachments: Attachment[];
  payments?: InvoicePayment[];
  auditLogs?: InvoiceAuditLog[];
  createdDate: Date;
  createdBy: string;
  updatedDate?: Date;
  updatedBy?: string;
}

export type InvoiceStatusType =
  | 'Draft'
  | 'Sent'
  | 'Viewed'
  | 'PartiallyPaid'
  | 'Paid'
  | 'Overdue'
  | 'Void'
  | 'WriteOff'
  | 'CreditNote'
  | 'Refunded';

export type PaymentStatusType =
  | 'Pending'
  | 'PartiallyPaid'
  | 'Paid'
  | 'Overdue'
  | 'Refunded'
  | 'PartiallyRefunded'
  | 'Failed'
  | 'Cancelled';

export type InvoiceTypeType =
  | 'Standard'
  | 'Recurring'
  | 'Proforma'
  | 'CreditNote'
  | 'DebitNote'
  | 'Commercial'
  | 'Timesheet'
  | 'Expense';

export interface InvoiceItem {
  id?: number;
  lineNumber?: number;
  description: string;
  itemCode?: string;
  productId?: number;
  quantity: number;
  unitPrice: number;
  amount: number;
  discountAmount?: number;
  discountPercentage?: number;
  taxCode?: string;
  taxType?: string;
  taxPercentage?: number;
  taxAmount: number;
  totalAmount?: number;
  unitOfMeasure?: string;
  notes?: string;
  isTaxable?: boolean;
}

export interface TaxDetail {
  id?: number;
  taxName: string;
  taxCode?: string;
  rate: number;
  taxableAmount?: number;
  taxAmount: number;
  isCompound?: boolean;
  parentTaxId?: number;
  jurisdiction?: string;
  region?: string;
  taxCalculationMethod?: string;
  // Add amount for backward compatibility
  amount?: number;
}

export interface Discount {
  id?: number;
  description: string;
  discountType: number;
  amount: number;
  percentage?: number;
  couponCode?: string;
  itemId?: number;
  applyBeforeTax?: boolean;
  validFrom?: Date;
  validTo?: Date;
  // Add isPercentage for backward compatibility
  isPercentage?: boolean;
  // Add calculatedAmount for display
  calculatedAmount?: number;
}

export type DiscountTypeType = number;

export interface InvoicePayment {
  id: number;
  paymentDate: Date;
  amount: number;
  paymentMethod: string;
  paymentReference?: string;
  notes?: string;
  paymentStatus?: string;
  bankAccountId?: number;
  isRefund: boolean;
}

export interface InvoiceAuditLog {
  id: number;
  action: string;
  description: string;
  changes?: string;
  ipAddress?: string;
  userAgent?: string;
  createdDate: Date;
  createdBy: string;
}

export interface Attachment {
  id?: number;
  fileName: string;
  filePath?: string;
  fileUrl?: string;
  contentType?: string;
  fileSize?: number;
  description?: string;
  isPublic?: boolean;
  file?: File;
}

export interface Customer {
  id: number;
  name: string;
  email: string;
  phoneNumber: string;
  address: Address;
  companyId?: number;
}

export interface Address {
  address1: string;
  address2: string;
  city: string;
  state: string;
  country: string;
  zipCode: string;
}

export interface Company {
  id: number;
  name: string;
  email: string;
  address: Address;
  taxId?: string;
  phoneNumber?: string;
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
  all: StatsItem;
  draft: StatsItem;
  sent: StatsItem;
  viewed: StatsItem; // Add viewed (matches backend)
  partiallyPaid: StatsItem;
  paid: StatsItem; // Change from 'completed' to 'paid' to match backend
  overdue: StatsItem;
  void: StatsItem; // Add void (matches backend)
  cancelled: StatsItem;
  writeOff?: StatsItem; // Optional (backend has WriteOff)
  creditNote?: StatsItem; // Optional (backend has CreditNote)
  refunded: StatsItem;
  pending: StatsItem;
  // Remove 'approved' and 'completed' if they don't exist in backend
  // Keep for backward compatibility but map from paid
  approved?: StatsItem;
  completed?: StatsItem;
}

export interface StatsItem {
  count: number;
  amount: number;
  change: number;
}

export interface InvoiceSettings {
  companyId?: number;
  isAutomated: boolean;
  invoicePrefix: string;
  invoiceStartingNumber: number;
  invoiceNumberFormat?: string;
  lastUsedNumber?: number;
  lastUsedYear?: number;
  includeYear: boolean;
  separator: string;
  numberPadding: number;
}

// Extended interface with discount settings
export interface DiscountSettings {
  enableItemLevelDiscounts: boolean;
  enableOverallDiscounts: boolean;
  defaultDiscountType: 'Percentage' | 'Fixed';
  maxDiscountPercentage: number;
  maxDiscountAmount: number;
  allowMultipleDiscounts: boolean;
  applyDiscountBeforeTax: boolean;
  showDiscountColumnOnInvoice: boolean;
}

export interface ExtendedInvoiceSettings extends InvoiceSettings {
  discountSettings: DiscountSettings;
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

export interface ApiResponse<T> {
  isSuccess: boolean;
  data: T;
  errorMessage: string | null;
  errors: string[];
}
