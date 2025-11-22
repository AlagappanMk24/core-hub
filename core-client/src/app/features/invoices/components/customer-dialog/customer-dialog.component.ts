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
} from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { CommonModule } from '@angular/common';
import { animate, style, transition, trigger } from '@angular/animations';
import { CustomerService } from '../../../../services/customer/customer.service';
import { CustomerUpdateDto, CustomerCreateDto } from '../../../../services/customer/models/customer.model';

interface CustomerFormData {
  id?: number;
  name: string;
  email: string;
  phoneNumber: string;
  address1: string;
  address2: string;
  city: string;
  state: string;
  country: string;
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
    MatSnackBarModule,
  ],
  animations: [
    trigger('dialogFadeIn', [
      transition(':enter', [
        style({ opacity: 0, transform: 'translateY(20px)' }),
        animate(
          '300ms ease-out',
          style({ opacity: 1, transform: 'translateY(0)' })
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
    address1: '',
    address2: '',
    city: '',
    state: '',
    country: '',
    zipCode: '',
  };
  isSaving = false;

  constructor(
    public dialogRef: MatDialogRef<CustomerDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: { mode: 'create' | 'edit'; customer?: CustomerFormData },
    private customerService: CustomerService,
    private snackBar: MatSnackBar
  ) {
    if (this.data.mode === 'edit' && this.data.customer) {
      this.customer = { ...this.data.customer };
    }
  }

  onCancel(): void {
    this.dialogRef.close();
  }

  onSave(customerForm: NgForm): void {
    if (customerForm.invalid) {
      customerForm.control.markAllAsTouched();
      this.snackBar.open(
        'Please fill all required fields correctly.',
        'Close',
        {
          duration: 3000,
          panelClass: ['error-snackbar'],
        }
      );
      return;
    }

    this.isSaving = true;
    const customerData: CustomerUpdateDto = {
      id: this.customer.id!, // Non-null assertion since id is guaranteed in edit mode
      name: this.customer.name,
      email: this.customer.email,
      phoneNumber: this.customer.phoneNumber,
      address1: this.customer.address1,
      address2: this.customer.address2 || undefined,
      city: this.customer.city,
      state: this.customer.state || undefined,
      country: this.customer.country,
      zipCode: this.customer.zipCode,
    };

    if (this.data.mode === 'edit' && this.customer.id) {
      this.customerService.updateCustomer(this.customer.id, customerData).subscribe({
        next: (response) => {
          this.isSaving = false;
          this.snackBar.open('Customer updated successfully!', 'Close', {
            duration: 3000,
            panelClass: ['success-snackbar'],
          });
          this.dialogRef.close(response);
        },
        error: (err) => {
          this.isSaving = false;
          this.snackBar.open(err.message || 'Failed to update customer', 'Close', {
            duration: 5000,
            panelClass: ['error-snackbar'],
          });
        },
      });
    } else {
      const createData: CustomerCreateDto = {
        name: customerData.name,
        email: customerData.email,
        phoneNumber: customerData.phoneNumber,
        address1: customerData.address1,
        address2: customerData.address2,
        city: customerData.city,
        state: customerData.state,
        country: customerData.country,
        zipCode: customerData.zipCode,
      };
      this.customerService.createCustomer(createData).subscribe({
        next: (response) => {
          this.isSaving = false;
          this.snackBar.open('Customer created successfully!', 'Close', {
            duration: 3000,
            panelClass: ['success-snackbar'],
          });
          this.dialogRef.close(response);
        },
        error: (err) => {
          this.isSaving = false;
          this.snackBar.open(err.message || 'Failed to create customer', 'Close', {
            duration: 5000,
            panelClass: ['error-snackbar'],
          });
        },
      });
    }
  }
}