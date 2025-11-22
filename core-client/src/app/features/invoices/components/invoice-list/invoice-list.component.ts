import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatInputModule } from '@angular/material/input';
import { MatMenuModule } from '@angular/material/menu';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { Router } from '@angular/router';
import { FormBuilder, FormGroup, FormsModule } from '@angular/forms';
import { debounceTime, distinctUntilChanged, Subject } from 'rxjs';
import {
  Invoice,
  InvoiceFilter,
  InvoiceStats,
  MoreFiltersDialogData,
  PaginatedResult,
  TaxType,
} from '../../../../interfaces/invoice/invoice.interface';
import { InvoiceService } from '../../../../services/invoice/invoice.service';
import { InvoiceImportDialogComponent } from '../import-invoice/invoice-import.component';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { animate, style, transition, trigger } from '@angular/animations';
import { SendInvoiceDialogComponent } from '../send-invoice/send-invoice-dialog.component';
import { AuthService } from '../../../../services/auth/auth.service';
import { DeleteConfirmationDialogComponent } from '../../../common/delete-confirmation/delete-confirmation-dialog.component';
import { NotificationDialogComponent } from '../../../../components/notification/notification-dialog.component';
import { MoreFiltersDialogComponent } from '../invoice-filters/more-filters-dialog.component';
import { CustomerService } from '../../../../services/customer/customer.service';
import { Customer, CustomerFilterRequest } from '../../../../services/customer/models/customer.model';
@Component({
  selector: 'app-invoice',
  templateUrl: './invoice-list.component.html',
  styleUrls: ['./invoice-list.component.css'],
  animations: [
    trigger('fadeIn', [
      transition(':enter', [
        style({ opacity: 0 }),
        animate('300ms ease', style({ opacity: 1 })),
      ]),
    ]),
    trigger('scaleIn', [
      transition('void => *', [
        style({ transform: 'scale(0.95)', opacity: 0.8 }),
        animate('200ms ease-out', style({ transform: 'scale(1)', opacity: 1 })),
      ]),
      transition('* => *', [
        style({ transform: 'scale(0.95)' }),
        animate('150ms ease-out', style({ transform: 'scale(1)' })),
      ]),
    ]),
    trigger('pulse', [
      transition(':enter', [
        style({ boxShadow: '0 0 0 0 rgba(138, 43, 226, 0.5)' }),
        animate(
          '600ms ease-in-out',
          style({ boxShadow: '0 0 0 8px rgba(138, 43, 226, 0)' })
        ),
      ]),
    ]),
  ],
  standalone: true,
  imports: [
    CommonModule,
    MatIconModule,
    MatButtonModule,
    MatInputModule,
    MatMenuModule,
    MatProgressSpinnerModule,
    MatDialogModule,
    FormsModule,
  ],
})
export class InvoiceComponent implements OnInit {
  invoices: Invoice[] = [];
  isLoading = false;
  exportingFormat: 'excel' | 'pdf' | null = null;
  isIndividualDownload: boolean = false; // New property to track individual download
  isDeleting: boolean = false;
  currentPage = 1;
  itemsPerPage = 10;
  totalItems = 0;
  totalPages = 0;
  selectedInvoiceStatus: string | null = 'All';
  selectedPaymentStatus: string | null = null;
  stats: InvoiceStats = {
    all: { count: 0, amount: 0, change: 0 },
    draft: { count: 0, amount: 0, change: 0 },
    sent: { count: 0, amount: 0, change: 0 },
    approved: { count: 0, amount: 0, change: 0 },
    cancelled: { count: 0, amount: 0, change: 0 },
    pending: { count: 0, amount: 0, change: 0 },
    completed: { count: 0, amount: 0, change: 0 },
    partiallyPaid: { count: 0, amount: 0, change: 0 },
    overdue: { count: 0, amount: 0, change: 0 },
    refunded: { count: 0, amount: 0, change: 0 },
  };
  searchTerm = '';
  // downloadingInvoiceId: string | null = null;
  private searchSubject = new Subject<string>();
  private destroy$ = new Subject<void>();
  isAdmin: boolean = false;
  isUser: boolean = false;
  isCustomer: boolean = false;
  customerId: string | null = null;
  customers: { id: number; name: string }[] = [];
  taxTypes: { name: string }[] = [];
  moreFiltersForm: FormGroup;
  constructor(
    private invoiceService: InvoiceService,
    private customerService: CustomerService,
    private router: Router,
    private dialog: MatDialog,
    private authService: AuthService,
    private fb: FormBuilder
  ) {
    this.moreFiltersForm = this.fb.group({
      customerId: [null],
      invoiceStatus: [null],
      paymentStatus: [null],
      taxType: [null],
      minAmount: [null],
      maxAmount: [null],
      invoiceNumberFrom: [''],
      invoiceNumberTo: [''],
      issueDateFrom: [null],
      issueDateTo: [null],
      dueDateFrom: [null],
      dueDateTo: [null],
    });
  }

