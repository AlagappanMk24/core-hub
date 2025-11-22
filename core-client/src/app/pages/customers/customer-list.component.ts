import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatInputModule } from '@angular/material/input';
import { MatMenuModule } from '@angular/material/menu';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { FormsModule } from '@angular/forms';
import { Subject } from 'rxjs';
import { debounceTime, distinctUntilChanged, takeUntil } from 'rxjs/operators';
import { animate, style, transition, trigger } from '@angular/animations';
import { AuthService } from '../../services/auth/auth.service';
import { CustomerDialogComponent } from '../../features/invoices/components/customer-dialog/customer-dialog.component';
import { DeleteConfirmationDialogComponent } from '../../features/common/delete-confirmation/delete-confirmation-dialog.component';
import { NotificationDialogComponent } from '../../components/notification/notification-dialog.component';
import { Customer, CustomerFilterRequest, CustomerStats, PaginatedResult } from '../../services/customer/models/customer.model';
import { CustomerService } from '../../services/customer/customer.service';

@Component({
  selector: 'app-customer-list',
  templateUrl: './customer-list.component.html',
  styleUrls: ['./customer-list.component.css'],
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
  isUser: boolean = false;
  stats: CustomerStats = {
    allCount: 0,
    allChange: 0,
    activeCount: 0,
    activeChange: 0,
    inactiveCount: 0,
    inactiveChange: 0,
  };

  constructor(
    private customerService: CustomerService,
    private authService: AuthService,
    private dialog: MatDialog
  ) {}

  ngOnInit(): void {
    this.isAdmin = this.authService.hasRole('Admin');
    this.isUser = this.authService.hasRole('User');
    this.setupSearch();
    this.loadCustomers();
    this.loadStats();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private setupSearch(): void {
    this.searchSubject
      .pipe(
        debounceTime(300),
        distinctUntilChanged(),
        takeUntil(this.destroy$)
      )
      .subscribe((searchTerm) => {
        this.searchTerm = searchTerm;
        this.currentPage = 1;
        this.loadCustomers();
      });
  }

  loadCustomers(): void {
    this.isLoading = true;
    const filter: CustomerFilterRequest = {
      pageNumber: this.currentPage,
      pageSize: this.itemsPerPage,
      search: this.searchTerm,
      status: this.selectedStatus
    };
    this.customerService.getCustomers(filter).subscribe({
      next: (result: PaginatedResult<Customer>) => {
        this.customers = result.items;
        this.totalItems = result.totalCount;
        this.totalPages = Math.ceil(result.totalCount / this.itemsPerPage);
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

  loadStats(): void {
    this.isLoading = true;
    this.customerService.getCustomerStats().subscribe({
      next: (stats: CustomerStats) => {
        this.stats = stats;
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error fetching customer stats:', error);
        this.openDialog(
          'error',
          'Stats Load Failed',
          'Failed to load customer statistics. Please try again.',
          'Customer statistics could not be retrieved from the server.'
        );
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
      name
        .split('')
        .reduce((sum, char) => sum + char.charCodeAt(0), 0) % colors.length;
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

  trackByCustomer(index: number, customer: Customer): number {
    return customer.id;
  }

  onCreateCustomer(): void {
    const dialogRef = this.dialog.open(CustomerDialogComponent, {
      width: '600px',
      disableClose: true,
      data: { mode: 'create' },
    });

    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        this.loadCustomers();
        this.loadStats();
        this.openDialog(
          'success',
          'Customer Created Successfully',
          `Customer ${result.name} has been created successfully!`,
          'The customer has been added to the system.'
        );
      }
    });
  }

  onEditCustomer(customer: Customer): void {
    const dialogRef = this.dialog.open(CustomerDialogComponent, {
      width: '600px',
      disableClose: true,
      data: {
        mode: 'edit',
        customer: {
          id: customer.id,
          name: customer.name,
          email: customer.email,
          phoneNumber: customer.phoneNumber,
          address1: customer.address.address1,
          address2: customer.address.address2 || '',
          city: customer.address.city,
          state: customer.address.state || '',
          country: customer.address.country,
          zipCode: customer.address.zipCode,
        },
      },
    });

    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        this.loadCustomers();
        this.loadStats();
        this.openDialog(
          'success',
          'Customer Updated Successfully',
          `Customer ${result.name} has been updated successfully!`,
          'The customer details have been updated in the system.'
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
              'Customer Deleted Successfully',
              `Customer ${customer.name} has been deleted successfully!`,
              'The customer has been removed from the system.'
            );
            this.isLoading = false;
          },
          error: (error) => {
            console.error('Error deleting customer:', error);
            this.openDialog(
              'error',
              'Delete Failed',
              'Failed to delete customer. Please try again.',
              'The customer could not be deleted due to a system error. Please try again or contact support.'
            );
            this.isLoading = false;
          },
        });
      }
    });
  }

  onExport(format: 'excel' | 'pdf'): void {
    this.exportingFormat = format;
    if (format === 'excel') {
      this.customerService.exportCustomersExcel(this.searchTerm).subscribe({
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
            'Customers exported successfully as Excel!',
            'The Excel file containing your customer data has been downloaded.'
          );
          this.exportingFormat = null;
        },
        error: (error) => {
          console.error('Error exporting Excel:', error);
          this.openDialog(
            'error',
            'Export Failed',
            'Failed to export Excel. Please try again.',
            'The Excel export could not be completed due to a system error.'
          );
          this.exportingFormat = null;
        },
      });
    } else if (format === 'pdf') {
      this.customerService.exportCustomersPdf(this.searchTerm).subscribe({
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
            'Customers exported successfully as PDF!',
            'The PDF file containing your customer data has been downloaded.'
          );
          this.exportingFormat = null;
        },
        error: (error) => {
          console.error('Error exporting PDF:', error);
          this.openDialog(
            'error',
            'Export Failed',
            'Failed to export PDF. Please try again.',
            'The PDF export could not be completed due to a system error.'
          );
          this.exportingFormat = null;
        },
      });
    }
  }

  onImport(): void {
    // Placeholder for import logic
  }

  onMoreFilters(): void {
    // Placeholder for filter logic
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