import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { SendInvoiceDialogComponent } from '../send-invoice/send-invoice-dialog.component';
import { animate, style, transition, trigger } from '@angular/animations';
import { Invoice } from '../../../../interfaces/invoice/invoice.interface';
import { InvoiceService } from '../../../../services/invoice/invoice.service';
import { CompanyService } from '../../../../services/company/company.service';
import { catchError, forkJoin, switchMap } from 'rxjs';
import { of } from 'rxjs';
import { Company, Address } from '../../../../interfaces/company/company.interface';

@Component({
  selector: 'app-view-invoice',
  templateUrl: './view-invoice.component.html',
  styleUrls: ['./view-invoice.component.css'],
  animations: [
    trigger('fadeIn', [
      transition(':enter', [
        style({ opacity: 0 }),
        animate('300ms ease', style({ opacity: 1 }))
      ])
    ])
  ],
  standalone: true,
  imports: [
    CommonModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
    MatDialogModule
  ]
})
export class ViewInvoiceComponent implements OnInit {
  invoice: Invoice | null = null;
  company: Company | null = null;
  isLoading = true;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private invoiceService: InvoiceService,
    private companyService: CompanyService,
    private snackBar: MatSnackBar,
    private dialog: MatDialog
  ) {}

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.loadInvoiceAndCompany(id);
    } else {
      this.snackBar.open('Invalid invoice ID.', 'Close', {
        duration: 3000,
        panelClass: ['error-snackbar']
      });
      this.router.navigate(['/invoices']);
    }
  }

  loadInvoiceAndCompany(id: string): void {
    this.isLoading = true;
    forkJoin([
      this.invoiceService.getInvoiceById(id),
      this.invoiceService.getInvoiceById(id).pipe(
        switchMap((invoice) => {
          console.log(invoice, "inv");
          if (invoice.customer?.companyId) {
            return this.companyService.getCompanyById(invoice.customer?.companyId);
          }
          return of({
            id: 0,
            name: 'Angular Core Hub',
            email: 'support@angularcorehub.com',
            address: {
              address1: '123 Business St, Suite 100',
              address2: '',
              city: '',
              state: '',
              zipCode: '',
              country: ''
            }
          } as Company);
        }),
        catchError((err) => {
          console.error('Error fetching company:', err);
          this.snackBar.open('Failed to load company data. Using defaults.', 'Close', {
            duration: 3000,
            panelClass: ['warning-snackbar']
          });
          return of({
            id: 0,
            name: 'Angular Core Hub',
            email: 'support@angularcorehub.com',
            address: {
              address1: '123 Business St, Suite 100',
              address2: '',
              city: '',
              state: '',
              zipCode: '',
              country: ''
            }
          } as Company);
        })
      )
    ]).subscribe({
      next: ([invoice, company]) => {
        this.invoice = invoice;
        this.company = company;
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error fetching invoice or company:', error);
        this.snackBar.open('Failed to load invoice. Please try again.', 'Close', {
          duration: 3000,
          panelClass: ['error-snackbar']
        });
        this.isLoading = false;
        this.router.navigate(['/invoices']);
      }
    });
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
        this.snackBar.open(`Invoice ${this.invoice!.invoiceNumber} downloaded successfully!`, 'Close', {
          duration: 3000,
          panelClass: ['success-snackbar']
        });
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error downloading PDF:', error);
        this.snackBar.open('Failed to download PDF. Please try again.', 'Close', {
          duration: 3000,
          panelClass: ['error-snackbar']
        });
        this.isLoading = false;
      }
    });
  }

  onSendInvoice(): void {
    if (!this.invoice) return;
    this.dialog.open(SendInvoiceDialogComponent, {
      width: '600px',
      data: {
        invoiceId: this.invoice.id,
        invoiceNumber: this.invoice.invoiceNumber,
        customerEmail: this.invoice.customer?.email || 'alagappantest@gmail.com'
      }
    }).afterClosed().subscribe(result => {
      if (result) {
        this.snackBar.open(`Invoice ${this.invoice!.invoiceNumber} sent successfully!`, 'Close', {
          duration: 3000,
          panelClass: ['success-snackbar']
        });
        this.loadInvoiceAndCompany(this.invoice!.id); // Refresh invoice data
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