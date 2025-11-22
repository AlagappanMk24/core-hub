import { CommonModule } from '@angular/common';
import { Component, ElementRef, OnInit, ViewChild } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSelectModule } from '@angular/material/select';
import { MatInputModule } from '@angular/material/input';
import { debounceTime, Observable, Subject } from 'rxjs';
import { ActivatedRoute, Router } from '@angular/router';
import {
  Address,
  Attachment,
  Invoice,
  InvoiceUpsert,
  TaxType,
} from '../../../../interfaces/invoice/invoice.interface';
import { CustomerService } from '../../../../services/customer/customer.service';
import { InvoiceService } from '../../../../services/invoice/invoice.service';
import { NotificationDialogComponent } from '../../../../components/notification/notification-dialog.component';
import { Company } from '../../../../interfaces/company/company.interface';
import { CompanyService } from '../../../../services/company/company.service';
import { AuthService } from '../../../../services/auth/auth.service';
import { InvoiceDisplayComponent } from '../invoice-display/invoice-display/invoice-display.component';
import { CustomerDialogComponent } from '../customer-dialog/customer-dialog.component';
import { CustomerFilterRequest } from '../../../../services/customer/models/customer.model';
import { environment } from '../../../../environments/environment.development';
interface Customer {
  id: number;
  name: string;
  address: string;
  initials: string;
  color: string;
}
interface ValidationErrors {
  [key: string]: string; // e.g., { 'invoiceNumber': 'Invoice number is required', 'items[0].description': 'Description is required' }
}
interface InvoiceData {
  id?: number;
  invoiceNumber: string;
  poNumber: string;
  projectDetail: string;
  items: {
    description: string;
    quantity: number;
    unitPrice: number;
    taxType: string;
    taxAmount: number;
    amount: number;
  }[];
  taxDetails: { taxType: string; rate: number; amount: number }[];
  discounts: { description: string; amount: number; isPercentage: boolean }[];
  paymentMethod: string;
  issuedDate: string;
  dueDate: string;
  notes: string;
  customerId: number;
  currency: string;
  isAutomated: boolean;
  invoiceStatus?: string;
  paymentStatus?: string;
  invoiceAttachments: Attachment[];
}

@Component({
  selector: 'app-create-invoice',
  templateUrl: './create-invoice.component.html',
  styleUrls: ['./create-invoice.component.css'],
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatDialogModule,
    MatButtonModule,
    MatIconModule,
    MatSelectModule,
    MatInputModule,
  ],
})
export class CreateInvoiceComponent implements OnInit {
  @ViewChild('fileInput') fileInput!: ElementRef<HTMLInputElement>;
  invoiceData: InvoiceData = {
    invoiceNumber: '',
    poNumber: '',
    projectDetail: '',
    items: [],
    taxDetails: [],
    discounts: [],
    paymentMethod: '',
    issuedDate: '',
    dueDate: '',
    notes: '',
    customerId: 0,
    currency: 'USD',
    isAutomated: false,
    invoiceAttachments: [],
  };
  errors: ValidationErrors = {}; // Track validation errors
  company: Company | null = null;
  customers: Customer[] = [];
  filteredCustomers: Customer[] = [];
  selectedCustomer: Customer | null = null;
  showCustomerDropdown: boolean = false;
  customerSearch: string = '';
  private searchSubject: Subject<string> = new Subject();
  private dueDateSubject: Subject<string> = new Subject();
  private itemChangeSubject: Subject<{
    index: number;
    type: 'quantity' | 'cost' | 'description';
  }> = new Subject();
  showCalendar: boolean = false;
  calendarType: 'issued' | 'due' = 'issued';
  currentMonth: number = 0;
  currentYear: number = 2025;
  selectedDay: number = 0;
  taxTypes: TaxType[] = [];
  newTaxType: { name: string; rate: number } = { name: '', rate: 0 };
  months: string[] = [
    'January',
    'February',
    'March',
    'April',
    'May',
    'June',
    'July',
    'August',
    'September',
    'October',
    'November',
    'December',
  ];
  weekdays: string[] = ['SUN', 'MON', 'TUE', 'WED', 'THU', 'FRI', 'SAT'];
  years: number[] = [2023, 2024, 2025, 2026];
  showInvoiceTypeDropdown: boolean = false;
  showCurrencyDropdown: boolean = false;
  showPaymentMethodDropdown: boolean = false;
  paymentMethods = [
    { value: 'paypal', display: 'PayPal', icon: 'payment' },
    { value: 'bank', display: 'Bank Transfer', icon: 'account_balance' },
    { value: 'cash', display: 'Cash', icon: 'money' },
    { value: 'credit_card', display: 'Credit Card', icon: 'credit_card' },
    { value: 'upi', display: 'UPI', icon: 'upi' },
  ];
  isEditMode: boolean = false; // Track edit mode
  invoiceId: string | null = null; // Store invoice ID for edit mode
  private blobUrls: string[] = []; // Track temporary blob URLs for cleanup
  isDragging: boolean = false; // Track drag state

  constructor(
    private customerService: CustomerService,
    private invoiceService: InvoiceService,
    private companyService: CompanyService,
    private authService: AuthService,
    private dialog: MatDialog,
    private router: Router,
    private route: ActivatedRoute
  ) {}

  ngOnInit(): void {
    const today = new Date();
    this.currentMonth = today.getMonth();
    this.currentYear = today.getFullYear();

    // Set default dates
    this.invoiceData.issuedDate = this.formatDate(today);
    this.invoiceData.dueDate = this.formatDate(
      new Date(today.setDate(today.getDate() + 30))
    );

    // Load company data first
    this.loadCompanyData().subscribe({
      next: () => {
        // Then load tax types, customers, and invoice data
        this.loadTaxTypes().subscribe(() => {
          this.loadCustomers();
          this.addItem();
          this.setupSearchDebounce();
          this.setupDueDateDebounce();
          this.setupItemChangeDebounce();
          // Check if in edit mode
          this.route.paramMap.subscribe((params) => {
            const id = params.get('id');
            if (id) {
              this.isEditMode = true;
              this.invoiceId = id;
              this.loadInvoiceData(Number(id));
            } else {
              this.isEditMode = false; // Explicitly set to false for new invoices
            }
          });
        });
      },
      error: (err) => {
        console.error('Error loading company data:', err);
        this.openDialog(
          'error',
          'Error',
          'Failed to load company data.',
          'Unable to retrieve company information. A default company profile will be used.'
        );
        // Set default company data as fallback
        this.company = {
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
        };
      },
    });
  }

  // Fetch company data based on authenticated user
  loadCompanyData(): Observable<void> {
    return new Observable((observer) => {
      const userDetails = this.authService.getUserDetail();
      if (!userDetails || !userDetails.companyId) {
        console.error('No user details or company ID found');
        observer.error(
          new Error('User not authenticated or no company ID found')
        );
        return;
      }

      // Assuming the user's company ID is stored in AuthService or can be fetched
      this.authService.getUserCompanyId().subscribe({
        next: (companyId) => {
          if (companyId) {
            this.companyService
              .getCompanyById(userDetails.companyId)
              .subscribe({
                next: (company) => {
                  this.company = company;
                  observer.next();
                  observer.complete();
                },
                error: (err) => {
                  console.error('Error fetching company:', err);
                  observer.error(err);
                },
              });
          } else {
            console.warn('No company ID associated with user');
            observer.error(new Error('No company ID found for user'));
          }
        },
        error: (err) => {
          console.error('Error fetching user company ID:', err);
          observer.error(err);
        },
      });
    });
  }

  // Get company initials for logo
  getCompanyInitials(): string {
    if (!this.company || !this.company.name) {
      return 'AC';
    }
    return this.company.name
      .split(' ')
      .map((n) => n[0])
      .join('')
      .substring(0, 2)
      .toUpperCase();
  }

  // Get company address string
  getCompanyAddress(): string {
    if (!this.company || !this.company.address) {
      return 'Besant Nagar, Chennai 45';
    }
    const { address1, address2, city, state, zipCode, country } =
      this.company.address;
    return [address1, address2, city, state, zipCode, country]
      .filter((part) => part)
      .join(', ');
  }

  ngOnDestroy(): void {
    // Revoke all temporary blob URLs when component is destroyed
    this.blobUrls.forEach((url) => URL.revokeObjectURL(url));
    this.blobUrls = [];
  }

  loadInvoiceData(id: number): void {
    this.invoiceService.getInvoiceById(id).subscribe({
      next: (invoice) => {
        // Format dates to dd/mm/yyyy
        const formatDate = (date: Date | string): string => {
          const d = new Date(date);
          if (isNaN(d.getTime())) {
            return '';
          }
          return this.formatDate(d);
        };
        // Map invoice data to component's invoiceData
        this.invoiceData = {
          id: invoice.id,
          invoiceNumber: invoice.invoiceNumber,
          poNumber: invoice.poNumber || '',
          projectDetail: invoice.projectDetail || '',
          items: invoice.items.map((item) => {
            // Map taxType to match taxTypes.name (e.g., "GST" -> "GST 10%")
            const taxTypeEntry = this.taxTypes.find(
              (t) =>
                t.name?.toLowerCase().startsWith(item.taxType?.toLowerCase()) ||
                item.taxType === ''
            );
            return {
              description: item.description,
              quantity: item.quantity,
              unitPrice: item.unitPrice,
              taxType: taxTypeEntry ? taxTypeEntry.name : '',
              taxAmount: item.taxAmount,
              amount: item.amount,
            };
          }),
          taxDetails: invoice.taxDetails.map((tax) => ({
            taxType: tax.taxType,
            rate: tax.rate,
            amount: tax.amount,
          })),
          discounts: invoice.discounts.map((discount) => ({
            description: discount.description,
            amount: discount.amount,
            isPercentage: discount.isPercentage,
          })),
          paymentMethod: invoice.paymentMethod || '',
          issuedDate: formatDate(invoice.issueDate),
          dueDate: formatDate(invoice.dueDate),
          notes: invoice.notes || '',
          customerId: invoice.customerId,
          currency: invoice.currency || 'USD',
          isAutomated: invoice.isAutomated || false,
          invoiceStatus: invoice.invoiceStatus,
          paymentStatus: invoice.paymentStatus,
          invoiceAttachments: invoice.invoiceAttachments
            ? invoice.invoiceAttachments.map((attachment: Attachment) => ({
                id: attachment.id,
                fileName: attachment.fileName,
                fileUrl: `${environment.apiBaseUrl}/${this.company?.id}/${
                  invoice.id
                }/${encodeURIComponent(attachment.fileName)}`,
              }))
            : [],
        };
        // Set selected customer
        const filter: CustomerFilterRequest = {
          pageNumber: 1,
          pageSize: 20,
          search: '',
          status: 'Active', // Only fetch active customers for invoice
        };
        // Set selected customer
        this.customerService.getCustomers(filter).subscribe({
          next: (response) => {
            this.customers = response.items.map((c) => ({
              id: c.id,
              name: c.name,
              address: `${c.address.address1}, ${c.address.city}, ${c.address.country}`,
              initials: c.name
                .split(' ')
                .map((n) => n[0])
                .join('')
                .substring(0, 2)
                .toUpperCase(),
              color: this.getRandomColor(),
            }));
            this.filteredCustomers = this.customers;
            this.selectedCustomer =
              this.customers.find(
                (c) => c.id === this.invoiceData.customerId
              ) || null;
          },
          error: (err) => {
            console.error('Error fetching customers:', err);
            this.openDialog(
              'error',
              'Error',
              'Failed to load customers. Please try again.',
              'Unable to retrieve customer data. Please refresh the page or check your network connection.'
            );
          },
        });

        // Update tax details and totals
        this.updateTaxDetails();
      },
      error: (err) => {
        console.error('Error fetching invoice:', err);
        this.openDialog(
          'error',
          'Error',
          'Failed to load invoice data. Please try again.',
          'The requested invoice could not be found or loaded. Please verify the invoice ID and try again.'
        );
      },
    });
  }