  ngOnInit(): void {
    // Check roles
    this.isAdmin = this.authService.hasRole('Admin');
    this.isUser = this.authService.hasRole('User');
    this.isCustomer = this.authService.hasRole('Customer');

    // Get customerId from JWT for Customer role
    const user = this.authService.getUserDetail();
    this.customerId = user?.customerId || null;

    // Load customers for Admin/User, skip for Customer
    if (this.isAdmin || this.isUser) {
      this.loadCustomers();
    }

    // Load tax types for filters (Admin only)
    if (this.isAdmin) {
      this.loadTaxTypes();
    }

    this.loadInvoices();
    this.loadStats();
    this.setupSearch();
  }
  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
  private setupSearch(): void {
    this.searchSubject
      .pipe(debounceTime(300), distinctUntilChanged())
      .subscribe((searchTerm) => {
        this.searchTerm = searchTerm;
        this.currentPage = 1;
        this.loadInvoices();
      });
  }

  private loadTaxTypes(): void {
    this.invoiceService.getTaxTypes().subscribe({
      next: (taxTypes: TaxType[]) => {
        this.taxTypes = taxTypes.map((tax) => ({ name: tax.name }));
      },
      error: (error) => {
        console.error('Error fetching tax types:', error);
        this.openDialog(
          'error',
          'Load Failed',
          'Failed to load tax types. Please try again.',
          'Tax type data could not be retrieved. Please check your internet connection and refresh the page.'
        );
      },
    });
  }
  loadInvoices(): void {
    this.isLoading = true;
    const filter: InvoiceFilter = {
      pageNumber: this.currentPage,
      pageSize: this.itemsPerPage,
      search: this.searchTerm || undefined,
      invoiceStatus:
        this.moreFiltersForm.get('invoiceStatus')?.value ||
        (this.selectedInvoiceStatus === 'All'
          ? undefined
          : this.selectedInvoiceStatus),
      paymentStatus:
        this.moreFiltersForm.get('paymentStatus')?.value ||
        this.selectedPaymentStatus ||
        undefined,
      customerId: this.isCustomer
        ? Number(this.customerId) // Restrict to customer's own invoices
        : this.moreFiltersForm.get('customerId')?.value || undefined,
      taxType: this.isAdmin
        ? this.moreFiltersForm.get('taxType')?.value || undefined
        : undefined,
      minAmount: this.isAdmin
        ? this.moreFiltersForm.get('minAmount')?.value || undefined
        : undefined,
      maxAmount: this.isAdmin
        ? this.moreFiltersForm.get('maxAmount')?.value || undefined
        : undefined,
      invoiceNumberFrom: this.isAdmin
        ? this.moreFiltersForm.get('invoiceNumberFrom')?.value || undefined
        : undefined,
      invoiceNumberTo: this.isAdmin
        ? this.moreFiltersForm.get('invoiceNumberTo')?.value || undefined
        : undefined,
      issueDateFrom: this.isAdmin
        ? this.moreFiltersForm.get('issueDateFrom')?.value
          ? new Date(
              this.moreFiltersForm.get('issueDateFrom')?.value
            ).toISOString()
          : undefined
        : undefined,
      issueDateTo: this.isAdmin
        ? this.moreFiltersForm.get('issueDateTo')?.value
          ? new Date(
              this.moreFiltersForm.get('issueDateTo')?.value
            ).toISOString()
          : undefined
        : undefined,
      dueDateFrom: this.isAdmin
        ? this.moreFiltersForm.get('dueDateFrom')?.value
          ? new Date(
              this.moreFiltersForm.get('dueDateFrom')?.value
            ).toISOString()
          : undefined
        : undefined,
      dueDateTo: this.isAdmin
        ? this.moreFiltersForm.get('dueDateTo')?.value
          ? new Date(this.moreFiltersForm.get('dueDateTo')?.value).toISOString()
          : undefined
        : undefined,
    };

    this.invoiceService.getPagedInvoices(filter).subscribe({
      next: (result: PaginatedResult<Invoice>) => {
        // this.invoices = result.items;
        this.invoices = result.items.map((item) => ({
          ...item,
          customerName: item.customer?.name || 'Unknown Customer', // Map customer.name to customerName
          issueDate: new Date(item.issueDate),
          dueDate: new Date(item.dueDate),
        }));
        this.totalItems = result.totalCount;
        this.totalPages = Math.ceil(result.totalCount / this.itemsPerPage);
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error fetching invoices:', error);
        this.isLoading = false;
      },
    });
  }

