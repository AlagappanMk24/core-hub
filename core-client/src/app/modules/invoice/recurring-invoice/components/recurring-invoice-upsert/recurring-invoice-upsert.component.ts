// components/create-recurring-invoice/create-recurring-invoice.component.ts
import { CommonModule } from '@angular/common';
import {
  Component,
  OnInit,
  ViewChild,
  ElementRef,
  HostListener,
} from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSelectModule } from '@angular/material/select';
import { MatInputModule } from '@angular/material/input';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatTabsModule } from '@angular/material/tabs';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatSliderModule } from '@angular/material/slider';
import { ActivatedRoute, Router } from '@angular/router';
import { debounceTime, Subject } from 'rxjs';

import { CustomerService } from '../../../../customer/services/customer.service';
import { InvoiceService } from '../../../standard-invoice/services/invoice.service';
import { CompanyService } from '../../../../company/services/company.service';
import { AuthService } from '../../../../../core/services/auth/auth.service';
import { LoaderService } from '../../../../../core/services/loader/loader.service';
import { NotificationDialogComponent } from '../../../../../shared/components/notification/notification-dialog.component';

import {
  CustomerDisplay,
  FREQUENCY_OPTIONS,
  DAYS_OF_WEEK,
  getDayOfWeekNumber,
  getDayOfWeekString,
  MONTHS,
  getMonthNumber,
} from '../../../../../shared/constants/recurring-invoice.constants';
import { CreateRecurringInvoiceDto, RecurringFrequency, RecurringInvoiceResponseDto } from '../../../../../interfaces/invoice/recurring-invoice/recurring-invoice.interface';
import { RecurringInvoiceService } from '../../services/recurring-invoice.service';

@Component({
  selector: 'app-create-recurring-invoice',
  templateUrl: './recurring-invoice-upsert.component.html',
  styleUrls: ['./recurring-invoice-upsert.component.css'],
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatDialogModule,
    MatButtonModule,
    MatIconModule,
    MatSelectModule,
    MatInputModule,
    MatCheckboxModule,
    MatTabsModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatSliderModule,
        ReactiveFormsModule, 
  ],
})
export class CreateRecurringInvoiceComponent implements OnInit {
     today: Date = new Date();
  // Form Data
  recurringInvoiceData: CreateRecurringInvoiceDto = {
    name: '',
    description: '',
    customerId: 0,
    currency: 'USD',
    currencyRate: 1,
    frequency: RecurringFrequency.Monthly,
    frequencyInterval: 1,
    startDate: new Date(),
    generateInAdvanceDays: 0,
    autoSend: false,
    autoEmail: false,
    autoCharge: false,
    reminderBeforeDue: false,
    reminderDaysBefore: 3,
    overridePaymentTerms: null,
  };

  // Edit Mode
  isEditMode: boolean = false;
  recurringInvoiceId: number | null = null;
  currentStatus: string = '';

  // Customer Data
  customers: CustomerDisplay[] = [];
  filteredCustomers: CustomerDisplay[] = [];
  selectedCustomer: CustomerDisplay | null = null;
  showCustomerDropdown: boolean = false;
  customerSearch: string = '';
  private searchSubject: Subject<string> = new Subject();

  // Source Invoice
  sourceInvoices: any[] = [];
  filteredSourceInvoices: any[] = [];
  showSourceInvoiceDropdown: boolean = false;
  sourceInvoiceSearch: string = '';
  selectedSourceInvoice: any = null;

  // Company Data
  company: any = null;

  // UI State
  showFrequencyDropdown: boolean = false;
  showDayOfWeekDropdown: boolean = false;
  showDayOfMonthDropdown: boolean = false;
  showWeekOfMonthDropdown: boolean = false;
  showMonthOfYearDropdown: boolean = false;

  // Recurrence Schedule UI
  frequencyOptions = FREQUENCY_OPTIONS;
  daysOfWeek = DAYS_OF_WEEK;
  daysOfMonth = Array.from({ length: 31 }, (_, i) => i + 1);
  weeksOfMonth = [
    { value: 1, label: 'First' },
    { value: 2, label: 'Second' },
    { value: 3, label: 'Third' },
    { value: 4, label: 'Fourth' },
    { value: 5, label: 'Last' },
  ];
  monthsOfYear = MONTHS;

  // Lifecycle Options
  lifecycleType: 'indefinite' | 'endDate' | 'maxOccurrences' = 'indefinite';
  maxOccurrencesOptions = [1, 2, 3, 6, 12, 24, 36, 48, 60];

