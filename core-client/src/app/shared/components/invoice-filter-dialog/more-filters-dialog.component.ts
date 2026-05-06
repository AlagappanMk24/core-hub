// shared/components/invoice-filter-dialog/more-filters-dialog.component.ts
import { CommonModule } from '@angular/common';
import { Component, Inject, OnInit } from '@angular/core';
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
  AbstractControl,
  ValidationErrors,
} from '@angular/forms';
import { animate, style, transition, trigger } from '@angular/animations';
import { MoreFiltersConfig } from '../../../interfaces/invoice/invoice-filter/more-filters.interface';

@Component({
  selector: 'app-more-filters-dialog',
  template: `
    <div
      class="dialog-container bg-white rounded-xl shadow-2xl overflow-hidden"
      @dialogFadeIn
    >
      <div class="dialog-header flex justify-between items-center p-6 bg-white border-b border-gray-100">
        <h2 class="text-xl font-bold text-purple-600">{{ data.title || 'More Filters' }}</h2>
        <button
          class="text-gray-600 hover:text-purple-600 transition-transform duration-300 transform hover:scale-110"
          (click)="onCancel()"
          aria-label="Close dialog"
        >
          <mat-icon>close</mat-icon>
        </button>
      </div>

      <div class="dialog-content p-6 max-h-[65vh] overflow-y-auto bg-gray-50">
        <form [formGroup]="filterForm" class="filter-form">
          <!-- Dynamic Fields based on filterType -->
          <div class="filters-grid" [class.recurring-grid]="data.filterType === 'recurring'">
            
            <!-- Customer Field (Common for both) -->
            <div class="filter-field">
              <label class="field-label">
                <mat-icon class="field-icon">business</mat-icon>
                Customer
              </label>
              <select formControlName="customerId" class="field-input">
                <option [value]="null">All Customers</option>
                <option *ngFor="let customer of data.customers" [value]="customer.id">
                  {{ customer.name }}
                </option>
              </select>
            </div>

            <!-- INVOICE TYPE FIELDS -->
            <ng-container *ngIf="data.filterType === 'invoice'">
              <!-- Invoice Status -->
              <div class="filter-field">
                <label class="field-label">
                  <mat-icon class="field-icon">receipt</mat-icon>
                  Invoice Status
                </label>
                <select formControlName="invoiceStatus" class="field-input">
                  <option [value]="null">All Invoice Statuses</option>
                  <option *ngFor="let status of invoiceStatusOptions" [value]="status.value">
                    {{ status.label }}
                  </option>
                </select>
              </div>

              <!-- Payment Status -->
              <div class="filter-field">
                <label class="field-label">
                  <mat-icon class="field-icon">payments</mat-icon>
                  Payment Status
                </label>
                <select formControlName="paymentStatus" class="field-input">
                  <option [value]="null">All Payment Statuses</option>
                  <option *ngFor="let status of paymentStatusOptions" [value]="status.value">
                    {{ status.label }}
                  </option>
                </select>
              </div>

              <!-- Tax Type (Admin only) -->
              <div class="filter-field" *ngIf="data.taxTypes && data.taxTypes.length">
                <label class="field-label">
                  <mat-icon class="field-icon">percent</mat-icon>
                  Tax Type
                </label>
                <select formControlName="taxType" class="field-input">
                  <option [value]="null">All Tax Types</option>
                  <option *ngFor="let tax of data.taxTypes" [value]="tax.name">
                    {{ tax.name }}
                  </option>
                </select>
              </div>

              <!-- Invoice Number Range -->
              <div class="filter-field range-field">
                <label class="field-label">
                  <mat-icon class="field-icon">numbers</mat-icon>
                  Invoice Number
                </label>
                <div class="range-inputs">
                  <input type="text" formControlName="invoiceNumberFrom" placeholder="From" class="field-input">
                  <span class="range-separator">—</span>
                  <input type="text" formControlName="invoiceNumberTo" placeholder="To" class="field-input">
                </div>
              </div>

              <!-- Issue Date Range -->
              <div class="filter-field range-field">
                <label class="field-label">
                  <mat-icon class="field-icon">calendar_today</mat-icon>
                  Issue Date
                </label>
                <div class="range-inputs">
                  <input type="date" formControlName="issueDateFrom" placeholder="From" class="field-input">
                  <span class="range-separator">—</span>
                  <input type="date" formControlName="issueDateTo" placeholder="To" class="field-input">
                </div>
              </div>

              <!-- Due Date Range -->
              <div class="filter-field range-field">
                <label class="field-label">
                  <mat-icon class="field-icon">event</mat-icon>
                  Due Date
                </label>
                <div class="range-inputs">
                  <input type="date" formControlName="dueDateFrom" placeholder="From" class="field-input">
                  <span class="range-separator">—</span>
                  <input type="date" formControlName="dueDateTo" placeholder="To" class="field-input">
                </div>
              </div>
            </ng-container>

            <!-- RECURRING TYPE FIELDS -->
            <ng-container *ngIf="data.filterType === 'recurring'">
              <!-- Status -->
              <div class="filter-field" *ngIf="data.statuses && data.statuses.length">
                <label class="field-label">
                  <mat-icon class="field-icon">flag</mat-icon>
                  Status
                </label>
                <select formControlName="status" class="field-input">
                  <option [value]="null">All Statuses</option>
                  <option *ngFor="let status of data.statuses" [value]="status.value">
                    {{ status.label }}
                  </option>
                </select>
              </div>

              <!-- Frequency -->
              <div class="filter-field" *ngIf="data.frequencies && data.frequencies.length">
                <label class="field-label">
                  <mat-icon class="field-icon">repeat</mat-icon>
                  Frequency
                </label>
                <select formControlName="frequency" class="field-input">
                  <option [value]="null">All Frequencies</option>
                  <option *ngFor="let freq of data.frequencies" [value]="freq.value">
                    {{ freq.label }}
                  </option>
                </select>
              </div>

              <!-- Next Invoice Date Range -->
              <div class="filter-field range-field">
                <label class="field-label">
                  <mat-icon class="field-icon">schedule</mat-icon>
                  Next Invoice Date
                </label>
                <div class="range-inputs">
                  <input type="date" formControlName="nextDateFrom" placeholder="From" class="field-input">
                  <span class="range-separator">—</span>
                  <input type="date" formControlName="nextDateTo" placeholder="To" class="field-input">
                </div>
              </div>

              <!-- Start Date Range -->
              <div class="filter-field range-field">
                <label class="field-label">
                  <mat-icon class="field-icon">play_circle</mat-icon>
                  Start Date
                </label>
                <div class="range-inputs">
                  <input type="date" formControlName="startDateFrom" placeholder="From" class="field-input">
                  <span class="range-separator">—</span>
                  <input type="date" formControlName="startDateTo" placeholder="To" class="field-input">
                </div>
              </div>

              <!-- End Date Range -->
              <div class="filter-field range-field">
                <label class="field-label">
                  <mat-icon class="field-icon">stop_circle</mat-icon>
                  End Date
                </label>
                <div class="range-inputs">
                  <input type="date" formControlName="endDateFrom" placeholder="From" class="field-input">
                  <span class="range-separator">—</span>
                  <input type="date" formControlName="endDateTo" placeholder="To" class="field-input">
                </div>
              </div>

              <!-- Auto Send Checkbox -->
              <div class="filter-field checkbox-field">
                <label class="checkbox-label">
                  <input type="checkbox" formControlName="autoSend">
                  <span class="checkbox-text">
                    <mat-icon class="field-icon">send</mat-icon>
                    Auto Send Only
                  </span>
                </label>
              </div>
            </ng-container>

            <!-- Amount Range (Common for both) -->
            <div class="filter-field range-field amount-range">
              <label class="field-label">
                <mat-icon class="field-icon">attach_money</mat-icon>
                Amount Range
              </label>
              <div class="range-inputs">
                <input type="number" formControlName="minAmount" placeholder="Min Amount" class="field-input">
                <span class="range-separator">—</span>
                <input type="number" formControlName="maxAmount" placeholder="Max Amount" class="field-input">
              </div>
              <div *ngIf="filterForm.hasError('amountRange') && (filterForm.get('minAmount')?.touched || filterForm.get('maxAmount')?.touched)" 
                   class="field-error">
                Minimum amount must be less than maximum amount
              </div>
            </div>
          </div>
        </form>
      </div>

      <div class="dialog-actions p-6 bg-white border-t border-gray-100 flex justify-end gap-3">
        <button
          class="clear-btn px-5 py-2.5 border-2 border-gray-300 text-gray-600 rounded-lg text-sm font-medium hover:bg-gray-50 hover:border-gray-400 hover:scale-105 transition-all duration-300"
          (click)="onClearFilters()"
        >
          <mat-icon class="btn-icon">clear</mat-icon>
          Clear All
        </button>
        <button
          class="cancel-btn px-5 py-2.5 border-2 border-purple-300 text-purple-600 rounded-lg text-sm font-medium hover:bg-purple-50 hover:border-purple-400 hover:scale-105 transition-all duration-300"
          (click)="onCancel()"
        >
          <mat-icon class="btn-icon">close</mat-icon>
          Cancel
        </button>
        <button
          class="apply-btn px-5 py-2.5 bg-purple-600 text-white rounded-lg text-sm font-medium flex items-center gap-2 hover:bg-purple-700 hover:scale-105 transition-all duration-300 shadow-md hover:shadow-lg"
          (click)="onApply()"
          [disabled]="filterForm.invalid"
          [matTooltip]="filterForm.invalid ? 'Please correct all errors before applying' : 'Apply filters'"
        >
          <mat-icon class="btn-icon">filter_list</mat-icon>
          Apply Filters
        </button>
      </div>
    </div>
  `,
  styles: [
    `
      :host {
        display: block;
        font-family: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
      }

      .dialog-container {
        max-width: 680px;
        width: 100%;
      }

      .dialog-header {
        background: linear-gradient(135deg, #ffffff 0%, #faf5ff 100%);
      }

      .dialog-content {
        background: #f8fafc;
      }

      .filters-grid {
        display: grid;
        grid-template-columns: repeat(2, 1fr);
        gap: 20px;
      }

      .filters-grid.recurring-grid {
        grid-template-columns: repeat(2, 1fr);
      }

      .filter-field {
        display: flex;
        flex-direction: column;
        gap: 8px;
      }

      .range-field {
        grid-column: span 2;
      }

      .amount-range {
        margin-top: 8px;
        padding-top: 8px;
        border-top: 1px dashed #e2e8f0;
      }

      .checkbox-field {
        flex-direction: row;
        align-items: center;
      }

      .field-label {
        display: flex;
        align-items: center;
        gap: 8px;
        font-size: 13px;
        font-weight: 600;
        color: #1e293b;
        text-transform: uppercase;
        letter-spacing: 0.5px;
      }

      .field-icon {
        font-size: 18px;
        width: 18px;
        height: 18px;
        color: #8A2BE2;
      }

      .field-input {
        padding: 10px 12px;
        border: 1px solid #e2e8f0;
        border-radius: 10px;
        font-size: 14px;
        transition: all 0.2s ease;
        background: white;
        color: #1e293b;
      }

      .field-input:focus {
        outline: none;
        border-color: #8A2BE2;
        box-shadow: 0 0 0 3px rgba(138, 43, 226, 0.1);
      }

      .field-input:hover:not(:focus) {
        border-color: #cbd5e1;
      }

      .range-inputs {
        display: flex;
        align-items: center;
        gap: 12px;
      }

      .range-inputs .field-input {
        flex: 1;
      }

      .range-separator {
        color: #94a3b8;
        font-weight: 500;
      }

      .checkbox-label {
        display: flex;
        align-items: center;
        gap: 10px;
        cursor: pointer;
        padding: 8px 0;
      }

      .checkbox-label input[type="checkbox"] {
        width: 18px;
        height: 18px;
        cursor: pointer;
        accent-color: #8A2BE2;
      }

      .checkbox-text {
        display: flex;
        align-items: center;
        gap: 8px;
        font-size: 14px;
        font-weight: 500;
        color: #1e293b;
      }

      .field-error {
        font-size: 12px;
        color: #ef4444;
        margin-top: 4px;
      }

      .dialog-actions {
        gap: 12px;
      }

      .clear-btn, .cancel-btn, .apply-btn {
        display: flex;
        align-items: center;
        gap: 8px;
      }

      .btn-icon {
        font-size: 18px;
        width: 18px;
        height: 18px;
      }

      .apply-btn:disabled {
        opacity: 0.6;
        cursor: not-allowed;
        transform: none;
      }

      @media (max-width: 640px) {
        .filters-grid,
        .filters-grid.recurring-grid {
          grid-template-columns: 1fr;
        }
        
        .range-field {
          grid-column: span 1;
        }
        
        .range-inputs {
          flex-direction: column;
        }
        
        .range-separator {
          display: none;
        }
        
        .dialog-actions {
          flex-wrap: wrap;
        }
        
        .clear-btn, .cancel-btn, .apply-btn {
          flex: 1;
          justify-content: center;
        }
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
        animate('300ms ease-out', style({ opacity: 1, transform: 'translateY(0)' })),
      ]),
    ]),
  ],
})
export class MoreFiltersDialogComponent implements OnInit {
  filterForm: FormGroup;
  
  invoiceStatusOptions = [
    { value: 'Draft', label: 'Draft' },
    { value: 'Sent', label: 'Sent' },
    { value: 'Viewed', label: 'Viewed' },
    { value: 'Approved', label: 'Approved' },
    { value: 'Cancelled', label: 'Cancelled' },
    { value: 'Void', label: 'Void' },
  ];
  
  paymentStatusOptions = [
    { value: 'Pending', label: 'Pending' },
    { value: 'Processing', label: 'Processing' },
    { value: 'PartiallyPaid', label: 'Partially Paid' },
    { value: 'Paid', label: 'Paid' },
    { value: 'Overdue', label: 'Overdue' },
    { value: 'Failed', label: 'Failed' },
    { value: 'Refunded', label: 'Refunded' },
  ];

  constructor(
    public dialogRef: MatDialogRef<MoreFiltersDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: MoreFiltersConfig,
    private fb: FormBuilder
  ) {
    this.filterForm = this.createForm();
  }

  ngOnInit(): void {
    // Set custom validator for amount range
    this.filterForm.setValidators(this.amountRangeValidator.bind(this));
  }

  private createForm(): FormGroup {
    const baseControls: any = {
      customerId: [this.data.formData?.customerId || null],
      minAmount: [this.data.formData?.minAmount || null],
      maxAmount: [this.data.formData?.maxAmount || null],
    };

    if (this.data.filterType === 'invoice') {
      baseControls.invoiceStatus = [this.data.formData?.invoiceStatus || null];
      baseControls.paymentStatus = [this.data.formData?.paymentStatus || null];
      baseControls.taxType = [this.data.formData?.taxType || null];
      baseControls.invoiceNumberFrom = [this.data.formData?.invoiceNumberFrom || ''];
      baseControls.invoiceNumberTo = [this.data.formData?.invoiceNumberTo || ''];
      baseControls.issueDateFrom = [this.data.formData?.issueDateFrom || null];
      baseControls.issueDateTo = [this.data.formData?.issueDateTo || null];
      baseControls.dueDateFrom = [this.data.formData?.dueDateFrom || null];
      baseControls.dueDateTo = [this.data.formData?.dueDateTo || null];
    } else {
      baseControls.status = [this.data.formData?.status || null];
      baseControls.frequency = [this.data.formData?.frequency || null];
      baseControls.nextDateFrom = [this.data.formData?.nextDateFrom || null];
      baseControls.nextDateTo = [this.data.formData?.nextDateTo || null];
      baseControls.startDateFrom = [this.data.formData?.startDateFrom || null];
      baseControls.startDateTo = [this.data.formData?.startDateTo || null];
      baseControls.endDateFrom = [this.data.formData?.endDateFrom || null];
      baseControls.endDateTo = [this.data.formData?.endDateTo || null];
      baseControls.autoSend = [this.data.formData?.autoSend || false];
    }

    return this.fb.group(baseControls);
  }

  amountRangeValidator(group: AbstractControl): ValidationErrors | null {
    const minAmount = group.get('minAmount')?.value;
    const maxAmount = group.get('maxAmount')?.value;
    
    if (minAmount !== null && maxAmount !== null && minAmount > maxAmount) {
      return { amountRange: true };
    }
    return null;
  }

  onClearFilters(): void {
    const clearedValues: any = { cleared: true };
    
    if (this.data.filterType === 'invoice') {
      clearedValues.customerId = null;
      clearedValues.invoiceStatus = null;
      clearedValues.paymentStatus = null;
      clearedValues.taxType = null;
      clearedValues.minAmount = null;
      clearedValues.maxAmount = null;
      clearedValues.invoiceNumberFrom = '';
      clearedValues.invoiceNumberTo = '';
      clearedValues.issueDateFrom = null;
      clearedValues.issueDateTo = null;
      clearedValues.dueDateFrom = null;
      clearedValues.dueDateTo = null;
    } else {
      clearedValues.customerId = null;
      clearedValues.status = null;
      clearedValues.frequency = null;
      clearedValues.minAmount = null;
      clearedValues.maxAmount = null;
      clearedValues.nextDateFrom = null;
      clearedValues.nextDateTo = null;
      clearedValues.startDateFrom = null;
      clearedValues.startDateTo = null;
      clearedValues.endDateFrom = null;
      clearedValues.endDateTo = null;
      clearedValues.autoSend = false;
    }
    
    this.dialogRef.close(clearedValues);
  }

  onApply(): void {
    if (this.filterForm.valid) {
      const formValue = { ...this.filterForm.value };
      
      // Clean up null/empty values
      Object.keys(formValue).forEach(key => {
        if (formValue[key] === null || formValue[key] === '' || formValue[key] === undefined) {
          delete formValue[key];
        }
      });
      
      this.dialogRef.close(formValue);
    }
  }

  onCancel(): void {
    this.dialogRef.close(null);
  }
}