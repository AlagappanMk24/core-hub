import {
  DiscountTypeDisplay,
  PaymentMethod,
} from '../types/invoice-form.types';

export const SUPPORTED_CURRENCIES = ['USD', 'EUR', 'INR', 'GBP'] as const;
export type SupportedCurrency = (typeof SUPPORTED_CURRENCIES)[number];

export const PAYMENT_METHODS: PaymentMethod[] = [
  { value: 'Bank Transfer', display: 'Bank Transfer', icon: 'account_balance' },
  { value: 'Credit Card', display: 'Credit Card', icon: 'credit_card' },
  { value: 'PayPal', display: 'PayPal', icon: 'payment' },
  { value: 'Cash', display: 'Cash', icon: 'money' },
  { value: 'UPI', display: 'UPI', icon: 'upi' },
];

export interface PaymentTermOption {
  value: string;
  label: string;
  days: number | null;
  isCustom: boolean;
  specialCalculation?: 'EOM' | '15thFollowing' | 'EOM+10';
  description?: string;
}

export interface DiscountTypeOption {
  value: number; // Backend enum value
  label: string; // Display label
  displayValue: DiscountTypeDisplay; // UI display value
}

export const PAYMENT_TERM_OPTIONS: PaymentTermOption[] = [
  {
    value: 'Due on Receipt',
    label: 'Due on Receipt',
    days: 0,
    isCustom: false,
    description: 'Payment due immediately upon receipt',
  },
  {
    value: 'Net 7',
    label: 'Net 7 Days',
    days: 7,
    isCustom: false,
    description: 'Payment due within 7 days',
  },
  {
    value: 'Net 10',
    label: 'Net 10 Days',
    days: 10,
    isCustom: false,
    description: 'Payment due within 10 days',
  },
  {
    value: 'Net 15',
    label: 'Net 15 Days',
    days: 15,
    isCustom: false,
    description: 'Payment due within 15 days',
  },
  {
    value: 'Net 30',
    label: 'Net 30 Days',
    days: 30,
    isCustom: false,
    description: 'Payment due within 30 days',
  },
  {
    value: 'Net 45',
    label: 'Net 45 Days',
    days: 45,
    isCustom: false,
    description: 'Payment due within 45 days',
  },
  {
    value: 'Net 60',
    label: 'Net 60 Days',
    days: 60,
    isCustom: false,
    description: 'Payment due within 60 days',
  },
  {
    value: 'Net 90',
    label: 'Net 90 Days',
    days: 90,
    isCustom: false,
    description: 'Payment due within 90 days',
  },
  {
    value: 'Net 120',
    label: 'Net 120 Days',
    days: 120,
    isCustom: false,
    description: 'Payment due within 120 days',
  },
  {
    value: 'EOM',
    label: 'End of Month',
    days: null,
    isCustom: false,
    specialCalculation: 'EOM',
    description: 'Payment due at the end of the current month',
  },
  {
    value: '15th Following Month',
    label: '15th of Following Month',
    days: null,
    isCustom: false,
    specialCalculation: '15thFollowing',
    description: 'Payment due on the 15th day of the next month',
  },
  {
    value: 'EOM+10',
    label: 'End of Month + 10 Days',
    days: null,
    isCustom: false,
    specialCalculation: 'EOM+10',
    description: 'Payment due 10 days after the end of the month',
  },
  {
    value: 'Custom',
    label: 'Custom',
    days: null,
    isCustom: true,
    description: 'Define your own payment terms',
  },
];

export const DISCOUNT_TYPES: DiscountTypeOption[] = [
  { value: 0, label: 'Percentage (%)', displayValue: 'Percentage' },
  { value: 1, label: 'Fixed Amount', displayValue: 'Fixed' },
  { value: 2, label: 'Early Payment', displayValue: 'EarlyPayment' },
  { value: 3, label: 'Volume', displayValue: 'Volume' },
  { value: 4, label: 'Promotional', displayValue: 'Promotional' },
];

// Conversion functions
export function discountTypeToDisplayValue(
  discountTypeValue: number,
): DiscountTypeDisplay {
  const found = DISCOUNT_TYPES.find((type) => type.value === discountTypeValue);
  return found ? found.displayValue : 'Fixed';
}

export function discountTypeToNumber(
  displayValue: DiscountTypeDisplay,
): number {
  const found = DISCOUNT_TYPES.find(
    (type) => type.displayValue === displayValue,
  );
  return found ? found.value : 1; // Default to Fixed (1)
}

export function discountTypeToLabel(discountTypeValue: number): string {
  const found = DISCOUNT_TYPES.find((type) => type.value === discountTypeValue);
  return found ? found.label : 'Fixed Amount';
}

export const MONTHS = [
  'January',
  'February',
  'March',
  'April',
  'May',
  'June',
  'July',
  'August',
  'September',
  'October',
  'November',
  'December',
];

export const WEEKDAYS = ['SUN', 'MON', 'TUE', 'WED', 'THU', 'FRI', 'SAT'];

export const AVATAR_COLORS = [
  '#6D28D9',
  '#A78BFA',
  '#059669',
  '#DC2626',
  '#4682B4',
  '#FF2E63',
  '#00D4B9',
  '#FF6B6B',
  '#FFD93D',
  '#1E90FF',
  '#8A2BE2',
  '#4B0082',
  '#FF8C00',
  '#2ECC71',
  '#E74C3C',
  '#3498DB',
  '#9B59B6',
];

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

// Helper function to calculate due date based on payment terms
export function calculateDueDateFromPaymentTerm(
  issueDate: Date,
  paymentTermValue: string,
  customDays?: number,
): Date {
  const dueDate = new Date(issueDate);

  // Check for custom Net terms
  if (
    paymentTermValue &&
    paymentTermValue.startsWith('Net ') &&
    !isNaN(parseInt(paymentTermValue.split(' ')[1]))
  ) {
    const days = parseInt(paymentTermValue.split(' ')[1]);
    dueDate.setDate(issueDate.getDate() + days);
    return dueDate;
  }

  const matchedTerm = PAYMENT_TERM_OPTIONS.find(
    (term) => term.value === paymentTermValue,
  );

  if (matchedTerm) {
    if (matchedTerm.days !== null) {
      dueDate.setDate(issueDate.getDate() + matchedTerm.days);
      return dueDate;
    }

    switch (matchedTerm.specialCalculation) {
      case 'EOM':
        return new Date(issueDate.getFullYear(), issueDate.getMonth() + 1, 0);
      case '15thFollowing':
        return new Date(issueDate.getFullYear(), issueDate.getMonth() + 1, 15);
      case 'EOM+10':
        const eom = new Date(
          issueDate.getFullYear(),
          issueDate.getMonth() + 1,
          0,
        );
        eom.setDate(eom.getDate() + 10);
        return eom;
      default:
        return dueDate;
    }
  }

  // Default to 30 days
  dueDate.setDate(issueDate.getDate() + 30);
  return dueDate;
}