  getInitials(name: string): string {
    return name
      .split(' ')
      .map((n) => n.charAt(0).toUpperCase())
      .join('')
      .substring(0, 2);
  }

  getAvatarColor(name: string | undefined): string {
    const colors = [
      '#FF2E63',
      '#00D4B9',
      '#FF6B6B',
      '#FFD93D',
      '#1E90FF',
      '#8A2BE2',
      '#4B0082',
    ];
    const fallbackName = name || 'Unknown';
    const index =
      fallbackName
        .split('')
        .reduce((sum, char) => sum + char.charCodeAt(0), 0) % colors.length;
    return colors[index];
  }

  loadStats(): void {
    this.isLoading = true;
    this.invoiceService.getInvoiceStats().subscribe({
      next: (stats: InvoiceStats) => {
        this.stats = stats;
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error fetching stats:', error);
        this.isLoading = false;
      },
    });
  }

  loadCustomers(): void {
    this.isLoading = true;
    const filter: CustomerFilterRequest = {
      pageNumber: 1,
      pageSize: 100,
      search: '',
      status: 'Active', // Adjust based on requirements (e.g., 'All' or 'Active')
    };
    this.customerService.getCustomers(filter).subscribe({
      next: (result: PaginatedResult<Customer>) => {
        this.customers = result.items.map((customer) => ({
          id: customer.id,
          name: customer.name,
        }));
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error fetching customers:', error);
        this.openDialog(
          'error',
          'Load Failed',
          'Failed to load customers. Please try again.',
          'Customer data could not be retrieved from the server. Please check your internet connection and refresh the page.'
        );
        this.isLoading = false;
      },
    });
  }

  onSearch(searchTerm: string): void {
    this.searchSubject.next(searchTerm);
  }

  onSelectInvoiceStatus(status: string): void {
    this.selectedInvoiceStatus = status;
    this.selectedPaymentStatus = null;
    this.currentPage = 1;
    this.loadInvoices();
  }

  onSelectPaymentStatus(status: string): void {
    this.selectedPaymentStatus = status;
    this.selectedInvoiceStatus = null;
    this.currentPage = 1;
    this.loadInvoices();
  }

  getStatusText(status: string): string {
    return status.replace(/([A-Z])/g, ' $1').trim();
  }

  goToPage(page: number): void {
    if (page >= 1 && page <= this.totalPages) {
      this.currentPage = page;
      this.loadInvoices();
    }
  }

  nextPage(): void {
    if (this.currentPage < this.totalPages) {
      this.currentPage++;
      this.loadInvoices();
    }
  }

  previousPage(): void {
    if (this.currentPage > 1) {
      this.currentPage--;
      this.loadInvoices();
    }
  }

  getVisiblePages(): number[] {
    const maxVisiblePages = 5;
    let startPage = Math.max(
      1,
      this.currentPage - Math.floor(maxVisiblePages / 2)
    );
    let endPage = Math.min(this.totalPages, startPage + maxVisiblePages - 1);

    if (endPage - startPage + 1 < maxVisiblePages) {
      startPage = Math.max(1, endPage - maxVisiblePages + 1);
    }

    const visiblePages: number[] = [];
    for (let i = startPage; i <= endPage; i++) {
      visiblePages.push(i);
    }
    return visiblePages;
  }

