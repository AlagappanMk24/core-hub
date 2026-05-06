import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatInputModule } from '@angular/material/input';
import { MatMenuModule } from '@angular/material/menu';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatTooltipModule } from '@angular/material/tooltip';
import { FormsModule } from '@angular/forms';
import { Subject } from 'rxjs';
import { debounceTime, distinctUntilChanged, takeUntil } from 'rxjs/operators';
import { animate, style, transition, trigger } from '@angular/animations';
import { Router } from '@angular/router';
import { AuthService } from '../../../../core/services/auth/auth.service';
import { CustomerDialogComponent } from '../customer-dialog/customer-dialog.component';
import { DeleteConfirmationDialogComponent } from '../../../../features/common/delete-confirmation/delete-confirmation-dialog.component';
import { NotificationDialogComponent } from '../../../../shared/components/notification/notification-dialog.component';
import {
  CustomerFilterRequest,
  CustomerStats,
  Customer,
  PaginatedResult,
  ColumnVisibility,
} from '../../services/models/customer.model';
import { CustomerService } from '../../services/customer.service';
import { MatCheckbox } from '@angular/material/checkbox';
import { AddNoteDialogComponent } from '../add-note-dialog/add-note-dialog.component';
import { ScheduleFollowUpDialogComponent } from '../schedule-follow-up-dialog/schedule-follow-up-dialog.component';
import { QuickActionsDialogComponent } from '../quick-actions-dialog/quick-actions-dialog.component';