  setupSearchDebounce(): void {
    this.searchSubject.pipe(debounceTime(300)).subscribe((search) => {
      this.loadCustomers(search);
    });
  }

  setupDueDateDebounce(): void {
    this.dueDateSubject.pipe(debounceTime(300)).subscribe((dueDate) => {
      this.validateDueDate(dueDate);
    });
  }
  setupItemChangeDebounce(): void {
    this.itemChangeSubject
      .pipe(debounceTime(300))
      .subscribe(({ index, type }) => {
        this.updateTaxDetails(index);
      });
  }

  loadCustomers(search: string = ''): void {
    const filter: CustomerFilterRequest = {
      pageNumber: 1,
      pageSize: 20,
      search,
      status: 'Active', // Only fetch active customers for invoice creation
    };
    this.customerService.getCustomers(filter).subscribe({
      next: (response) => {
        this.customers = response.items.map((c) => ({
          id: c.id,
          name: c.name,
          address: `${c.address.address1}, ${c.address.city}, ${c.address.country}`,
          initials: c.name
            .split(' ')
            .map((n) => n[0])
            .join('')
            .substring(0, 2)
            .toUpperCase(),
          color: this.getRandomColor(),
        }));
        this.filteredCustomers = this.customers;
      },
      error: (err) => {
        console.error('Error fetching customers:', err);
        this.openDialog(
          'error',
          'Error',
          'Failed to load customers.',
          'Customer data could not be retrieved. Please try refreshing the page or check your internet connection.'
        );
      },
    });
  }

  searchCustomers(): void {
    this.searchSubject.next(this.customerSearch);
  }