  getStatusClass(status: string, type: 'invoice' | 'payment'): string {
    switch (type) {
      case 'invoice':
        switch (status.toLowerCase()) {
          case 'draft':
            return 'bg-gray-500';
          case 'sent':
            return 'bg-blue-500';
          case 'approved':
            return 'bg-green-500';
          case 'cancelled':
            return 'bg-red-500';
          default:
            return 'bg-gray-500';
        }
      case 'payment':
        switch (status.toLowerCase()) {
          case 'pending':
            return 'bg-blue-400';
          case 'processing':
            return 'bg-yellow-400';
          case 'completed':
            return 'bg-green-400';
          case 'partiallypaid':
            return 'bg-yellow-500';
          case 'overdue':
            return 'bg-red-400';
          case 'failed':
            return 'bg-red-600';
          case 'refunded':
            return 'bg-purple-500';
          default:
            return 'bg-gray-500';
        }
    }
  }

  trackByInvoice(index: number, invoice: Invoice): number {
    return invoice.id;
  }

  onCreateInvoice(): void {
    this.router.navigate(['/invoices/create']);
  }

  onEditInvoice(invoice: Invoice): void {
    this.router.navigate([`/invoices/edit/${invoice.id}`]);
  }

  onViewInvoice(invoice: Invoice): void {
    this.router.navigate([`/invoices/view/${invoice.id}`]);
  }