@Component({
  selector: 'app-customer-list',
  templateUrl: './customer-list.component.html',
  styleUrls: ['./customer-list.component.css'],
  animations: [
    trigger('fadeIn', [
      transition(':enter', [
        style({ opacity: 0, transform: 'translateY(10px)' }),
        animate(
          '300ms ease-out',
          style({ opacity: 1, transform: 'translateY(0)' }),
        ),
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
    trigger('slideIn', [
      transition(':enter', [
        style({ transform: 'translateX(-100%)', opacity: 0 }),
        animate(
          '300ms ease-out',
          style({ transform: 'translateX(0)', opacity: 1 }),
        ),
      ]),
      transition(':leave', [
        animate(
          '300ms ease-in',
          style({ transform: 'translateX(-100%)', opacity: 0 }),
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
    MatTooltipModule,
    FormsModule,
    MatCheckbox,
  ],
})
export class CustomerListComponent implements OnInit, OnDestroy {
  customers: Customer[] = [];
  isLoading = false;
  exportingFormat: 'excel' | 'pdf' | null = null;
  currentPage = 1;
  itemsPerPage = 10;
  totalItems = 0;
  totalPages = 0;
  selectedStatus: string | null = 'All';
  searchTerm = '';
  private searchSubject = new Subject<string>();
  private destroy$ = new Subject<void>();
  isAdmin: boolean = false;
  isSuperAdmin: boolean = false;
  isUser: boolean = false;

  // Bulk selection
  selectedCustomers: Set<number> = new Set();
  isBulkSelectMode = false;

  // Advanced filters
  showAdvancedFilters = false;
  advancedFilters = {
    minTotalPurchases: null as number | null,
    maxTotalPurchases: null as number | null,
    country: '',
    customerGroup: '',
    minCreditLimit: null as number | null,
    maxCreditLimit: null as number | null,
    dateFrom: null as Date | null,
    dateTo: null as Date | null,
  };

  customerGroups: string[] = [];
  countries: string[] = [];

  // Column visibility
  visibleColumns: ColumnVisibility = {
    name: true,
    company: true,
    email: true,
    phone: true,
    address: true,
    status: true,
    totalPurchases: true,
    creditLimit: true,
    lastPurchase: true,
    actions: true,
  };

  columnOptions: { key: keyof ColumnVisibility; label: string }[] = [
    { key: 'name', label: 'Name' },
    { key: 'company', label: 'Company' },
    { key: 'email', label: 'Email' },
    { key: 'phone', label: 'Phone' },
    { key: 'address', label: 'Address' },
    { key: 'status', label: 'Status' },
    { key: 'totalPurchases', label: 'Total Purchases' },
    { key: 'creditLimit', label: 'Credit Limit' },
    { key: 'lastPurchase', label: 'Last Purchase' },
    { key: 'actions', label: 'Actions' },
  ];

  stats: CustomerStats = {
    allCount: 0,
    allChange: 0,
    activeCount: 0,
    activeChange: 0,
    inactiveCount: 0,
    inactiveChange: 0,
  };

  // Animation values
  animatedValues = {
    allCount: 0,
    activeCount: 0,
    inactiveCount: 0,
  };

  // Sort properties
  sortField: string = 'name';
  sortDirection: 'asc' | 'desc' = 'asc';
  lastRefreshTime: Date = new Date();

  constructor(
    private customerService: CustomerService,
    private authService: AuthService,
    private dialog: MatDialog,
    private router: Router,
  ) {}

  ngOnInit(): void {
    this.isAdmin = this.authService.hasRole('Admin');
    this.isSuperAdmin = this.authService.hasRole('Super Admin'); // Add this
    this.isUser = this.authService.hasRole('User');
    this.setupSearch();
    this.loadCustomers();
    this.loadStats();
    this.loadFilterOptions();
    this.loadColumnPreferences();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private setupSearch(): void {
    this.searchSubject
      .pipe(debounceTime(300), distinctUntilChanged(), takeUntil(this.destroy$))
      .subscribe((searchTerm) => {
        this.searchTerm = searchTerm;
        this.currentPage = 1;
        this.loadCustomers();
      });
  }

  loadFilterOptions(): void {
    this.customerService
      .getCustomerGroups()
      .subscribe((groups) => (this.customerGroups = groups));
    this.customerService
      .getCustomerCountries()
      .subscribe((countries) => (this.countries = countries));
  }

  loadColumnPreferences(): void {
    const saved = localStorage.getItem('customerColumnVisibility');
    if (saved) {
      this.visibleColumns = JSON.parse(saved);
    }
  }

  saveColumnPreferences(): void {
    localStorage.setItem(
      'customerColumnVisibility',
      JSON.stringify(this.visibleColumns),
    );
  }

  toggleColumn(column: keyof ColumnVisibility): void {
    this.visibleColumns[column] = !this.visibleColumns[column];
    this.saveColumnPreferences();
  }

  loadCustomers(): void {
    this.isLoading = true;
    const filter: CustomerFilterRequest = {
      pageNumber: this.currentPage,
      pageSize: this.itemsPerPage,
      search: this.searchTerm,
      status: this.selectedStatus,
      ...this.advancedFilters,
    };
    this.customerService.getCustomers(filter).subscribe({
      next: (result: PaginatedResult<Customer>) => {
        this.customers = result.items;
        this.totalItems = result.totalCount;
        this.totalPages = Math.ceil(result.totalCount / this.itemsPerPage);
        this.sortCustomers();
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error fetching customers:', error);
        this.openDialog(
          'error',
          'Load Failed',
          'Failed to load customers. Please try again.',
          'Customer data could not be retrieved from the server.',
        );
        this.isLoading = false;
      },
    });
  }

  loadStats(): void {
    this.customerService.getCustomerStats().subscribe({
      next: (stats: CustomerStats) => {
        this.stats = stats;
        this.startValueAnimation();
      },
      error: (error) => {
        console.error('Error fetching customer stats:', error);
        this.openDialog(
          'error',
          'Stats Load Failed',
          'Failed to load customer statistics.',
          'Customer statistics could not be retrieved from the server.',
        );
      },
    });
  }

  startValueAnimation(): void {
    const duration = 1000;
    const steps = 60;
    const stepDuration = duration / steps;
    let currentStep = 0;

    const targets = {
      allCount: this.stats.allCount,
      activeCount: this.stats.activeCount,
      inactiveCount: this.stats.inactiveCount,
    };

    const startValues = { ...this.animatedValues };

    const interval = setInterval(() => {
      currentStep++;
      const progress = Math.min(1, currentStep / steps);

      Object.keys(targets).forEach((key) => {
        const start = startValues[key as keyof typeof startValues];
        const end = targets[key as keyof typeof targets];
        this.animatedValues[key as keyof typeof this.animatedValues] =
          Math.round(start + (end - start) * progress);
      });

      if (progress >= 1) {
        clearInterval(interval);
      }
    }, stepDuration);
  }

  animateValue(key: string): number {
    return this.animatedValues[key as keyof typeof this.animatedValues] || 0;
  }

  getActivePercentage(): number {
    if (this.stats.allCount === 0) return 0;
    return Math.round((this.stats.activeCount / this.stats.allCount) * 100);
  }

  getInactivePercentage(): number {
    if (this.stats.allCount === 0) return 0;
    return Math.round((this.stats.inactiveCount / this.stats.allCount) * 100);
  }

  getTotalLifetimeValue(): number {
    return this.customers.reduce((sum, c) => sum + c.totalPurchases, 0);
  }

  getAverageOrderValue(): number {
    if (this.customers.length === 0) return 0;
    return this.getTotalLifetimeValue() / this.customers.length;
  }

  getCustomerHealth(): number {
    if (this.stats.allCount === 0) return 100;
    return Math.round((this.stats.activeCount / this.stats.allCount) * 100);
  }

  getAveragePaymentDays(): number {
    const customersWithData = this.customers.filter(
      (c) => c.averagePaymentDays,
    );
    if (customersWithData.length === 0) return 0;
    const avg =
      customersWithData.reduce(
        (sum, c) => sum + (c.averagePaymentDays || 0),
        0,
      ) / customersWithData.length;
    return Math.round(avg);
  }

  getFullAddress(customer: Customer): string {
    const parts = [customer.addressLine1];
    if (customer.addressLine2) parts.push(customer.addressLine2);
    parts.push(customer.city);
    if (customer.state) parts.push(customer.state);
    parts.push(customer.zipCode);
    parts.push(customer.countryName);
    return parts.join(', ');
  }

  sortCustomers(): void {
    this.customers.sort((a, b) => {
      let aVal = a[this.sortField as keyof Customer];
      let bVal = b[this.sortField as keyof Customer];

      if (typeof aVal === 'number' && typeof bVal === 'number') {
        return this.sortDirection === 'asc' ? aVal - bVal : bVal - aVal;
      }

      const aStr = String(aVal || '').toLowerCase();
      const bStr = String(bVal || '').toLowerCase();

      if (this.sortDirection === 'asc') {
        return aStr.localeCompare(bStr);
      } else {
        return bStr.localeCompare(aStr);
      }
    });
  }

  sortBy(field: string): void {
    if (this.sortField === field) {
      this.sortDirection = this.sortDirection === 'asc' ? 'desc' : 'asc';
    } else {
      this.sortField = field;
      this.sortDirection = 'asc';
    }
    this.sortCustomers();
  }

  getInitials(name: string): string {
    return name
      .split(' ')
      .map((n) => n.charAt(0).toUpperCase())
      .join('')
      .substring(0, 2);
  }

  getAvatarColor(name: string): string {
    const colors = [
      '#FF2E63',
      '#00D4B9',
      '#FF6B6B',
      '#FFD93D',
      '#1E90FF',
      '#8A2BE2',
      '#4B0082',
    ];
    const index =
      name.split('').reduce((sum, char) => sum + char.charCodeAt(0), 0) %
      colors.length;
    return colors[index];
  }

  onSearch(searchTerm: string): void {
    this.searchSubject.next(searchTerm);
  }

  onSelectStatus(status: string): void {
    this.selectedStatus = status;
    this.currentPage = 1;
    this.loadCustomers();
  }

  clearFilters(): void {
    this.searchTerm = '';
    this.selectedStatus = 'All';
    this.advancedFilters = {
      minTotalPurchases: null,
      maxTotalPurchases: null,
      country: '',
      customerGroup: '',
      minCreditLimit: null,
      maxCreditLimit: null,
      dateFrom: null,
      dateTo: null,
    };
    this.currentPage = 1;
    this.loadCustomers();
  }

  applyAdvancedFilters(): void {
    this.currentPage = 1;
    this.loadCustomers();
    this.showAdvancedFilters = false;
  }

  resetAdvancedFilters(): void {
    this.advancedFilters = {
      minTotalPurchases: null,
      maxTotalPurchases: null,
      country: '',
      customerGroup: '',
      minCreditLimit: null,
      maxCreditLimit: null,
      dateFrom: null,
      dateTo: null,
    };
    this.loadCustomers();
  }

  refreshData(): void {
    this.lastRefreshTime = new Date();
    this.loadCustomers();
    this.loadStats();
    this.openDialog(
      'success',
      'Data Refreshed',
      'Customer data has been refreshed.',
      `Last updated: ${this.lastRefreshTime.toLocaleTimeString()}`,
    );
  }

  toggleSelectAll(): void {
    if (
      this.selectedCustomers.size === this.customers.length &&
      this.customers.length > 0
    ) {
      // If all are selected, clear all
      this.selectedCustomers.clear();
    } else {
      // Otherwise, select all
      this.customers.forEach((customer) => {
        this.selectedCustomers.add(customer.id);
      });
    }
    // Force change detection if needed
    this.selectedCustomers = new Set(this.selectedCustomers);
  }

  // Add these methods to your CustomerListComponent class

  isAllSelected(): boolean {
    return (
      this.customers.length > 0 &&
      this.selectedCustomers.size === this.customers.length
    );
  }

  isIndeterminate(): boolean {
    return (
      this.selectedCustomers.size > 0 &&
      this.selectedCustomers.size < this.customers.length
    );
  }

  toggleSelectCustomer(id: number): void {
    if (this.selectedCustomers.has(id)) {
      this.selectedCustomers.delete(id);
    } else {
      this.selectedCustomers.add(id);
    }
  }

  bulkDelete(): void {
    const dialogRef = this.dialog.open(DeleteConfirmationDialogComponent, {
      width: '450px',
      data: {
        title: 'Bulk Delete Customers',
        message: `Are you sure you want to delete ${this.selectedCustomers.size} customer(s)? This action cannot be undone.`,
        confirmText: 'Delete All',
        cancelText: 'Cancel',
      },
    });

    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        this.isLoading = true;
        const deletePromises = Array.from(this.selectedCustomers).map((id) =>
          this.customerService.deleteCustomer(id).toPromise(),
        );

        Promise.all(deletePromises)
          .then(() => {
            this.loadCustomers();
            this.loadStats();
            this.selectedCustomers.clear();
            this.openDialog(
              'success',
              'Bulk Delete Complete',
              `${this.selectedCustomers.size} customer(s) deleted successfully.`,
              '',
            );
            this.isLoading = false;
          })
          .catch((error) => {
            this.openDialog(
              'error',
              'Bulk Delete Failed',
              'Some customers could not be deleted.',
              error.message,
            );
            this.isLoading = false;
          });
      }
    });
  }

  bulkExportSelected(): void {
    const selectedIds = Array.from(this.selectedCustomers);
    this.exportingFormat = 'excel';
    this.customerService.exportSelectedCustomers(selectedIds).subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = `selected_customers_${new Date().toISOString()}.xlsx`;
        link.click();
        window.URL.revokeObjectURL(url);
        this.openDialog(
          'success',
          'Export Successful',
          'Selected customers exported successfully.',
          '',
        );
        this.exportingFormat = null;
      },
      error: (error) => {
        this.openDialog(
          'error',
          'Export Failed',
          'Failed to export selected customers.',
          error.message,
        );
        this.exportingFormat = null;
      },
    });
  }

  exportCustomers(format: string): void {
    this.exportingFormat = format as 'excel' | 'pdf';
    const filter: CustomerFilterRequest = {
      pageNumber: this.currentPage,
      pageSize: this.itemsPerPage,
      search: this.searchTerm,
      status: this.selectedStatus,
      ...this.advancedFilters,
    };

    this.customerService.exportCustomers(filter, format).subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = `customers_${new Date().toISOString()}.${format === 'excel' ? 'xlsx' : 'pdf'}`;
        link.click();
        window.URL.revokeObjectURL(url);
        this.openDialog(
          'success',
          'Export Successful',
          `Customers exported successfully as ${format.toUpperCase()}.`,
          '',
        );
        this.exportingFormat = null;
      },
      error: (error) => {
        this.openDialog(
          'error',
          'Export Failed',
          'Failed to export customers.',
          error.message,
        );
        this.exportingFormat = null;
      },
    });
  }

  printCustomerList(): void {
    const printWindow = window.open('', '_blank');
    printWindow?.document.write(`
      <html>
        <head>
          <title>Customer List</title>
          <style>
            body { font-family: Arial, sans-serif; margin: 40px; }
            table { width: 100%; border-collapse: collapse; margin-top: 20px; }
            th, td { border: 1px solid #ddd; padding: 8px; text-align: left; }
            th { background-color: #f2f2f2; }
            h1 { color: #333; }
            .header { text-align: center; margin-bottom: 30px; }
            .date { text-align: right; color: #666; margin-bottom: 20px; }
            @media print {
              button { display: none; }
            }
          </style>
        </head>
        <body>
          <div class="header">
            <h1>Customer List</h1>
            <p>Generated on ${new Date().toLocaleString()}</p>
          </div>
          <div class="date"></div>
          <table>
            <thead>
              <tr>
                <th>Name</th>
                <th>Email</th>
                <th>Phone</th>
                <th>Total Purchases</th>
                <th>Status</th>
              </tr>
            </thead>
            <tbody>
              ${this.customers
                .map(
                  (c) => `
                <tr>
                  <td>${c.name}</td>
                  <td>${c.email}</td>
                  <td>${c.phoneNumber}</td>
                  <td>$${c.totalPurchases.toFixed(2)}</td>
                  <td>${c.status}</td>
                </tr>
              `,
                )
                .join('')}
            </tbody>
          </table>
        </body>
      </html>
    `);
    printWindow?.document.close();
    printWindow?.print();
  }

  goToPage(page: number): void {
    if (page >= 1 && page <= this.totalPages) {
      this.currentPage = page;
      this.loadCustomers();
    }
  }

  nextPage(): void {
    if (this.currentPage < this.totalPages) {
      this.currentPage++;
      this.loadCustomers();
    }
  }

  previousPage(): void {
    if (this.currentPage > 1) {
      this.currentPage--;
      this.loadCustomers();
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

  trackByCustomer(index: number, customer: Customer): number {
    return customer.id;
  }

  onCreateCustomer(): void {
    const dialogRef = this.dialog.open(CustomerDialogComponent, {
      width: '650px',
      disableClose: true,
      data: { mode: 'create' },
    });

    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        this.loadCustomers();
        this.loadStats();
        this.openDialog(
          'success',
          'Customer Created',
          `Customer ${result.name} has been created successfully!`,
          'The customer has been added to the system.',
        );
      }
    });
  }

  onEditCustomer(customer: Customer): void {
    const dialogRef = this.dialog.open(CustomerDialogComponent, {
      width: '650px',
      disableClose: true,
      data: {
        mode: 'edit',
        customer: customer,
      },
    });

    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        this.loadCustomers();
        this.loadStats();
        this.openDialog(
          'success',
          'Customer Updated',
          `Customer ${result.name} has been updated successfully!`,
          'The customer details have been updated.',
        );
      }
    });
  }

  onDeleteCustomer(customer: Customer): void {
    const dialogRef = this.dialog.open(DeleteConfirmationDialogComponent, {
      width: '450px',
      disableClose: true,
      panelClass: 'delete-dialog-panel',
      data: {
        title: 'Delete Customer',
        message: `Are you sure you want to delete customer ${customer.name}? This action cannot be undone.`,
        itemName: customer.name,
        confirmText: 'Delete',
        cancelText: 'Cancel',
      },
    });

    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        this.isLoading = true;
        this.customerService.deleteCustomer(customer.id).subscribe({
          next: () => {
            this.loadCustomers();
            this.loadStats();
            this.openDialog(
              'success',
              'Customer Deleted',
              `Customer ${customer.name} has been deleted successfully!`,
              'The customer has been removed from the system.',
            );
            this.isLoading = false;
          },
          error: (error) => {
            console.error('Error deleting customer:', error);
            this.openDialog(
              'error',
              'Delete Failed',
              'Failed to delete customer.',
              error.message || 'Please try again.',
            );
            this.isLoading = false;
          },
        });
      }
    });
  }

  viewCustomerDetails(customer: Customer): void {
    this.router.navigate([`/customers/view/${customer.id}`]);
  }

  sendEmailToCustomer(customer: Customer): void {
    window.location.href = `mailto:${customer.email}`;
  }

  viewCustomerInvoices(customer: Customer): void {
    this.router.navigate(['/invoices'], {
      queryParams: { customerId: customer.id },
    });
  }

  onExport(format: 'excel' | 'pdf'): void {
    this.exportingFormat = format;
    const filter: CustomerFilterRequest = {
      pageNumber: this.currentPage,
      pageSize: this.itemsPerPage,
      search: this.searchTerm,
      status: this.selectedStatus,
    };

    if (format === 'excel') {
      this.customerService.exportCustomersExcel(filter).subscribe({
        next: (blob) => {
          const url = window.URL.createObjectURL(blob);
          const link = document.createElement('a');
          link.href = url;
          link.download = `customers_${new Date().toISOString()}.xlsx`;
          link.click();
          window.URL.revokeObjectURL(url);
          this.openDialog(
            'success',
            'Export Successful',
            'Customers exported successfully!',
            '',
          );
          this.exportingFormat = null;
        },
        error: (error) => {
          console.error('Error exporting Excel:', error);
          this.openDialog(
            'error',
            'Export Failed',
            'Failed to export customers.',
            '',
          );
          this.exportingFormat = null;
        },
      });
    } else if (format === 'pdf') {
      this.customerService.exportCustomersPdf(filter).subscribe({
        next: (blob) => {
          const url = window.URL.createObjectURL(blob);
          const link = document.createElement('a');
          link.href = url;
          link.download = `customers_${new Date().toISOString()}.pdf`;
          link.click();
          window.URL.revokeObjectURL(url);
          this.openDialog(
            'success',
            'Export Successful',
            'Customers exported successfully!',
            '',
          );
          this.exportingFormat = null;
        },
        error: (error) => {
          console.error('Error exporting PDF:', error);
          this.openDialog(
            'error',
            'Export Failed',
            'Failed to export customers.',
            '',
          );
          this.exportingFormat = null;
        },
      });
    }
  }

  onImport(): void {
    // Placeholder for import logic - can be implemented later
    this.openDialog(
      'info',
      'Coming Soon',
      'Import feature is under development.',
      '',
    );
  }

  onMoreFilters(): void {
    // Placeholder for more filters - can be implemented later
    this.openDialog(
      'info',
      'Coming Soon',
      'Advanced filters are under development.',
      '',
    );
  }

  quickActions(customer: Customer): void {
    const dialogRef = this.dialog.open(QuickActionsDialogComponent, {
      width: '500px',
      data: { customer },
    });
    dialogRef.afterClosed().subscribe((result) => {
      if (result?.action === 'createInvoice') {
        this.router.navigate(['/invoices/create'], {
          queryParams: { customerId: customer.id },
        });
      } else if (result?.action === 'sendEmail') {
        this.sendEmailToCustomer(customer);
      } else if (result?.action === 'viewStatement') {
        this.customerService
          .generateStatement(customer.id)
          .subscribe((blob) => {
            const url = window.URL.createObjectURL(blob);
            window.open(url);
          });
      } else if (result?.action === 'addNote') {
        const noteDialog = this.dialog.open(AddNoteDialogComponent, {
          width: '500px',
          data: { customerId: customer.id },
        });
        noteDialog.afterClosed().subscribe((noteResult) => {
          if (noteResult) {
            this.openDialog(
              'success',
              'Note Added',
              'Note has been added successfully.',
              '',
            );
          }
        });
      } else if (result?.action === 'scheduleFollowUp') {
        const followUpDialog = this.dialog.open(
          ScheduleFollowUpDialogComponent,
          {
            width: '500px',
            data: { customerId: customer.id, customerName: customer.name },
          },
        );
      }
    });
  }

  openDialog(
    type: 'success' | 'error' | 'info',
    title: string,
    message: string,
    submessage: string,
  ): void {
    this.dialog.open(NotificationDialogComponent, {
      width: '400px',
      data: { type, title, message, submessage },
    });
  }
}
