import { Component, Directive, Inject } from '@angular/core';
import {
  AbstractControl,
  FormsModule,
  NG_VALIDATORS,
  NgForm,
  Validator,
} from '@angular/forms';
import {
  MatDialogModule,
  MatDialogRef,
  MAT_DIALOG_DATA,
  MatDialog,
} from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { CommonModule } from '@angular/common';
import { animate, style, transition, trigger } from '@angular/animations';
import { CustomerService } from '../../services/customer.service';
import {
  CustomerCreateDto,
  CustomerResponseDto,
  CustomerUpdateDto,
} from '../../services/models/customer.model';

import { NotificationDialogComponent } from '../../../../shared/components/notification/notification-dialog.component';

interface CustomerFormData {
  id?: number;
  name: string;
  email: string;
  phoneNumber: string;
  taxId?: string;
  website?: string;
  creditLimit?: number;
  defaultPaymentTerms?: string;
  defaultCurrency?: string;
  customerGroupId?: number;
  addressLine1: string;
  addressLine2?: string;
  city: string;
  state?: string;
  countryCode: string;
  zipCode: string;
}

@Directive({
  selector: '[appEmailValidator]',
  providers: [
    {
      provide: NG_VALIDATORS,
      useExisting: EmailValidatorDirective,
      multi: true,
    },
  ],
})
export class EmailValidatorDirective implements Validator {
  validate(control: AbstractControl): { [key: string]: any } | null {
    const emailRegex = /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/;
    return control.value && !emailRegex.test(control.value)
      ? { invalidEmail: true }
      : null;
  }
}

@Directive({
  selector: '[appPhoneValidator]',
  providers: [
    {
      provide: NG_VALIDATORS,
      useExisting: PhoneValidatorDirective,
      multi: true,
    },
  ],
})
export class PhoneValidatorDirective implements Validator {
  validate(control: AbstractControl): { [key: string]: any } | null {
    const phoneRegex = /^\+?[0-9]{7,20}$/;
    return control.value && !phoneRegex.test(control.value)
      ? { invalidPhone: true }
      : null;
  }
}
@Directive({
  selector: '[appZipCodeValidator]',
  providers: [
    {
      provide: NG_VALIDATORS,
      useExisting: ZipCodeValidatorDirective,
      multi: true,
    },
  ],
})
export class ZipCodeValidatorDirective implements Validator {
  validate(control: AbstractControl): { [key: string]: any } | null {
    const zipRegex = /^[A-Za-z0-9-]{3,10}$/;
    return control.value && !zipRegex.test(control.value)
      ? { invalidZipCode: true }
      : null;
  }
}

@Component({
  selector: 'app-customer-dialog',
  templateUrl: './customer-dialog.component.html',
  styleUrls: ['./customer-dialog.component.css'],
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatDialogModule,
    MatButtonModule,
    MatInputModule,
    MatFormFieldModule,
    MatIconModule,
    MatTooltipModule,
    MatProgressSpinnerModule,
  ],
  animations: [
    trigger('dialogFadeIn', [
      transition(':enter', [
        style({ opacity: 0, transform: 'translateY(20px)' }),
        animate(
          '300ms ease-out',
          style({ opacity: 1, transform: 'translateY(0)' }),
        ),
      ]),
    ]),
    trigger('fadeIn', [
      transition(':enter', [
        style({ opacity: 0 }),
        animate('300ms ease', style({ opacity: 1 })),
      ]),
    ]),
  ],
})
export class CustomerDialogComponent {
  customer: CustomerFormData = {
    name: '',
    email: '',
    phoneNumber: '',
    addressLine1: '',
    addressLine2: '',
    city: '',
    state: '',
    countryCode: '',
    zipCode: '',
    taxId: '',
    website: '',
    creditLimit: 0,
    defaultPaymentTerms: 'Net 30',
    defaultCurrency: 'USD',
  };
  isSaving = false;