  // Errors
  errors: any = {};

  // Automation
  autoGenerateOptions = [0, 1, 3, 5, 7, 14, 30];
  reminderOptions = [1, 2, 3, 5, 7, 10, 14, 21, 30];

  // Template Overrides Tab
  showOverrideFields: boolean = false;

  constructor(
    private recurringInvoiceService: RecurringInvoiceService,
    private customerService: CustomerService,
    private invoiceService: InvoiceService,
    private companyService: CompanyService,
    private authService: AuthService,
    private loaderService: LoaderService,
    private dialog: MatDialog,
    private router: Router,
    private route: ActivatedRoute,
  ) {}

  ngOnInit(): void {
    this.loadInitialData();
    this.setupSearchDebounce();
    this.checkEditMode();
  }

  private loadInitialData(): void {
    this.loadCompanyData();
    this.loadCustomers();
    this.loadSourceInvoices();
  }

  private loadCompanyData(): void {
    const userDetails = this.authService.getUserDetail();
    if (userDetails?.companyId) {
      this.companyService.getCompanyById(userDetails.companyId).subscribe({
        next: (company) => {
          this.company = company;
        },
        error: (err) => {
          console.error('Error fetching company:', err);
        },
      });
    }
  }

  private loadCustomers(search: string = ''): void {
    this.customerService
      .getCustomers({
        pageNumber: 1,
        pageSize: 50,
        search,
        status: 'Active',
      })
      .subscribe({
        next: (response) => {
          this.customers = response.items.map((c: any) => ({
            id: c.id,
            name: c.name,
            address: `${c.addressLine1 || ''}, ${c.city || ''}, ${c.countryName || ''}`,
            initials: this.getInitials(c.name),
            color: this.getRandomColor(),
          }));
          this.filteredCustomers = this.customers;
        },
        error: (err) => console.error('Error fetching customers:', err),
      });
  }

  private loadSourceInvoices(): void {
    this.invoiceService
      .getPagedInvoices({
        pageNumber: 1,
        pageSize: 100,
        search: '',
      })
      .subscribe({
        next: (response) => {
          this.sourceInvoices = response.items.map((invoice: any) => ({
            id: invoice.id,
            invoiceNumber: invoice.invoiceNumber,
            customerName: invoice.customer?.name,
            totalAmount: invoice.totalAmount,
            issueDate: invoice.issueDate,
          }));
          this.filteredSourceInvoices = this.sourceInvoices;
        },
        error: (err) => console.error('Error loading source invoices:', err),
      });
  }

  private checkEditMode(): void {
    this.route.paramMap.subscribe((params) => {
      const id = params.get('id');
      if (id) {
        this.isEditMode = true;
        this.recurringInvoiceId = Number(id);
        this.loadRecurringInvoice(Number(id));
      }
    });
  }

  private loadRecurringInvoice(id: number): void {
    this.recurringInvoiceService.getRecurringInvoiceById(id).subscribe({
      next: (invoice) => {
        this.mapResponseToForm(invoice);
        this.currentStatus = invoice.status;
      },
      error: (err) => {
        console.error('Error loading recurring invoice:', err);
        this.openDialog(
          'error',
          'Error',
          'Failed to load recurring invoice',
          'The requested recurring invoice could not be found.',
        );
      },
    });
  }

