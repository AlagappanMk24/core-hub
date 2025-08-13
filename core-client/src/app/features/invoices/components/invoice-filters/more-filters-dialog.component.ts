import { CommonModule } from '@angular/common';
import { Component, Inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import {
  MatDialogModule,
  MatDialogRef,
  MAT_DIALOG_DATA,
} from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import {
  ReactiveFormsModule,
  FormBuilder,
  FormGroup,
  Validators,
} from '@angular/forms';
import { animate, style, transition, trigger } from '@angular/animations';
import { MoreFiltersDialogData } from '../../../../interfaces/invoice/invoice.interface';

@Component({
  selector: 'app-more-filters-dialog',
  template: `
    <div
      class="dialog-container bg-white rounded-xl shadow-2xl overflow-hidden"
      @dialogFadeIn
    >
      <div class="dialog-header flex justify-between items-center p-6 bg-white">
        <h2 class="text-xl font-bold text-purple-600">More Filters</h2>
        <button
          class="text-gray-600 hover:text-purple-600 transition-transform duration-300 transform hover:scale-110"
          (click)="onCancel()"
          aria-label="Close dialog"
        >
          <mat-icon>close</mat-icon>
        </button>
      </div>
      <div
        class="dialog-content p-6 max-h-[60vh] overflow-y-auto relative bg-white"
      >
        <form
          [formGroup]="moreFiltersForm"
          class="filter-form flex flex-col gap-5 p-4 bg-gray-50 rounded-lg shadow-sm"
        >
          <!-- Customer Field -->
          <div class="form-row flex items-start gap-4">
            <label
              class="form-label w-32 text-sm font-semibold text-gray-800 flex items-center mt-2"
              for="customer-id"
              >Customer</label
            >
            <div class="flex-1 flex flex-col">
              <select
                id="customer-id"
                formControlName="customerId"
                class="p-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-purple-500 focus:border-transparent bg-white text-gray-800 placeholder-gray-400"
              >
                <option [value]="null">All Customers</option>
                <option
                  *ngFor="let customer of data.customers"
                  [value]="customer.id"
                >
                  {{ customer.name }}
                </option>
              </select>
            </div>
          </div>

          <!-- Invoice Status Field -->
          <div class="form-row flex items-start gap-4">
            <label
              class="form-label w-32 text-sm font-semibold text-gray-800 flex items-center mt-2"
              for="invoice-status"
              >Invoice Status</label
            >
            <div class="flex-1 flex flex-col">
              <select
                id="invoice-status"
                formControlName="invoiceStatus"
                class="p-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-purple-500 focus:border-transparent bg-white text-gray-800 placeholder-gray-400"
              >
                <option [value]="null">All Invoice Statuses</option>
                <option *ngFor="let status of invoiceStatuses" [value]="status">
                  {{ status }}
                </option>
              </select>
            </div>
          </div>

          <!-- Payment Status Field -->
          <div class="form-row flex items-start gap-4">
            <label
              class="form-label w-32 text-sm font-semibold text-gray-800 flex items-center mt-2"
              for="payment-status"
              >Payment Status</label
            >
            <div class="flex-1 flex flex-col">
              <select
                id="payment-status"
                formControlName="paymentStatus"
                class="p-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-purple-500 focus:border-transparent bg-white text-gray-800 placeholder-gray-400"
              >
                <option [value]="null">All Payment Statuses</option>
                <option *ngFor="let status of paymentStatuses" [value]="status">
                  {{ status }}
                </option>
              </select>
            </div>
          </div>

          <!-- Amount Range Field -->
          <div class="form-row flex items-start gap-4">
            <label
              class="form-label w-32 text-sm font-semibold text-gray-800 flex items-center mt-2"
              >Amount Range</label
            >
            <div class="flex-1 flex gap-4">
              <div class="flex-1 flex flex-col">
                <input
                  type="number"
                  id="min-amount"
                  formControlName="minAmount"
                  placeholder="Min Amount"
                  class="p-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-purple-500 focus:border-transparent bg-white text-gray-800 placeholder-gray-400"
                />
                <div
                  *ngIf="
                    moreFiltersForm.get('minAmount')?.invalid &&
                    (moreFiltersForm.get('minAmount')?.dirty ||
                      moreFiltersForm.get('minAmount')?.touched)
                  "
                  class="text-red-500 text-sm mt-1"
                >
                  <span
                    *ngIf="moreFiltersForm.get('minAmount')?.hasError('min')"
                    >Minimum amount must be at least 0.</span
                  >
                  <span
                    *ngIf="
                      moreFiltersForm.get('minAmount')?.hasError('maxAmount')
                    "
                    >Minimum amount must be less than maximum amount.</span
                  >
                </div>
              </div>
              <div class="flex-1 flex flex-col">
                <input
                  type="number"
                  id="max-amount"
                  formControlName="maxAmount"
                  placeholder="Max Amount"
                  class="p-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-purple-500 focus:border-transparent bg-white text-gray-800 placeholder-gray-400"
                />
                <div
                  *ngIf="
                    moreFiltersForm.get('maxAmount')?.invalid &&
                    (moreFiltersForm.get('maxAmount')?.dirty ||
                      moreFiltersForm.get('maxAmount')?.touched)
                  "
                  class="text-red-500 text-sm mt-1"
                >
                  <span
                    *ngIf="moreFiltersForm.get('maxAmount')?.hasError('min')"
                    >Maximum amount must be at least 0.</span
                  >
                </div>
              </div>
            </div>
          </div>

          <!-- Invoice Number Range Field -->
          <div class="form-row flex items-start gap-4">
            <label
              class="form-label w-32 text-sm font-semibold text-gray-800 flex items-center mt-2"
              >Invoice Number</label
            >
            <div class="flex-1 flex gap-4">
              <div class="flex-1 flex flex-col">
                <input
                  type="text"
                  id="invoice-number-from"
                  formControlName="invoiceNumberFrom"
                  placeholder="From"
                  class="p-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-purple-500 focus:border-transparent bg-white text-gray-800 placeholder-gray-400"
                />
                <div
                  *ngIf="
                    moreFiltersForm.get('invoiceNumberFrom')?.invalid &&
                    (moreFiltersForm.get('invoiceNumberFrom')?.dirty ||
                      moreFiltersForm.get('invoiceNumberFrom')?.touched)
                  "
                  class="text-red-500 text-sm mt-1"
                >
                  <span
                    *ngIf="
                      moreFiltersForm
                        .get('invoiceNumberFrom')
                        ?.hasError('maxlength')
                    "
                    >Invoice number cannot exceed 50 characters.</span
                  >
                </div>
              </div>
              <div class="flex-1 flex flex-col">
                <input
                  type="text"
                  id="invoice-number-to"
                  formControlName="invoiceNumberTo"
                  placeholder="To"
                  class="p-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-purple-500 focus:border-transparent bg-white text-gray-800 placeholder-gray-400"
                />
                <div
                  *ngIf="
                    moreFiltersForm.get('invoiceNumberTo')?.invalid &&
                    (moreFiltersForm.get('invoiceNumberTo')?.dirty ||
                      moreFiltersForm.get('invoiceNumberTo')?.touched)
                  "
                  class="text-red-500 text-sm mt-1"
                >
                  <span
                    *ngIf="
                      moreFiltersForm
                        .get('invoiceNumberTo')
                        ?.hasError('maxlength')
                    "
                    >Invoice number cannot exceed 50 characters.</span
                  >
                </div>
              </div>
            </div>
          </div>

          <!-- Issue Date Range Field -->
          <div class="form-row flex items-start gap-4">
            <label
              class="form-label w-32 text-sm font-semibold text-gray-800 flex items-center mt-2"
              >Issue Date</label
            >
            <div class="flex-1 flex gap-4">
              <div class="flex-1 flex flex-col">
                <input
                  type="date"
                  id="issue-date-from"
                  formControlName="issueDateFrom"
                  placeholder="From"
                  class="p-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-purple-500 focus:border-transparent bg-white text-gray-800 placeholder-gray-400"
                />
                <div
                  *ngIf="
                    moreFiltersForm.get('issueDateFrom')?.invalid &&
                    (moreFiltersForm.get('issueDateFrom')?.dirty ||
                      moreFiltersForm.get('issueDateFrom')?.touched)
                  "
                  class="text-red-500 text-sm mt-1"
                >
                  <span
                    *ngIf="
                      moreFiltersForm
                        .get('issueDateFrom')
                        ?.hasError('dateRange')
                    "
                    >Issue date 'From' must be before 'To'.</span
                  >
                </div>
              </div>
              <div class="flex-1 flex flex-col">
                <input
                  type="date"
                  id="issue-date-to"
                  formControlName="issueDateTo"
                  placeholder="To"
                  class="p-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-purple-500 focus:border-transparent bg-white text-gray-800 placeholder-gray-400"
                />
              </div>
            </div>
          </div>

          <!-- Due Date Range Field -->
          <div class="form-row flex items-start gap-4">
            <label
              class="form-label w-32 text-sm font-semibold text-gray-800 flex items-center mt-2"
              >Due Date</label
            >
            <div class="flex-1 flex gap-4">
              <div class="flex-1 flex flex-col">
                <input
                  type="date"
                  id="due-date-from"
                  formControlName="dueDateFrom"
                  placeholder="From"
                  class="p-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-purple-500 focus:border-transparent bg-white text-gray-800 placeholder-gray-400"
                />
                <div
                  *ngIf="
                    moreFiltersForm.get('dueDateFrom')?.invalid &&
                    (moreFiltersForm.get('dueDateFrom')?.dirty ||
                      moreFiltersForm.get('dueDateFrom')?.touched)
                  "
                  class="text-red-500 text-sm mt-1"
                >
                  <span
                    *ngIf="
                      moreFiltersForm.get('dueDateFrom')?.hasError('dateRange')
                    "
                    >Due date 'From' must be before 'To'.</span
                  >
                </div>
              </div>
              <div class="flex-1 flex flex-col">
                <input
                  type="date"
                  id="due-date-to"
                  formControlName="dueDateTo"
                  placeholder="To"
                  class="p-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-purple-500 focus:border-transparent bg-white text-gray-800 placeholder-gray-400"
                />
              </div>
            </div>
          </div>
        </form>
      </div>
      <div
        class="dialog-actions p-6 bg-white border-t border-gray-200 flex justify-end gap-3"
      >
        <button
          class="clear-btn px-4 py-2 border-2 border-gray-400 text-gray-600 rounded-lg text-sm font-medium hover:bg-gray-50 hover:border-gray-500 hover:scale-105 transition-all duration-300"
          (click)="onClearFilters()"
        >
          Clear Filters
        </button>
        <button
          class="cancel-btn px-4 py-2 border-2 border-purple-400 text-purple-600 rounded-lg text-sm font-medium hover:bg-purple-50 hover:border-purple-500 hover:scale-105 transition-all duration-300"
          (click)="onCancel()"
        >
          Cancel
        </button>
        <button
          class="save-btn px-4 py-2 bg-purple-600 text-white rounded-lg text-sm font-medium flex items-center gap-2 hover:bg-purple-700 hover:scale-105 transition-all duration-300"
          (click)="onApply()"
          [disabled]="moreFiltersForm.invalid"
          [matTooltip]="
            moreFiltersForm.invalid
              ? 'Please correct all errors before applying'
              : 'Apply filters'
          "
          matTooltipPosition="above"
        >
          <mat-icon>filter_list</mat-icon>
          Apply
        </button>
      </div>
    </div>
  `,
  styles: [
    `
      :host {
        display: block;
        font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto,
          sans-serif;
      }

      .dialog-container {
        max-width: 600px;
        width: 100%;
      }

      .dialog-header {
        background-color: #ffffff;
      }

      .dialog-content {
        background-color: #ffffff;
      }

      .form-row {
        display: flex;
        align-items: flex-start;
        gap: 1rem;
      }

      .form-label {
        width: 8rem;
        font-size: 0.875rem;
        font-weight: 600;
        color: #1f2937;
      }

      .form-row input,
      .form-row select {
        padding: 0.5rem;
        border: 1px solid #d1d5db;
        border-radius: 0.5rem;
        background-color: #ffffff;
        color: #1f2937;
      }

      .form-row input:focus,
      .form-row select:focus {
        outline: none;
        border-color: transparent;
        box-shadow: 0 0 0 2px #8b5cf6;
      }

      .form-row input::placeholder,
      .form-row select::placeholder {
        color: #9ca3af;
      }

      .dialog-actions {
        display: flex;
        justify-content: flex-end;
        gap: 0.75rem;
      }

      .cancel-btn:hover {
        background-color: #faf5ff;
        border-color: #a78bfa;
      }

      .save-btn:hover {
        background-color: #6d28d9;
      }

      .text-red-500 {
        color: #ef4444;
      }

      :host ::ng-deep .mat-tooltip {
        background-color: #374151;
        color: #ffffff;
        font-size: 0.875rem;
        padding: 0.5rem;
        border-radius: 0.25rem;
      }
    `,
  ],
  standalone: true,
  imports: [
    CommonModule,
    MatDialogModule,
    MatButtonModule,
    MatIconModule,
    MatTooltipModule,
    ReactiveFormsModule,
  ],
  animations: [
    trigger('dialogFadeIn', [
      transition(':enter', [
        style({ opacity: 0, transform: 'translateY(20px)' }),
        animate(
          '300ms ease-out',
          style({ opacity: 1, transform: 'translateY(0)' })
        ),
      ]),
    ]),
    trigger('fadeIn', [
      transition(':enter', [
        style({ opacity: 0 }),
        animate('300ms ease', style({ opacity: 1 })),
      ]),
    ]),
  ],
})
export class MoreFiltersDialogComponent {
  moreFiltersForm: FormGroup;
  invoiceStatuses = ['Draft', 'Sent', 'Approved', 'Cancelled'];
  paymentStatuses = [
    'Pending',
    'Processing',
    'Completed',
    'PartiallyPaid',
    'Overdue',
    'Failed',
    'Refunded',
  ];
  constructor(
    public dialogRef: MatDialogRef<MoreFiltersDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: MoreFiltersDialogData,
    private fb: FormBuilder
  ) {
    this.dialogRef.updateSize('600px');
    this.moreFiltersForm = this.fb.group({
      customerId: [data.formData.customerId || null],
      invoiceStatus: [data.formData.invoiceStatus || null],
      paymentStatus: [data.formData.paymentStatus || null],
      minAmount: [
        data.formData.minAmount || null,
        [Validators.min(0), this.maxAmountValidator.bind(this)],
      ],
      maxAmount: [data.formData.maxAmount || null, [Validators.min(0)]],
      invoiceNumberFrom: [
        data.formData.invoiceNumberFrom || '',
        [Validators.maxLength(50)],
      ],
      invoiceNumberTo: [
        data.formData.invoiceNumberTo || '',
        [Validators.maxLength(50)],
      ],
      issueDateFrom: [
        this.data.formData.issueDateFrom || null,
        this.dateRangeValidator.bind(this, 'issueDateFrom', 'issueDateTo'),
      ],
      issueDateTo: [this.data.formData.issueDateTo || null],
      dueDateFrom: [
        this.data.formData.dueDateFrom || null,
        this.dateRangeValidator.bind(this, 'dueDateFrom', 'dueDateTo'),
      ],
      dueDateTo: [this.data.formData.dueDateTo || null],
    });
  }

  // Custom validator to ensure minAmount is less than maxAmount
  maxAmountValidator(control: any): { [key: string]: any } | null {
    const minAmount = control.value;
    const maxAmount = this.moreFiltersForm?.get('maxAmount')?.value;
    if (
      minAmount !== null &&
      maxAmount !== null &&
      minAmount > maxAmount &&
      maxAmount > 0
    ) {
      return { maxAmount: true };
    }
    return null;
  }

  // Custom validator for date ranges
  dateRangeValidator(startField: string, endField: string): any {
    return (control: any): { [key: string]: any } | null => {
      const startDate = control.value;
      const endDate = this.moreFiltersForm?.get(endField)?.value;
      if (startDate && endDate && new Date(startDate) > new Date(endDate)) {
        return { dateRange: true };
      }
      return null;
    };
  }

  onClearFilters(): void {
    this.moreFiltersForm.reset({
      customerId: null,
      invoiceStatus: null,
      paymentStatus: null,
      taxType: null,
      minAmount: null,
      maxAmount: null,
      invoiceNumberFrom: '',
      invoiceNumberTo: '',
      issueDateFrom: null,
      issueDateTo: null,
      dueDateFrom: null,
      dueDateTo: null,
    });
    this.dialogRef.close(this.moreFiltersForm.value);
  }

  onApply(): void {
    if (this.moreFiltersForm.valid) {
      this.dialogRef.close(this.moreFiltersForm.value);
    }
  }

  onCancel(): void {
    this.dialogRef.close();
  }
}
