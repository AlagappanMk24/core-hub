import { Component, Inject, OnInit, Optional } from '@angular/core';
import {
  MatDialogRef,
  MAT_DIALOG_DATA,
  MatDialogModule,
  MatDialog,
} from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { animate, style, transition, trigger } from '@angular/animations';
import { catchError, forkJoin } from 'rxjs';
import { of } from 'rxjs';
import { Invoice } from '../../../../../interfaces/invoice/invoice.interface';
import { Company } from '../../../../../interfaces/company/company.interface';
import { InvoiceService } from '../../../../../services/invoice/invoice.service';
import { CompanyService } from '../../../../../services/company/company.service';
import { SendInvoiceDialogComponent } from '../../send-invoice/send-invoice-dialog.component';
import { AuthService } from '../../../../../services/auth/auth.service';

@Component({
  selector: 'app-invoice-display',
  templateUrl: './invoice-display.component.html',
  styleUrls: ['./invoice-display.component.css'],
  standalone: true,
  imports: [
    CommonModule,
    MatDialogModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
  ],
  animations: [
    trigger('fadeIn', [
      transition(':enter', [
        style({ opacity: 0, transform: 'translateY(20px)' }),
        animate(
          '500ms ease',
          style({ opacity: 1, transform: 'translateY(0)' })
        ),
      ]),
    ]),
    trigger('scaleIn', [
      transition(':enter', [
        style({ opacity: 0, transform: 'scale(0.95)' }),
        animate('300ms ease', style({ opacity: 1, transform: 'scale(1)' })),
      ]),
    ]),
  ],
})
export class InvoiceDisplayComponent implements OnInit {
  invoice: Invoice | null = null;
  company: Company | null = null;
  isLoading = false;
  mode: 'preview' | 'view' = 'preview'; // Default to preview

  constructor(
    @Optional()
    @Inject(MAT_DIALOG_DATA)
    public data: {
      invoice: Invoice;
      company: Company;
      mode?: 'preview' | 'view';
    },
    @Optional() public dialogRef: MatDialogRef<InvoiceDisplayComponent>,
    private invoiceService: InvoiceService,
    private companyService: CompanyService,
    private authService: AuthService,
    private snackBar: MatSnackBar,
    private dialog: MatDialog,
    private router: Router,
    private route: ActivatedRoute
  ) {
    // Set mode based on data or default to 'view' if no dialog data is provided
    this.mode = data?.mode || 'view';
    if (this.mode === 'preview' && data) {
      this.invoice = data.invoice || null;
      this.company = data.company || null;
    }
  }

  ngOnInit(): void {
    if (this.mode === 'view') {
      const id = this.route.snapshot.paramMap.get('id');
      if (id) {
        const invoiceId = Number(id);
        if (isNaN(invoiceId)) {
          this.snackBar.open('Invalid invoice ID format.', 'Close', {
            duration: 3000,
            panelClass: ['error-snackbar'],
          });
          this.router.navigate(['/invoices']);
          return;
        }
        this.loadInvoiceAndCompany(invoiceId);
      } else {
        this.snackBar.open('Invalid invoice ID.', 'Close', {
          duration: 3000,
          panelClass: ['error-snackbar'],
        });
        this.router.navigate(['/invoices']);
      }
    } else if (this.invoice) {
      // Preview mode: Initialize discounts
      this.invoice = {
        ...this.invoice,
        discounts: this.invoice.discounts.map((discount) => ({
          ...discount,
          calculatedAmount: discount.isPercentage
            ? (this.invoice!.subtotal * discount.amount) / 100
            : discount.amount,
        })),
      };
    }
  }

  loadInvoiceAndCompany(id: number): void {
    this.isLoading = true;
    // Get the company ID from AuthService
    const companyId = this.authService.getUserDetail()?.companyId;
    if (!companyId) {
      this.snackBar.open('No company ID found for user.', 'Close', {
        duration: 3000,
        panelClass: ['error-snackbar'],
      });
      this.isLoading = false;
      this.router.navigate(['/invoices']);
      return;
    }
    forkJoin([
      this.invoiceService.getInvoiceById(id),
      this.companyService.getCompanyById(companyId).pipe(
        catchError(() => {
          this.snackBar.open(
            'Failed to load company data. Using defaults.',
            'Close',
            {
              duration: 3000,
              panelClass: ['warning-snackbar'],
            }
          );
          return of({
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
          } as Company);
        })
      ),
    ]).subscribe({
      next: ([invoice, company]) => {
        this.invoice = {
          ...invoice,
          discounts: invoice.discounts.map((discount) => ({
            ...discount,
            calculatedAmount: discount.isPercentage
              ? (invoice.subtotal * discount.amount) / 100
              : discount.amount,
          })),
        };
        this.company = company;
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error fetching invoice or company:', error);
        this.snackBar.open(
          'Failed to load invoice. Please try again.',
          'Close',
          {
            duration: 3000,
            panelClass: ['error-snackbar'],
          }
        );
        this.isLoading = false;
        this.router.navigate(['/invoices']);
      },
    });
  }

  onClose(): void {
    if (this.dialogRef) {
      this.dialogRef.close();
    } else {
      this.router.navigate(['/invoices']);
    }
  }

  onDownload(): void {
    if (!this.invoice) return;
    this.isLoading = true;
    this.invoiceService.downloadInvoicePdf(this.invoice.id).subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = `invoice_${this.invoice!.invoiceNumber}.pdf`;
        link.click();
        window.URL.revokeObjectURL(url);
        this.snackBar.open(
          `Invoice ${this.invoice!.invoiceNumber} downloaded successfully!`,
          'Close',
          {
            duration: 3000,
            panelClass: ['success-snackbar'],
          }
        );
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error downloading PDF:', error);
        this.snackBar.open(
          'Failed to download PDF. Please try again.',
          'Close',
          {
            duration: 3000,
            panelClass: ['error-snackbar'],
          }
        );
        this.isLoading = false;
      },
    });
  }

  onSendInvoice(): void {
    if (!this.invoice) return;
    this.dialog
      .open(SendInvoiceDialogComponent, {
        width: '600px',
        data: {
          invoiceId: this.invoice.id,
          invoiceNumber: this.invoice.invoiceNumber,
          customerEmail:
            this.invoice.customer?.email || 'alagappantest@gmail.com',
        },
      })
      .afterClosed()
      .subscribe((result) => {
        if (result) {
          this.snackBar.open(
            `Invoice ${this.invoice!.invoiceNumber} sent successfully!`,
            'Close',
            {
              duration: 3000,
              panelClass: ['success-snackbar'],
            }
          );
          if (this.mode === 'view') {
            this.loadInvoiceAndCompany(this.invoice!.id);
          } else {
            this.dialogRef?.close({ action: 'sendInvoice' });
          }
        }
      });
  }

  onEditInvoice(): void {
    if (!this.invoice) return;
    this.router.navigate([`/invoices/edit/${this.invoice.id}`]);
  }

  onBack(): void {
    this.router.navigate(['/invoices']);
  }
}