  private mapResponseToForm(response: RecurringInvoiceResponseDto): void {
    this.recurringInvoiceData = {
      name: response.name,
      description: response.description,
      customerId: response.customerId,
      billingAddressId: response.billingAddressId,
      shippingAddressId: response.shippingAddressId,
      currency: response.currency,
      currencyRate: response.currencyRate,
      poNumber: response.poNumber,
      frequency: this.getFrequencyEnum(response.frequency),
      frequencyInterval: response.frequencyInterval,
      dayOfMonth: response.dayOfMonth,
      dayOfWeek: response.dayOfWeek
        ? this.getDayOfWeekNumber(response.dayOfWeek)
        : undefined,
      weekOfMonth: response.weekOfMonth,
      monthOfYear: response.monthOfYear,
      startDate: new Date(response.startDate),
      endDate: response.endDate ? new Date(response.endDate) : undefined,
      maxOccurrences: response.maxOccurrences,
      generateInAdvanceDays: response.generateInAdvanceDays,
      autoSend: response.autoSend,
      autoEmail: response.autoEmail,
      autoCharge: response.autoCharge,
      reminderBeforeDue: response.reminderBeforeDue,
      reminderDaysBefore: response.reminderDaysBefore,
      sourceInvoiceId: response.sourceInvoiceId,
      overridePONumber: response.overridePONumber,
      overrideCustomerNotes: response.overrideCustomerNotes,
      overrideTermsAndConditions: response.overrideTermsAndConditions,
      overrideFooterNote: response.overrideFooterNote,
      overrideProjectDetail: response.overrideProjectDetail,
      overridePaymentMethod: response.overridePaymentMethod,
      overridePaymentTerms: response.overridePaymentTerms,
      overrideShippingAmount: response.overrideShippingAmount,
      overrideAdjustmentAmount: response.overrideAdjustmentAmount,
      overrideAdjustmentDescription: response.overrideAdjustmentDescription,
      customerNotes: response.customerNotes,
      internalNotes: response.internalNotes,
      termsAndConditions: response.termsAndConditions,
      footerNote: response.footerNote,
      projectDetail: response.projectDetail,
      paymentMethod: response.paymentMethod,
      paymentTerms: response.paymentTerms,
    };

    // Set lifecycle type
    if (response.maxOccurrences) {
      this.lifecycleType = 'maxOccurrences';
    } else if (response.endDate) {
      this.lifecycleType = 'endDate';
    } else {
      this.lifecycleType = 'indefinite';
    }

    // Select customer
    this.selectedCustomer = {
      id: response.customerId,
      name: response.customerName,
      address: '',
      initials: this.getInitials(response.customerName),
      color: this.getRandomColor(),
    };
    this.customerSearch = response.customerName;

    // Select source invoice
    if (response.sourceInvoiceId) {
      this.selectedSourceInvoice = this.sourceInvoices.find(
        (i) => i.id === response.sourceInvoiceId,
      );
    }
  }

  private getFrequencyEnum(frequencyString: string): RecurringFrequency {
    const frequencyMap: { [key: string]: RecurringFrequency } = {
      Daily: RecurringFrequency.Daily,
      Weekly: RecurringFrequency.Weekly,
      BiWeekly: RecurringFrequency.BiWeekly,
      Monthly: RecurringFrequency.Monthly,
      BiMonthly: RecurringFrequency.BiMonthly,
      Quarterly: RecurringFrequency.Quarterly,
      SemiAnnually: RecurringFrequency.SemiAnnually,
      Annually: RecurringFrequency.Annually,
    };
    return frequencyMap[frequencyString] || RecurringFrequency.Monthly;
  }

  private getDayOfWeekNumber(dayOfWeekString: string): number {
    const days = [
      'Sunday',
      'Monday',
      'Tuesday',
      'Wednesday',
      'Thursday',
      'Friday',
      'Saturday',
    ];
    return days.indexOf(dayOfWeekString);
  }

  private setupSearchDebounce(): void {
    this.searchSubject.pipe(debounceTime(300)).subscribe((search) => {
      this.loadCustomers(search);
    });
  }

  // Customer Selection
  searchCustomers(): void {
    this.searchSubject.next(this.customerSearch);
  }

  toggleCustomerDropdown(): void {
    if (!this.showCustomerDropdown) {
      this.filteredCustomers = this.customerSearch
        ? this.customers.filter((c) =>
            c.name.toLowerCase().includes(this.customerSearch.toLowerCase()),
          )
        : [...this.customers];
    }
    this.showCustomerDropdown = !this.showCustomerDropdown;
  }

  onCustomerSearchChange(): void {
    this.filteredCustomers = this.customerSearch
      ? this.customers.filter((c) =>
          c.name.toLowerCase().includes(this.customerSearch.toLowerCase()),
        )
      : [...this.customers];
  }

  selectCustomer(customer: CustomerDisplay): void {
    this.selectedCustomer = customer;
    this.recurringInvoiceData.customerId = customer.id;
    this.customerSearch = customer.name;
    this.filteredCustomers = [];
    this.showCustomerDropdown = false;
    if (this.errors['customerId']) delete this.errors['customerId'];
  }

  // Source Invoice Selection
  toggleSourceInvoiceDropdown(): void {
    this.filteredSourceInvoices = this.sourceInvoiceSearch
      ? this.sourceInvoices.filter(
          (i) =>
            i.invoiceNumber
              .toLowerCase()
              .includes(this.sourceInvoiceSearch.toLowerCase()) ||
            (i.customerName &&
              i.customerName
                .toLowerCase()
                .includes(this.sourceInvoiceSearch.toLowerCase())),
        )
      : [...this.sourceInvoices];
    this.showSourceInvoiceDropdown = !this.showSourceInvoiceDropdown;
  }