  onDeleteInvoice(invoice: Invoice): void {
    const dialogRef = this.dialog.open(DeleteConfirmationDialogComponent, {
      width: '450px',
      disableClose: true,
      panelClass: 'delete-dialog-panel',
      data: {
        title: 'Delete Invoice',
        message: `Are you sure you want to delete invoice ${invoice.invoiceNumber}? This action cannot be undone.`,
        itemName: invoice.invoiceNumber,
        confirmText: 'Delete',
        cancelText: 'Cancel',
      },
    });

    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        this.isDeleting = false; // End delete loading
        this.invoiceService.deleteInvoice(invoice.id).subscribe({
          next: () => {
            this.loadInvoices();
            this.loadStats();
            // Success dialog
            this.openDialog(
              'success',
              'Invoice Deleted Successfully',
              `Invoice ${invoice.invoiceNumber} has been deleted successfully!`,
              'The invoice has been moved to trash and is no longer visible in your active invoice list. It can be restored if needed through the system administrator.'
            );
          },
          error: (error) => {
            console.error('Error deleting invoice:', error);
            this.isDeleting = false; // End delete loading
            this.openDialog(
              'error',
              'Delete Failed',
              'Failed to delete invoice. Please try again.',
              'The invoice could not be deleted due to a system error. Please check your internet connection and try again. If the problem persists, contact support.'
            );
          },
        });
      }
    });
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

  onSendInvoice(invoice: any): void {
    console.log(invoice, "Invoice");
    this.dialog
      .open(SendInvoiceDialogComponent, {
        width: '600px',
        data: {
          invoiceId: invoice.id,
          invoiceNumber: invoice.invoiceNumber,
          customerEmail: invoice.customer.email || 'alagappantest@gmail.com',
        },
      })
      .afterClosed()
      .subscribe((result) => {
        if (result) {
          this.loadInvoices(); // Refresh invoices to update status
          this.loadStats(); // Refresh stats
        }
      });
  }

  onDuplicateInvoice(invoice: Invoice): void {
    this.isLoading = true;
    this.invoiceService.duplicateInvoice(invoice.id).subscribe({
      next: () => {
        this.loadInvoices();
        this.openDialog(
          'success',
          'Invoice Duplicated Successfully',
          `Invoice ${invoice.invoiceNumber} duplicated successfully!`,
          'A copy of the invoice has been created with a new invoice number. You can find it in your invoice list and modify it as needed.'
        );
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error duplicating invoice:', error);
        this.openDialog(
          'error',
          'Duplication Failed',
          'Failed to duplicate invoice. Please try again.',
          'The invoice could not be duplicated due to a system error. Please try again or contact support if the issue persists.'
        );
        this.isLoading = false;
      },
    });
  }

  onDownloadInvoice(invoice: Invoice): void {
    this.exportingFormat = 'pdf';
    this.isIndividualDownload = true;
    this.invoiceService.downloadInvoicePdf(invoice.id).subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = `invoice_${invoice.invoiceNumber}.pdf`;
        link.click();
        window.URL.revokeObjectURL(url);
        this.openDialog(
          'success',
          'Download Successful',
          `Invoice ${invoice.invoiceNumber} downloaded successfully!`,
          'The PDF file has been saved to your downloads folder. You can now view, print, or share the invoice as needed.'
        );
        this.exportingFormat = null; // Clear to hide loader and blur
        this.isIndividualDownload = false; // Reset
      },
      error: (error) => {
        console.error('Error downloading PDF:', error);
        this.openDialog(
          'error',
          'Download Failed',
          'Failed to download PDF. Please try again.',
          'The invoice PDF could not be generated or downloaded. Please check your internet connection and try again.'
        );
        this.exportingFormat = null;
        this.isIndividualDownload = false; // Reset
      },
    });
  }

  onExport(format: 'excel' | 'pdf'): void {
    this.exportingFormat = format;
    const filter: InvoiceFilter = {
      pageNumber: this.currentPage,
      pageSize: this.itemsPerPage,
      search: this.searchTerm || undefined,
      invoiceStatus: this.selectedInvoiceStatus || undefined,
      paymentStatus: this.selectedPaymentStatus || undefined,
      customerId: this.isCustomer ? Number(this.customerId) : undefined,
    };
    if (format === 'excel') {
      this.invoiceService.exportInvoicesExcel(filter).subscribe({
        next: (blob) => {
          const url = window.URL.createObjectURL(blob);
          const link = document.createElement('a');
          link.href = url;
          link.download = `invoices_${new Date().toISOString()}.xlsx`;
          link.click();
          window.URL.revokeObjectURL(url);
          this.openDialog(
            'success',
            'Export Successful',
            'Invoices exported successfully as Excel!',
            'The Excel file containing your invoice data has been downloaded. You can now open it in Excel or other spreadsheet applications for further analysis.'
          );
          this.exportingFormat = null;
        },
        error: (error) => {
          console.error('Error exporting Excel:', error);
          this.openDialog(
            'error',
            'Export Failed',
            'Failed to export Excel. Please try again.',
            'The Excel export could not be completed due to a system error. Please try again or contact support if the issue persists.'
          );
          this.exportingFormat = null;
        },
      });
    } else if (format === 'pdf') {
      this.invoiceService.exportInvoicesPdf(filter).subscribe({
        next: (blob) => {
          const url = window.URL.createObjectURL(blob);
          const link = document.createElement('a');
          link.href = url;
          link.download = `invoices_${new Date().toISOString()}.pdf`;
          link.click();
          window.URL.revokeObjectURL(url);
          this.openDialog(
            'success',
            'Export Successful',
            'Invoices exported successfully as PDF!',
            'The PDF file containing your invoice data has been downloaded. You can now view, print, or share the consolidated invoice report.'
          );
          this.exportingFormat = null;
        },
        error: (error) => {
          console.error('Error exporting PDF:', error);
          this.openDialog(
            'error',
            'Export Failed',
            'Failed to export PDF. Please try again.',
            'The PDF export could not be completed due to a system error. Please try again or contact support if the issue persists.'
          );
          this.exportingFormat = null;
        },
      });
    }
  }

  onImport(): void {
    const dialogRef = this.dialog.open(InvoiceImportDialogComponent, {
      width: '600px',
    });

    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        this.loadInvoices();
        this.loadStats();
      }
    });
  }

  onDateFilter(): void {}

  onMoreFilters(): void {
    const dialogRef = this.dialog.open(MoreFiltersDialogComponent, {
      width: '600px',
      data: {
        customers: this.customers,
        taxTypes: this.taxTypes,
        formData: this.moreFiltersForm.value,
      } as MoreFiltersDialogData,
    });

    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        this.moreFiltersForm.patchValue(result);
        this.currentPage = 1;
        this.loadInvoices();
      }
    });
  }

  onSettings(): void {}
}
