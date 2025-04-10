import { Component } from '@angular/core';
import {
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { AuthService } from '../../../services/auth.service';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';

@Component({
  selector: 'app-forgot-password',
  standalone: true,
  imports: [
    MatInputModule,
    RouterLink,
    MatIconModule,
    MatInputModule,
    ReactiveFormsModule,
    CommonModule
  ],
  templateUrl: './forgot-password.component.html',
  styleUrls: ['./forgot-password.component.css'],
})
export class ForgotPasswordComponent {
  form: FormGroup;
  loading = false;
  showEmailSent = false;
  isSubmitting = false;

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private snackBar: MatSnackBar
  ) {
    this.form = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
    });
  }

  submit() {
    if (this.form.valid) {
      this.isSubmitting = true;
      const email = this.form.value.email;
      this.authService.forgotPassword(email).subscribe({
        next : (response) => {
          if(response.isSucceeded){
            this.snackBar.open(
              'Password reset link sent to your email',
              'Close',
              {
                duration: 3000,
                panelClass: ['bg-green-500', 'text-white'],
              }
            );
            this.showEmailSent = true;
          }else{
            this.snackBar.open(
              'Password reset link sent to your email',
              'Close',
              {
                duration: 3000,
                panelClass: ['bg-green-500', 'text-white'],
              }
            );
          }
        },
        error : (err : HttpErrorResponse) => {
          this.snackBar.open('Error sending reset email', 'Close', {
            duration: 3000,
            panelClass: ['bg-red-500', 'text-white'],
          });
        },
        complete : () =>{
          this.isSubmitting = false;
        }
    });
    }
  }
}