  onSourceInvoiceSearchChange(): void {
    this.filteredSourceInvoices = this.sourceInvoiceSearch
      ? this.sourceInvoices.filter(
          (i) =>
            i.invoiceNumber
              .toLowerCase()
              .includes(this.sourceInvoiceSearch.toLowerCase()) ||
            (i.customerName &&
              i.customerName
                .toLowerCase()
                .includes(this.sourceInvoiceSearch.toLowerCase())),
        )
      : [...this.sourceInvoices];
  }

  selectSourceInvoice(invoice: any): void {
    this.selectedSourceInvoice = invoice;
    this.recurringInvoiceData.sourceInvoiceId = invoice.id;
    this.sourceInvoiceSearch = `${invoice.invoiceNumber} - ${invoice.customerName}`;
    this.filteredSourceInvoices = [];
    this.showSourceInvoiceDropdown = false;
  }

  clearSourceInvoice(): void {
    this.selectedSourceInvoice = null;
    this.recurringInvoiceData.sourceInvoiceId = undefined;
    this.sourceInvoiceSearch = '';
  }

  // Frequency Helpers
  getFrequencyLabel(frequency: RecurringFrequency): string {
    const option = this.frequencyOptions.find((f) => f.value === frequency);
    return option?.label || 'Monthly';
  }

  getFrequencyDescription(): string {
    const frequency = this.recurringInvoiceData.frequency;
    const interval = this.recurringInvoiceData.frequencyInterval;

    if (interval === 1) {
      return this.getFrequencyLabel(frequency);
    }

    const baseLabel = this.getFrequencyLabel(frequency).toLowerCase();
    return `Every ${interval} ${baseLabel}s`;
  }

  onFrequencyChange(): void {
    // Reset schedule-specific fields when frequency changes
    this.recurringInvoiceData.dayOfMonth = undefined;
    this.recurringInvoiceData.dayOfWeek = undefined;
    this.recurringInvoiceData.weekOfMonth = undefined;
    this.recurringInvoiceData.monthOfYear = undefined;
  }

  // Validation
  validateForm(showErrors: boolean = true): boolean {
    this.errors = {};
    let isValid = true;

    // Name validation
    if (!this.recurringInvoiceData.name?.trim()) {
      if (showErrors) this.errors['name'] = 'Name is required';
      isValid = false;
    } else if (this.recurringInvoiceData.name.length > 200) {
      if (showErrors) this.errors['name'] = 'Name cannot exceed 200 characters';
      isValid = false;
    }

    // Customer validation
    if (!this.recurringInvoiceData.customerId) {
      if (showErrors) this.errors['customerId'] = 'Please select a customer';
      isValid = false;
    }

    // Start date validation
    if (!this.recurringInvoiceData.startDate) {
      if (showErrors) this.errors['startDate'] = 'Start date is required';
      isValid = false;
    }

    // End date vs start date validation
    if (
      this.recurringInvoiceData.endDate &&
      this.recurringInvoiceData.startDate &&
      this.recurringInvoiceData.endDate < this.recurringInvoiceData.startDate
    ) {
      if (showErrors)
        this.errors['endDate'] = 'End date must be after start date';
      isValid = false;
    }

    // Max occurrences validation
    if (
      this.recurringInvoiceData.maxOccurrences &&
      (this.recurringInvoiceData.maxOccurrences < 1 ||
        this.recurringInvoiceData.maxOccurrences > 9999)
    ) {
      if (showErrors)
        this.errors['maxOccurrences'] =
          'Max occurrences must be between 1 and 9999';
      isValid = false;
    }

    // Reminder days validation
    if (
      this.recurringInvoiceData.reminderBeforeDue &&
      (this.recurringInvoiceData.reminderDaysBefore < 1 ||
        this.recurringInvoiceData.reminderDaysBefore > 30)
    ) {
      if (showErrors)
        this.errors['reminderDaysBefore'] =
          'Reminder days must be between 1 and 30';
      isValid = false;
    }

    return isValid;
  }

