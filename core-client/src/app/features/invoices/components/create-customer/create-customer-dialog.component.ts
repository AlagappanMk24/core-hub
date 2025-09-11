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

interface CustomerFormData {
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
  selector: 'app-create-customer-dialog',
  templateUrl: './create-customer-dialog.component.html',
  styleUrls: ['./create-customer-dialog.component.css'],
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
export class CreateCustomerDialogComponent {
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
    public dialogRef: MatDialogRef<CreateCustomerDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: any,
    private customerService: CustomerService,
    private snackBar: MatSnackBar
  ) {
    this.dialogRef.updateSize('600px');
  }

  onCancel(): void {
    this.dialogRef.close();
  }

  onSave(customerForm: NgForm): void {
    console.log(customerForm, 'cf');
    if (customerForm.invalid) {
      customerForm.control.markAllAsTouched(); // Mark all fields as touched to show errors
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
    const newCustomer = {
      name: this.customer.name,
      email: this.customer.email,
      phoneNumber: this.customer.phoneNumber,
      address1: this.customer.address1,
      address2: this.customer.address2,
      city: this.customer.city,
      state: this.customer.state,
      country: this.customer.country,
      zipCode: this.customer.zipCode,
    };

    console.log(newCustomer, 'newCustomer');

    this.customerService.createCustomer(newCustomer).subscribe({
      next: (response) => {
        this.isSaving = false;
        this.snackBar.open('Customer created successfully!', 'Close', {
          duration: 3000,
          panelClass: ['success-snackbar'],
        });
        this.dialogRef.close({
          id: response.id,
          name: response.name,
          email: response.email,
          phoneNumber: response.phoneNumber,
          address1: response.address.address1,
          address2: response.address.address2,
          city: response.address.city,
          state: response.address.state,
          country: response.address.country,
          zipCode: response.address.zipCode,
        });
      },
      error: (err) => {
        this.isSaving = false;
        let errorMessage = 'Failed to create customer. Please try again.';
        if (err.status === 400 && err.error?.detail) {
          errorMessage = err.error.detail; // e.g., "A customer with this email already exists."
        }
        this.snackBar.open(errorMessage, 'Close', {
          duration: 5000,
          panelClass: ['error-snackbar'],
        });
      },
    });
  }
}
