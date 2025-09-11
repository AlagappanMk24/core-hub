import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
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
  Invoice,
  InvoiceUpsert,
  TaxType,
} from '../../../../interfaces/invoice/invoice.interface';
import { CustomerService } from '../../../../services/customer/customer.service';
import { InvoiceService } from '../../../../services/invoice/invoice.service';
import { NotificationDialogComponent } from '../../../../components/notification/notification-dialog.component';
import { CreateCustomerDialogComponent } from '../create-customer/create-customer-dialog.component';
import { Company } from '../../../../interfaces/company/company.interface';
import { CompanyService } from '../../../../services/company/company.service';
import { AuthService } from '../../../../services/auth/auth.service';
import { InvoiceDisplayComponent } from '../invoice-display/invoice-display/invoice-display.component';

interface Customer {
  id: number;
  name: string;
  address: string;
  initials: string;
  color: string;
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
  };
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
    type: 'quantity' | 'cost';
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
              this.loadInvoiceData(id);
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

  loadInvoiceData(id: string): void {
    this.invoiceService.getInvoiceById(id).subscribe({
      next: (invoice) => {
        // Format dates to dd/mm/yyyy
        const formatDate = (date: Date | string): string => {
          const d = new Date(date);
          return `${d.getDate().toString().padStart(2, '0')}/${(
            d.getMonth() + 1
          )
            .toString()
            .padStart(2, '0')}/${d.getFullYear()}`;
        };
        // Map invoice data to component's invoiceData
        this.invoiceData = {
          id: parseInt(id),
          invoiceNumber: invoice.invoiceNumber,
          poNumber: invoice.poNumber || '',
          projectDetail: invoice.projectDetail || '',
          items: invoice.items.map((item) => {
            // Map taxType to match taxTypes.name (e.g., "GST" -> "GST 10%")
            const taxTypeEntry = this.taxTypes.find(
              (t) =>
                t.name.toLowerCase().startsWith(item.taxType.toLowerCase()) ||
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
        };

        // Set selected customer
        this.customerService.getCustomers(1, 20).subscribe({
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
    this.customerService.getCustomers(1, 20, search).subscribe({
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
    const dialogRef = this.dialog.open(CreateCustomerDialogComponent, {
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
          console.log(taxTypes, 'TaxTypes');
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
            id: String(this.invoiceData.id || '0'),
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
  parseDate(dateStr: string): Date | null {
    if (!dateStr || !/^\d{2}\/\d{2}\/\d{4}$/.test(dateStr)) {
      return null;
    }
    const [day, month, year] = dateStr.split('/').map(Number);
    if (month < 1 || month > 12 || day < 1 || day > 31) {
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

  formatDate(date: Date): string {
    return `${date.getDate().toString().padStart(2, '0')}/${(
      date.getMonth() + 1
    )
      .toString()
      .padStart(2, '0')}/${date.getFullYear()}`;
  }

  validateDates(showDialog: boolean = true): boolean {
    const issueDate = this.parseDate(this.invoiceData.issuedDate);
    const dueDate = this.parseDate(this.invoiceData.dueDate);

    if (this.invoiceData.issuedDate && !issueDate) {
      if (showDialog) {
        this.openDialog(
          'error',
          'Invalid Date Format',
          'Please enter a valid issue date.',
          'Use the format dd/mm/yyyy (e.g., 31/12/2024). Make sure the date exists and is properly formatted.'
        );
      }
      return false;
    }
    if (this.invoiceData.dueDate && !dueDate) {
      if (showDialog) {
        this.openDialog(
          'error',
          'Invalid Date Format',
          'Please enter a valid issue date.',
          'Use the format dd/mm/yyyy (e.g., 31/12/2024). Make sure the date exists and is properly formatted.'
        );
      }
      return false;
    }
    if (issueDate && dueDate && dueDate < issueDate) {
      if (showDialog) {
        this.openDialog(
          'error',
          'Invalid Date Range',
          'Due date cannot be earlier than the issue date.',
          'Please select a due date that comes after the issue date to ensure proper invoice processing.'
        );
      }
      return false;
    }
    return true;
  }

  onIssueDateChange(): void {
    if (!this.invoiceData.issuedDate) {
      return; // Allow empty issue date
    }
    const issueDate = this.parseDate(this.invoiceData.issuedDate);
    if (!issueDate) {
      this.openDialog(
        'error',
        'Invalid Date Format',
        'Please enter a valid issue date.',
        'Use the format dd/mm/yyyy (e.g., 15/01/2024). Make sure the date exists and is correctly formatted.'
      );
      this.invoiceData.issuedDate = '';
      return;
    }
    if (this.invoiceData.dueDate) {
      const dueDate = this.parseDate(this.invoiceData.dueDate);
      if (!dueDate) {
        this.invoiceData.dueDate = '';
        this.openDialog(
          'error',
          'Invalid Date Format',
          'Please enter a valid due date.',
          'The due date format is incorrect. Use dd/mm/yyyy format (e.g., 28/02/2024).'
        );
        return;
      }
      if (dueDate && issueDate && dueDate < issueDate) {
        this.invoiceData.dueDate = '';
        this.openDialog(
          'error',
          'Date Conflict Resolved',
          'Due date has been reset.',
          'The due date was earlier than the issue date, so it has been cleared. Please select a new due date that comes after the issue date.'
        );
      }
    }
  }

  validateDueDate(dueDate: string): void {
    if (!dueDate) {
      this.invoiceData.dueDate = '';
      return; // Allow empty due date
    }
    const parsedDueDate = this.parseDate(this.invoiceData.dueDate);
    if (!parsedDueDate) {
      this.openDialog(
        'error',
        'Invalid Date Format',
        'Please enter a valid due date in the format dd/mm/yyyy.',
        'The due date must be in dd/mm/yyyy format (e.g., 25/03/2024). Please check your input and try again.'
      );
      this.invoiceData.dueDate = '';
      return;
    }
    if (this.invoiceData.issuedDate) {
      const issueDate = this.parseDate(this.invoiceData.issuedDate);
      if (!issueDate) {
        this.invoiceData.issuedDate = '';
        this.openDialog(
          'error',
          'Invalid Date Format',
          'Please enter a valid issue date in the format dd/mm/yyyy.',
          'The issue date must be in dd/mm/yyyy format (e.g., 15/03/2024). Please correct the issue date first.'
        );
        this.invoiceData.dueDate = '';
        return;
      }
      if (issueDate && parsedDueDate < issueDate) {
        this.openDialog(
          'error',
          'Invalid Date Range',
          'Due date cannot be earlier than the issue date. Please select a valid due date.',
          'Choose a due date that comes after the issue date. This ensures proper payment terms and invoice validity.'
        );
        this.invoiceData.dueDate = '';
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
    if (this.validateDates()) {
      this.saveInvoice('Draft');
    }
  }

  saveAndContinue(): void {
    if (this.validateDates()) {
      this.saveInvoice('Draft', true);
    }
  }

  sendInvoice(): void {
    if (this.validateDates()) {
      this.saveInvoice('Sent');
    }
  }

  saveInvoice(
    status: 'Draft' | 'Sent',
    continueEditing: boolean = false
  ): void {
    if (!this.invoiceData.customerId) {
      this.openDialog(
        'error',
        'Missing Customer Information',
        'Please select a customer.',
        'A customer must be selected before saving the invoice. Choose from the customer dropdown or create a new customer.'
      );
      return;
    }
    if (
      !this.invoiceData.items.length ||
      this.invoiceData.items.some(
        (i) => !i.description || i.quantity <= 0 || i.unitPrice < 0
      )
    ) {
      this.openDialog(
        'error',
        'Invalid Item Information',
        'Please ensure all items have valid descriptions, quantities, and prices.',
        'Each item must have a description, quantity greater than 0, and a valid unit price. Please review and correct any incomplete items.'
      );
      return;
    }
    if (
      this.invoiceData.discounts.some((d) => !d.description || d.amount <= 0)
    ) {
      this.openDialog(
        'error',
        'Invalid Discount Information',
        'Please ensure all discounts have valid descriptions and amounts.',
        'Each discount must have a description and an amount greater than 0. Please review and correct any incomplete discounts.'
      );
      return;
    }

    if (!this.invoiceData.issuedDate || !this.invoiceData.dueDate) {
      this.openDialog(
        'error',
        'Missing Date Information',
        'Both issue date and due date are required.',
        'Please provide both an issue date and due date for the invoice. These dates are required for proper invoice processing.'
      );
      return;
    }
    const issueDate = this.parseDate(this.invoiceData.issuedDate);
    const dueDate = this.parseDate(this.invoiceData.dueDate);
    if (!issueDate || !dueDate) {
      this.openDialog(
        'error',
        'Invalid Date Format',
        'Please enter valid issue and due dates in the format dd/mm/yyyy.',
        'Both dates must be in dd/mm/yyyy format (e.g., 15/01/2024). Please check your date entries and correct any formatting errors.'
      );
      return;
    }

    // Update items.amount to reflect quantity * unitPrice
    this.invoiceData.items = this.invoiceData.items.map((item) => ({
      ...item,
      amount: parseFloat((item.quantity * item.unitPrice).toFixed(2)),
    }));

    const payload: InvoiceUpsert = {
      id: this.invoiceData.id,
      invoiceNumber: this.invoiceData.isAutomated
        ? `INV${Date.now()}`
        : this.invoiceData.invoiceNumber.trim(),
      poNumber: this.invoiceData.poNumber.trim(),
      projectDetail: this.invoiceData.projectDetail.trim(),
      issueDate: issueDate,
      dueDate: dueDate,
      type: 'Standard',
      currency: this.invoiceData.currency || 'USD',
      customerId: this.invoiceData.customerId,
      notes: this.invoiceData.notes.trim(),
      paymentMethod: this.invoiceData.paymentMethod.trim(),
      items: this.invoiceData.items.map((i) => ({
        description: i.description.trim(),
        quantity: i.quantity,
        unitPrice: i.unitPrice,
        taxType: i.taxType || '',
        taxAmount: i.taxAmount,
        amount: i.amount,
      })),
      taxDetails: this.invoiceData.taxDetails,
      discounts: this.invoiceData.discounts.map((d) => ({
        description: d.description,
        amount: d.amount,
        isPercentage: d.isPercentage,
      })),
      invoiceStatus: status,
      paymentStatus:
        status === 'Sent'
          ? 'Pending'
          : this.invoiceData.paymentStatus || 'Pending',
    };
    const request = this.isEditMode
      ? this.invoiceService.updateInvoice(payload)
      : this.invoiceService.createInvoice(payload);

    request.subscribe({
      next: (response) => {
        console.log('Invoice saved:', response);
        if (status === 'Sent') {
          this.invoiceService.sendInvoice1(response.model).subscribe({
            next: () => {
              this.openDialog(
                'success',
                'Invoice Sent Successfully',
                'Invoice sent successfully!',
                'The invoice has been saved and sent to the customer. They will receive it via email shortly.'
              );
              if (!continueEditing) {
                this.resetForm();
              }
            },
            error: (err) => {
              console.error('Error sending invoice:', err);
              this.openDialog(
                'error',
                'Send Invoice Failed',
                'Failed to send invoice. Please try again.',
                'The invoice was saved but could not be sent to the customer. You can try sending it again from the invoice list.'
              );
            },
          });
        } else {
          this.openDialog(
            'success',
            'Draft Saved',
            'Invoice saved as draft!',
            'The invoice has been saved as a draft. You can continue editing it later or send it to the customer when ready.'
          );
          if (!continueEditing) {
            this.resetForm();
          }
        }
      },
      error: (err) => {
        console.error('Error saving invoice:', err);
        this.openDialog(
          'error',
          'Save Failed',
          'Failed to save invoice. Please try again.',
          'The invoice could not be saved due to a system error. Please check your internet connection and try again.'
        );
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
    };
    this.selectedCustomer =
      this.customers.length > 0 ? this.customers[0] : null;
    this.addItem();
    this.resetCalendarState();
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
    console.log(value, 'value');
    const method = this.paymentMethods.find(
      (m) => m.value === value.toLowerCase().replace(' ', '_')
    );
    console.log(method, 'method');
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
}