  // Save Operations
  saveRecurringInvoice(
    status: 'Draft' | 'Active',
    continueEditing: boolean = false,
  ): void {
    if (!this.validateForm()) return;

    this.loaderService.showProgress(
      this.isEditMode
        ? 'Updating Recurring Invoice'
        : 'Creating Recurring Invoice',
      'Preparing your recurring invoice template...',
      ['Validating Data', 'Saving Template', 'Finalizing'],
    );

    setTimeout(
      () =>
        this.updateLoaderProgress(
          1,
          25,
          'Validating recurring invoice data...',
        ),
      500,
    );

    const formData = this.buildDto();

    setTimeout(
      () =>
        this.updateLoaderProgress(
          2,
          50,
          'Saving recurring invoice template...',
        ),
      1500,
    );

    const request =
      this.isEditMode && this.recurringInvoiceId
        ? this.recurringInvoiceService.updateRecurringInvoice(
            this.recurringInvoiceId,
            { ...formData, id: this.recurringInvoiceId, status: status as any },
          )
        : this.recurringInvoiceService.createRecurringInvoice(formData);

    request.subscribe({
      next: (response) => {
        this.updateLoaderProgress(3, 75, 'Finalizing...');
        setTimeout(() => {
          this.updateLoaderProgress(4, 100, 'Complete!');
          setTimeout(() => {
            this.loaderService.hide();
            this.handleSaveSuccess(response, status, continueEditing);
          }, 500);
        }, 500);
      },
      error: (err) => {
        this.loaderService.hide();
        this.handleSaveError(err);
      },
    });
  }

  private buildDto(): CreateRecurringInvoiceDto {
    const dto: CreateRecurringInvoiceDto = {
      name: this.recurringInvoiceData.name.trim(),
      description: this.recurringInvoiceData.description?.trim(),
      customerId: this.recurringInvoiceData.customerId,
      currency: this.recurringInvoiceData.currency,
      currencyRate: this.recurringInvoiceData.currencyRate,
      frequency: this.recurringInvoiceData.frequency,
      frequencyInterval: this.recurringInvoiceData.frequencyInterval,
      startDate: this.recurringInvoiceData.startDate,
      generateInAdvanceDays: this.recurringInvoiceData.generateInAdvanceDays,
      autoSend: this.recurringInvoiceData.autoSend,
      autoEmail: this.recurringInvoiceData.autoEmail,
      autoCharge: this.recurringInvoiceData.autoCharge,
      reminderBeforeDue: this.recurringInvoiceData.reminderBeforeDue,
      reminderDaysBefore: this.recurringInvoiceData.reminderDaysBefore,
      poNumber: this.recurringInvoiceData.poNumber?.trim(),
      overridePaymentTerms: this.recurringInvoiceData.overridePaymentTerms,
      overridePONumber: this.recurringInvoiceData.overridePONumber?.trim(),
      overrideCustomerNotes:
        this.recurringInvoiceData.overrideCustomerNotes?.trim(),
      overrideTermsAndConditions:
        this.recurringInvoiceData.overrideTermsAndConditions?.trim(),
      overrideFooterNote: this.recurringInvoiceData.overrideFooterNote?.trim(),
      overrideProjectDetail:
        this.recurringInvoiceData.overrideProjectDetail?.trim(),
      overridePaymentMethod:
        this.recurringInvoiceData.overridePaymentMethod?.trim(),
      overrideShippingAmount: this.recurringInvoiceData.overrideShippingAmount,
      overrideAdjustmentAmount:
        this.recurringInvoiceData.overrideAdjustmentAmount,
      overrideAdjustmentDescription:
        this.recurringInvoiceData.overrideAdjustmentDescription?.trim(),
      customerNotes: this.recurringInvoiceData.customerNotes?.trim(),
      internalNotes: this.recurringInvoiceData.internalNotes?.trim(),
      termsAndConditions: this.recurringInvoiceData.termsAndConditions?.trim(),
      footerNote: this.recurringInvoiceData.footerNote?.trim(),
      projectDetail: this.recurringInvoiceData.projectDetail?.trim(),
      paymentMethod: this.recurringInvoiceData.paymentMethod?.trim(),
      paymentTerms: this.recurringInvoiceData.paymentTerms?.trim(),
    };

    // Add schedule-specific fields
    if (this.recurringInvoiceData.dayOfMonth)
      dto.dayOfMonth = this.recurringInvoiceData.dayOfMonth;
    if (this.recurringInvoiceData.dayOfWeek !== undefined)
      dto.dayOfWeek = this.recurringInvoiceData.dayOfWeek;
    if (this.recurringInvoiceData.weekOfMonth)
      dto.weekOfMonth = this.recurringInvoiceData.weekOfMonth;
    if (this.recurringInvoiceData.monthOfYear)
      dto.monthOfYear = this.recurringInvoiceData.monthOfYear;

    // Add lifecycle fields based on selected type
    if (this.lifecycleType === 'endDate' && this.recurringInvoiceData.endDate) {
      dto.endDate = this.recurringInvoiceData.endDate;
    } else if (
      this.lifecycleType === 'maxOccurrences' &&
      this.recurringInvoiceData.maxOccurrences
    ) {
      dto.maxOccurrences = this.recurringInvoiceData.maxOccurrences;
    }

    // Add source invoice
    if (this.recurringInvoiceData.sourceInvoiceId) {
      dto.sourceInvoiceId = this.recurringInvoiceData.sourceInvoiceId;
    }

    return dto;
  }

