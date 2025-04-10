import { Component, OnInit, inject } from '@angular/core';
import {
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { Router, RouterLink } from '@angular/router';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { AuthService } from '../../../services/auth.service';
import { CommonModule } from '@angular/common'; 
@Component({
  selector: 'app-register', // Changed selector
  standalone: true,
  imports: [
    MatInputModule,
    CommonModule, 
    RouterLink,
    MatSnackBarModule,
    MatIconModule,
    ReactiveFormsModule,
  ],
  templateUrl: './register.component.html', // Created register template
  styleUrl: './register.component.css', // Created register styles
})
export class RegisterComponent implements OnInit { // Changed component name
  authService = inject(AuthService);
  matSnackBar = inject(MatSnackBar);
  router = inject(Router);
  hidePassword = true;  // Separate hide variables
  hideConfirmPassword = true;
  form!: FormGroup;
  errorMessage: string = '';  // Store API error message
  errorTimeout: any; // Store the timeout ID
  loading = false;

  fb = inject(FormBuilder);

  register() { // Changed method name
    if (this.form.valid) {
      this.authService.register(this.form.value).subscribe({ // Call register service
        next: (response) => {
          this.matSnackBar.open(response.message, 'Close', {
            duration: 5000,
            horizontalPosition: 'center',
          });
          this.router.navigate(['/login']); // Navigate to login after registration
        },
        error: (error) => {
          this.errorMessage = error.error.message || "Something went wrong!";
          this.setAutoHideError(); // Call the function to set auto-hide
        },
      });
    }
  }

  ngOnInit(): void {
    this.form = this.fb.group({
      fullname: ['', Validators.required], // Added username
      email: ['', [Validators.required, Validators.email]],
      streetAddress :['', Validators.required],
      city:['', Validators.required],
      state :['', Validators.required],
      postalCode :['', Validators.required],
      password: ['', Validators.required],
      confirmPassword: ['', Validators.required], // Added confirm password
    }, { validators: this.checkPasswords }); // Add custom validator
  }

  checkPasswords(group: FormGroup) { // Custom validator function
    let pass = group.get('password')?.value;
    let confirmPass = group.get('confirmPassword')?.value;

    return pass === confirmPass ? null : { notSame: true }     
  }
  
  setAutoHideError() {
    if (this.errorTimeout) {
        clearTimeout(this.errorTimeout); // Clear any existing timeout
    }
    this.errorTimeout = setTimeout(() => {
        this.errorMessage = ''; // Clear the message after the timeout
        this.errorTimeout = null; // Reset the timeout ID
    }, 5000); // 5000 milliseconds = 5 seconds (adjust as needed)
}
 ngOnDestroy() {
      if (this.errorTimeout) {
          clearTimeout(this.errorTimeout); // Prevent memory leaks
      }
  }
}