  openCreateCustomerDialog(): void {
    const dialogRef = this.dialog.open(CustomerDialogComponent, {
      width: '500px',
      data: {},
    });

    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        const newCustomer: Customer = {
          id: result.id,
          name: result.name,
          address: `${result.address1}, ${result.city}, ${result.country}`,
          initials: result.name
            .split(' ')
            .map((n: string) => n[0])
            .join('')
            .substring(0, 2)
            .toUpperCase(),
          color: this.getRandomColor(),
        };
        this.customers = [...this.customers, newCustomer];
        this.filteredCustomers = [...this.customers];
        this.selectCustomer(newCustomer);
        this.showCustomerDropdown = false;
      }
    });
  }

  loadTaxTypes(): Observable<void> {
    return new Observable((observer) => {
      this.invoiceService.getTaxTypes().subscribe({
        next: (taxTypes) => {
          this.taxTypes = taxTypes;
          observer.next();
          observer.complete();
        },
        error: (err) => {
          console.error('Error fetching tax types:', err);
          this.openDialog(
            'error',
            'Error',
            'Failed to load tax types.',
            'Tax configuration data is unavailable. Some features may not work properly until this is resolved.'
          );
          observer.error(err);
        },
      });
    });
  }

  // Attachment handling methods
  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      const allowedTypes = ['image/jpeg', 'image/png', 'application/pdf'];
      const maxSize = 5 * 1024 * 1024; // 5MB
      const newAttachments: Attachment[] = [];

      Array.from(input.files).forEach((file) => {
        if (!allowedTypes.includes(file.type)) {
          this.openDialog(
            'error',
            'Invalid File Type',
            `File ${file.name} is not allowed. Only JPEG, PNG, and PDF files are supported.`,
            'Please upload a file with a supported format.'
          );
          return;
        }
        if (file.size > maxSize) {
          this.openDialog(
            'error',
            'File Too Large',
            `File ${file.name} exceeds the 5MB limit.`,
            'Please upload a file smaller than 5MB.'
          );
          return;
        }
        const blobUrl = URL.createObjectURL(file);
        this.blobUrls.push(blobUrl); // Track for cleanup
        newAttachments.push({
          fileName: file.name,
          fileUrl: blobUrl, // For preview only
          file, // Store file content
        });
      });

      if (newAttachments.length > 0) {
        this.invoiceData.invoiceAttachments = [
          ...this.invoiceData.invoiceAttachments,
          ...newAttachments,
        ];
        // Reset the file input
        input.value = '';
      }
    }
  }

  previewAttachment(attachment: {
    id?: number;
    fileName: string;
    fileUrl: string;
  }): void {
    if (!attachment.fileUrl) {
      this.openDialog(
        'error',
        'Preview Error',
        'No file URL available for this attachment.',
        'Please ensure the attachment has a valid URL.'
      );
      return;
    }

    this.invoiceService.getAttachment(attachment.fileUrl).subscribe({
      next: (blob: Blob) => {
        const url = window.URL.createObjectURL(blob);
        window.open(url, '_blank');
        setTimeout(() => window.URL.revokeObjectURL(url), 1000);
      },
      error: (err) => {
        console.error('Error fetching attachment:', err);
        this.openDialog(
          'error',
          'Preview Failed',
          'Unable to preview the attachment.',
          'The file could not be retrieved. Please try again or check if the file exists.'
        );
      },
    });
  }

  removeAttachment(index: number): void {
    const attachment = this.invoiceData.invoiceAttachments[index];
    if (attachment.id && this.isEditMode && this.invoiceData.id !== undefined) {
      // Existing attachment: call API to delete
      this.invoiceService
        .deleteAttachment(this.invoiceData.id, attachment.id)
        .subscribe({
          next: () => {
            // Revoke blob URL if it exists
            if (this.blobUrls.includes(attachment.fileUrl)) {
              URL.revokeObjectURL(attachment.fileUrl);
              this.blobUrls = this.blobUrls.filter(
                (url) => url !== attachment.fileUrl
              );
            }
            this.invoiceData.invoiceAttachments.splice(index, 1);
            this.openDialog(
              'success',
              'Success',
              'Attachment removed successfully!',
              'The file has been removed from the invoice.'
            );
          },
          error: (err) => {
            console.error('Error deleting attachment:', err);
            this.openDialog(
              'error',
              'Remove Failed',
              'Failed to remove attachment.',
              'Please try again or check your network connection.'
            );
          },
        });
    } else {
      // New attachment: remove locally and revoke blob URL
      if (this.blobUrls.includes(attachment.fileUrl)) {
        URL.revokeObjectURL(attachment.fileUrl);
        this.blobUrls = this.blobUrls.filter(
          (url) => url !== attachment.fileUrl
        );
      }
      this.invoiceData.invoiceAttachments.splice(index, 1);
    }
  }

  previewInvoice(): void {
    // Minimal validation for preview
    if (!this.invoiceData.customerId) {
      this.openDialog(
        'error',
        'Missing Information',
        'Please select a customer for preview.',
        'A customer must be selected before you can preview the invoice. Choose a customer from the dropdown list.'
      );
      return;
    }

    // Fetch full customer data
    this.customerService
      .getCustomerById(this.invoiceData.customerId)
      .subscribe({
        next: (customer) => {
          // Map invoiceData to Invoice interface
          const invoice: Invoice = {
            id: this.invoiceData.id || 0,
            customerName: customer.name,
            invoiceNumber: this.invoiceData.isAutomated
              ? `INV${Date.now()}`
              : this.invoiceData.invoiceNumber || 'N/A',
            poNumber: this.invoiceData.poNumber || '',
            projectDetail: this.invoiceData.projectDetail || '',
            issueDate:
              this.parseDate(this.invoiceData.issuedDate) || new Date(),
            dueDate: this.parseDate(this.invoiceData.dueDate) || new Date(),
            // type: 'Standard',
            currency: this.invoiceData.currency || 'USD',
            customer: {
              id: customer.id,
              name: customer.name,
              email: customer.email || '',
              phoneNumber: customer.phoneNumber || '',
              address: {
                address1: customer.address.address1 || '',
                address2: customer.address.address2 || '',
                city: customer.address.city || '',
                state: customer.address.state || '',
                zipCode: customer.address.zipCode || '',
                country: customer.address.country || '',
              } as Address,
              companyId: customer.id, // Assuming companyId is same as customer id or adjust as needed
            },
            notes:
              this.invoiceData.notes ||
              'Thank you for your business! Payment is due within 30 days of invoice date.',
            paymentMethod:
              this.getPaymentMethodDisplayName(
                this.invoiceData.paymentMethod
              ) || '',
            items: this.invoiceData.items.map((item) => ({
              description: item.description || 'N/A',
              quantity: item.quantity || 1,
              unitPrice: item.unitPrice || 0,
              taxType: item.taxType || '',
              taxAmount: item.taxAmount || 0,
              amount: item.quantity * item.unitPrice || 0,
            })),
            taxDetails: this.invoiceData.taxDetails.map((tax) => ({
              taxType: tax.taxType,
              rate: tax.rate,
              amount: tax.amount,
            })),
            discounts: this.invoiceData.discounts.map((discount) => ({
              description: discount.description || '',
              amount: discount.amount,
              isPercentage: discount.isPercentage,
              calculatedAmount: discount.isPercentage
                ? (parseFloat(this.getSubtotal()) * discount.amount) / 100
                : discount.amount,
            })),
            invoiceStatus:
              (this.invoiceData.invoiceStatus as Invoice['invoiceStatus']) ||
              'Draft',
            paymentStatus:
              (this.invoiceData.paymentStatus as Invoice['paymentStatus']) ||
              'Pending',
            customerId: this.invoiceData.customerId,
            isAutomated: this.invoiceData.isAutomated,
            subtotal: parseFloat(this.getSubtotal()),
            totalAmount: parseFloat(this.getTotal()),
            invoiceAttachments: this.invoiceData.invoiceAttachments.map(
              (attachment) => ({
                id: attachment.id || 0,
                fileName: attachment.fileName,
                fileUrl: attachment.fileUrl || '',
              })
            ),
          };

          // Use dynamic company data or fallback
          const companyData = this.company || {
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
          };
          this.dialog
            .open(InvoiceDisplayComponent, {
              width: '100%', // Full width
              maxWidth: '100vw', // Override Material's default max-width
              height: 'auto', // Auto height to fit content
              maxHeight: '95vh', // Slightly reduced to avoid overlap with browser chrome
              panelClass: 'full-screen-modal', // Custom class for additional styling
              data: { invoice, company: companyData, mode: 'preview' },
            })
            .afterClosed()
            .subscribe((result) => {
              if (result?.action === 'sendInvoice') {
                this.sendInvoice();
              }
            });
        },
        //   });
        // },
        error: (err) => {
          console.error('Error fetching customer:', err);
          this.openDialog(
            'error',
            'Customer Data Error',
            'Failed to load customer data for preview.',
            'Customer information could not be retrieved. Please try again or select a different customer.'
          );
        },
      });
  }

  addTaxType(): void {
    if (!this.newTaxType.name || this.newTaxType.rate <= 0) {
      this.openDialog(
        'error',
        'Invalid Input',
        'Please provide a valid tax name and rate.',
        'Both tax name and rate are required. Rate must be greater than 0 (e.g., 10 for 10%).'
      );
      return;
    }
    this.invoiceService
      .createTaxType({ name: this.newTaxType.name, rate: this.newTaxType.rate })
      .subscribe({
        // next: () => {
        //   this.loadTaxTypes();
        //   this.newTaxType = { name: '', rate: 0 };
        //   this.openDialog(
        //     'success',
        //     'Success',
        //     'Tax type added successfully!',
        //     'The new tax type has been created and is now available for use in your invoices.'
        //   );
        // },
        next: (createdTaxType: TaxType) => {
          // Append the new tax type to the taxTypes array
          this.taxTypes = [...this.taxTypes, createdTaxType];
          // Reset the tax type form
          this.newTaxType = { name: '', rate: 0 };
          // Show success dialog
          this.openDialog(
            'success',
            'Success',
            'Tax type added successfully!',
            'The new tax type has been created and is now available for use in your invoices.'
          );
        },
        error: (err) => {
          console.error('Error adding tax type:', err);
          const errorMessage =
            err.error?.detail || 'Failed to add tax type. Please try again.';
          this.openDialog(
            'error',
            'Error',
            errorMessage,
            'The tax type could not be created. Please check if a tax type with the same name already exists.'
          );
        },
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

  getRandomColor(): string {
    const colors = ['#6D28D9', '#A78BFA', '#059669', '#DC2626', '#4682B4'];
    return colors[Math.floor(Math.random() * colors.length)];
  }

  // Enhanced parseDate to handle edge cases
  parseDate(dateStr: string): Date | null {
    if (!dateStr) {
      return null;
    }
    // Ensure the date string matches dd/mm/yyyy or yyyy-mm-dd
    const ddmmyyyyRegex = /^\d{2}\/\d{2}\/\d{4}$/;
    const yyyymmddRegex = /^\d{4}-\d{2}-\d{2}$/;
    let day, month, year;

    if (ddmmyyyyRegex.test(dateStr)) {
      [day, month, year] = dateStr.split('/').map(Number);
    } else if (yyyymmddRegex.test(dateStr)) {
      [year, month, day] = dateStr.split('-').map(Number);
    } else {
      return null;
    }

    if (month < 1 || month > 12 || day < 1 || day > 31 || year < 1900) {
      return null;
    }

    const date = new Date(year, month - 1, day);
    if (
      isNaN(date.getTime()) ||
      date.getMonth() + 1 !== month ||
      date.getDate() !== day ||
      date.getFullYear() !== year
    ) {
      return null;
    }
    return date;
  }

  // Ensure consistent date formatting
  formatDate(date: Date): string {
    if (!date || isNaN(date.getTime())) {
      return '';
    }
    return `${date.getDate().toString().padStart(2, '0')}/${(
      date.getMonth() + 1
    )
      .toString()
      .padStart(2, '0')}/${date.getFullYear()}`;
  }

  // validateDates(showDialog: boolean = true): boolean {
  //   const issueDate = this.parseDate(this.invoiceData.issuedDate);
  //   const dueDate = this.parseDate(this.invoiceData.dueDate);

  //   if (this.invoiceData.issuedDate && !issueDate) {
  //     if (showDialog) {
  //       this.openDialog(
  //         'error',
  //         'Invalid Date Format',
  //         'Please enter a valid issue date.',
  //         'Use the format dd/mm/yyyy (e.g., 31/12/2024). Make sure the date exists and is properly formatted.'
  //       );
  //     }
  //     return false;
  //   }
  //   if (this.invoiceData.dueDate && !dueDate) {
  //     if (showDialog) {
  //       this.openDialog(
  //         'error',
  //         'Invalid Date Format',
  //         'Please enter a valid issue date.',
  //         'Use the format dd/mm/yyyy (e.g., 31/12/2024). Make sure the date exists and is properly formatted.'
  //       );
  //     }
  //     return false;
  //   }
  //   if (issueDate && dueDate && dueDate < issueDate) {
  //     if (showDialog) {
  //       this.openDialog(
  //         'error',
  //         'Invalid Date Range',
  //         'Due date cannot be earlier than the issue date.',
  //         'Please select a due date that comes after the issue date to ensure proper invoice processing.'
  //       );
  //     }
  //     return false;
  //   }
  //   return true;
  // }

  validateDates(showErrors: boolean = true): boolean {
    this.errors = { ...this.errors }; // Create a new reference to trigger change detection
    delete this.errors['issuedDate'];
    delete this.errors['dueDate'];

    const issueDate = this.parseDate(this.invoiceData.issuedDate);
    const dueDate = this.parseDate(this.invoiceData.dueDate);

    if (this.invoiceData.issuedDate && !issueDate) {
      if (showErrors) {
        this.errors['issuedDate'] = 'Enter a valid issue date (dd/mm/yyyy).';
      }
      return false;
    }
    if (this.invoiceData.dueDate && !dueDate) {
      if (showErrors) {
        this.errors['dueDate'] = 'Enter a valid due date (dd/mm/yyyy).';
      }
      return false;
    }
    if (issueDate && dueDate && dueDate < issueDate) {
      if (showErrors) {
        this.errors['dueDate'] = 'Due date must be after issue date.';
      }
      return false;
    }
    return true;
  }
  onIssueDateChange(): void {
    if (!this.invoiceData.issuedDate) {
      delete this.errors['issuedDate'];
      return;
    }
    this.validateDates();
    // const issueDate = this.parseDate(this.invoiceData.issuedDate);
    // if (!issueDate) {
    //   this.openDialog(
    //     'error',
    //     'Invalid Date Format',
    //     'Please enter a valid issue date.',
    //     'Use the format dd/mm/yyyy (e.g., 15/01/2024). Make sure the date exists and is correctly formatted.'
    //   );
    //   this.invoiceData.issuedDate = '';
    //   return;
    // }
    // if (this.invoiceData.dueDate) {
    //   const dueDate = this.parseDate(this.invoiceData.dueDate);
    //   if (!dueDate) {
    //     this.invoiceData.dueDate = '';
    //     this.openDialog(
    //       'error',
    //       'Invalid Date Format',
    //       'Please enter a valid due date.',
    //       'The due date format is incorrect. Use dd/mm/yyyy format (e.g., 28/02/2024).'
    //     );
    //     return;
    //   }
    //   if (dueDate && issueDate && dueDate < issueDate) {
    //     this.invoiceData.dueDate = '';
    //     this.openDialog(
    //       'error',
    //       'Date Conflict Resolved',
    //       'Due date has been reset.',
    //       'The due date was earlier than the issue date, so it has been cleared. Please select a new due date that comes after the issue date.'
    //     );
    //   }
    // }
  }

  // validateDueDate(dueDate: string): void {
  //   if (!dueDate) {
  //     this.invoiceData.dueDate = '';
  //     this.openDialog(
  //       'error',
  //       'Missing Due Date',
  //       'Due date is required.',
  //       'Please enter a valid due date in the format dd/mm/yyyy.'
  //     );
  //     return; // Allow empty due date
  //   }
  //   const parsedDueDate = this.parseDate(dueDate);
  //   if (!parsedDueDate || isNaN(parsedDueDate.getTime())) {
  //     this.invoiceData.dueDate = '';
  //     this.openDialog(
  //       'error',
  //       'Invalid Due Date Format',
  //       'Please enter a valid due date in the format dd/mm/yyyy.',
  //       'The due date must be in dd/mm/yyyy format (e.g., 25/03/2024).'
  //     );
  //     return;
  //   }
  //   if (this.invoiceData.issuedDate) {
  //     const issueDate = this.parseDate(this.invoiceData.issuedDate);
  //     if (!issueDate || isNaN(issueDate.getTime())) {
  //       this.invoiceData.issuedDate = '';
  //       this.invoiceData.dueDate = '';
  //       this.openDialog(
  //         'error',
  //         'Invalid Issue Date',
  //         'Issue date is invalid. Please correct it first.',
  //         'The issue date must be in dd/mm/yyyy format (e.g., 15/03/2024).'
  //       );
  //       return;
  //     }
  //     if (parsedDueDate < issueDate) {
  //       this.invoiceData.dueDate = '';
  //       this.openDialog(
  //         'error',
  //         'Invalid Date Range',
  //         'Due date cannot be earlier than the issue date.',
  //         'Please choose a due date that comes after the issue date.'
  //       );
  //       return;
  //     }
  //   }
  //   this.invoiceData.dueDate = dueDate;
  // }

  validateDueDate(dueDate: string): void {
    delete this.errors['dueDate'];
    if (!dueDate) {
      this.errors['dueDate'] = 'Due date is required.';
      return;
    }
    const parsedDueDate = this.parseDate(dueDate);
    if (!parsedDueDate || isNaN(parsedDueDate.getTime())) {
      this.errors['dueDate'] = 'Enter a valid due date (dd/mm/yyyy).';
      return;
    }
    if (this.invoiceData.issuedDate) {
      const issueDate = this.parseDate(this.invoiceData.issuedDate);
      if (!issueDate || isNaN(issueDate.getTime())) {
        this.errors['issuedDate'] = 'Enter a valid issue date (dd/mm/yyyy).';
        return;
      }
      if (parsedDueDate < issueDate) {
        this.errors['dueDate'] = 'Due date must be after issue date.';
        return;
      }
    }
    this.invoiceData.dueDate = dueDate;
  }

  onDueDateChange(): void {
    this.dueDateSubject.next(this.invoiceData.dueDate);
  }

  resetCalendarState(): void {
    const today = new Date();
    this.currentMonth = today.getMonth();
    this.currentYear = today.getFullYear();
    this.selectedDay = 0;
  }

  goBack(): void {
    this.router.navigate(['/invoices']);
  }

  saveToDraft(): void {
    if (this.validateInvoiceData()) {
      this.saveInvoice('Draft');
    }
  }

  saveAndContinue(): void {
    if (this.validateInvoiceData()) {
      this.saveInvoice('Draft', true);
    }
  }

  sendInvoice(): void {
    if (this.validateInvoiceData()) {
      this.saveInvoice('Sent');
    }
  }

  // validateInvoiceData(showDialog: boolean = true): boolean {
  //   // Invoice Number
  //   if (
  //     !this.invoiceData.isAutomated &&
  //     !this.invoiceData.invoiceNumber.trim()
  //   ) {
  //     if (showDialog) {
  //       this.openDialog(
  //         'error',
  //         'Missing Invoice Number',
  //         'Invoice number is required for manual invoices.',
  //         'Please enter a valid invoice number or enable automated numbering.'
  //       );
  //     }
  //     return false;
  //   }
  //   if (
  //     this.invoiceData.invoiceNumber.length > 50 ||
  //     (this.invoiceData.isAutomated &&
  //       this.invoiceData.invoiceNumber.length > 50)
  //   ) {
  //     if (showDialog) {
  //       this.openDialog(
  //         'error',
  //         'Invalid Invoice Number',
  //         'Invoice number is too long.',
  //         'Maximum length is 50 characters.'
  //       );
  //     }
  //     return false;
  //   }
  //   if (
  //     this.invoiceData.invoiceNumber &&
  //     !/^[a-zA-Z0-9-_]+$/.test(this.invoiceData.invoiceNumber.trim())
  //   ) {
  //     if (showDialog) {
  //       this.openDialog(
  //         'error',
  //         'Invalid Invoice Number',
  //         'Invoice number contains invalid characters.',
  //         'Use only alphanumeric characters, hyphens, or underscores.'
  //       );
  //     }
  //     return false;
  //   }

  //   // PO Number
  //   if (this.invoiceData.poNumber && this.invoiceData.poNumber.length > 50) {
  //     if (showDialog) {
  //       this.openDialog(
  //         'error',
  //         'Invalid PO Number',
  //         'PO number is too long.',
  //         'Maximum length is 50 characters.'
  //       );
  //     }
  //     return false;
  //   }
  //   if (
  //     this.invoiceData.poNumber &&
  //     !/^[a-zA-Z0-9-_]+$/.test(this.invoiceData.poNumber.trim())
  //   ) {
  //     if (showDialog) {
  //       this.openDialog(
  //         'error',
  //         'Invalid PO Number',
  //         'PO number contains invalid characters.',
  //         'Use only alphanumeric characters, hyphens, or underscores.'
  //       );
  //     }
  //     return false;
  //   }

  //   // Project Detail
  //   if (
  //     this.invoiceData.projectDetail &&
  //     this.invoiceData.projectDetail.length > 500
  //   ) {
  //     if (showDialog) {
  //       this.openDialog(
  //         'error',
  //         'Invalid Project Detail',
  //         'Project detail is too long.',
  //         'Maximum length is 500 characters.'
  //       );
  //     }
  //     return false;
  //   }

  //   // Customer ID
  //   if (!this.invoiceData.customerId || this.invoiceData.customerId <= 0) {
  //     if (showDialog) {
  //       this.openDialog(
  //         'error',
  //         'Missing Customer',
  //         'Please select a customer.',
  //         'A valid customer must be selected.'
  //       );
  //     }
  //     return false;
  //   }
  //   if (!this.customers.some((c) => c.id === this.invoiceData.customerId)) {
  //     if (showDialog) {
  //       this.openDialog(
  //         'error',
  //         'Invalid Customer',
  //         'Selected customer is invalid.',
  //         'Please select a valid customer from the list.'
  //       );
  //     }
  //     return false;
  //   }

  //   // Items
  //   if (!this.invoiceData.items.length) {
  //     if (showDialog) {
  //       this.openDialog(
  //         'error',
  //         'Missing Items',
  //         'At least one item is required.',
  //         'Please add at least one item to the invoice.'
  //       );
  //     }
  //     return false;
  //   }
  //   for (let i = 0; i < this.invoiceData.items.length; i++) {
  //     const item = this.invoiceData.items[i];
  //     if (!item.description.trim()) {
  //       if (showDialog) {
  //         this.openDialog(
  //           'error',
  //           'Invalid Item',
  //           `Item ${i + 1} description is required.`,
  //           'Please provide a valid description for all items.'
  //         );
  //       }
  //       return false;
  //     }
  //     if (item.description.length > 200) {
  //       if (showDialog) {
  //         this.openDialog(
  //           'error',
  //           'Invalid Item',
  //           `Item ${i + 1} description is too long.`,
  //           'Maximum length is 200 characters.'
  //         );
  //       }
  //       return false;
  //     }
  //     if (item.quantity < 1 || !Number.isInteger(item.quantity)) {
  //       if (showDialog) {
  //         this.openDialog(
  //           'error',
  //           'Invalid Item',
  //           `Item ${i + 1} quantity must be a positive integer.`,
  //           'Please enter a quantity greater than or equal to 1.'
  //         );
  //       }
  //       return false;
  //     }
  //     if (item.unitPrice < 0 || isNaN(item.unitPrice)) {
  //       if (showDialog) {
  //         this.openDialog(
  //           'error',
  //           'Invalid Item',
  //           `Item ${i + 1} unit price must be non-negative.`,
  //           'Please enter a valid unit price.'
  //         );
  //       }
  //       return false;
  //     }
  //     if (item.unitPrice.toString().split('.')[1]?.length > 2) {
  //       if (showDialog) {
  //         this.openDialog(
  //           'error',
  //           'Invalid Item',
  //           `Item ${i + 1} unit price has too many decimal places.`,
  //           'Maximum 2 decimal places allowed.'
  //         );
  //       }
  //       return false;
  //     }
  //     if (item.taxType && !this.taxTypes.some((t) => t.name === item.taxType)) {
  //       if (showDialog) {
  //         this.openDialog(
  //           'error',
  //           'Invalid Tax Type',
  //           `Item ${i + 1} tax type is invalid.`,
  //           'Please select a valid tax type or leave it empty.'
  //         );
  //       }
  //       return false;
  //     }
  //     const calculatedAmount = item.quantity * item.unitPrice;
  //     if (Math.abs(item.amount - calculatedAmount) > 0.01) {
  //       if (showDialog) {
  //         this.openDialog(
  //           'error',
  //           'Invalid Item Amount',
  //           `Item ${i + 1} amount is incorrect.`,
  //           'Amount must equal quantity × unit price.'
  //         );
  //       }
  //       return false;
  //     }
  //     if (item.taxType) {
  //       const taxType = this.taxTypes.find((t) => t.name === item.taxType);
  //       if (taxType) {
  //         const expectedTaxAmount = parseFloat(
  //           (calculatedAmount * (taxType.rate / 100)).toFixed(2)
  //         );
  //         if (Math.abs(item.taxAmount - expectedTaxAmount) > 0.01) {
  //           if (showDialog) {
  //             this.openDialog(
  //               'error',
  //               'Invalid Tax Amount',
  //               `Item ${i + 1} tax amount is incorrect.`,
  //               'Tax amount must match the calculated tax rate.'
  //             );
  //           }
  //           return false;
  //         }
  //       }
  //     } else if (item.taxAmount !== 0) {
  //       if (showDialog) {
  //         this.openDialog(
  //           'error',
  //           'Invalid Tax Amount',
  //           `Item ${i + 1} tax amount must be 0 if no tax type is selected.`,
  //           'Please select a tax type or set tax amount to 0.'
  //         );
  //       }
  //       return false;
  //     }
  //   }

  //   // Tax Details
  //   const taxTypeMap = new Map<string, { rate: number; amount: number }>();
  //   this.invoiceData.items.forEach((item) => {
  //     if (item.taxType && item.taxAmount > 0) {
  //       const taxType = this.taxTypes.find((t) => t.name === item.taxType);
  //       if (taxType) {
  //         if (taxTypeMap.has(item.taxType)) {
  //           const existing = taxTypeMap.get(item.taxType)!;
  //           existing.amount += item.taxAmount;
  //           taxTypeMap.set(item.taxType, existing);
  //         } else {
  //           taxTypeMap.set(item.taxType, {
  //             rate: taxType.rate,
  //             amount: item.taxAmount,
  //           });
  //         }
  //       }
  //     }
  //   });
  //   for (const [index, tax] of this.invoiceData.taxDetails.entries()) {
  //     if (!tax.taxType || !taxTypeMap.has(tax.taxType)) {
  //       if (showDialog) {
  //         this.openDialog(
  //           'error',
  //           'Invalid Tax Detail',
  //           `Tax detail ${index + 1} has an invalid tax type.`,
  //           'Please ensure all tax details match valid tax types.'
  //         );
  //       }
  //       return false;
  //     }
  //     const expected = taxTypeMap.get(tax.taxType)!;
  //     if (
  //       tax.rate !== expected.rate ||
  //       Math.abs(tax.amount - expected.amount) > 0.01
  //     ) {
  //       if (showDialog) {
  //         this.openDialog(
  //           'error',
  //           'Invalid Tax Detail',
  //           `Tax detail ${index + 1} has incorrect rate or amount.`,
  //           'Tax details must match calculated values from items.'
  //         );
  //       }
  //       return false;
  //     }
  //   }

  //   // Discounts
  //   for (let i = 0; i < this.invoiceData.discounts.length; i++) {
  //     const discount = this.invoiceData.discounts[i];
  //     if (!discount.description.trim()) {
  //       if (showDialog) {
  //         this.openDialog(
  //           'error',
  //           'Invalid Discount',
  //           `Discount ${i + 1} description is required.`,
  //           'Please provide a valid description.'
  //         );
  //       }
  //       return false;
  //     }
  //     if (discount.description.length > 200) {
  //       if (showDialog) {
  //         this.openDialog(
  //           'error',
  //           'Invalid Discount',
  //           `Discount ${i + 1} description is too long.`,
  //           'Maximum length is 200 characters.'
  //         );
  //       }
  //       return false;
  //     }
  //     if (discount.amount <= 0 || isNaN(discount.amount)) {
  //       if (showDialog) {
  //         this.openDialog(
  //           'error',
  //           'Invalid Discount',
  //           `Discount ${i + 1} amount must be positive.`,
  //           'Please enter a valid amount greater than 0.'
  //         );
  //       }
  //       return false;
  //     }
  //     if (
  //       discount.isPercentage &&
  //       (discount.amount > 100 || discount.amount < 0)
  //     ) {
  //       if (showDialog) {
  //         this.openDialog(
  //           'error',
  //           'Invalid Discount',
  //           `Discount ${i + 1} percentage must be between 0 and 100.`,
  //           'Please enter a valid percentage.'
  //         );
  //       }
  //       return false;
  //     }
  //     if (discount.amount.toString().split('.')[1]?.length > 2) {
  //       if (showDialog) {
  //         this.openDialog(
  //           'error',
  //           'Invalid Discount',
  //           `Discount ${i + 1} amount has too many decimal places.`,
  //           'Maximum 2 decimal places allowed.'
  //         );
  //       }
  //       return false;
  //     }
  //   }

  //   // Payment Method
  //   if (
  //     !this.invoiceData.paymentMethod ||
  //     !this.paymentMethods.some(
  //       (m) => m.value === this.invoiceData.paymentMethod
  //     )
  //   ) {
  //     if (showDialog) {
  //       this.openDialog(
  //         'error',
  //         'Invalid Payment Method',
  //         'Please select a valid payment method.',
  //         'Choose from PayPal, Bank Transfer, Cash, Credit Card, or UPI.'
  //       );
  //     }
  //     return false;
  //   }

  //   // Notes
  //   if (this.invoiceData.notes && this.invoiceData.notes.length > 1000) {
  //     if (showDialog) {
  //       this.openDialog(
  //         'error',
  //         'Invalid Notes',
  //         'Notes are too long.',
  //         'Maximum length is 1000 characters.'
  //       );
  //     }
  //     return false;
  //   }

  //   // Currency
  //   if (!['USD', 'EUR', 'INR'].includes(this.invoiceData.currency)) {
  //     if (showDialog) {
  //       this.openDialog(
  //         'error',
  //         'Invalid Currency',
  //         'Please select a valid currency.',
  //         'Choose from USD, EUR, or INR.'
  //       );
  //     }
  //     return false;
  //   }

  //   // Attachments
  //   for (let i = 0; i < this.invoiceData.invoiceAttachments.length; i++) {
  //     const attachment = this.invoiceData.invoiceAttachments[i];
  //     if (!attachment.fileName || attachment.fileName.length > 255) {
  //       if (showDialog) {
  //         this.openDialog(
  //           'error',
  //           'Invalid Attachment',
  //           `Attachment ${i + 1} file name is invalid.`,
  //           'File name is required and must be less than 255 characters.'
  //         );
  //       }
  //       return false;
  //     }
  //     const extension = attachment.fileName.split('.').pop()?.toLowerCase();
  //     if (!['jpg', 'jpeg', 'png', 'pdf'].includes(extension || '')) {
  //       if (showDialog) {
  //         this.openDialog(
  //           'error',
  //           'Invalid Attachment',
  //           `Attachment ${i + 1} has an unsupported file type.`,
  //           'Only JPEG, PNG, and PDF files are supported.'
  //         );
  //       }
  //       return false;
  //     }
  //     if (
  //       !attachment.id &&
  //       (!attachment.file || attachment.file.size > 5 * 1024 * 1024)
  //     ) {
  //       if (showDialog) {
  //         this.openDialog(
  //           'error',
  //           'Invalid Attachment',
  //           `Attachment ${
  //             i + 1
  //           } exceeds the 5MB limit or is missing file content.`,
  //           'Please upload a file smaller than 5MB.'
  //         );
  //       }
  //       return false;
  //     }
  //     if (
  //       !attachment.id &&
  //       attachment.file &&
  //       !['image/jpeg', 'image/png', 'application/pdf'].includes(
  //         attachment.file.type
  //       )
  //     ) {
  //       if (showDialog) {
  //         this.openDialog(
  //           'error',
  //           'Invalid Attachment',
  //           `Attachment ${i + 1} has an unsupported file type.`,
  //           'Only JPEG, PNG, and PDF files are supported.'
  //         );
  //       }
  //       return false;
  //     }
  //   }

  //   // Dates (already validated, but recheck for consistency)
  //   if (!this.validateDates(showDialog)) {
  //     return false;
  //   }

  //   // Company
  //   if (!this.company || !this.company.id) {
  //     if (showDialog) {
  //       this.openDialog(
  //         'error',
  //         'Missing Company Data',
  //         'Company information is required.',
  //         'Please ensure you are logged in with a valid company account.'
  //       );
  //     }
  //     return false;
  //   }

  //   return true;
  // }

  validateInvoiceData(showErrors: boolean = true): boolean {
    this.errors = {}; // Reset errors
    let isValid = true;

    // Invoice Number
    if (
      !this.invoiceData.isAutomated &&
      !this.invoiceData.invoiceNumber.trim()
    ) {
      if (showErrors) {
        this.errors['invoiceNumber'] = 'Invoice number is required for manual invoices.';
      }
      isValid = false;
    } else if (
      this.invoiceData.invoiceNumber.length > 50 ||
      (this.invoiceData.isAutomated &&
        this.invoiceData.invoiceNumber.length > 50)
    ) {
      if (showErrors) {
        this.errors['invoiceNumber'] = 'Invoice number must be less than 50 characters.';
      }
      isValid = false;
    } else if (
      this.invoiceData.invoiceNumber &&
      !/^[a-zA-Z0-9-_]+$/.test(this.invoiceData.invoiceNumber.trim())
    ) {
      if (showErrors) {
        this.errors['invoiceNumber'] = 'Use only alphanumeric characters, hyphens, or underscores.';
      }
      isValid = false;
    }

    // PO Number
    if (this.invoiceData.poNumber && this.invoiceData.poNumber.length > 50) {
      if (showErrors) {
        this.errors['poNumber'] = 'PO number must be less than 50 characters.';
      }
      isValid = false;
    } else if (
      this.invoiceData.poNumber &&
      !/^[a-zA-Z0-9-_]+$/.test(this.invoiceData.poNumber.trim())
    ) {
      if (showErrors) {
        this.errors['poNumber'] = 'Use only alphanumeric characters, hyphens, or underscores.';
      }
      isValid = false;
    }

    // Project Detail
    if (
      this.invoiceData.projectDetail &&
      this.invoiceData.projectDetail.length > 500
    ) {
      if (showErrors) {
        this.errors['projectDetail'] = 'Project detail must be less than 500 characters.';
      }
      isValid = false;
    }

    // Customer ID
    if (!this.invoiceData.customerId || this.invoiceData.customerId <= 0) {
      if (showErrors) {
        this.errors['customerId'] = 'Please select a customer.';
      }
      isValid = false;
    } else if (!this.customers.some((c) => c.id === this.invoiceData.customerId)) {
      if (showErrors) {
        this.errors['customerId'] = 'Selected customer is invalid.';
      }
      isValid = false;
    }

    // Items
    if (!this.invoiceData.items.length) {
      if (showErrors) {
        this.errors['items'] = 'At least one item is required.';
      }
      isValid = false;
    }
    this.invoiceData.items.forEach((item, i) => {
      if (!item.description.trim()) {
        if (showErrors) {
          this.errors[`items[${i}].description`] = 'Description is required.';
        }
        isValid = false;
      } else if (item.description.length > 200) {
        if (showErrors) {
          this.errors[`items[${i}].description`] = 'Description must be less than 200 characters.';
        }
        isValid = false;
      }
      if (item.quantity < 1 || !Number.isInteger(item.quantity)) {
        if (showErrors) {
          this.errors[`items[${i}].quantity`] = 'Quantity must be a positive integer.';
        }
        isValid = false;
      }
      if (item.unitPrice < 0 || isNaN(item.unitPrice)) {
        if (showErrors) {
          this.errors[`items[${i}].unitPrice`] = 'Unit price must be non-negative.';
        }
        isValid = false;
      } else if (item.unitPrice.toString().split('.')[1]?.length > 2) {
        if (showErrors) {
          this.errors[`items[${i}].unitPrice`] = 'Unit price must have at most 2 decimal places.';
        }
        isValid = false;
      }
      if (item.taxType && !this.taxTypes.some((t) => t.name === item.taxType)) {
        if (showErrors) {
          this.errors[`items[${i}].taxType`] = 'Invalid tax type.';
        }
        isValid = false;
      }
      const calculatedAmount = item.quantity * item.unitPrice;
      if (Math.abs(item.amount - calculatedAmount) > 0.01) {
        if (showErrors) {
          this.errors[`items[${i}].amount`] = 'Amount must equal quantity × unit price.';
        }
        isValid = false;
      }
      if (item.taxType) {
        const taxType = this.taxTypes.find((t) => t.name === item.taxType);
        if (taxType) {
          const expectedTaxAmount = parseFloat(
            (calculatedAmount * (taxType.rate / 100)).toFixed(2)
          );
          if (Math.abs(item.taxAmount - expectedTaxAmount) > 0.01) {
            if (showErrors) {
              this.errors[`items[${i}].taxAmount`] = 'Tax amount does not match calculated value.';
            }
            isValid = false;
          }
        }
      } else if (item.taxAmount !== 0) {
        if (showErrors) {
          this.errors[`items[${i}].taxAmount`] = 'Tax amount must be 0 if no tax type is selected.';
        }
        isValid = false;
      }
    });

    // Tax Details
    const taxTypeMap = new Map<string, { rate: number; amount: number }>();
    this.invoiceData.items.forEach((item) => {
      if (item.taxType && item.taxAmount > 0) {
        const taxType = this.taxTypes.find((t) => t.name === item.taxType);
        if (taxType) {
          if (taxTypeMap.has(item.taxType)) {
            const existing = taxTypeMap.get(item.taxType)!;
            existing.amount += item.taxAmount;
            taxTypeMap.set(item.taxType, existing);
          } else {
            taxTypeMap.set(item.taxType, {
              rate: taxType.rate,
              amount: item.taxAmount,
            });
          }
        }
      }
    });
    this.invoiceData.taxDetails.forEach((tax, i) => {
      if (!tax.taxType || !taxTypeMap.has(tax.taxType)) {
        if (showErrors) {
          this.errors[`taxDetails[${i}]`] = 'Invalid tax type.';
        }
        isValid = false;
      } else {
        const expected = taxTypeMap.get(tax.taxType)!;
        if (
          tax.rate !== expected.rate ||
          Math.abs(tax.amount - expected.amount) > 0.01
        ) {
          if (showErrors) {
            this.errors[`taxDetails[${i}]`] = 'Tax rate or amount is incorrect.';
          }
          isValid = false;
        }
      }
    });

    // Discounts
    this.invoiceData.discounts.forEach((discount, i) => {
      if (!discount.description.trim()) {
        if (showErrors) {
          this.errors[`discounts[${i}].description`] = 'Description is required.';
        }
        isValid = false;
      } else if (discount.description.length > 200) {
        if (showErrors) {
          this.errors[`discounts[${i}].description`] = 'Description must be less than 200 characters.';
        }
        isValid = false;
      }
      if (discount.amount <= 0 || isNaN(discount.amount)) {
        if (showErrors) {
          this.errors[`discounts[${i}].amount`] = 'Amount must be positive.';
        }
        isValid = false;
      } else if (
        discount.isPercentage &&
        (discount.amount > 100 || discount.amount < 0)
      ) {
        if (showErrors) {
          this.errors[`discounts[${i}].amount`] = 'Percentage must be between 0 and 100.';
        }
        isValid = false;
      } else if (discount.amount.toString().split('.')[1]?.length > 2) {
        if (showErrors) {
          this.errors[`discounts[${i}].amount`] = 'Amount must have at most 2 decimal places.';
        }
        isValid = false;
      }
    });

    // Payment Method
    if (
      !this.invoiceData.paymentMethod ||
      !this.paymentMethods.some(
        (m) => m.value === this.invoiceData.paymentMethod
      )
    ) {
      if (showErrors) {
        this.errors['paymentMethod'] = 'Please select a valid payment method.';
      }
      isValid = false;
    }

    // Notes
    if (this.invoiceData.notes && this.invoiceData.notes.length > 1000) {
      if (showErrors) {
        this.errors['notes'] = 'Notes must be less than 1000 characters.';
      }
      isValid = false;
    }

    // Currency
    if (!['USD', 'EUR', 'INR'].includes(this.invoiceData.currency)) {
      if (showErrors) {
        this.errors['currency'] = 'Please select a valid currency.';
      }
      isValid = false;
    }

    // Attachments
    this.validateAttachments(showErrors);

    // Dates
    if (!this.validateDates(showErrors)) {
      isValid = false;
    }

    // Company
    if (!this.company || !this.company.id) {
      if (showErrors) {
        this.openDialog(
          'error',
          'Missing Company Data',
          'Company information is required.',
          'Please ensure you are logged in with a valid company account.'
        );
      }
      isValid = false;
    }

    return isValid;
  }
  validateAttachments(showErrors: boolean = true): boolean {
    let isValid = true;
    this.invoiceData.invoiceAttachments.forEach((attachment, i) => {
      if (!attachment.fileName || attachment.fileName.length > 255) {
        if (showErrors) {
          this.errors[`attachments[${i}]`] = 'File name must be less than 255 characters.';
        }
        isValid = false;
      }
      const extension = attachment.fileName.split('.').pop()?.toLowerCase();
      if (!['jpg', 'jpeg', 'png', 'pdf'].includes(extension || '')) {
        if (showErrors) {
          this.errors[`attachments[${i}]`] = 'Only JPEG, PNG, and PDF files are supported.';
        }
        isValid = false;
      }
      if (
        !attachment.id &&
        (!attachment.file || attachment.file.size > 5 * 1024 * 1024)
      ) {
        if (showErrors) {
          this.errors[`attachments[${i}]`] = 'File must be smaller than 5MB.';
        }
        isValid = false;
      }
      if (
        !attachment.id &&
        attachment.file &&
        !['image/jpeg', 'image/png', 'application/pdf'].includes(
          attachment.file.type
        )
      ) {
        if (showErrors) {
          this.errors[`attachments[${i}]`] = 'Only JPEG, PNG, and PDF files are supported.';
        }
        isValid = false;
      }
    });
    return isValid;
  }

  validateItem(index: number, type: 'quantity' | 'cost' | 'description' | 'taxType'): void {
    const item = this.invoiceData.items[index];
    delete this.errors[`items[${index}].description`];
    delete this.errors[`items[${index}].quantity`];
    delete this.errors[`items[${index}].unitPrice`];
    delete this.errors[`items[${index}].taxType`];
    delete this.errors[`items[${index}].taxAmount`];
    delete this.errors[`items[${index}].amount`];

    if (type === 'description' && !item.description.trim()) {
      this.errors[`items[${index}].description`] = 'Description is required.';
    } else if (item.description.length > 200) {
      this.errors[`items[${index}].description`] = 'Description must be less than 200 characters.';
    }

    if (type === 'quantity' && (item.quantity < 1 || !Number.isInteger(item.quantity))) {
      this.errors[`items[${index}].quantity`] = 'Quantity must be a positive integer.';
      item.quantity = 1;
    }

    if (type === 'cost') {
      if (item.unitPrice < 0 || isNaN(item.unitPrice)) {
        this.errors[`items[${index}].unitPrice`] = 'Unit price must be non-negative.';
        item.unitPrice = 0;
      } else if (item.unitPrice.toString().split('.')[1]?.length > 2) {
        this.errors[`items[${index}].unitPrice`] = 'Unit price must have at most 2 decimal places.';
      }
    }

    if (type === 'taxType' && item.taxType && !this.taxTypes.some((t) => t.name === item.taxType)) {
      this.errors[`items[${index}].taxType`] = 'Invalid tax type.';
    }

    const calculatedAmount = item.quantity * item.unitPrice;
    if (Math.abs(item.amount - calculatedAmount) > 0.01) {
      this.errors[`items[${index}].amount`] = 'Amount must equal quantity × unit price.';
    }

    if (item.taxType) {
      const taxType = this.taxTypes.find((t) => t.name === item.taxType);
      if (taxType) {
        const expectedTaxAmount = parseFloat(
          (calculatedAmount * (taxType.rate / 100)).toFixed(2)
        );
        if (Math.abs(item.taxAmount - expectedTaxAmount) > 0.01) {
          this.errors[`items[${index}].taxAmount`] = 'Tax amount does not match calculated value.';
        }
      }
    } else if (item.taxAmount !== 0) {
      this.errors[`items[${index}].taxAmount`] = 'Tax amount must be 0 if no tax type is selected.';
    }

    this.errors = { ...this.errors }; // Trigger change detection
  }
  
  saveInvoice(
    status: 'Draft' | 'Sent',
    continueEditing: boolean = false
  ): void {
    if (!this.validateInvoiceData()) {
      return;
    }
    
    const issueDate = this.parseDate(this.invoiceData.issuedDate)!;
    const dueDate = this.parseDate(this.invoiceData.dueDate)!;

    // Prepare FormData for the request
    const formData = new FormData();
    formData.append(
      'InvoiceNumber',
      this.invoiceData.isAutomated
        ? `INV${Date.now()}`
        : this.invoiceData.invoiceNumber.trim()
    );
    formData.append('PONumber', this.invoiceData.poNumber.trim());
    formData.append('ProjectDetail', this.invoiceData.projectDetail.trim());
    formData.append('IssueDate', this.formatDateOnly(issueDate));
    formData.append('DueDate', this.formatDateOnly(dueDate));
    formData.append('Type', 'Standard');
    formData.append('Currency', this.invoiceData.currency || 'USD');
    formData.append('CustomerId', this.invoiceData.customerId.toString());
    formData.append('Notes', this.invoiceData.notes.trim());
    formData.append('PaymentMethod', this.invoiceData.paymentMethod.trim());
    formData.append('InvoiceStatus', status);
    formData.append(
      'PaymentStatus',
      status === 'Sent'
        ? 'Pending'
        : this.invoiceData.paymentStatus || 'Pending'
    );
    if (this.isEditMode && this.invoiceData.id) {
      formData.append('id', this.invoiceData.id.toString());
    }

    // Add items
    this.invoiceData.items.forEach((item, index) => {
      formData.append(`Items[${index}].Description`, item.description.trim());
      formData.append(`Items[${index}].Quantity`, item.quantity.toString());
      formData.append(`Items[${index}].UnitPrice`, item.unitPrice.toString());
      formData.append(`Items[${index}].TaxType`, item.taxType || '');
      formData.append(`Items[${index}].TaxAmount`, item.taxAmount.toString());
      formData.append(
        `Items[${index}].Amount`,
        (item.quantity * item.unitPrice).toFixed(2)
      );
    });

    // Add tax details
    this.invoiceData.taxDetails.forEach((tax, index) => {
      formData.append(`TaxDetails[${index}].TaxType`, tax.taxType);
      formData.append(`TaxDetails[${index}].Rate`, tax.rate.toString());
      formData.append(`TaxDetails[${index}].Amount`, tax.amount.toFixed(2));
    });

    // Add discounts
    this.invoiceData.discounts.forEach((discount, index) => {
      formData.append(`Discounts[${index}].Description`, discount.description);
      formData.append(`Discounts[${index}].Amount`, discount.amount.toString());
      formData.append(
        `Discounts[${index}].IsPercentage`,
        discount.isPercentage.toString()
      );
    });

    // Add attachments
    this.invoiceData.invoiceAttachments.forEach((attachment, index) => {
      if (attachment.id && attachment.fileUrl) {
        // Existing attachment
        formData.append(`Attachments[${index}].Id`, attachment.id.toString());
        formData.append(`Attachments[${index}].FileName`, attachment.fileName);
        formData.append(`Attachments[${index}].FileUrl`, attachment.fileUrl);
      } else if (attachment.file) {
        // New attachment
        formData.append(`Attachments[${index}].FileName`, attachment.fileName);
        formData.append(`Attachments[${index}].FileContent`, attachment.file);
      }
    });

    // Debug FormData contents
    console.log('FormData contents:');
    formData.forEach((value, key) => {
      console.log(`${key}: ${value}`);
    });

    // Determine if this is a create or update operation
    const isUpdate = this.isEditMode && this.invoiceData.id;
    const request = isUpdate
      ? this.invoiceService.updateInvoice(formData)
      : this.invoiceService.createInvoice(formData);

    request.subscribe({
      next: (response) => {
        // Update invoiceData with response
        this.invoiceData.id = response.id;
        this.invoiceData.invoiceAttachments = response.invoiceAttachments || [];

        // Set edit mode after initial save
        if (!this.isEditMode) {
          this.isEditMode = true;
          this.invoiceId = response.id.toString();
          this.router.navigate([`/invoices/edit/${this.invoiceId}`], {
            replaceUrl: true,
          });
        }

        // Show correct success message based on initial isEditMode state
        this.openDialog(
          'success',
          isUpdate ? 'Invoice Updated' : 'Invoice Created',
          isUpdate
            ? 'Invoice updated successfully!'
            : 'Invoice created successfully!',
          `The invoice has been ${
            isUpdate ? 'updated' : 'created'
          } and saved with status "${status}".`
        );

        if (!continueEditing) {
          this.resetForm();
          this.router.navigate(['/invoices']);
        } else {
          // Reload invoice data to ensure consistency
          this.invoiceService.getInvoiceById(response.id).subscribe({
            next: (invoice) => {
              this.invoiceData = {
                ...this.invoiceData,
                invoiceAttachments: invoice.invoiceAttachments || [],
                issuedDate: this.formatDate(new Date(invoice.issueDate)),
                dueDate: this.formatDate(new Date(invoice.dueDate)),
              };
            },
            error: (err) => {
              console.error('Error fetching updated invoice:', err);
              this.openDialog(
                'error',
                'Fetch Failed',
                'Failed to retrieve updated invoice data.',
                'The invoice was saved, but there was an issue retrieving the updated data.'
              );
            },
          });
        }
      },
      error: (err) => {
        console.error('Error saving invoice:', err);
        const errorMessage = err.error?.detail || 'Failed to save invoice.';
        if (errorMessage.includes('Invoice number already exists')) {
          this.openDialog(
            'error',
            'Invoice Number Conflict',
            'The invoice number already exists.',
            'Please choose a different invoice number or enable automated numbering.'
          );
        } else {
          this.openDialog(
            'error',
            'Save Failed',
            'Failed to save invoice.',
            errorMessage
          );
        }
      },
    });
  }

  resetForm(): void {
    this.invoiceData = {
      invoiceNumber: '',
      poNumber: '',
      projectDetail: '',
      items: [],
      taxDetails: [],
      discounts: [],
      paymentMethod: '',
      issuedDate: '',
      dueDate: '',
      notes: '',
      customerId: this.customers.length > 0 ? this.customers[0].id : 0,
      currency: 'USD',
      isAutomated: false,
      invoiceAttachments: [],
    };
    this.selectedCustomer =
      this.customers.length > 0 ? this.customers[0] : null;
    this.addItem();
    this.resetCalendarState();
  }

  formatDateOnly(date: Date): string {
    if (!date || isNaN(date.getTime())) {
      throw new Error('Invalid date provided for formatting');
    }
    return date.toISOString().split('T')[0]; // e.g. "2025-09-01"
  }

  toggleCustomerDropdown(): void {
    this.showCustomerDropdown = !this.showCustomerDropdown;
  }

  selectCustomer(customer: Customer): void {
    this.selectedCustomer = customer;
    this.invoiceData.customerId = customer.id;
    this.showCustomerDropdown = false;
  }

  addItem(): void {
    this.invoiceData.items.push({
      description: '',
      quantity: 1,
      unitPrice: 0,
      taxType: '',
      taxAmount: 0,
      amount: 0,
    });
  }

  removeItem(index: number): void {
    this.invoiceData.items.splice(index, 1);
    this.updateTaxDetails();
  }

  addDiscount(): void {
    this.invoiceData.discounts.push({
      description: '',
      amount: 0,
      isPercentage: false,
    });
  }

  removeDiscount(index: number): void {
    this.invoiceData.discounts.splice(index, 1);
    this.updateDiscounts();
  }

  updateDiscounts(): void {
    this.getTotal();
  }

  toggleCalendar(type: 'issued' | 'due'): void {
    this.calendarType = type;
    this.showCalendar = !this.showCalendar;
    if (this.showCalendar) {
      const date =
        type === 'issued'
          ? this.parseDate(this.invoiceData.issuedDate)
          : this.parseDate(this.invoiceData.dueDate);
      if (date && !isNaN(date.getTime())) {
        this.currentMonth = date.getMonth();
        this.currentYear = date.getFullYear();
        this.selectedDay = date.getDate();
      } else {
        this.resetCalendarState();
      }
    }
  }

  isDateDisabled(day: number): boolean {
    if (day <= 0) {
      return true;
    }
    const selectedDate = new Date(this.currentYear, this.currentMonth, day);
    if (this.calendarType === 'due' && this.invoiceData.issuedDate) {
      const issueDate = this.parseDate(this.invoiceData.issuedDate);
      if (issueDate) {
        return selectedDate < issueDate;
      }
    }
    return false;
  }

  selectDay(day: number): void {
    if (day <= 0) {
      return;
    }

    // Ensure calendar state is valid
    if (
      this.currentMonth < 0 ||
      this.currentMonth > 11 ||
      this.currentYear < 1900 ||
      this.currentYear > 9999
    ) {
      this.openDialog(
        'error',
        'Invalid Calendar Date',
        'The selected date is invalid. Please select a valid date.',
        'There was an error with the calendar selection. Please try selecting the date again or use manual date entry.'
      );
      this.resetCalendarState();
      return;
    }

    const selectedDate = new Date(this.currentYear, this.currentMonth, day);
    const formattedDate = `${day.toString().padStart(2, '0')}/${(
      this.currentMonth + 1
    )
      .toString()
      .padStart(2, '0')}/${this.currentYear}`;

    // Validate the formatted date
    const parsedFormattedDate = this.parseDate(formattedDate);
    if (!parsedFormattedDate || isNaN(parsedFormattedDate.getTime())) {
      this.openDialog(
        'error',
        'Invalid Date Selection',
        'The selected date is invalid. Please select a valid date.',
        'The date you selected could not be processed. Please try selecting a different date or enter the date manually.'
      );
      this.resetCalendarState();
      return;
    }

    if (this.calendarType === 'issued') {
      this.invoiceData.issuedDate = formattedDate;
      if (this.invoiceData.dueDate) {
        const dueDate = this.parseDate(this.invoiceData.dueDate);
        if (!dueDate || isNaN(dueDate.getTime())) {
          this.invoiceData.dueDate = '';
          this.openDialog(
            'error',
            'Invalid Due Date Format',
            'Please enter a valid due date in the format dd/mm/yyyy.',
            'The due date format is incorrect. Please use the calendar picker or enter the date in dd/mm/yyyy format.'
          );
          this.showCalendar = false;
          return;
        }
        if (dueDate && selectedDate && dueDate < selectedDate) {
          this.invoiceData.dueDate = '';
          this.openDialog(
            'error',
            'Due Date Reset',
            'Due date has been reset because it is earlier than the issue date. Please select a new due date.',
            'The selected due date was before the issue date, so it has been cleared. Please choose a due date that comes after the issue date.'
          );
        }
      }
      this.showCalendar = false;
    } else {
      const issueDate = this.parseDate(this.invoiceData.issuedDate);
      if (issueDate && selectedDate < issueDate) {
        this.openDialog(
          'error',
          'Invalid Due Date Selection',
          'Due date cannot be earlier than the issue date. Please select a valid due date.',
          'The selected date is before the issue date. Please choose a date that comes after the issue date to ensure proper invoice terms.'
        );
        this.resetCalendarState();
        return;
      }
      this.invoiceData.dueDate = formattedDate;
      this.showCalendar = false;
    }
    this.selectedDay = day;
  }

  getCalendarDays(): number[] {
    const year = this.currentYear;
    const month = this.currentMonth;
    const firstDay = new Date(year, month, 1);
    const lastDay = new Date(year, month + 1, 0);
    const daysInMonth = lastDay.getDate();
    const startingDayOfWeek = firstDay.getDay();
    const days: number[] = [];
    for (let i = 0; i < startingDayOfWeek; i++) {
      days.push(0);
    }
    for (let day = 1; day <= daysInMonth; day++) {
      days.push(day);
    }
    while (days.length < 42) {
      days.push(0);
    }
    return days;
  }

  getSubtotal(): string {
    const subtotal = this.invoiceData.items.reduce(
      (sum, item) => sum + item.quantity * item.unitPrice,
      0
    );
    return subtotal.toFixed(2);
  }

  updateTaxDetails(index?: number): void {
    if (index !== undefined) {
      const item = this.invoiceData.items[index];
      const amount = parseFloat((item.quantity * item.unitPrice).toFixed(2));
      let taxAmount = 0;

      if (item.taxType) {
        const taxType = this.taxTypes.find((t) => t.name === item.taxType);
        if (taxType) {
          taxAmount = parseFloat((amount * (taxType.rate / 100)).toFixed(2));
        }
      }

      item.amount = amount;
      item.taxAmount = taxAmount;
    } else {
      this.invoiceData.items.forEach((item) => {
        const amount = parseFloat((item.quantity * item.unitPrice).toFixed(2));
        let taxAmount = 0;

        if (item.taxType) {
          const taxType = this.taxTypes.find((t) => t.name === item.taxType);
          if (taxType) {
            taxAmount = parseFloat((amount * (taxType.rate / 100)).toFixed(2));
          }
        }

        item.amount = amount;
        item.taxAmount = taxAmount;
      });
    }

    const taxTypeMap = new Map<string, { rate: number; amount: number }>();
    this.invoiceData.items.forEach((item) => {
      if (item.taxType && item.taxAmount > 0) {
        const taxType = this.taxTypes.find((t) => t.name === item.taxType);
        if (taxType) {
          if (taxTypeMap.has(item.taxType)) {
            const existing = taxTypeMap.get(item.taxType)!;
            existing.amount += item.taxAmount;
            taxTypeMap.set(item.taxType, existing);
          } else {
            taxTypeMap.set(item.taxType, {
              rate: taxType.rate,
              amount: item.taxAmount,
            });
          }
        }
      }
    });

    this.invoiceData.taxDetails = Array.from(taxTypeMap.entries()).map(
      ([taxType, { rate, amount }]) => ({
        taxType,
        rate,
        amount: parseFloat(amount.toFixed(2)),
      })
    );
  }

  trackByItem(index: number, item: any): number {
    return index;
  }

  getTaxAmount(): string {
    const taxAmount = this.invoiceData.taxDetails.reduce(
      (sum, tax) => sum + tax.amount,
      0
    );
    return taxAmount.toFixed(2);
  }

  getDiscountAmount(): string {
    let discountAmount = 0;
    this.invoiceData.discounts.forEach((discount) => {
      if (discount.isPercentage) {
        const subtotal = parseFloat(this.getSubtotal());
        discountAmount += (subtotal * discount.amount) / 100;
      } else {
        discountAmount += discount.amount;
      }
    });
    return discountAmount.toFixed(2);
  }

  getTotal(): string {
    const subtotal = parseFloat(this.getSubtotal());
    const tax = parseFloat(this.getTaxAmount());
    const discount = parseFloat(this.getDiscountAmount());
    return (subtotal + tax - discount).toFixed(2);
  }

  onItemQuantityChange(index: number): void {
    const item = this.invoiceData.items[index];
    if (item.quantity < 1) {
      item.quantity = 1;
    }
    this.itemChangeSubject.next({ index, type: 'quantity' });
  }

  onItemCostChange(index: number): void {
    const item = this.invoiceData.items[index];
    if (item.unitPrice < 0) {
      item.unitPrice = 0;
    }
    this.itemChangeSubject.next({ index, type: 'cost' });
  }

  onTaxTypeChange(): void {
    this.updateTaxDetails();
  }

  onInvoiceTypeChange(): void {
    if (this.invoiceData.isAutomated) {
      this.invoiceData.invoiceNumber = `INV${Date.now()}`;
    } else {
      this.invoiceData.invoiceNumber = '';
    }
  }

  updateCurrency(): void {
    this.updateTaxDetails();
  }

  toggleInvoiceTypeDropdown(): void {
    this.showInvoiceTypeDropdown = !this.showInvoiceTypeDropdown;
    this.showCurrencyDropdown = false;
    this.showPaymentMethodDropdown = false;
  }

  selectInvoiceType(isAutomated: boolean): void {
    this.invoiceData.isAutomated = isAutomated;
    this.onInvoiceTypeChange();
    this.showInvoiceTypeDropdown = false;
  }

  toggleCurrencyDropdown(): void {
    this.showCurrencyDropdown = !this.showCurrencyDropdown;
    this.showInvoiceTypeDropdown = false;
    this.showPaymentMethodDropdown = false;
  }

  selectCurrency(currency: string): void {
    this.invoiceData.currency = currency;
    this.updateCurrency();
    this.showCurrencyDropdown = false;
  }

  togglePaymentMethodDropdown(): void {
    this.showPaymentMethodDropdown = !this.showPaymentMethodDropdown;
    this.showInvoiceTypeDropdown = false;
    this.showCurrencyDropdown = false;
  }

  selectPaymentMethod(method: string): void {
    this.invoiceData.paymentMethod = method;
    this.showPaymentMethodDropdown = false;
  }

  getPaymentMethodDisplayName(value: string): string {
    const method = this.paymentMethods.find(
      (m) => m.value === value.toLowerCase().replace(' ', '_')
    );
    return method ? method.display : 'Select';
  }

  onClickOutside(event: Event): void {
    const target = event.target as HTMLElement;
    if (this.showCustomerDropdown && !target.closest('.recipient-dropdown')) {
      this.showCustomerDropdown = false;
    }
    if (
      this.showCalendar &&
      !target.closest('.calendar-wrapper') &&
      !target.closest('.date-input-wrapper')
    ) {
      this.showCalendar = false;
    }
    if (
      this.showInvoiceTypeDropdown &&
      !target.closest('.invoice-type-dropdown')
    ) {
      this.showInvoiceTypeDropdown = false;
    }
    if (this.showCurrencyDropdown && !target.closest('.currency-dropdown')) {
      this.showCurrencyDropdown = false;
    }
    if (
      this.showPaymentMethodDropdown &&
      !target.closest('.payment-method-dropdown')
    ) {
      this.showPaymentMethodDropdown = false;
    }
  }
  // Drag-and-drop event handlers
  onDragOver(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.isDragging = true;
  }

  onDragLeave(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.isDragging = false;
  }

  onFileDropped(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.isDragging = false;
    const files = event.dataTransfer?.files;
    if (files && files.length > 0) {
      this.handleFiles(files);
    }
  }

  // Handle file selection (both drag-and-drop and browse)
  private handleFiles(files: FileList): void {
    const allowedTypes = ['image/jpeg', 'image/png', 'application/pdf'];
    const maxSize = 5 * 1024 * 1024; // 5MB
    const newAttachments: Attachment[] = [];

    Array.from(files).forEach((file) => {
      if (!allowedTypes.includes(file.type)) {
        this.openDialog(
          'error',
          'Invalid File Type',
          `File ${file.name} is not allowed.`,
          'Only JPEG, PNG, and PDF files are supported.'
        );
        return;
      }
      if (file.size > maxSize) {
        this.openDialog(
          'error',
          'File Too Large',
          `File ${file.name} exceeds the 5MB limit.`,
          'Please upload a file smaller than 5MB.'
        );
        return;
      }
      if (file.name.length > 255) {
        this.openDialog(
          'error',
          'Invalid File Name',
          `File ${file.name} name is too long.`,
          'File name must be less than 255 characters.'
        );
        return;
      }
      const extension = file.name.split('.').pop()?.toLowerCase();
      if (!['jpg', 'jpeg', 'png', 'pdf'].includes(extension || '')) {
        this.openDialog(
          'error',
          'Invalid File Extension',
          `File ${file.name} has an unsupported extension.`,
          'Only .jpg, .jpeg, .png, and .pdf are supported.'
        );
        return;
      }
      const blobUrl = URL.createObjectURL(file);
      this.blobUrls.push(blobUrl);
      newAttachments.push({
        fileName: file.name,
        fileUrl: blobUrl, // For preview only
        file,
      });
    });

    if (newAttachments.length > 0) {
      this.invoiceData.invoiceAttachments = [
        ...this.invoiceData.invoiceAttachments,
        ...newAttachments,
      ];
      this.fileInput.nativeElement.value = ''; // Reset file input
    }
  }

  onInvoiceNumberChange(): void {
    delete this.errors['invoiceNumber'];
    if (
      !this.invoiceData.isAutomated &&
      !this.invoiceData.invoiceNumber.trim()
    ) {
      this.errors['invoiceNumber'] = 'Invoice number is required for manual invoices.';
    } else if (this.invoiceData.invoiceNumber.length > 50) {
      this.errors['invoiceNumber'] = 'Invoice number must be less than 50 characters.';
    } else if (
      this.invoiceData.invoiceNumber &&
      !/^[a-zA-Z0-9-_]+$/.test(this.invoiceData.invoiceNumber.trim())
    ) {
      this.errors['invoiceNumber'] = 'Use only alphanumeric characters, hyphens, or underscores.';
    }
    this.errors = { ...this.errors };
  }

  onPoNumberChange(): void {
    delete this.errors['poNumber'];
    if (this.invoiceData.poNumber && this.invoiceData.poNumber.length > 50) {
      this.errors['poNumber'] = 'PO number must be less than 50 characters.';
    } else if (
      this.invoiceData.poNumber &&
      !/^[a-zA-Z0-9-_]+$/.test(this.invoiceData.poNumber.trim())
    ) {
      this.errors['poNumber'] = 'Use only alphanumeric characters, hyphens, or underscores.';
    }
    this.errors = { ...this.errors };
  }

  onProjectDetailChange(): void {
    delete this.errors['projectDetail'];
    if (
      this.invoiceData.projectDetail &&
      this.invoiceData.projectDetail.length > 500
    ) {
      this.errors['projectDetail'] = 'Project detail must be less than 500 characters.';
    }
    this.errors = { ...this.errors };
  }

  onNotesChange(): void {
    delete this.errors['notes'];
    if (this.invoiceData.notes && this.invoiceData.notes.length > 1000) {
      this.errors['notes'] = 'Notes must be less than 1000 characters.';
    }
    this.errors = { ...this.errors };
  }

  onItemDescriptionChange(index: number): void {
    this.itemChangeSubject.next({ index, type: 'description' });
  }
}