  private handleSaveSuccess(
    response: RecurringInvoiceResponseDto,
    status: string,
    continueEditing: boolean,
  ): void {
    const wasNew = !this.isEditMode;

    if (wasNew) {
      this.isEditMode = true;
      this.recurringInvoiceId = response.id;
      this.router.navigate(
        [`/recurring-invoices/edit/${this.recurringInvoiceId}`],
        { replaceUrl: true },
      );
    }

    this.openDialog(
      'success',
      wasNew ? 'Recurring Invoice Created' : 'Recurring Invoice Updated',
      wasNew
        ? 'Recurring invoice template created successfully!'
        : 'Recurring invoice template updated successfully!',
      `The template has been ${wasNew ? 'created' : 'updated'} with status "${status}".`,
    );

    if (!continueEditing) {
      this.router.navigate(['/recurring-invoices']);
    }
  }

  private handleSaveError(err: any): void {
    console.error('Error saving recurring invoice:', err);

    let title = 'Save Failed';
    let subMessage = 'We encountered an issue:';
    let message = 'An unexpected error occurred.';

    if (err.status === 0) {
      title = 'Connection Error';
      subMessage = 'Network issue detected:';
      message =
        'Could not connect to the server. Please check if the API is running.';
    } else if (err.error) {
      title = err.error.title || 'Validation Error';
      subMessage =
        err.status === 400 ? 'Please check the following:' : 'Server response:';
      message =
        err.error.detail ||
        err.error.message ||
        'The server returned an error.';

      if (err.error.errors) {
        const validationMessages = Object.values(err.error.errors).flat();
        if (validationMessages.length > 0) {
          message = validationMessages.join('\n');
        }
      }
    }

    this.openDialog('error', title, subMessage, message);
  }

  saveToDraft(): void {
    this.saveRecurringInvoice('Draft');
  }

  saveAndContinue(): void {
    this.saveRecurringInvoice('Draft', true);
  }

  activateTemplate(): void {
    if (this.validateForm()) {
      this.loaderService.showWarning(
        'Activate Template',
        'Are you sure you want to activate this recurring invoice template?',
        () => this.saveRecurringInvoice('Active'),
      );
    }
  }

  // UI Helpers
  private updateLoaderProgress(
    step: number,
    progress: number,
    message: string,
  ): void {
    this.loaderService.updateProgress(progress, step, message);
  }

  private getInitials(name: string): string {
    return name
      .split(' ')
      .map((n) => n[0])
      .join('')
      .substring(0, 2)
      .toUpperCase();
  }

  private getRandomColor(): string {
    const colors = [
      '#6366F1',
      '#8B5CF6',
      '#EC4899',
      '#F43F5E',
      '#EF4444',
      '#F59E0B',
      '#10B981',
      '#06B6D4',
      '#3B82F6',
    ];
    return colors[Math.floor(Math.random() * colors.length)];
  }

  goBack(): void {
    this.router.navigate(['/recurring-invoices']);
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

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent): void {
    const target = event.target as HTMLElement;

    if (this.showCustomerDropdown && !target.closest('.customer-dropdown')) {
      this.showCustomerDropdown = false;
    }

    if (
      this.showSourceInvoiceDropdown &&
      !target.closest('.source-invoice-dropdown')
    ) {
      this.showSourceInvoiceDropdown = false;
    }

    if (this.showFrequencyDropdown && !target.closest('.frequency-dropdown')) {
      this.showFrequencyDropdown = false;
    }
  }
}
