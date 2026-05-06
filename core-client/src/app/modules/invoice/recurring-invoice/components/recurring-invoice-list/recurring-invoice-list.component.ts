// recurring-invoice-list.component.ts
import { CommonModule } from '@angular/common';
import { Component, OnInit, OnDestroy } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatInputModule } from '@angular/material/input';
import { MatMenuModule } from '@angular/material/menu';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { Router } from '@angular/router';
import { FormBuilder, FormGroup, FormsModule } from '@angular/forms';
import { debounceTime, distinctUntilChanged, Subject } from 'rxjs';
import { animate, style, transition, trigger } from '@angular/animations';

import { RecurringInvoiceService } from '../../services/recurring-invoice.service';
import { CustomerService } from '../../../../customer/services/customer.service';
import { AuthService } from '../../../../../core/services/auth/auth.service';
import { DeleteConfirmationDialogComponent } from '../../../../../features/common/delete-confirmation/delete-confirmation-dialog.component';
import { NotificationDialogComponent } from '../../../../../shared/components/notification/notification-dialog.component';
import { MoreFiltersDialogComponent } from '../../../../../shared/components/invoice-filter-dialog/more-filters-dialog.component';
import {
  CustomerFilterRequest,
  Customer,
  PaginatedResult as CustomerPaginatedResult,
} from '../../../../customer/services/models/customer.model';

import {
  PaginatedRecurringInvoices,
  RecurringInvoice,
  RecurringInvoiceFilter,
  RecurringInvoiceStats,
  MoreFiltersDialogData,
} from '../../../../../interfaces/invoice/recurring-invoice/recurring-invoice.interface';
import { MoreFiltersConfig } from '../../../../../interfaces/invoice/invoice-filter/more-filters.interface';

@Component({
  selector: 'app-recurring-invoice-list',
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
  templateUrl: './recurring-invoice-list.component.html',
  styleUrls: ['./recurring-invoice-list.component.css'],
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
          style({ boxShadow: '0 0 0 8px rgba(138, 43, 226, 0)' }),
        ),
      ]),
    ]),
  ],
})
export class RecurringInvoiceListComponent implements OnInit, OnDestroy {
  recurringInvoices: RecurringInvoice[] = [];
  isLoading = false;
  isFirstLoad = true;
  exportingFormat: 'excel' | 'pdf' | null = null;
  isDeleting: boolean = false;
  currentPage = 1;
  itemsPerPage = 10;
  totalItems = 0;
  totalPages = 0;
  selectedStatus: string | null = null;

  stats: RecurringInvoiceStats = {
    draft: { count: 0, amount: 0, change: 0 },
    active: { count: 0, amount: 0, change: 0 },
    paused: { count: 0, amount: 0, change: 0 },
    completed: { count: 0, amount: 0, change: 0 },
    cancelled: { count: 0, amount: 0, change: 0 },
    expired: { count: 0, amount: 0, change: 0 },
    totalActiveAmount: 0,
    totalMonthlyRecurring: 0,
    totalYearlyRecurring: 0,
  };

  searchTerm = '';
  private searchSubject = new Subject<string>();
  private destroy$ = new Subject<void>();

  isAdmin: boolean = false;
  isUser: boolean = false;
  isCustomer: boolean = false;
  customerId: string | null = null;

  customers: { id: number; name: string }[] = [];
  frequencies: { value: string; label: string }[] = [];
  statuses: { value: string; label: string }[] = [];

  moreFiltersForm: FormGroup;

  constructor(
    private recurringInvoiceService: RecurringInvoiceService,
    private customerService: CustomerService,
    private router: Router,
    private dialog: MatDialog,
    private authService: AuthService,
    private fb: FormBuilder,
  ) {
    this.moreFiltersForm = this.fb.group({
      customerId: [null],
      status: [null],
      frequency: [null],
      minAmount: [null],
      maxAmount: [null],
      nextDateFrom: [null],
      nextDateTo: [null],
      startDateFrom: [null],
      startDateTo: [null],
      endDateFrom: [null],
      endDateTo: [null],
      autoSend: [false],
    });
  }

