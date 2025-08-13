import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { MatDialog } from '@angular/material/dialog';
import { InvoiceService } from '../../../../services/invoice/invoice.service';
import { NotificationDialogComponent } from '../../../../components/notification/notification-dialog.component';
import { InvoiceSettings } from '../../../../interfaces/invoice/invoice.interface';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

@Component({
  selector: 'app-invoice-settings',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, MatProgressSpinnerModule],
  templateUrl: './invoice-settings.component.html',
  styleUrls: ['./invoice-settings.component.css'],
})
export class InvoiceSettingsComponent implements OnInit {
  invoiceSettings: InvoiceSettings = {
    isAutomated: false,
    invoicePrefix: 'INV',
    invoiceStartingNumber: 1,
  };

  isSaving = false; // Add loading state

  constructor(
    private invoiceService: InvoiceService,
    private dialog: MatDialog
  ) {}

  ngOnInit(): void {
    this.loadInvoiceSettings();
  }

  loadInvoiceSettings(): void {
    this.invoiceService.getInvoiceSettings().subscribe({
      next: (settings: InvoiceSettings) => {
        this.invoiceSettings = settings;
      },
      error: (err) => {
        console.error('Error fetching invoice settings:', err);
        this.openDialog(
          'error',
          'Error',
          'Failed to load invoice settings.',
          'Please check your internet connection and try again. If the problem persists, contact support.'
        );
      },
    });
  }

  saveSettings(): void {
    if (this.validateSettings()) {
      this.isSaving = true; // Start loading

      this.invoiceService.saveInvoiceSettings(this.invoiceSettings).subscribe({
        next: () => {
          this.isSaving = false; // Stop loading
          this.openDialog(
            'success',
            'Success',
            'Invoice settings saved successfully!',
            'Your changes have been applied and will take effect immediately for new invoices.'
          );
        },
        error: (err) => {
          this.isSaving = false; // Stop loading on error
          console.error('Error saving invoice settings:', err);
          this.openDialog(
            'error',
            'Error',
            'Failed to save invoice settings.',
            'Please verify your input data and try again. If the issue continues, contact support.'
          );
        },
      });
    }
  }

  cancel(): void {
    if (!this.isSaving) {
      this.loadInvoiceSettings();
    }
  }

  validateSettings(): boolean {
    if (!this.invoiceSettings.invoicePrefix.trim()) {
      this.openDialog(
        'error',
        'Error',
        'Invoice prefix is required.',
        'Please enter a valid prefix (e.g., INV, BILL, or any custom prefix) to generate invoice numbers.'
      );
      return false;
    }
    if (this.invoiceSettings.invoiceStartingNumber < 1) {
      this.openDialog(
        'error',
        'Error',
        'Starting number must be at least 1.',
        'Please enter a positive number greater than 0 for invoice numbering sequence.'
      );
      return false;
    }
    return true;
  }

  openDialog(
    type: 'success' | 'error',
    title: string,
    message: string,
    submessage: string
  ): void {
    this.dialog.open(NotificationDialogComponent, {
      width: '400px',
      data: { type, title, message, submessage },
    });
  }
}
