import { Component, Inject, OnInit } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule, MatDialog } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { CommonModule } from '@angular/common';
import { Invoice, Customer, Address } from '../../../../interfaces/invoice/invoice.interface';
import { Company } from '../../../../interfaces/company/company.interface';
import { InvoiceService } from '../../../../services/invoice/invoice.service';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { animate, style, transition, trigger } from '@angular/animations';
import { SendInvoiceDialogComponent } from '../send-invoice/send-invoice-dialog.component';

@Component({
  selector: 'app-invoice-preview',
  templateUrl: './invoice-preview.component.html',
  styleUrls: ['./invoice-preview.component.css'],
  standalone: true,
  imports: [
    CommonModule,
    MatDialogModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatSnackBarModule
  ],
  animations: [
    trigger('fadeIn', [
      transition(':enter', [
        style({ opacity: 0 }),
        animate('300ms ease', style({ opacity: 1 }))
      ])
    ])
  ]
})
export class InvoicePreviewComponent implements OnInit {
  invoice: Invoice;
  company: Company;
  isLoading = false;

  constructor(
    public dialogRef: MatDialogRef<InvoicePreviewComponent>,
    @Inject(MAT_DIALOG_DATA) public data: { invoice: Invoice, company: Company },
    private invoiceService: InvoiceService,
    private snackBar: MatSnackBar,
    private dialog: MatDialog
  ) {
    this.invoice = data.invoice;
    this.company = data.company;
  }

  ngOnInit(): void {
    // Ensure invoice data is properly initialized
    this.invoice = {
      ...this.data.invoice,
      discounts: this.data.invoice.discounts.map(discount => ({
        ...discount,
        calculatedAmount: discount.isPercentage
          ? (this.data.invoice.subtotal * discount.amount) / 100
          : discount.amount
      }))
    };
  }

  onClose(): void {
    this.dialogRef.close();
  }

  onDownload(): void {
    this.isLoading = true;
    // Note: For preview, we may need to save the invoice temporarily to generate a PDF
    this.snackBar.open('Download PDF is not available in preview mode.', 'Close', {
      duration: 3000,
      panelClass: ['warning-snackbar']
    });
    this.isLoading = false;
  }

  onSendInvoice(): void {
    this.dialogRef.close({ action: 'sendInvoice' });
  }
}