  ngOnInit(): void {
    this.isAdmin = this.authService.hasRole('Admin');
    this.isUser = this.authService.hasRole('User');
    this.isCustomer = this.authService.hasRole('Customer');

    const user = this.authService.getUserDetail();
    this.customerId = user?.customerId || null;

    if (this.isAdmin || this.isUser) {
      this.loadCustomers();
    }

    if (this.isAdmin) {
      this.loadFrequencies();
      this.loadStatuses();
    }

    this.isLoading = true;
    this.isFirstLoad = true;
    this.loadRecurringInvoices();
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
        this.loadRecurringInvoices();
      });
  }

  private loadFrequencies(): void {
    this.recurringInvoiceService.getFrequencies().subscribe({
      next: (frequencies) => {
        this.frequencies = frequencies;
      },
      error: (error) => {
        console.error('Error fetching frequencies:', error);
      },
    });
  }

  private loadStatuses(): void {
    this.recurringInvoiceService.getStatuses().subscribe({
      next: (statuses) => {
        this.statuses = statuses;
      },
      error: (error) => {
        console.error('Error fetching statuses:', error);
      },
    });
  }

  loadRecurringInvoices(): void {
    this.isLoading = true;

    const filter: RecurringInvoiceFilter = {
      pageNumber: this.currentPage,
      pageSize: this.itemsPerPage,
      search: this.searchTerm || undefined,
      status:
        this.moreFiltersForm.get('status')?.value ||
        this.selectedStatus ||
        undefined,
      frequency: this.moreFiltersForm.get('frequency')?.value || undefined,
      customerId: this.isCustomer
        ? Number(this.customerId)
        : this.moreFiltersForm.get('customerId')?.value || undefined,
      minAmount: this.isAdmin
        ? this.moreFiltersForm.get('minAmount')?.value || undefined
        : undefined,
      maxAmount: this.isAdmin
        ? this.moreFiltersForm.get('maxAmount')?.value || undefined
        : undefined,
      nextDateFrom:
        this.isAdmin && this.moreFiltersForm.get('nextDateFrom')?.value
          ? new Date(
              this.moreFiltersForm.get('nextDateFrom')?.value,
            ).toISOString()
          : undefined,
      nextDateTo:
        this.isAdmin && this.moreFiltersForm.get('nextDateTo')?.value
          ? new Date(
              this.moreFiltersForm.get('nextDateTo')?.value,
            ).toISOString()
          : undefined,
      startDateFrom:
        this.isAdmin && this.moreFiltersForm.get('startDateFrom')?.value
          ? new Date(
              this.moreFiltersForm.get('startDateFrom')?.value,
            ).toISOString()
          : undefined,
      startDateTo:
        this.isAdmin && this.moreFiltersForm.get('startDateTo')?.value
          ? new Date(
              this.moreFiltersForm.get('startDateTo')?.value,
            ).toISOString()
          : undefined,
      endDateFrom:
        this.isAdmin && this.moreFiltersForm.get('endDateFrom')?.value
          ? new Date(
              this.moreFiltersForm.get('endDateFrom')?.value,
            ).toISOString()
          : undefined,
      endDateTo:
        this.isAdmin && this.moreFiltersForm.get('endDateTo')?.value
          ? new Date(this.moreFiltersForm.get('endDateTo')?.value).toISOString()
          : undefined,
      autoSend: this.moreFiltersForm.get('autoSend')?.value || undefined,
      sortBy: 'NextInvoiceDate',
      sortOrder: 'asc',
    };

    this.recurringInvoiceService.getPagedRecurringInvoices(filter).subscribe({
      next: (result: PaginatedRecurringInvoices) => {
        this.recurringInvoices = result.items;
        this.totalItems = result.totalCount;
        this.totalPages = Math.ceil(result.totalCount / this.itemsPerPage);
        this.isLoading = false;
        this.isFirstLoad = false;
      },
      error: (error) => {
        console.error('Error fetching recurring invoices:', error);
          this.recurringInvoices = [];
        this.isLoading = false;
        this.isFirstLoad = false;
          this.openDialog(
        'error',
        'Load Failed',
        'Failed to load recurring invoices.',
        error?.error?.detail || 'Please try again later.',
      );
      },
    });
  }

  loadStats(): void {
    this.recurringInvoiceService.getStats().subscribe({
      next: (stats: RecurringInvoiceStats) => {
        this.stats = stats;
      },
      error: (error: any) => {
        console.error('Error fetching stats:', error);
        this.isLoading = false;
      },
    });
  }

  loadCustomers(): void {
    const filter: CustomerFilterRequest = {
      pageNumber: 1,
      pageSize: 100,
      search: '',
      status: 'Active',
    };
    this.customerService.getCustomers(filter).subscribe({
      next: (result: CustomerPaginatedResult<Customer>) => {
        this.customers = result.items.map((customer) => ({
          id: customer.id,
          name: customer.name,
        }));
      },
      error: (error) => {
        console.error('Error fetching customers:', error);
        this.openDialog(
          'error',
          'Load Failed',
          'Failed to load customers. Please try again.',
          'Customer data could not be retrieved from the server.',
        );
      },
    });
  }

  onSearch(searchTerm: string): void {
    this.searchSubject.next(searchTerm);
  }

  onSelectStatus(status: string): void {
    this.selectedStatus = status;
    this.currentPage = 1;
    this.loadRecurringInvoices();
  }

  getStatusText(status: string): string {
    return status.charAt(0).toUpperCase() + status.slice(1);
  }

  getFrequencyLabel(frequency: string): string {
    const freqMap: Record<string, string> = {
      Daily: 'Daily',
      Weekly: 'Weekly',
      Monthly: 'Monthly',
      Quarterly: 'Quarterly',
      HalfYearly: 'Half-Yearly',
      Annually: 'Annually',
    };
    return freqMap[frequency] || frequency;
  }

  goToPage(page: number): void {
    if (page >= 1 && page <= this.totalPages) {
      this.currentPage = page;
      this.loadRecurringInvoices();
    }
  }

  nextPage(): void {
    if (this.currentPage < this.totalPages) {
      this.currentPage++;
      this.loadRecurringInvoices();
    }
  }

  previousPage(): void {
    if (this.currentPage > 1) {
      this.currentPage--;
      this.loadRecurringInvoices();
    }
  }

  getVisiblePages(): number[] {
    const maxVisiblePages = 5;
    let startPage = Math.max(
      1,
      this.currentPage - Math.floor(maxVisiblePages / 2),
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

  getStatusClass(status: string): string {
    switch (status.toLowerCase()) {
      case 'active':
        return 'bg-green-500';
      case 'draft':
        return 'bg-gray-500';
      case 'paused':
        return 'bg-yellow-500';
      case 'completed':
        return 'bg-blue-500';
      case 'cancelled':
        return 'bg-red-500';
      case 'expired':
        return 'bg-orange-500';
      default:
        return 'bg-gray-500';
    }
  }

  getInitials(name: string | undefined): string {
    const displayName = name || 'Unknown';
    return displayName
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

  trackByRecurringInvoice(index: number, invoice: RecurringInvoice): number {
    return invoice.id;
  }

  onCreateRecurringInvoice(): void {
    this.router.navigate(['/invoices/recurring/create']);
  }

  onEditRecurringInvoice(invoice: RecurringInvoice): void {
    this.router.navigate([`/invoices/recurring/edit/${invoice.id}`]);
  }

  onViewRecurringInvoice(invoice: RecurringInvoice): void {
    this.router.navigate([`/invoices/recurring/view/${invoice.id}`]);
  }

  onDeleteRecurringInvoice(invoice: RecurringInvoice): void {
    const dialogRef = this.dialog.open(DeleteConfirmationDialogComponent, {
      width: '450px',
      disableClose: true,
      panelClass: 'delete-dialog-panel',
      data: {
        title: 'Delete Recurring Invoice',
        message: `Are you sure you want to delete recurring invoice "${invoice.name}"? This action cannot be undone.`,
        itemName: invoice.name,
        confirmText: 'Delete',
        cancelText: 'Cancel',
      },
    });

    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        this.recurringInvoiceService
          .deleteRecurringInvoice(invoice.id)
          .subscribe({
            next: () => {
              this.loadRecurringInvoices();
              this.loadStats();
              this.openDialog(
                'success',
                'Recurring Invoice Deleted',
                `"${invoice.name}" has been deleted successfully!`,
                'The recurring invoice template has been removed from your system.',
              );
            },
            error: (error) => {
              console.error('Error deleting recurring invoice:', error);
              this.openDialog(
                'error',
                'Delete Failed',
                'Failed to delete recurring invoice. Please try again.',
                'The recurring invoice could not be deleted due to a system error.',
              );
            },
          });
      }
    });
  }

  onPauseRecurringInvoice(invoice: RecurringInvoice): void {
    const dialogRef = this.dialog.open(DeleteConfirmationDialogComponent, {
      width: '450px',
      disableClose: true,
      panelClass: 'delete-dialog-panel',
      data: {
        title: 'Pause Recurring Invoice',
        message: `Are you sure you want to pause "${invoice.name}"? No further invoices will be generated until resumed.`,
        itemName: invoice.name,
        confirmText: 'Pause',
        cancelText: 'Cancel',
      },
    });

    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        this.recurringInvoiceService
          .pauseRecurringInvoice(invoice.id)
          .subscribe({
            next: () => {
              this.loadRecurringInvoices();
              this.loadStats();
              this.openDialog(
                'success',
                'Recurring Invoice Paused',
                `"${invoice.name}" has been paused successfully!`,
                'The schedule is now paused. Click Resume to reactivate automatic generation.',
              );
            },
            error: (error) => {
              console.error('Error pausing recurring invoice:', error);
              this.openDialog(
                'error',
                'Pause Failed',
                'Failed to pause recurring invoice. Please try again.',
                'The recurring invoice could not be paused due to a system error.',
              );
            },
          });
      }
    });
  }

  onResumeRecurringInvoice(invoice: RecurringInvoice): void {
    this.recurringInvoiceService.resumeRecurringInvoice(invoice.id).subscribe({
      next: () => {
        this.loadRecurringInvoices();
        this.loadStats();
        this.openDialog(
          'success',
          'Recurring Invoice Resumed',
          `"${invoice.name}" has been resumed successfully!`,
          'The schedule is now active and will generate invoices according to the configured frequency.',
        );
      },
      error: (error) => {
        console.error('Error resuming recurring invoice:', error);
        this.openDialog(
          'error',
          'Resume Failed',
          'Failed to resume recurring invoice. Please try again.',
          'The recurring invoice could not be resumed due to a system error.',
        );
      },
    });
  }

  onCancelRecurringInvoice(invoice: RecurringInvoice): void {
    const dialogRef = this.dialog.open(DeleteConfirmationDialogComponent, {
      width: '450px',
      disableClose: true,
      panelClass: 'delete-dialog-panel',
      data: {
        title: 'Cancel Recurring Invoice',
        message: `Are you sure you want to cancel "${invoice.name}"? This will permanently stop all future generations.`,
        itemName: invoice.name,
        confirmText: 'Cancel',
        cancelText: 'Go Back',
      },
    });

    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        this.recurringInvoiceService
          .cancelRecurringInvoice(invoice.id)
          .subscribe({
            next: () => {
              this.loadRecurringInvoices();
              this.loadStats();
              this.openDialog(
                'success',
                'Recurring Invoice Cancelled',
                `"${invoice.name}" has been cancelled successfully!`,
                'The schedule has been permanently cancelled and will no longer generate invoices.',
              );
            },
            error: (error) => {
              console.error('Error cancelling recurring invoice:', error);
              this.openDialog(
                'error',
                'Cancel Failed',
                'Failed to cancel recurring invoice. Please try again.',
                'The recurring invoice could not be cancelled due to a system error.',
              );
            },
          });
      }
    });
  }

  onGenerateNow(invoice: RecurringInvoice): void {
    const dialogRef = this.dialog.open(DeleteConfirmationDialogComponent, {
      width: '450px',
      disableClose: true,
      panelClass: 'delete-dialog-panel',
      data: {
        title: 'Generate Invoice Now',
        message: `Generate an invoice immediately from "${invoice.name}"? This will create an invoice outside the regular schedule.`,
        itemName: invoice.name,
        confirmText: 'Generate',
        cancelText: 'Cancel',
      },
    });

    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        this.isLoading = true;
        this.recurringInvoiceService.generateNow(invoice.id).subscribe({
          next: (instance) => {
            this.isLoading = false;
            this.openDialog(
              'success',
              'Invoice Generated',
              `Invoice ${instance.generatedInvoiceNumber} has been generated successfully!`,
              'The invoice has been created and can be viewed in the invoices section.',
            );
            this.loadRecurringInvoices();
            this.loadStats();
          },
          error: (error) => {
            console.error('Error generating invoice:', error);
            this.isLoading = false;
            this.openDialog(
              'error',
              'Generation Failed',
              'Failed to generate invoice. Please try again.',
              error?.error?.detail ||
                'The invoice could not be generated due to a system error.',
            );
          },
        });
      }
    });
  }

  onExport(format: 'excel' | 'pdf'): void {
    this.exportingFormat = format;
    const filter: RecurringInvoiceFilter = {
      pageNumber: this.currentPage,
      pageSize: this.itemsPerPage,
      search: this.searchTerm || undefined,
      status: this.selectedStatus || undefined,
      customerId: this.isCustomer ? Number(this.customerId) : undefined,
    };

    if (format === 'excel') {
      this.recurringInvoiceService
        .exportRecurringInvoicesExcel(filter)
        .subscribe({
          next: (blob) => {
            const url = window.URL.createObjectURL(blob);
            const link = document.createElement('a');
            link.href = url;
            link.download = `recurring_invoices_${new Date().toISOString()}.xlsx`;
            link.click();
            window.URL.revokeObjectURL(url);
            this.openDialog(
              'success',
              'Export Successful',
              'Recurring invoices exported successfully as Excel!',
              'The Excel file containing your recurring invoice data has been downloaded.',
            );
            this.exportingFormat = null;
          },
          error: (error) => {
            console.error('Error exporting Excel:', error);
            this.openDialog(
              'error',
              'Export Failed',
              'Failed to export Excel. Please try again.',
              'The Excel export could not be completed due to a system error.',
            );
            this.exportingFormat = null;
          },
        });
    } else if (format === 'pdf') {
      this.recurringInvoiceService
        .exportRecurringInvoicesPdf(filter)
        .subscribe({
          next: (blob) => {
            const url = window.URL.createObjectURL(blob);
            const link = document.createElement('a');
            link.href = url;
            link.download = `recurring_invoices_${new Date().toISOString()}.pdf`;
            link.click();
            window.URL.revokeObjectURL(url);
            this.openDialog(
              'success',
              'Export Successful',
              'Recurring invoices exported successfully as PDF!',
              'The PDF file containing your recurring invoice data has been downloaded.',
            );
            this.exportingFormat = null;
          },
          error: (error) => {
            console.error('Error exporting PDF:', error);
            this.openDialog(
              'error',
              'Export Failed',
              'Failed to export PDF. Please try again.',
              'The PDF export could not be completed due to a system error.',
            );
            this.exportingFormat = null;
          },
        });
    }
  }

  onMoreFilters(): void {
    const dialogConfig: MoreFiltersConfig = {
      title: 'Recurring Invoice Filters',
      filterType: 'recurring',
      customers: this.customers,
      frequencies: this.frequencies,
      statuses: this.statuses,
      formData: this.moreFiltersForm.value,
    };
    const dialogRef = this.dialog.open(MoreFiltersDialogComponent, {
      width: '700px',
      data: dialogConfig,
    });

    dialogRef.afterClosed().subscribe((result) => {
      if (result && !result.cleared) {
        this.moreFiltersForm.patchValue(result);
        this.currentPage = 1;
        this.loadRecurringInvoices();
      } else if (result?.cleared) {
        this.moreFiltersForm.reset({
          customerId: null,
          status: null,
          frequency: null,
          minAmount: null,
          maxAmount: null,
          nextDateFrom: null,
          nextDateTo: null,
          startDateFrom: null,
          startDateTo: null,
          endDateFrom: null,
          endDateTo: null,
          autoSend: false,
        });
        this.currentPage = 1;
        this.loadRecurringInvoices();
      }
    });
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

  // Stats helper methods
  getStatsChange(
    statsItem: { count: number; amount: number; change: number } | undefined,
  ): number {
    return statsItem?.change ?? 0;
  }

  isStatsPositive(
    statsItem: { count: number; amount: number; change: number } | undefined,
  ): boolean {
    return (statsItem?.change ?? 0) >= 0;
  }

  getStatsIcon(
    statsItem: { count: number; amount: number; change: number } | undefined,
  ): string {
    return (statsItem?.change ?? 0) >= 0 ? 'trending_up' : 'trending_down';
  }

  getStatsChangeClass(
    statsItem: { count: number; amount: number; change: number } | undefined,
  ): string {
    return (statsItem?.change ?? 0) >= 0 ? 'positive' : 'negative';
  }

  getFormattedChange(
    statsItem: { count: number; amount: number; change: number } | undefined,
  ): string {
    const change = statsItem?.change ?? 0;
    return `${change >= 0 ? '+' : ''}${Math.abs(change).toFixed(1)}%`;
  }
  // Add this method to RecurringInvoiceListComponent
  getFrequencyIcon(frequency: string): string {
    const iconMap: Record<string, string> = {
      Daily: 'today',
      Weekly: 'calendar_view_week',
      Monthly: 'calendar_month',
      Quarterly: 'calendar_view_month',
      HalfYearly: 'date_range',
      Annually: 'calendar_today',
    };
    return iconMap[frequency] || 'schedule';
  }
}
