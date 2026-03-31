// components/invoice/create-invoice/create-invoice.component.ts

import { CommonModule } from '@angular/common';
import {
  Component,
  ElementRef,
  HostListener,
  OnInit,
  ViewChild,
} from '@angular/core';
import { FormsModule, ValidationErrors } from '@angular/forms';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSelectModule } from '@angular/material/select';
import { MatInputModule } from '@angular/material/input';
import { debounceTime, Observable, Subject } from 'rxjs';
import { ActivatedRoute, Router } from '@angular/router';

// Interfaces & Types
import {
  Attachment,
  Invoice,
  TaxType,
} from '../../../../interfaces/invoice/invoice.interface';
import {
  CustomerDisplay,
  DiscountTypeDisplay,
  InvoiceFormData,
} from '../../../../types/invoice-form.types';

// Services
import { CustomerService } from '../../../../services/customer/customer.service';
import { InvoiceService } from '../../../../services/invoice/invoice.service';
import { CompanyService } from '../../../../services/company/company.service';
import { AuthService } from '../../../../services/auth/auth.service';

// Components & Dialogs
import { NotificationDialogComponent } from '../../../../components/notification/notification-dialog.component';
import { InvoiceDisplayComponent } from '../invoice-display/invoice-display/invoice-display.component';
import { CustomerDialogComponent } from '../customer-dialog/customer-dialog.component';

// Models & Constants
import { CustomerFilterRequest } from '../../../../services/customer/models/customer.model';
import {
  AVATAR_COLORS,
  DEFAULT_COMPANY,
  DEFAULT_INVOICE_FORM_DATA,
  DISCOUNT_TYPES,
  discountTypeToDisplayValue,
  discountTypeToNumber,
  MONTHS,
  PAYMENT_METHODS,
  SUPPORTED_CURRENCIES,
  WEEKDAYS,
} from '../../../../constants/invoice.constants';
import { environment } from '../../../../environments/environment.development';

