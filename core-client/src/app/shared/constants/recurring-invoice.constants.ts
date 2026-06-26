// components/create-recurring-invoice/recurring-invoice.constants.ts
import { RecurringFrequency } from "../../interfaces/invoice/recurring-invoice/recurring-invoice.interface";

export interface CustomerDisplay {
  id: number;
  name: string;
  address: string;
  initials: string;
  color: string;
}

export const FREQUENCY_OPTIONS = [
  { value: RecurringFrequency.Daily, label: 'Daily' },
  { value: RecurringFrequency.Weekly, label: 'Weekly' },
  { value: RecurringFrequency.BiWeekly, label: 'Bi-Weekly' },
  { value: RecurringFrequency.Monthly, label: 'Monthly' },
  { value: RecurringFrequency.BiMonthly, label: 'Bi-Monthly' },
  { value: RecurringFrequency.Quarterly, label: 'Quarterly' },
  { value: RecurringFrequency.SemiAnnually, label: 'Semi-Annually' },
  { value: RecurringFrequency.Annually, label: 'Annually' }
];

export const DAYS_OF_WEEK = [
  'Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'
];

export const MONTHS = [
  { value: 1, label: 'January' },
  { value: 2, label: 'February' },
  { value: 3, label: 'March' },
  { value: 4, label: 'April' },
  { value: 5, label: 'May' },
  { value: 6, label: 'June' },
  { value: 7, label: 'July' },
  { value: 8, label: 'August' },
  { value: 9, label: 'September' },
  { value: 10, label: 'October' },
  { value: 11, label: 'November' },
  { value: 12, label: 'December' }
];

export function getDayOfWeekNumber(dayOfWeek: string): number {
  return DAYS_OF_WEEK.indexOf(dayOfWeek);
}

export function getDayOfWeekString(dayNumber: number): string {
  return DAYS_OF_WEEK[dayNumber] || 'Monday';
}

export function getMonthNumber(monthName: string): number {
  const month = MONTHS.find(m => m.label === monthName);
  return month?.value || 1;
}