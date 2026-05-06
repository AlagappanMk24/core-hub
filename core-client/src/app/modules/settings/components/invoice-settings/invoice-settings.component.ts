import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { MatDialog } from '@angular/material/dialog';
import { InvoiceService } from '../../../invoice/standard-invoice/services/invoice.service';
import { NotificationDialogComponent } from '../../../../shared/components/notification/notification-dialog.component';
import {
  Company,
  ExtendedInvoiceSettings,
  InvoiceSettings,
} from '../../../../interfaces/invoice/standard-invoice/invoice.interface';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { AuthService } from '../../../../core/services/auth/auth.service';
import { MatIcon } from '@angular/material/icon';

@Component({
  selector: 'app-invoice-settings',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    RouterModule,
    MatProgressSpinnerModule,
    MatIcon,
  ],
  templateUrl: './invoice-settings.component.html',
  styleUrls: ['./invoice-settings.component.css'],
})
export class InvoiceSettingsComponent implements OnInit {
  invoiceSettings: ExtendedInvoiceSettings = {
    isAutomated: true,
    invoicePrefix: 'INV',
    invoiceStartingNumber: 1,
    includeYear: true,
    separator: '-',
    numberPadding: 4,
    invoiceNumberFormat: '{prefix}{number:D4}',
    lastUsedNumber: 0,
    lastUsedYear: 0,
    companyId: undefined,
    discountSettings: {
      enableItemLevelDiscounts: true,
      enableOverallDiscounts: false, // Industry standard: disabled by default
      defaultDiscountType: 'Percentage',
      maxDiscountPercentage: 25, // Requires approval above 25%
      maxDiscountAmount: 500, // Safety ceiling
      allowMultipleDiscounts: false, // Stacking disabled by default
      applyDiscountBeforeTax: true,
      showDiscountColumnOnInvoice: true,
    },
  };
  previewInvoiceNumber: string = '';
  isSaving = false;
  isLoading = false;
  isSuperAdmin = false;

  // Company selection for Super Admin
  companies: Company[] = [];
  selectedCompanyId: number | null = null;
  showCompanySelector = false;

  // Sample preview values
  sampleNumber = 1000;
  currentYear = new Date().getFullYear();

  // Active tab
  activeTab: 'numbering' | 'discounts' = 'numbering';

  constructor(
    private invoiceService: InvoiceService,
    private authService: AuthService,
    private dialog: MatDialog,
  ) {
    this.isSuperAdmin =
      this.authService.hasRole('Super Admin') ||
      this.authService.hasRole('SuperAdmin');
  }

  ngOnInit(): void {
    if (this.isSuperAdmin) {
      this.loadCompanies();
    } else {
      this.loadInvoiceSettings();
    }
    this.updatePreview();
  }

  loadCompanies(): void {
    this.isLoading = true;
    this.invoiceService.getAllCompanies().subscribe({
      next: (companies: Company[]) => {
        this.companies = companies;
        this.isLoading = false;
        // Auto-select first company if available
        if (companies.length > 0) {
          this.selectedCompanyId = companies[0].id;
          this.loadInvoiceSettings(this.selectedCompanyId);
        }
      },
      error: (err) => {
        console.error('Error fetching companies:', err);
        this.isLoading = false;
        this.openDialog(
          'error',
          'Error',
          'Failed to load companies.',
          'Please check your internet connection and try again.',
        );
      },
    });
  }

  loadInvoiceSettings(companyId?: number): void {
    this.isLoading = true;
    this.invoiceService.getInvoiceSettings(companyId).subscribe({
      next: (settings: InvoiceSettings) => {
        this.invoiceSettings = {
          ...settings,
          discountSettings: {
            enableItemLevelDiscounts: true,
            enableOverallDiscounts: false,
            defaultDiscountType: 'Percentage',
            maxDiscountPercentage: 25,
            maxDiscountAmount: 500,
            allowMultipleDiscounts: false,
            applyDiscountBeforeTax: true,
            showDiscountColumnOnInvoice: true,
            ...(settings as any).discountSettings,
          },
        };
        this.updatePreview();
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Error fetching invoice settings:', err);
        this.isLoading = false;
        this.openDialog(
          'error',
          'Error',
          'Failed to load invoice settings.',
          'Please check your internet connection and try again. If the problem persists, contact support.',
        );
      },
    });
  }

  onCompanyChange(): void {
    if (this.selectedCompanyId) {
      this.loadInvoiceSettings(this.selectedCompanyId);
    }
  }

