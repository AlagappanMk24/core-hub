import { Attachment } from '../../interfaces/invoice/standard-invoice/invoice.interface';

export interface CustomerDisplay {
  id: number;
  name: string;
  address: string;
  initials: string;
  color: string;
}

export interface ValidationErrors {
  [key: string]: string;
}

export interface InvoiceItemForm {
  id?: number;
  productId?: number; // Added for product selection
  description: string;
  quantity: number;
  unitPrice: number;
  taxType: string;
  taxAmount: number;
  amount: number;
  discountPercentage?: number; // Added for item-level percentage discount
  discountAmount?: number; // Added for item-level fixed discount
}

export interface TaxDetailForm {
  id?: number;
  taxName: string;
  rate: number;
  taxAmount: number;
}

export interface DiscountForm {
  id?: number;
  description: string;
  amount: number;
  discountType: DiscountTypeDisplay; // String for UI display
}

export type DiscountTypeDisplay =
  | 'Percentage'
  | 'Fixed'
  | 'EarlyPayment'
  | 'Volume'
  | 'Promotional';

export interface InvoiceFormData {
  id?: number;
  invoiceNumber: string;
  poNumber: string;
  projectDetail: string;
  items: InvoiceItemForm[];
  taxDetails: TaxDetailForm[];
  discounts: DiscountForm[];
  paymentMethod: string;
  paymentTerms?: string;
  customerNotes: string;
  internalNotes?: string;
  termsAndConditions?: string;
  footerNote?: string;
  issuedDate: string;
  dueDate: string;
  customerId: number;
  currency: string;
  currencyRate?: number;
  isAutomated: boolean;
  invoiceStatus?: string;
  paymentStatus?: string;
  shippingAmount?: number;
  adjustmentAmount?: number;
  adjustmentDescription?: string;
  invoiceAttachments: Attachment[];
}

export interface PaymentMethod {
  value: string;
  display: string;
  icon: string;
}