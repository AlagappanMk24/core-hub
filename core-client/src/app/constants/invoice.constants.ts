import { DiscountTypeDisplay, PaymentMethod } from "../types/invoice-form.types";

// constants/invoice.constants.ts
export const SUPPORTED_CURRENCIES = ['USD', 'EUR', 'INR', 'GBP'] as const;
export type SupportedCurrency = typeof SUPPORTED_CURRENCIES[number];

export const PAYMENT_METHODS: PaymentMethod[] = [
  { value: 'Bank Transfer', display: 'Bank Transfer', icon: 'account_balance' },
  { value: 'Credit Card', display: 'Credit Card', icon: 'credit_card' },
  { value: 'PayPal', display: 'PayPal', icon: 'payment' },
  { value: 'Cash', display: 'Cash', icon: 'money' },
  { value: 'UPI', display: 'UPI', icon: 'upi' },
];

export interface DiscountTypeOption {
  value: number;        // Backend enum value
  label: string;        // Display label
  displayValue: DiscountTypeDisplay;  // UI display value
}

export const DISCOUNT_TYPES: DiscountTypeOption[] = [
  { value: 0, label: 'Percentage (%)', displayValue: 'Percentage' },
  { value: 1, label: 'Fixed Amount', displayValue: 'Fixed' },
  { value: 2, label: 'Early Payment', displayValue: 'EarlyPayment' },
  { value: 3, label: 'Volume', displayValue: 'Volume' },
  { value: 4, label: 'Promotional', displayValue: 'Promotional' }
];

// Conversion functions
export function discountTypeToDisplayValue(discountTypeValue: number): DiscountTypeDisplay {
  const found = DISCOUNT_TYPES.find(type => type.value === discountTypeValue);
  return found ? found.displayValue : 'Fixed';
}

export function discountTypeToNumber(displayValue: DiscountTypeDisplay): number {
  const found = DISCOUNT_TYPES.find(type => type.displayValue === displayValue);
  return found ? found.value : 1; // Default to Fixed (1)
}

export function discountTypeToLabel(discountTypeValue: number): string {
  const found = DISCOUNT_TYPES.find(type => type.value === discountTypeValue);
  return found ? found.label : 'Fixed Amount';
}

export const MONTHS = [
  'January', 'February', 'March', 'April', 'May', 'June',
  'July', 'August', 'September', 'October', 'November', 'December'
];

export const WEEKDAYS = ['SUN', 'MON', 'TUE', 'WED', 'THU', 'FRI', 'SAT'];

export const AVATAR_COLORS = ['#6D28D9', '#A78BFA', '#059669', '#DC2626', '#4682B4'];

export const DEFAULT_COMPANY = {
  id: 0,
  name: 'KL Infotech',
  email: 'support@klinfotech.com',
  address: {
    address1: 'Besant Nagar, Chennai 45',
    address2: '',
    city: 'Chennai',
    state: 'Tamil Nadu',
    zipCode: '600045',
    country: 'India',
  },
};

export const DEFAULT_INVOICE_FORM_DATA = {
  invoiceNumber: '',
  poNumber: '',
  projectDetail: '',
  items: [],
  taxDetails: [],
  discounts: [],
  paymentMethod: '',
  paymentTerms: 'Net 30',
  customerNotes: '',
  internalNotes: '',
  termsAndConditions: '',
  footerNote: '',
  issuedDate: '',
  dueDate: '',
  customerId: 0,
  currency: 'USD',
  // currencyRate: 1,
  isAutomated: false,
  shippingAmount: 0,
  adjustmentAmount: 0,
  invoiceAttachments: [],
};