/**
 * Component for creating and editing invoices
 * Supports both manual and automated invoice creation with full CRUD operations
 */
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
  // ──────────────────────────────────────────────────────────────────
  // ViewChild References
  // ──────────────────────────────────────────────────────────────────
  @ViewChild('fileInput') fileInput!: ElementRef<HTMLInputElement>;

  // ──────────────────────────────────────────────────────────────────
  // Invoice Data
  // ──────────────────────────────────────────────────────────────────
  invoiceData: InvoiceFormData = { ...DEFAULT_INVOICE_FORM_DATA };
  errors: ValidationErrors = {};

  // ──────────────────────────────────────────────────────────────────
  // Company & Customer Data
  // ──────────────────────────────────────────────────────────────────
  company: any = null;
  customers: CustomerDisplay[] = [];
  filteredCustomers: CustomerDisplay[] = [];
  selectedCustomer: CustomerDisplay | null = null;
  showCustomerDropdown: boolean = false;
  customerSearch: string = '';
  private searchSubject: Subject<string> = new Subject();

  // ──────────────────────────────────────────────────────────────────
  // Calendar & Date Handling
  // ──────────────────────────────────────────────────────────────────
  showCalendar: boolean = false;
  calendarType: 'issued' | 'due' = 'issued';
  currentMonth: number = 0;
  currentYear: number = 2026;
  selectedDay: number = 0;

  // ──────────────────────────────────────────────────────────────────
  // Tax Management
  // ──────────────────────────────────────────────────────────────────
  taxTypes: TaxType[] = [];
  newTaxType: { name: string; rate: number } = { name: '', rate: 0 };
  showTaxDropdown: boolean = false;
  currentTaxItemIndex: number = -1;

  // UI Constants
  readonly months = MONTHS;
  readonly weekdays = WEEKDAYS;
  readonly years: number[] = Array.from(
    { length: 10 },
    (_, i) => new Date().getFullYear() - 5 + i,
  );
  readonly paymentMethods = PAYMENT_METHODS;
  readonly discountTypes = DISCOUNT_TYPES;

  // Payment terms options for dropdown
  readonly paymentTermOptions = [
    { value: 'Due on Receipt', label: 'Due on Receipt', days: 0 },
    { value: 'Net 7', label: 'Net 7 Days', days: 7 },
    { value: 'Net 15', label: 'Net 15 Days', days: 15 },
    { value: 'Net 30', label: 'Net 30 Days', days: 30 },
    { value: 'Net 45', label: 'Net 45 Days', days: 45 },
    { value: 'Net 60', label: 'Net 60 Days', days: 60 },
    { value: 'Custom', label: 'Custom (Enter Below)', days: null },
  ];

  // Flag to show/hide custom payment terms input
  showCustomPaymentTerms: boolean = false;

  // ──────────────────────────────────────────────────────────────────
  // Notes Section Toggle
  // ──────────────────────────────────────────────────────────────────
  showNotesSection: boolean = false;
  notesSections = {
    customerNotes: false,
    internalNotes: false,
    termsAndConditions: false,
    footerNote: false,
  };

  // ──────────────────────────────────────────────────────────────────
  // UI State Flags
  // ──────────────────────────────────────────────────────────────────
  showInvoiceTypeDropdown: boolean = false;
  showCurrencyDropdown: boolean = false;
  showPaymentMethodDropdown: boolean = false;
  showPaymentTermsDropdown: boolean = false;

  // ──────────────────────────────────────────────────────────────────
  // Edit Mode & State Management
  // ──────────────────────────────────────────────────────────────────
  isEditMode: boolean = false;
  invoiceId: string | null = null;
  private blobUrls: string[] = [];
  isDragging: boolean = false;

  // ──────────────────────────────────────────────────────────────────
  // Constructor & Dependency Injection
  // ──────────────────────────────────────────────────────────────────
  constructor(
    private customerService: CustomerService,
    private invoiceService: InvoiceService,
    private companyService: CompanyService,
    private authService: AuthService,
    private dialog: MatDialog,
    private router: Router,
    private route: ActivatedRoute,
  ) {}

  // ──────────────────────────────────────────────────────────────────
  // Lifecycle Hooks
  // ──────────────────────────────────────────────────────────────────
  ngOnInit(): void {
    this.initializeDates();
    this.loadInitialData();
  }

  ngOnDestroy(): void {
    this.cleanupBlobUrls();
  }

  // ──────────────────────────────────────────────────────────────────
  // Initialization Methods
  // ──────────────────────────────────────────────────────────────────
  /** Initializes issue and due dates */
  private initializeDates(): void {
    const today = new Date();
    this.currentMonth = today.getMonth();
    this.currentYear = today.getFullYear();

    this.invoiceData.issuedDate = this.formatDate(today);
    const dueDate = new Date(today);
    dueDate.setDate(today.getDate() + 30);
    this.invoiceData.dueDate = this.formatDate(dueDate);
  }

  /** Loads all required initial data */
  private loadInitialData(): void {
    this.loadCompanyData().subscribe({
      next: () => {
        this.loadTaxTypes().subscribe(() => {
          this.loadCustomers();
          this.addItem();
          this.setupSearchDebounce();
          this.checkEditMode();
        });
      },
      error: () => {
        this.handleCompanyLoadError();
      },
    });
  }

  /** Checks if in edit mode and loads invoice data if needed */
  private checkEditMode(): void {
    this.route.paramMap.subscribe((params) => {
      const id = params.get('id');
      if (id) {
        this.isEditMode = true;
        this.invoiceId = id;
        this.loadInvoiceData(Number(id));
      }
    });
  }

  /** Handles company data load error with fallback to default */
  private handleCompanyLoadError(): void {
    this.company = DEFAULT_COMPANY;
    this.openDialog(
      'error',
      'Error',
      'Failed to load company data.',
      'Unable to retrieve company information. A default company profile will be used.',
    );
  }

  // ──────────────────────────────────────────────────────────────────
  // Company Data Methods
  // ──────────────────────────────────────────────────────────────────
  loadCompanyData(): Observable<void> {
    return new Observable((observer) => {
      const userDetails = this.authService.getUserDetail();
      if (!userDetails?.companyId) {
        observer.error(
          new Error('User not authenticated or no company ID found'),
        );
        return;
      }

      this.companyService.getCompanyById(userDetails.companyId).subscribe({
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
    });
  }

  /** Gets company initials for avatar display */
  getCompanyInitials(): string {
    if (!this.company?.name) return 'AC';
    return this.company.name
      .split(' ')
      .map((n: string) => n[0])
      .join('')
      .substring(0, 2)
      .toUpperCase();
  }

  /** Gets formatted company address */
  getCompanyAddress(): string {
    if (!this.company || !this.company.address)
      return 'Besant Nagar, Chennai 45';
    const { address1, address2, city, state, zipCode, country } =
      this.company.address;
    return [address1, address2, city, state, zipCode, country]
      .filter((part) => part)
      .join(', ');
  }

  // ──────────────────────────────────────────────────────────────────
  // Invoice Data Loading & Mapping
  // ──────────────────────────────────────────────────────────────────
  loadInvoiceData(id: number): void {
    this.invoiceService.getInvoiceById(id).subscribe({
      next: (invoice) => {
        this.invoiceData = this.mapInvoiceToFormData(invoice);
        this.selectExistingCustomer(invoice.customerId);
        this.updateTaxDetails();
        this.setPaymentTermFromValue(invoice.paymentTerms);
        // Update due date based on payment terms if needed
        if (invoice.paymentTerms) {
          const matchedTerm = this.paymentTermOptions.find(
            (t) => t.value === invoice.paymentTerms,
          );
          if (
            matchedTerm &&
            matchedTerm.days !== null &&
            matchedTerm.days > 0
          ) {
            this.updateDueDateFromPaymentTerms(matchedTerm.days);
          }
        }
      },
      error: (err) => {
        console.error('Error fetching invoice:', err);
        this.openDialog(
          'error',
          'Error',
          'Failed to load invoice data.',
          'The requested invoice could not be found.',
        );
      },
    });
  }
  private updateDueDateFromPaymentTerms(days: number): void {
    const issueDate = this.parseDate(this.invoiceData.issuedDate);
    if (issueDate) {
      const newDueDate = new Date(issueDate);
      newDueDate.setDate(issueDate.getDate() + days);
      this.invoiceData.dueDate = this.formatDate(newDueDate);
    }
  }

  private setPaymentTermFromValue(paymentTerms: string | undefined): void {
    if (!paymentTerms) return;

    const matchedOption = this.paymentTermOptions.find(
      (opt) => opt.value === paymentTerms,
    );

    if (matchedOption) {
      this.invoiceData.paymentTerms = paymentTerms;
      this.showCustomPaymentTerms = false;
    } else {
      this.invoiceData.paymentTerms = paymentTerms;
      this.showCustomPaymentTerms = true;
    }
  }

  /** Maps Invoice entity to form data structure */
  private mapInvoiceToFormData(invoice: Invoice): InvoiceFormData {
    const formatDate = (date: Date | string): string => {
      const d = new Date(date);
      return isNaN(d.getTime()) ? '' : this.formatDate(d);
    };

    return {
      id: invoice.id,
      invoiceNumber: invoice.invoiceNumber,
      poNumber: invoice.poNumber || '',
      projectDetail: invoice.projectDetail || '',
      items: invoice.items.map((item) => ({
        id: item.id,
        description: item.description,
        quantity: item.quantity,
        unitPrice: item.unitPrice,
        taxType: item.taxType || '',
        taxAmount: item.taxAmount,
        amount: item.amount,
      })),
      taxDetails: invoice.taxDetails.map((tax) => ({
        id: tax.id,
        taxName: tax.taxName,
        rate: tax.rate,
        taxAmount: tax.taxAmount,
      })),
      discounts: invoice.discounts.map((discount) => ({
        id: discount.id,
        description: discount.description,
        discountType: discountTypeToDisplayValue(discount.discountType),
        amount: discount.amount,
      })),
      paymentMethod: invoice.paymentMethod || '',
      paymentTerms: invoice.paymentTerms || '',
      customerNotes: invoice.customerNotes || '',
      internalNotes: invoice.internalNotes || '',
      termsAndConditions: invoice.termsAndConditions || '',
      footerNote: invoice.footerNote || '',
      issuedDate: formatDate(invoice.issueDate),
      dueDate: formatDate(invoice.dueDate),
      customerId: invoice.customerId,
      currency: invoice.currency,
      currencyRate: 0,
      isAutomated: invoice.isAutomated,
      shippingAmount: invoice.shippingAmount,
      adjustmentAmount: invoice.adjustmentAmount,
      adjustmentDescription: invoice.adjustmentDescription,
      invoiceAttachments: invoice.invoiceAttachments.map((attachment) => ({
        id: attachment.id,
        fileName: attachment.fileName,
        fileUrl: `${environment.apiBaseUrl}/${this.company?.id}/${invoice.id}/${encodeURIComponent(attachment.fileName)}`,
      })),
    };
  }

  // ──────────────────────────────────────────────────────────────────
  // Customer Management
  // ──────────────────────────────────────────────────────────────────
  selectExistingCustomer(customerId: number): void {
    const filter: CustomerFilterRequest = {
      pageNumber: 1,
      pageSize: 20,
      search: '',
      status: 'Active',
    };
    this.customerService.getCustomers(filter).subscribe({
      next: (response) => {
        this.customers = this.mapCustomersToDisplay(response.items);
        this.filteredCustomers = this.customers;
        this.selectedCustomer =
          this.customers.find((c) => c.id === customerId) || null;
      },
      error: (err) => console.error('Error fetching customers:', err),
    });
  }

  /** Maps raw customer data to display format */
  private mapCustomersToDisplay(customers: any[]): CustomerDisplay[] {
    return customers.map((c) => ({
      id: c.id,
      name: c.name,
      address: `${c.address.address1}, ${c.address.city}, ${c.address.country}`,
      initials: c.name
        .split(' ')
        .map((n: string) => n[0])
        .join('')
        .substring(0, 2)
        .toUpperCase(),
      color: this.getRandomColor(),
    }));
  }

  setupSearchDebounce(): void {
    this.searchSubject.pipe(debounceTime(300)).subscribe((search) => {
      this.loadCustomers(search);
    });
  }

  loadCustomers(search: string = ''): void {
    const filter: CustomerFilterRequest = {
      pageNumber: 1,
      pageSize: 20,
      search,
      status: 'Active',
    };
    this.customerService.getCustomers(filter).subscribe({
      next: (response) => {
        this.customers = this.mapCustomersToDisplay(response.items);
        this.filteredCustomers = this.customers;
      },
      error: (err) => console.error('Error fetching customers:', err),
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
        const newCustomer: CustomerDisplay = {
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

  // ──────────────────────────────────────────────────────────────────
  // Tax Management
  // ──────────────────────────────────────────────────────────────────
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
          observer.error(err);
        },
      });
    });
  }

  addTaxType(): void {
    if (!this.newTaxType.name || this.newTaxType.rate <= 0) {
      this.openDialog(
        'error',
        'Invalid Input',
        'Please provide a valid tax name and rate.',
        'Both tax name and rate are required. Rate must be greater than 0.',
      );
      return;
    }

    this.invoiceService
      .createTaxType({ name: this.newTaxType.name, rate: this.newTaxType.rate })
      .subscribe({
        next: (createdTaxType) => {
          this.taxTypes = [...this.taxTypes, createdTaxType];
          this.newTaxType = { name: '', rate: 0 };
          this.openDialog(
            'success',
            'Success',
            'Tax type added successfully!',
            'The new tax type has been created and is now available.',
          );
        },
        error: (err) => {
          console.error('Error adding tax type:', err);
          this.openDialog(
            'error',
            'Error',
            'Failed to add tax type.',
            'The tax type could not be created. Please check if a tax type with the same name already exists.',
          );
        },
      });
  }

  // ──────────────────────────────────────────────────────────────────
  // Date Handling
  // ──────────────────────────────────────────────────────────────────
  parseDate(dateStr: string): Date | null {
    if (!dateStr) return null;
    const ddmmyyyyRegex = /^\d{2}\/\d{2}\/\d{4}$/;
    const yyyymmddRegex = /^\d{4}-\d{2}-\d{2}$/;
    let day: number, month: number, year: number;

    if (ddmmyyyyRegex.test(dateStr)) {
      [day, month, year] = dateStr.split('/').map(Number);
    } else if (yyyymmddRegex.test(dateStr)) {
      [year, month, day] = dateStr.split('-').map(Number);
    } else {
      return null;
    }

    if (month < 1 || month > 12 || day < 1 || day > 31 || year < 1900)
      return null;
    const date = new Date(year, month - 1, day);
    if (
      date.getMonth() + 1 !== month ||
      date.getDate() !== day ||
      date.getFullYear() !== year
    )
      return null;
    return date;
  }

  formatDate(date: Date): string {
    if (!date || isNaN(date.getTime())) return '';
    return `${date.getDate().toString().padStart(2, '0')}/${(date.getMonth() + 1).toString().padStart(2, '0')}/${date.getFullYear()}`;
  }

  formatDateOnly(date: Date): string {
    if (!date || isNaN(date.getTime()))
      throw new Error('Invalid date provided');
    return date.toISOString().split('T')[0];
  }

  validateDates(showErrors: boolean = true): boolean {
    delete this.errors['issuedDate'];
    delete this.errors['dueDate'];

    const issueDate = this.parseDate(this.invoiceData.issuedDate);
    const dueDate = this.parseDate(this.invoiceData.dueDate);

    if (this.invoiceData.issuedDate && !issueDate) {
      if (showErrors)
        this.errors['issuedDate'] = 'Enter a valid issue date (dd/mm/yyyy).';
      return false;
    }
    if (this.invoiceData.dueDate && !dueDate) {
      if (showErrors)
        this.errors['dueDate'] = 'Enter a valid due date (dd/mm/yyyy).';
      return false;
    }
    if (issueDate && dueDate && dueDate < issueDate) {
      if (showErrors)
        this.errors['dueDate'] = 'Due date must be after issue date.';
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
    // Also update due date based on payment terms if a standard term is selected
    if (this.invoiceData.paymentTerms && !this.showCustomPaymentTerms) {
      const matchedTerm = this.paymentTermOptions.find(
        (t) => t.value === this.invoiceData.paymentTerms,
      );
      if (matchedTerm && matchedTerm.days !== null && matchedTerm.days > 0) {
        this.updateDueDateFromPaymentTerms(matchedTerm.days);
      }
    }
  }

  onDueDateChange(): void {
    this.validateDates();
  }

  // ──────────────────────────────────────────────────────────────────
  // Calendar UI Methods
  // ──────────────────────────────────────────────────────────────────

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
        const today = new Date();
        this.currentMonth = today.getMonth();
        this.currentYear = today.getFullYear();
        this.selectedDay = 0;
      }
    }
  }

  getCalendarDays(): number[] {
    const firstDay = new Date(this.currentYear, this.currentMonth, 1);
    const lastDay = new Date(this.currentYear, this.currentMonth + 1, 0);
    const daysInMonth = lastDay.getDate();
    const startingDayOfWeek = firstDay.getDay();
    const days: number[] = [];

    for (let i = 0; i < startingDayOfWeek; i++) days.push(0);
    for (let day = 1; day <= daysInMonth; day++) days.push(day);
    while (days.length < 42) days.push(0);

    return days;
  }

  selectDay(day: number): void {
    if (day <= 0) return;

    const selectedDate = new Date(this.currentYear, this.currentMonth, day);
    const formattedDate = `${day.toString().padStart(2, '0')}/${(this.currentMonth + 1).toString().padStart(2, '0')}/${this.currentYear}`;

    if (this.calendarType === 'issued') {
      this.invoiceData.issuedDate = formattedDate;
      this.onIssueDateChange();
    } else {
      this.invoiceData.dueDate = formattedDate;
      this.onDueDateChange();
    }
    this.selectedDay = day;
    this.showCalendar = false;
  }

  // ──────────────────────────────────────────────────────────────────
  // Item Management
  // ──────────────────────────────────────────────────────────────────
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
  // Disable remove button when only one item exists
  canRemoveItem(): boolean {
    return this.invoiceData.items.length > 1;
  }

  removeItem(index: number): void {
    if (this.canRemoveItem()) {
      this.invoiceData.items.splice(index, 1);
      this.updateTaxDetails();
    }
  }

  onItemQuantityChange(index: number): void {
    const item = this.invoiceData.items[index];
    if (item.quantity < 1) item.quantity = 1;
    this.updateTaxDetails(index);
  }

  onItemCostChange(index: number): void {
    const item = this.invoiceData.items[index];
    if (item.unitPrice < 0) item.unitPrice = 0;
    this.updateTaxDetails(index);
  }

  onItemDescriptionChange(index: number): void {
    this.validateItem(index, 'description');
  }

  validateItem(
    index: number,
    type: 'quantity' | 'cost' | 'description' | 'taxType',
  ): void {
    const item = this.invoiceData.items[index];
    delete this.errors[`items[${index}].description`];
    delete this.errors[`items[${index}].quantity`];
    delete this.errors[`items[${index}].unitPrice`];

    if (type === 'description' && !item.description.trim()) {
      this.errors[`items[${index}].description`] = 'Description is required.';
    }
    if (
      type === 'quantity' &&
      (item.quantity < 1 || !Number.isInteger(item.quantity))
    ) {
      this.errors[`items[${index}].quantity`] =
        'Quantity must be a positive integer.';
      item.quantity = 1;
    }
    if (type === 'cost' && (item.unitPrice < 0 || isNaN(item.unitPrice))) {
      this.errors[`items[${index}].unitPrice`] =
        'Unit price must be non-negative.';
      item.unitPrice = 0;
    }
  }

  // ──────────────────────────────────────────────────────────────────
  // Discount Management
  // ──────────────────────────────────────────────────────────────────
  canAddNewDiscount(): boolean {
    // Check if any discount has empty description or zero amount
    const hasIncompleteDiscount = this.invoiceData.discounts.some(
      (discount) => !discount.description.trim() || discount.amount <= 0,
    );
    return !hasIncompleteDiscount;
  }

  addDiscount(): void {
    if (this.canAddNewDiscount()) {
      this.invoiceData.discounts.push({
        description: '',
        discountType: 'Fixed', // Default to Fixed
        amount: 0,
      });
    } else {
      this.openDialog(
        'error',
        'Complete Current Discount',
        'Please complete the current discount entry before adding a new one.',
        'Each discount requires a description and a positive amount.',
      );
    }
  }
  removeDiscount(index: number): void {
    this.invoiceData.discounts.splice(index, 1);
    this.updateTotals();
  }

  // ──────────────────────────────────────────────────────────────────
  // Notes Section Toggle
  // ──────────────────────────────────────────────────────────────────
  toggleNotesSection(): void {
    this.showNotesSection = !this.showNotesSection;
  }

  toggleNoteSection(section: keyof typeof this.notesSections): void {
    this.notesSections[section] = !this.notesSections[section];
  }

  // ──────────────────────────────────────────────────────────────────
  // Payment Terms Handling
  // ──────────────────────────────────────────────────────────────────
  onPaymentTermChange(selectedValue: string): void {
    if (selectedValue === 'Custom') {
      this.showCustomPaymentTerms = true;
      this.invoiceData.paymentTerms = '';
    } else {
      this.showCustomPaymentTerms = false;
      this.invoiceData.paymentTerms = selectedValue;

      // Update due date based on selected payment terms
      const matchedTerm = this.paymentTermOptions.find(
        (t) => t.value === selectedValue,
      );
      if (matchedTerm && matchedTerm.days !== null && matchedTerm.days > 0) {
        this.updateDueDateFromPaymentTerms(matchedTerm.days);
      }
    }
  }

  // ──────────────────────────────────────────────────────────────────
  // Financial Calculations
  // ──────────────────────────────────────────────────────────────────
  updateTotals(): void {
    this.updateTaxDetails();
  }

  getSubtotal(): string {
    const subtotal = this.invoiceData.items.reduce(
      (sum, item) => sum + item.quantity * item.unitPrice,
      0,
    );
    return subtotal.toFixed(2);
  }

  getDiscountTotal(): string {
    let discountAmount = 0;
    const subtotal = parseFloat(this.getSubtotal());
    this.invoiceData.discounts.forEach((discount) => {
      if (discount.discountType === 'Percentage') {
        discountAmount += (subtotal * discount.amount) / 100;
      } else {
        discountAmount += discount.amount;
      }
    });
    return discountAmount.toFixed(2);
  }

  getTaxTotal(): string {
    const taxAmount = this.invoiceData.taxDetails.reduce(
      (sum, tax) => sum + (tax.taxAmount || 0),
      0,
    );
    return taxAmount.toFixed(2);
  }

  getTotal(): string {
    const subtotal = parseFloat(this.getSubtotal());
    const discount = parseFloat(this.getDiscountTotal());
    const tax = parseFloat(this.getTaxTotal());
    const shipping = this.invoiceData.shippingAmount || 0;
    const adjustment = this.invoiceData.adjustmentAmount || 0;
    return (subtotal - discount + tax + shipping + adjustment).toFixed(2);
  }

  updateTaxDetails(index?: number): void {
    if (index !== undefined) {
      this.updateSingleItemTax(index);
    } else {
      this.updateAllItemsTax();
    }
    this.aggregateTaxDetails();
  }

  private updateSingleItemTax(index: number): void {
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
  }

  private updateAllItemsTax(): void {
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

  private aggregateTaxDetails(): void {
    const taxTypeMap = new Map<string, { rate: number; taxAmount: number }>();

    this.invoiceData.items.forEach((item) => {
      if (item.taxType && item.taxAmount > 0) {
        const taxType = this.taxTypes.find((t) => t.name === item.taxType);
        if (taxType) {
          if (taxTypeMap.has(item.taxType)) {
            const existing = taxTypeMap.get(item.taxType)!;
            existing.taxAmount += item.taxAmount;
            taxTypeMap.set(item.taxType, existing);
          } else {
            taxTypeMap.set(item.taxType, {
              rate: taxType.rate,
              taxAmount: item.taxAmount,
            });
          }
        }
      }
    });

    this.invoiceData.taxDetails = Array.from(taxTypeMap.entries()).map(
      ([taxName, { rate, taxAmount }]) => ({
        taxName,
        rate,
        taxAmount: parseFloat(taxAmount.toFixed(2)),
      }),
    );
  }

  onDiscountTypeChange(index: number, type: DiscountTypeDisplay): void {
    const discount = this.invoiceData.discounts[index];
    if (discount) {
      discount.discountType = type;
      this.updateTotals(); // Recalculate totals when discount type changes
    }
  }
  
  onTaxTypeChange(): void {
    this.updateTaxDetails();
  }

  toggleTaxDropdown(itemIndex: number): void {
    this.currentTaxItemIndex = itemIndex;
    this.showTaxDropdown = !this.showTaxDropdown;
  }

  selectTax(itemIndex: number, taxName: string): void {
    if (itemIndex >= 0 && this.invoiceData.items[itemIndex]) {
      this.invoiceData.items[itemIndex].taxType = taxName;
      this.updateTaxDetails(itemIndex);
    }
    this.showTaxDropdown = false;
    this.currentTaxItemIndex = -1;
  }
  // Optional: Close dropdown when clicking outside
  @HostListener('document:click', ['$event'])
  onClickOutside(event: Event): void {
    const target = event.target as HTMLElement;
    if (!target.closest('.custom-dropdown')) {
      this.showTaxDropdown = false;
    }
  }

  // ──────────────────────────────────────────────────────────────────
  // Validation
  // ──────────────────────────────────────────────────────────────────
  validateInvoiceData(showErrors: boolean = true): boolean {
    this.errors = {};
    let isValid = true;

    // Validate Invoice Number
    if (!this.validateInvoiceNumber(showErrors)) isValid = false;

    // Validate PO Number
    if (!this.validatePONumber(showErrors)) isValid = false;

    // Validate Project Detail
    if (!this.validateProjectDetail(showErrors)) isValid = false;

    // Validate Customer
    if (!this.validateCustomer(showErrors)) isValid = false;

    // Validate Items
    if (!this.validateItems(showErrors)) isValid = false;

    // Validate Payment Method
    if (!this.validatePaymentMethod(showErrors)) isValid = false;

    // Validate Currency
    if (!this.validateCurrency(showErrors)) isValid = false;

    // Validate Dates
    if (!this.validateDates(showErrors)) isValid = false;

    // Validate Company
    if (!this.validateCompany(showErrors)) isValid = false;

    return isValid;
  }

  private validateInvoiceNumber(showErrors: boolean): boolean {
    if (
      !this.invoiceData.isAutomated &&
      !this.invoiceData.invoiceNumber.trim()
    ) {
      if (showErrors)
        this.errors['invoiceNumber'] =
          'Invoice number is required for manual invoices.';
      return false;
    }
    if (this.invoiceData.invoiceNumber.length > 50) {
      if (showErrors)
        this.errors['invoiceNumber'] =
          'Invoice number must be less than 50 characters.';
      return false;
    }
    if (
      this.invoiceData.invoiceNumber &&
      !/^[a-zA-Z0-9-_]+$/.test(this.invoiceData.invoiceNumber.trim())
    ) {
      if (showErrors)
        this.errors['invoiceNumber'] =
          'Use only alphanumeric characters, hyphens, or underscores.';
      return false;
    }
    return true;
  }

  private validatePONumber(showErrors: boolean): boolean {
    if (this.invoiceData.poNumber && this.invoiceData.poNumber.length > 50) {
      if (showErrors)
        this.errors['poNumber'] = 'PO number must be less than 50 characters.';
      return false;
    }
    return true;
  }

  private validateProjectDetail(showErrors: boolean): boolean {
    if (
      this.invoiceData.projectDetail &&
      this.invoiceData.projectDetail.length > 500
    ) {
      if (showErrors)
        this.errors['projectDetail'] =
          'Project detail must be less than 500 characters.';
      return false;
    }
    return true;
  }

  private validateCustomer(showErrors: boolean): boolean {
    if (!this.invoiceData.customerId || this.invoiceData.customerId <= 0) {
      if (showErrors) this.errors['customerId'] = 'Please select a customer.';
      return false;
    }
    return true;
  }

  private validateItems(showErrors: boolean): boolean {
    if (!this.invoiceData.items.length) {
      if (showErrors) this.errors['items'] = 'At least one item is required.';
      return false;
    }

    let isValid = true;
    this.invoiceData.items.forEach((item, i) => {
      if (!item.description.trim()) {
        if (showErrors)
          this.errors[`items[${i}].description`] = 'Description is required.';
        isValid = false;
      }
      if (item.quantity < 1 || !Number.isInteger(item.quantity)) {
        if (showErrors)
          this.errors[`items[${i}].quantity`] =
            'Quantity must be a positive integer.';
        isValid = false;
      }
      if (item.unitPrice < 0 || isNaN(item.unitPrice)) {
        if (showErrors)
          this.errors[`items[${i}].unitPrice`] =
            'Unit price must be non-negative.';
        isValid = false;
      }
    });
    return isValid;
  }

  private validatePaymentMethod(showErrors: boolean): boolean {
    if (!this.invoiceData.paymentMethod) {
      if (showErrors)
        this.errors['paymentMethod'] = 'Please select a payment method.';
      return false;
    }
    return true;
  }

  private validateCurrency(showErrors: boolean): boolean {
    if (!SUPPORTED_CURRENCIES.includes(this.invoiceData.currency as any)) {
      if (showErrors)
        this.errors['currency'] = 'Please select a valid currency.';
      return false;
    }
    return true;
  }

  private validateCompany(showErrors: boolean): boolean {
    if (!this.company?.id) {
      if (showErrors) {
        this.openDialog(
          'error',
          'Missing Company Data',
          'Company information is required.',
          'Please ensure you are logged in with a valid company account.',
        );
      }
      return false;
    }
    return true;
  }

  // ──────────────────────────────────────────────────────────────────
  // Save Operations
  // ──────────────────────────────────────────────────────────────────
  saveInvoice(
    status: 'Draft' | 'Sent',
    continueEditing: boolean = false,
  ): void {
    if (!this.validateInvoiceData()) return;

    const issueDate = this.parseDate(this.invoiceData.issuedDate)!;
    const dueDate = this.parseDate(this.invoiceData.dueDate)!;
    const formData = this.buildFormData(status, issueDate, dueDate);

    const request =
      this.isEditMode && this.invoiceData.id
        ? this.invoiceService.updateInvoice(formData)
        : this.invoiceService.createInvoice(formData);

    request.subscribe({
      next: (response) =>
        this.handleSaveSuccess(response, status, continueEditing),
      error: (err) => this.handleSaveError(err),
    });
  }

  private buildFormData(
    status: 'Draft' | 'Sent',
    issueDate: Date,
    dueDate: Date,
  ): FormData {
    const formData = new FormData();

    // Basic invoice fields
    formData.append(
      'InvoiceNumber',
      this.invoiceData.isAutomated
        ? `INV${Date.now()}`
        : this.invoiceData.invoiceNumber.trim(),
    );
    formData.append('PONumber', this.invoiceData.poNumber?.trim() || '');
    formData.append(
      'ProjectDetail',
      this.invoiceData.projectDetail?.trim() || '',
    );
    formData.append('IssueDate', this.formatDateOnly(issueDate));
    formData.append('DueDate', this.formatDateOnly(dueDate));
    formData.append('Type', 'Standard');
    formData.append('Currency', this.invoiceData.currency || 'USD');
    formData.append('CustomerId', this.invoiceData.customerId.toString());
    formData.append(
      'CustomerNotes',
      this.invoiceData.customerNotes?.trim() || '',
    );
    formData.append(
      'InternalNotes',
      this.invoiceData.internalNotes?.trim() || '',
    );
    formData.append(
      'TermsAndConditions',
      this.invoiceData.termsAndConditions?.trim() || '',
    );
    formData.append('FooterNote', this.invoiceData.footerNote?.trim() || '');
    formData.append(
      'PaymentMethod',
      this.invoiceData.paymentMethod?.trim() || '',
    );
    formData.append(
      'PaymentTerms',
      this.invoiceData.paymentTerms?.trim() || 'Net 30',
    );
    formData.append(
      'ShippingAmount',
      (this.invoiceData.shippingAmount || 0).toString(),
    );
    formData.append(
      'AdjustmentAmount',
      (this.invoiceData.adjustmentAmount || 0).toString(),
    );
    formData.append(
      'AdjustmentDescription',
      this.invoiceData.adjustmentDescription?.trim() || '',
    );
    formData.append('InvoiceStatus', status);
    formData.append('PaymentStatus', status === 'Sent' ? 'Pending' : 'Pending');
    formData.append('IsAutomated', this.invoiceData.isAutomated.toString());

    if (this.isEditMode && this.invoiceData.id) {
      formData.append('Id', this.invoiceData.id.toString());
    }

    // Append collections
    this.appendItemsToFormData(formData);
    this.appendTaxDetailsToFormData(formData);
    this.appendDiscountsToFormData(formData);
    this.appendAttachmentsToFormData(formData);

    // To see everything in the FormData
    console.log('--- FormData Contents ---');
    formData.forEach((value, key) => {
      // Filter to only show Attachment keys if the log is too noisy
      if (key.includes('Attachments')) {
        console.log(`${key}:`, value);
      }
    });
    return formData;
  }

  private appendItemsToFormData(formData: FormData): void {
    this.invoiceData.items.forEach((item, index) => {
      formData.append(`Items[${index}].Description`, item.description.trim());
      formData.append(`Items[${index}].Quantity`, item.quantity.toString());
      formData.append(`Items[${index}].UnitPrice`, item.unitPrice.toString());
      formData.append(`Items[${index}].TaxType`, item.taxType || '');
      formData.append(`Items[${index}].TaxAmount`, item.taxAmount.toString());
      formData.append(
        `Items[${index}].Amount`,
        (item.quantity * item.unitPrice).toFixed(2),
      );
      if (item.id) formData.append(`Items[${index}].Id`, item.id.toString());
    });
  }

  private appendTaxDetailsToFormData(formData: FormData): void {
    this.invoiceData.taxDetails.forEach((tax, index) => {
      formData.append(`TaxDetails[${index}].TaxName`, tax.taxName);
      formData.append(`TaxDetails[${index}].Rate`, tax.rate.toString());
      formData.append(
        `TaxDetails[${index}].TaxAmount`,
        tax.taxAmount.toFixed(2),
      );
      if (tax.id) formData.append(`TaxDetails[${index}].Id`, tax.id.toString());
    });
  }

  private appendDiscountsToFormData(formData: FormData): void {
    this.invoiceData.discounts.forEach((discount, index) => {
      formData.append(`Discounts[${index}].Description`, discount.description);
      // Convert UI string to backend number enum value
      const discountTypeNumber = discountTypeToNumber(discount.discountType);
      formData.append(
        `Discounts[${index}].DiscountType`,
        discountTypeNumber.toString(),
      );
      formData.append(`Discounts[${index}].Amount`, discount.amount.toString());
      if (discount.id) {
        formData.append(`Discounts[${index}].Id`, discount.id.toString());
      }
    });
  }

  private appendAttachmentsToFormData(formData: FormData): void {
    this.invoiceData.invoiceAttachments.forEach((attachment, index) => {
      if (attachment.id && attachment.fileUrl) {
        formData.append(`Attachments[${index}].Id`, attachment.id.toString());
        formData.append(`Attachments[${index}].FileName`, attachment.fileName);
        formData.append(`Attachments[${index}].FileUrl`, attachment.fileUrl);
        if (attachment.contentType) {
          formData.append(
            `Attachments[${index}].ContentType`,
            attachment.contentType,
          );
        }
        if (attachment.fileSize) {
          formData.append(
            `Attachments[${index}].FileSize`,
            attachment.fileSize.toString(),
          );
        }
      } else if (attachment.file) {
        const fileName = attachment.fileName || attachment.file.name;
        formData.append(`Attachments[${index}].FileName`, fileName);
        formData.append(`Attachments[${index}].FileContent`, attachment.file);
        formData.append(
          `Attachments[${index}].ContentType`,
          attachment.file.type,
        );
        formData.append(
          `Attachments[${index}].FileSize`,
          attachment.file.size.toString(),
        );
      }
    });
  }

  private handleSaveSuccess(
    response: any,
    status: 'Draft' | 'Sent',
    continueEditing: boolean,
  ): void {
    // Store the initial state before changing it
    const wasNewInvoice = !this.isEditMode;

    this.invoiceData.id = response.id;

    if (wasNewInvoice) {
      this.isEditMode = true;
      this.invoiceId = response.id.toString();
      this.router.navigate([`/invoices/edit/${this.invoiceId}`], {
        replaceUrl: true,
      });
    }

    this.openDialog(
      'success',
      wasNewInvoice ? 'Invoice Created' : 'Invoice Updated',
      wasNewInvoice
        ? 'Invoice created successfully!'
        : 'Invoice updated successfully!',
      `The invoice has been ${wasNewInvoice ? 'created' : 'updated'} and saved with status "${status}".`,
    );

    if (!continueEditing) {
      this.router.navigate(['/invoices']);
    }
  }

  private handleSaveError(err: any): void {
    console.error('Error saving invoice:', err);
    const errorMessage =
      err.error?.detail || err.message || 'Failed to save invoice.';
    this.openDialog(
      'error',
      'Save Failed',
      'Failed to save invoice.',
      errorMessage,
    );
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

  // ──────────────────────────────────────────────────────────────────
  // Preview
  // ──────────────────────────────────────────────────────────────────
  previewInvoice(): void {
    if (!this.invoiceData.customerId) {
      this.openDialog(
        'error',
        'Missing Information',
        'Please select a customer for preview.',
        'A customer must be selected before you can preview the invoice.',
      );
      return;
    }

    this.customerService
      .getCustomerById(this.invoiceData.customerId)
      .subscribe({
        next: (customer) => {
          const invoice = this.buildPreviewInvoice(customer);
          this.openPreviewDialog(invoice);
        },
        error: (err) => console.error('Error fetching customer:', err),
      });
  }

  private buildPreviewInvoice(customer: any): Invoice {
    return {
      id: this.invoiceData.id || 0,
      invoiceNumber: this.invoiceData.invoiceNumber || 'N/A',
      poNumber: this.invoiceData.poNumber,
      issueDate: this.parseDate(this.invoiceData.issuedDate) || new Date(),
      dueDate: this.parseDate(this.invoiceData.dueDate) || new Date(),
      invoiceStatus: 'Draft',
      paymentStatus: 'Pending',
      type: 'Standard',
      customerId: this.invoiceData.customerId,
      companyId: this.company?.id || 0,
      currency: this.invoiceData.currency,
      currencyRate: 0,
      subtotal: parseFloat(this.getSubtotal()),
      discountTotal: parseFloat(this.getDiscountTotal()),
      taxTotal: parseFloat(this.getTaxTotal()),
      shippingAmount: this.invoiceData.shippingAmount || 0,
      adjustmentAmount: this.invoiceData.adjustmentAmount || 0,
      totalAmount: parseFloat(this.getTotal()),
      amountPaid: 0,
      amountDue: parseFloat(this.getTotal()),
      amountRefunded: 0,
      paymentMethod: this.invoiceData.paymentMethod,
      paymentTerms: this.invoiceData.paymentTerms,
      notes: this.invoiceData.customerNotes || '',
      customerNotes: this.invoiceData.customerNotes,
      internalNotes: this.invoiceData.internalNotes,
      termsAndConditions: this.invoiceData.termsAndConditions,
      footerNote: this.invoiceData.footerNote,
      projectDetail: this.invoiceData.projectDetail,
      isAutomated: this.invoiceData.isAutomated,
      isRecurring: false,
      sourceSystem: 'Manual',
      customer: {
        id: customer.id,
        name: customer.name,
        email: customer.email,
        phoneNumber: customer.phoneNumber,
        address: {
          address1: customer.address.address1,
          address2: customer.address.address2 || '',
          city: customer.address.city,
          state: customer.address.state || '',
          country: customer.address.country,
          zipCode: customer.address.zipCode,
        },
      },
      items: this.invoiceData.items.map((item) => ({
        description: item.description,
        quantity: item.quantity,
        unitPrice: item.unitPrice,
        taxType: item.taxType,
        taxAmount: item.taxAmount,
        amount: item.amount,
      })),
      taxDetails: this.invoiceData.taxDetails.map((tax) => ({
        taxName: tax.taxName,
        rate: tax.rate,
        taxAmount: tax.taxAmount,
      })),
      discounts: this.invoiceData.discounts.map((discount) => ({
        description: discount.description,
        amount: discount.amount,
        discountType: discountTypeToNumber(discount.discountType),
      })),
      invoiceAttachments: this.invoiceData.invoiceAttachments,
      createdDate: new Date(),
      createdBy: 'system',
    };
  }

  private openPreviewDialog(invoice: Invoice): void {
    this.dialog
      .open(InvoiceDisplayComponent, {
        width: '100%',
        maxWidth: '100vw',
        height: 'auto',
        maxHeight: '95vh',
        panelClass: 'full-screen-modal',
        data: { invoice, company: this.company, mode: 'preview' },
      })
      .afterClosed()
      .subscribe((result) => {
        if (result?.action === 'sendInvoice') {
          this.sendInvoice();
        }
      });
  }

  // ──────────────────────────────────────────────────────────────────
  // Attachment Handling
  // ──────────────────────────────────────────────────────────────────
  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      this.handleFiles(input.files);
      input.value = '';
    }
  }

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

  private handleFiles(files: FileList): void {
    const allowedTypes = ['image/jpeg', 'image/png', 'application/pdf'];
    const maxSize = 5 * 1024 * 1024;
    const newAttachments: Attachment[] = [];

    Array.from(files).forEach((file) => {
      if (!allowedTypes.includes(file.type)) {
        this.openDialog(
          'error',
          'Invalid File Type',
          `File ${file.name} is not allowed.`,
          'Only JPEG, PNG, and PDF files are supported.',
        );
        return;
      }
      if (file.size > maxSize) {
        this.openDialog(
          'error',
          'File Too Large',
          `File ${file.name} exceeds the 5MB limit.`,
          'Please upload a file smaller than 5MB.',
        );
        return;
      }
      const blobUrl = URL.createObjectURL(file);
      this.blobUrls.push(blobUrl);
      newAttachments.push({
        fileName: file.name,
        fileUrl: blobUrl,
        file,
        contentType: file.type,
        fileSize: file.size,
      });
    });

    if (newAttachments.length > 0) {
      this.invoiceData.invoiceAttachments = [
        ...this.invoiceData.invoiceAttachments,
        ...newAttachments,
      ];
    }
  }

  previewAttachment(attachment: Attachment): void {
    if (!attachment.fileUrl) {
      this.openDialog(
        'error',
        'Preview Error',
        'No file URL available for this attachment.',
        'The attachment URL is missing or invalid.',
      );
      return;
    }
    this.invoiceService.getAttachment(attachment.fileUrl).subscribe({
      next: (blob) => {
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
          'The file could not be retrieved. Please try again or check if the file exists.',
        );
      },
    });
  }

  removeAttachment(index: number): void {
    const attachment = this.invoiceData.invoiceAttachments[index];
    if (attachment.id && this.isEditMode && this.invoiceData.id) {
      this.invoiceService
        .deleteAttachment(this.invoiceData.id, attachment.id)
        .subscribe({
          next: () => {
            this.revokeAttachmentUrl(attachment);
            this.invoiceData.invoiceAttachments.splice(index, 1);
          },
          error: (err) => console.error('Error deleting attachment:', err),
        });
    } else {
      this.revokeAttachmentUrl(attachment);
      this.invoiceData.invoiceAttachments.splice(index, 1);
    }
  }

  private revokeAttachmentUrl(attachment: Attachment): void {
    if (attachment.fileUrl && this.blobUrls.includes(attachment.fileUrl)) {
      URL.revokeObjectURL(attachment.fileUrl);
      this.blobUrls = this.blobUrls.filter((url) => url !== attachment.fileUrl);
    }
  }

  private cleanupBlobUrls(): void {
    this.blobUrls.forEach((url) => URL.revokeObjectURL(url));
    this.blobUrls = [];
  }

  // ──────────────────────────────────────────────────────────────────
  // UI Helper Methods
  // ──────────────────────────────────────────────────────────────────
  getRandomColor(): string {
    return AVATAR_COLORS[Math.floor(Math.random() * AVATAR_COLORS.length)];
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

  goBack(): void {
    this.router.navigate(['/invoices']);
  }

  trackByItem(index: number, item: any): number {
    return index;
  }

  // ──────────────────────────────────────────────────────────────────
  // Dropdown Toggle Methods
  // ──────────────────────────────────────────────────────────────────
  toggleCustomerDropdown(): void {
    this.showCustomerDropdown = !this.showCustomerDropdown;
  }

  selectCustomer(customer: CustomerDisplay): void {
    // 1. Assign the selected customer
    this.selectedCustomer = customer;
    this.invoiceData.customerId = customer.id;

    // 2. Hide the validation message immediately
    if (this.errors['customerId']) {
      delete this.errors['customerId'];
      // Alternatively, if using a simple object: this.errors['customerId'] = null;
    }
    // 3. Close the dropdown
    this.showCustomerDropdown = false;
  }

  toggleInvoiceTypeDropdown(): void {
    this.showInvoiceTypeDropdown = !this.showInvoiceTypeDropdown;
  }

  selectInvoiceType(isAutomated: boolean): void {
    this.invoiceData.isAutomated = isAutomated;
    if (isAutomated) {
      this.invoiceData.invoiceNumber = 'AUTO-GENERATED';
      // Clear the invoice number error when switching to automated
      delete this.errors['invoiceNumber'];
    } else {
      this.invoiceData.invoiceNumber = '';
    }
    this.showInvoiceTypeDropdown = false;
  }

  toggleCurrencyDropdown(): void {
    this.showCurrencyDropdown = !this.showCurrencyDropdown;
  }

  selectCurrency(currency: string): void {
    this.invoiceData.currency = currency;
    this.updateTaxDetails();
    this.showCurrencyDropdown = false;
  }

  togglePaymentMethodDropdown(): void {
    this.showPaymentMethodDropdown = !this.showPaymentMethodDropdown;
  }

  selectPaymentMethod(method: string): void {
    this.invoiceData.paymentMethod = method;
    this.showPaymentMethodDropdown = false;
  }

  getPaymentMethodDisplayName(value: string): string {
    const method = this.paymentMethods.find((m) => m.value === value);
    return method ? method.display : 'Select';
  }

  togglePaymentTermsDropdown(): void {
    this.showPaymentTermsDropdown = !this.showPaymentTermsDropdown;
  }
  selectPaymentTerm(value: string): void {
    this.invoiceData.paymentTerms = value;
    this.onPaymentTermChange(value); // Keep your existing logic
    this.showPaymentTermsDropdown = false;
  }
  getSelectedPaymentTermLabel(): string {
    const selected = this.paymentTermOptions.find(
      (t) => t.value === this.invoiceData.paymentTerms,
    );
    return selected ? selected.label : 'Select Payment Terms';
  }
  // ──────────────────────────────────────────────────────────────────
  // Input Change Handlers
  // ──────────────────────────────────────────────────────────────────
  onInvoiceNumberChange(): void {
    delete this.errors['invoiceNumber'];
    if (
      !this.invoiceData.isAutomated &&
      !this.invoiceData.invoiceNumber.trim()
    ) {
      this.errors['invoiceNumber'] =
        'Invoice number is required for manual invoices.';
    } else if (this.invoiceData.invoiceNumber.length > 50) {
      this.errors['invoiceNumber'] =
        'Invoice number must be less than 50 characters.';
    } else if (
      this.invoiceData.invoiceNumber &&
      !/^[a-zA-Z0-9-_]+$/.test(this.invoiceData.invoiceNumber.trim())
    ) {
      this.errors['invoiceNumber'] =
        'Use only alphanumeric characters, hyphens, or underscores.';
    }
  }

  onPoNumberChange(): void {
    delete this.errors['poNumber'];
    if (this.invoiceData.poNumber && this.invoiceData.poNumber.length > 50) {
      this.errors['poNumber'] = 'PO number must be less than 50 characters.';
    }
  }

  onProjectDetailChange(): void {
    delete this.errors['projectDetail'];
    if (
      this.invoiceData.projectDetail &&
      this.invoiceData.projectDetail.length > 500
    ) {
      this.errors['projectDetail'] =
        'Project detail must be less than 500 characters.';
    }
  }

  // ──────────────────────────────────────────────────────────────────
  // Click Outside Handler
  // ──────────────────────────────────────────────────────────────────
  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent): void {
    const target = event.target as HTMLElement;

    // Close Customer Dropdown if clicking outside
    if (this.showCustomerDropdown && !target.closest('.recipient-dropdown')) {
      this.showCustomerDropdown = false;
    }

    // Close Invoice Type Dropdown if clicking outside
    if (this.showInvoiceTypeDropdown && !target.closest('.custom-dropdown')) {
      this.showInvoiceTypeDropdown = false;
    }

    // Close Currency Dropdown if clicking outside
    if (this.showCurrencyDropdown && !target.closest('.custom-dropdown')) {
      this.showCurrencyDropdown = false;
    }

    // Close Payment Method Dropdown if clicking outside
    if (this.showPaymentMethodDropdown && !target.closest('.custom-dropdown')) {
      this.showPaymentMethodDropdown = false;
    }

    // Close Payment Terms Dropdown if clicking outside
    if (this.showPaymentTermsDropdown && !target.closest('.custom-dropdown')) {
      this.showPaymentTermsDropdown = false;
    }

    // Close Tax Dropdown if clicking outside
    if (
      this.showTaxDropdown &&
      !target.closest('.tax-section .custom-dropdown')
    ) {
      this.showTaxDropdown = false;
    }

    // Close Calendar if clicking outside
    if (
      this.showCalendar &&
      !target.closest('.calendar-wrapper') &&
      !target.closest('.date-input-wrapper')
    ) {
      this.showCalendar = false;
    }
  }
}
