// shared/interfaces/more-filters.interface.ts
export interface FilterFieldConfig {
  key: string;
  label: string;
  type: 'select' | 'number' | 'text' | 'date' | 'checkbox' | 'select-multi';
  options?: { value: any; label: string }[];
  placeholder?: string;
  min?: number;
  max?: number;
  defaultValue?: any;
  gridColumn?: 'full' | 'half';
  required?: boolean;
  condition?: (formValues: any) => boolean;
}

export interface MoreFiltersConfig {
  title: string;
  filterType: 'invoice' | 'recurring';
  customers: { id: number; name: string }[];
  taxTypes?: { name: string }[];
  frequencies?: { value: string; label: string }[];
  statuses?: { value: string; label: string }[];
  invoiceStatuses?: { value: string; label: string }[];
  paymentStatuses?: { value: string; label: string }[];
  formData: any;
}

export interface InvoiceFilterData {
  customerId?: number | null;
  invoiceStatus?: string | null;
  paymentStatus?: string | null;
  taxType?: string | null;
  minAmount?: number | null;
  maxAmount?: number | null;
  invoiceNumberFrom?: string;
  invoiceNumberTo?: string;
  issueDateFrom?: Date | string | null;
  issueDateTo?: Date | string | null;
  dueDateFrom?: Date | string | null;
  dueDateTo?: Date | string | null;
}

export interface RecurringFilterData {
  customerId?: number | null;
  status?: string | null;
  frequency?: string | null;
  minAmount?: number | null;
  maxAmount?: number | null;
  nextDateFrom?: Date | string | null;
  nextDateTo?: Date | string | null;
  startDateFrom?: Date | string | null;
  startDateTo?: Date | string | null;
  endDateFrom?: Date | string | null;
  endDateTo?: Date | string | null;
  autoSend?: boolean;
}

export interface FilterDialogResult {
  cleared?: boolean;
  [key: string]: any;
}