  saveSettings(): void {
    if (this.validateSettings()) {
      this.isSaving = true;

      // Set the company ID for Super Admin
      if (this.isSuperAdmin && this.selectedCompanyId) {
        this.invoiceSettings.companyId = this.selectedCompanyId;
      }

      this.invoiceService.saveInvoiceSettings(this.invoiceSettings).subscribe({
        next: () => {
          this.isSaving = false;
          this.openDialog(
            'success',
            'Success',
            'Invoice settings saved successfully!',
            'Your changes have been applied and will take effect immediately for new invoices.',
          );
        },
        error: (err) => {
          this.isSaving = false;
          console.error('Error saving invoice settings:', err);
          this.openDialog(
            'error',
            'Error',
            'Failed to save invoice settings.',
            'Please verify your input data and try again. If the issue continues, contact support.',
          );
        },
      });
    }
  }

  cancel(): void {
    if (!this.isSaving) {
      if (this.isSuperAdmin && this.selectedCompanyId) {
        this.loadInvoiceSettings(this.selectedCompanyId);
      } else {
        this.loadInvoiceSettings();
      }
    }
  }

  validateSettings(): boolean {
    if (!this.invoiceSettings.invoicePrefix.trim()) {
      this.openDialog(
        'error',
        'Error',
        'Invoice prefix is required.',
        'Please enter a valid prefix (e.g., INV, BILL, or any custom prefix) to generate invoice numbers.',
      );
      return false;
    }

    if (this.invoiceSettings.invoicePrefix.length > 10) {
      this.openDialog(
        'error',
        'Error',
        'Invoice prefix too long.',
        'Invoice prefix cannot exceed 10 characters.',
      );
      return false;
    }

    if (this.invoiceSettings.invoiceStartingNumber < 1) {
      this.openDialog(
        'error',
        'Error',
        'Starting number must be at least 1.',
        'Please enter a positive number greater than 0 for invoice numbering sequence.',
      );
      return false;
    }

    if (
      this.invoiceSettings.numberPadding < 1 ||
      this.invoiceSettings.numberPadding > 10
    ) {
      this.openDialog(
        'error',
        'Error',
        'Invalid number padding.',
        'Number padding must be between 1 and 10.',
      );
      return false;
    }

    if (
      this.invoiceSettings.separator &&
      this.invoiceSettings.separator.length > 5
    ) {
      this.openDialog(
        'error',
        'Error',
        'Separator too long.',
        'Separator cannot exceed 5 characters.',
      );
      return false;
    }
    // Validate discount settings
    if (
      this.invoiceSettings.discountSettings.maxDiscountPercentage < 0 ||
      this.invoiceSettings.discountSettings.maxDiscountPercentage > 100
    ) {
      this.openDialog(
        'error',
        'Error',
        'Invalid maximum discount percentage.',
        'Maximum discount percentage must be between 0 and 100.',
      );
      return false;
    }

    if (this.invoiceSettings.discountSettings.maxDiscountAmount < 0) {
      this.openDialog(
        'error',
        'Error',
        'Invalid maximum discount amount.',
        'Maximum discount amount must be a positive number.',
      );
      return false;
    }
    return true;
  }

  updatePreview(): void {
    const prefix = this.invoiceSettings.invoicePrefix || 'INV';
    const separator = this.invoiceSettings.separator || '-';
    const padding = this.invoiceSettings.numberPadding || 4;
    const includeYear = this.invoiceSettings.includeYear;
    const currentYear = new Date().getFullYear();

    // Use the sample number or next available number for preview
    const previewNum = this.sampleNumber;
    const formattedNumber = previewNum.toString().padStart(padding, '0');

    if (includeYear) {
      this.previewInvoiceNumber = `${prefix}${separator}${currentYear}${separator}${formattedNumber}`;
    } else {
      this.previewInvoiceNumber = `${prefix}${separator}${formattedNumber}`;
    }
  }

  onSettingChange(): void {
    this.updatePreview();
  }

  getPreviewInvoiceNumber(): string {
    const prefix = this.invoiceSettings.invoicePrefix || 'INV';
   // Only use fallback if the value is undefined or null, NOT if it's an empty string
    const separator = this.invoiceSettings.separator !== undefined && this.invoiceSettings.separator !== null 
                    ? this.invoiceSettings.separator 
                    : '-';
    const padding = this.invoiceSettings.numberPadding || 4;
    const includeYear = this.invoiceSettings.includeYear;
    const currentYear = new Date().getFullYear();

    // Use the starting number or a sample for preview
    const previewNum = this.invoiceSettings.invoiceStartingNumber || 1;
    const formattedNumber = previewNum.toString().padStart(padding, '0');

    if (includeYear) {
      return `${prefix}${separator}${currentYear}${separator}${formattedNumber}`;
    }
    return `${prefix}${separator}${formattedNumber}`;
  }

  openDialog(
    type: 'success' | 'error',
    title: string,
    message: string,
    submessage: string,
  ): void {
    this.dialog.open(NotificationDialogComponent, {
      width: '400px',
      data: { type, title, message, submessage },
    });
  }
  setActiveTab(tab: 'numbering' | 'discounts'): void {
    this.activeTab = tab;
  }
  getCurrencySymbol(): string {
    // Default to USD, can be made configurable
    return '$';
  }
}