  constructor(
    public dialogRef: MatDialogRef<CustomerDialogComponent>,
    @Inject(MAT_DIALOG_DATA)
    public data: { mode: 'create' | 'edit'; customer?: CustomerResponseDto },
    private customerService: CustomerService,
    private dialog: MatDialog,
  ) {
    if (this.data.mode === 'edit' && this.data.customer) {
      this.customer = {
        id: this.data.customer.id,
        name: this.data.customer.name,
        email: this.data.customer.email,
        phoneNumber: this.data.customer.phoneNumber,
        taxId: this.data.customer.taxId,
        website: this.data.customer.website,
        creditLimit: this.data.customer.creditLimit,
        defaultPaymentTerms: this.data.customer.defaultPaymentTerms,
        defaultCurrency: this.data.customer.defaultCurrency,
        customerGroupId: this.data.customer.customerGroupId,
        addressLine1: this.data.customer.addressLine1,
        addressLine2: this.data.customer.addressLine2 || '',
        city: this.data.customer.city,
        state: this.data.customer.state,
        countryCode: this.data.customer.countryCode,
        zipCode: this.data.customer.zipCode,
      };
    }
  }

  onCancel(): void {
    this.dialogRef.close();
  }

  private showErrorDialog(
    title: string,
    message: string,
    submessage?: string,
  ): void {
    this.dialog.open(NotificationDialogComponent, {
      width: '400px',
      data: {
        type: 'error',
        title: title,
        message: message,
        submessage:
          submessage ||
          'Please try again or contact support if the issue persists.',
        buttonText: 'Retry',
      },
    });
  }

  private showSuccessDialog(
    title: string,
    message: string,
    submessage?: string,
  ): void {
    this.dialog.open(NotificationDialogComponent, {
      width: '400px',
      data: {
        type: 'success',
        title: title,
        message: message,
        submessage: submessage,
        buttonText: 'OK',
      },
    });
  }

  onSave(customerForm: NgForm): void {
    console.log('onSave called', customerForm.value);
    console.log('Form valid:', customerForm.valid);
    if (customerForm.invalid) {
      customerForm.control.markAllAsTouched();
      this.showErrorDialog(
        'Validation Error',
        'Please fill all required fields correctly.',
      );
      return;
    }
    console.log('Creating customer with data:', this.customer);
    this.isSaving = true;

    if (this.data.mode === 'edit' && this.customer.id) {
      const updateData: CustomerUpdateDto = {
        id: this.customer.id,
        name: this.customer.name,
        email: this.customer.email,
        phoneNumber: this.customer.phoneNumber,
        taxId: this.customer.taxId,
        website: this.customer.website,
        creditLimit: this.customer.creditLimit || 0,
        defaultPaymentTerms: this.customer.defaultPaymentTerms,
        defaultCurrency: this.customer.defaultCurrency,
        customerGroupId: this.customer.customerGroupId,
        addressLine1: this.customer.addressLine1,
        addressLine2: this.customer.addressLine2,
        city: this.customer.city,
        state: this.customer.state,
        countryCode: this.customer.countryCode,
        zipCode: this.customer.zipCode,
      };

      this.customerService
        .updateCustomer(this.customer.id, updateData)
        .subscribe({
          next: (response) => {
            this.isSaving = false;
            this.dialogRef.close(response);
          },
          error: (err) => {
            this.isSaving = false;
            this.showErrorDialog(
              'Update Failed',
              err.message || 'Failed to update customer',
            );
          },
        });
    } else {
      const createData: CustomerCreateDto = {
        name: this.customer.name,
        email: this.customer.email,
        phoneNumber: this.customer.phoneNumber,
        taxId: this.customer.taxId,
        website: this.customer.website,
        creditLimit: this.customer.creditLimit || 0,
        defaultPaymentTerms: this.customer.defaultPaymentTerms,
        defaultCurrency: this.customer.defaultCurrency,
        customerGroupId: this.customer.customerGroupId,
        addressLine1: this.customer.addressLine1,
        addressLine2: this.customer.addressLine2,
        city: this.customer.city,
        state: this.customer.state,
        countryCode: this.customer.countryCode,
        zipCode: this.customer.zipCode,
      };

      this.customerService.createCustomer(createData).subscribe({
        next: (response) => {
          this.isSaving = false;
          this.dialogRef.close(response);
        },
        error: (err) => {
          this.isSaving = false;
          this.showErrorDialog(
            'Creation Failed',
            err.message || 'Failed to create customer',
          );
        },
      });
    }
  }
}
