// import { Component, OnInit, OnDestroy, inject } from '@angular/core';
// import { Router, ActivatedRoute, RouterLink } from '@angular/router';
// import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
// import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
// import { MatIconModule } from '@angular/material/icon';
// import { CommonModule } from '@angular/common';
// import { AuthService } from '../../services/auth.service';

// @Component({
//   selector: 'app-auth',
//   standalone: true,
//   imports: [
//     CommonModule,
//     ReactiveFormsModule,
//     MatSnackBarModule,
//     MatIconModule,
//     RouterLink
//   ],
//   templateUrl: './auth.component.html',
//   styleUrls: ['./shared-auth-styles.css']
// })
// export class AuthComponent implements OnInit, OnDestroy {
//   private authService = inject(AuthService);
//   private matSnackBar = inject(MatSnackBar);
//   private router = inject(Router);
//   private activatedRoute = inject(ActivatedRoute);
//   private fb = inject(FormBuilder);

//   isSignUp = false;
//   registerForm!: FormGroup;
//   loginForm!: FormGroup;
//   hidePassword = true;
//   hideConfirmPassword = true;
//   loading = false;
//   registerError: string = '';
//   loginError: string = '';
//   private errorTimeout: any;

//   ngOnInit(): void {
//     // Initialize forms
//     this.registerForm = this.fb.group({
//       fullname: ['', Validators.required],
//       email: ['', [Validators.required, Validators.email]],
//       streetAddress: ['', Validators.required],
//       city: ['', Validators.required],
//       state: ['', Validators.required],
//       postalCode: ['', Validators.required],
//       password: ['', [Validators.required, Validators.minLength(6)]],
//       confirmPassword: ['', Validators.required],
//       terms: [false, Validators.requiredTrue]
//     }, { validators: this.checkPasswords });

//     this.loginForm = this.fb.group({
//       email: ['', [Validators.required, Validators.email]],
//       password: ['', Validators.required]
//     });

//     // Handle route data to determine form state
//     this.activatedRoute.data.subscribe(data => {
//       this.isSignUp = data['mode'] === 'register';
//     });

//     // Handle external login callback
//     this.activatedRoute.queryParams.subscribe(params => {
//       const authCode = params['code'];
//       const provider = params['state'];
//       if (authCode && provider) {
//         this.loading = true;
//         this.authService.externalLogin(authCode, provider).subscribe({
//           next: (response: any) => {
//             if (response.token) {
//               localStorage.setItem('authToken', response.token);
//               this.router.navigate(['/dashboard']);
//             }
//           },
//           error: (error) => {
//             this.loading = false;
//             this.loginError = error.error?.message || 'External login failed';
//             this.setAutoHideError('loginError');
//           }
//         });
//       }
//     });

//     // Clear errors on form value changes
//     this.registerForm.valueChanges.subscribe(() => {
//       this.registerError = '';
//     });
//     this.loginForm.valueChanges.subscribe(() => {
//       this.loginError = '';
//     });
//   }

//   ngOnDestroy(): void {
//     if (this.errorTimeout) {
//       clearTimeout(this.errorTimeout);
//     }
//   }

//   toggleForm() {
//     this.isSignUp = !this.isSignUp;
//     const newPath = this.isSignUp ? '/auth/register' : '/auth/login';
//     this.router.navigate([newPath], { replaceUrl: true });
//   }

//   togglePassword() {
//     this.hidePassword = !this.hidePassword;
//   }

//   toggleConfirmPassword() {
//     this.hideConfirmPassword = !this.hideConfirmPassword;
//   }

//   onRegister() {
//     if (this.registerForm.valid) {
//       this.loading = true;
//       this.authService.register(this.registerForm.value).subscribe({
//         next: (response) => {
//           this.loading = false;
//           this.matSnackBar.open(response.message, 'Close', {
//             duration: 5000,
//             horizontalPosition: 'center'
//           });
//           this.router.navigate(['/auth/verify-otp'], {
//             queryParams: { email: this.registerForm.value.email }
//           });
//         },
//         error: (error) => {
//           this.loading = false;
//           this.registerError = error.error?.message || 'Registration failed';
//           this.setAutoHideError('registerError');
//         }
//       });
//     }
//   }

//   onLogin() {
//     if (this.loginForm.valid) {
//       this.loading = true;
//       this.authService.login(this.loginForm.value).subscribe({
//         next: () => {
//           this.loading = false;
//           this.router.navigate(['/auth/verify-otp'], {
//             queryParams: { email: this.loginForm.value.email }
//           });
//         },
//         error: (error) => {
//           this.loading = false;
//           if (error.status === 400) {
//             this.loginError = error.error?.errors ? Object.values(error.error.errors).flat().join(' ') : error.error?.message || 'Invalid login attempt';
//           } else if (error.status === 403) {
//             this.loginError = error.error?.message || 'Unauthorized access';
//           } else {
//             this.loginError = error.error?.message || 'Login failed';
//           }
//           this.setAutoHideError('loginError');
//         }
//       });
//     }
//   }

//   loginWithProvider(provider: string) {
//     this.loading = true;
//     this.authService.getExternalLoginUrl(provider).subscribe({
//       next: (response: any) => {
//         window.location.href = response.redirectUrl;
//       },
//       error: (error) => {
//         this.loading = false;
//         this.loginError = error.error?.message || 'External login failed';
//         this.setAutoHideError('loginError');
//       }
//     });
//   }

//   private checkPasswords(group: FormGroup) {
//     const pass = group.get('password')?.value;
//     const confirmPass = group.get('confirmPassword')?.value;
//     return pass === confirmPass ? null : { notSame: true };
//   }

//   private setAutoHideError(errorField: 'registerError' | 'loginError') {
//     if (this.errorTimeout) {
//       clearTimeout(this.errorTimeout);
//     }
//     this.errorTimeout = setTimeout(() => {
//       this[errorField] = '';
//       this.errorTimeout = null;
//     }, 5000);
//   }
// }

// import { Component, OnInit, OnDestroy, inject } from '@angular/core';
// import { Router, ActivatedRoute, RouterLink } from '@angular/router';
// import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
// import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
// import { MatIconModule } from '@angular/material/icon';
// import { CommonModule } from '@angular/common';
// import { AuthService } from '../../services/auth.service';

// @Component({
//   selector: 'app-auth',
//   standalone: true,
//   imports: [
//     CommonModule,
//     ReactiveFormsModule,
//     MatSnackBarModule,
//     MatIconModule,
//     RouterLink
//   ],
//   templateUrl: './auth.component.html',
//   styleUrls: ['./shared-auth-styles.css']
// })
// export class AuthComponent implements OnInit, OnDestroy {
//   private authService = inject(AuthService);
//   private matSnackBar = inject(MatSnackBar);
//   private router = inject(Router);
//   private activatedRoute = inject(ActivatedRoute);
//   private fb = inject(FormBuilder);

//   isSignUp = false;
//   registerForm!: FormGroup;
//   loginForm!: FormGroup;
//   hidePassword = true;
//   hideConfirmPassword = true;
//   loading = false;
//   registerError: string = '';
//   loginError: string = '';
//   private errorTimeout: any;

//   ngOnInit(): void {
//     // Initialize forms
//     this.registerForm = this.fb.group({
//       fullname: ['', Validators.required],
//       email: ['', [Validators.required, Validators.email]],
//       streetAddress: ['', Validators.required],
//       city: ['', Validators.required],
//       state: ['', Validators.required],
//       postalCode: ['', Validators.required],
//       password: ['', [Validators.required, Validators.minLength(6)]],
//       confirmPassword: ['', Validators.required],
//       terms: [false, Validators.requiredTrue]
//     }, { validators: this.checkPasswords });

//     this.loginForm = this.fb.group({
//       email: ['', [Validators.required, Validators.email]],
//       password: ['', Validators.required]
//     });

//     // Handle route data to determine form state
//     this.activatedRoute.data.subscribe(data => {
//       this.isSignUp = data['mode'] === 'register';
//     });

//     // Handle external login callback
//     this.activatedRoute.queryParams.subscribe(params => {
//       const authCode = params['code'];
//       const provider = params['state'];
//       if (authCode && provider) {
//         this.loading = true;
//         this.authService.externalLogin(authCode, provider).subscribe({
//           next: (response: any) => {
//             if (response.token) {
//               localStorage.setItem('authToken', response.token);
//               this.router.navigate(['/dashboard']);
//             }
//           },
//           error: (error) => {
//             this.loading = false;
//             this.loginError = error.error?.message || 'External login failed';
//             this.setAutoHideError('loginError');
//           }
//         });
//       }
//     });

//     // Clear errors on form value changes
//     this.registerForm.valueChanges.subscribe(() => {
//       this.registerError = '';
//     });
//     this.loginForm.valueChanges.subscribe(() => {
//       this.loginError = '';
//     });
//   }

//   ngOnDestroy(): void {
//     if (this.errorTimeout) {
//       clearTimeout(this.errorTimeout);
//     }
//   }

//   toggleForm() {
//     this.isSignUp = !this.isSignUp;
//     const newPath = this.isSignUp ? '/auth/register' : '/auth/login';
//     this.router.navigate([newPath], { replaceUrl: true });
//   }

//   togglePassword() {
//     this.hidePassword = !this.hidePassword;
//   }

//   toggleConfirmPassword() {
//     this.hideConfirmPassword = !this.hideConfirmPassword;
//   }

//   onRegister() {
//     if (this.registerForm.valid) {
//       this.loading = true;
//       this.authService.register(this.registerForm.value).subscribe({
//         next: (response) => {
//           this.loading = false;
//           this.matSnackBar.open(response.message || 'Registration successful! Please log in.', 'Close', {
//             duration: 5000,
//             horizontalPosition: 'center'
//           });
//           this.isSignUp = false;
//           this.router.navigate(['/auth/login'], {
//             queryParams: { email: this.registerForm.value.email }
//           });
//         },
//         error: (error) => {
//           this.loading = false;
//           this.registerError = error.error?.message || 'Registration failed';
//           this.setAutoHideError('registerError');
//         }
//       });
//     } else {
//       this.registerForm.markAllAsTouched();
//       this.registerError = 'Please fill out all required fields correctly';
//       this.setAutoHideError('registerError');
//     }
//   }

//   onLogin() {
//     if (this.loginForm.valid) {
//       this.loading = true;
//       this.authService.login(this.loginForm.value).subscribe({
//         next: (response) => {
//           this.loading = false;
//           if (response.isSuccess) {
//             this.router.navigate(['/auth/verify-otp'], {
//               queryParams: { email: this.loginForm.value.email }
//             });
//           }
//         },
//         error: (error) => {
//           this.loading = false;
//           if (error.status === 400) {
//             this.loginError = error.error?.errors ? Object.values(error.error.errors).flat().join(' ') : error.error?.message || 'Invalid login attempt';
//           } else if (error.status === 403) {
//             this.loginError = error.error?.message || 'Unauthorized access';
//           } else {
//             this.loginError = error.error?.message || 'Login failed';
//           }
//           this.setAutoHideError('loginError');
//         }
//       });
//     } else {
//       this.loginForm.markAllAsTouched();
//       this.loginError = 'Please fill out all required fields correctly';
//       this.setAutoHideError('loginError');
//     }
//   }

//   loginWithProvider(provider: string) {
//     this.loading = true;
//     this.authService.getExternalLoginUrl(provider).subscribe({
//       next: (response: any) => {
//         window.location.href = response.redirectUrl;
//       },
//       error: (error) => {
//         this.loading = false;
//         this.loginError = error.error?.message || 'External login failed';
//         this.setAutoHideError('loginError');
//       }
//     });
//   }

//   private checkPasswords(group: FormGroup) {
//     const pass = group.get('password')?.value;
//     const confirmPass = group.get('confirmPassword')?.value;
//     return pass === confirmPass ? null : { notSame: true };
//   }

//   private setAutoHideError(errorField: 'registerError' | 'loginError') {
//     if (this.errorTimeout) {
//       clearTimeout(this.errorTimeout);
//     }
//     this.errorTimeout = setTimeout(() => {
//       this[errorField] = '';
//       this.errorTimeout = null;
//     }, 5000);
//   }
// }

// import { Component, OnInit, OnDestroy, inject } from '@angular/core';
// import { Router, ActivatedRoute, RouterLink } from '@angular/router';
// import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
// import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
// import { MatIconModule } from '@angular/material/icon';
// import { CommonModule } from '@angular/common';
// import { AuthService } from '../../services/auth.service';

// @Component({
//   selector: 'app-auth',
//   standalone: true,
//   imports: [
//     CommonModule,
//     ReactiveFormsModule,
//     MatSnackBarModule,
//     MatIconModule,
//     RouterLink
//   ],
//   templateUrl: './auth.component.html',
//   styleUrls: ['./shared-auth-styles.css']
// })
// export class AuthComponent implements OnInit, OnDestroy {
//   private authService = inject(AuthService);
//   private matSnackBar = inject(MatSnackBar);
//   private router = inject(Router);
//   private activatedRoute = inject(ActivatedRoute);
//   private fb = inject(FormBuilder);

//   isSignUp = false;
//   registerForm!: FormGroup;
//   loginForm!: FormGroup;
//   hidePassword = true;
//   hideConfirmPassword = true;
//   loading = false;
//   registerError: string = '';
//   loginError: string = '';
//   private errorTimeout: any;

//   ngOnInit(): void {
//     // Initialize forms
//     this.registerForm = this.fb.group({
//       fullname: ['', Validators.required],
//       email: ['', [Validators.required, Validators.email]],
//       streetAddress: ['', Validators.required],
//       city: ['', Validators.required],
//       state: ['', Validators.required],
//       postalCode: ['', Validators.required],
//       password: ['', [Validators.required, Validators.minLength(6)]],
//       confirmPassword: ['', Validators.required],
//       terms: [false, Validators.requiredTrue]
//     }, { validators: this.checkPasswords });

//     this.loginForm = this.fb.group({
//       email: ['', [Validators.required, Validators.email]],
//       password: ['', Validators.required]
//     });

//     // Handle route data to determine form state
//     this.activatedRoute.data.subscribe(data => {
//       this.isSignUp = data['mode'] === 'register';
//     });

//     // Handle external login callback
//     this.activatedRoute.queryParams.subscribe(params => {
//       const authCode = params['code'];
//       const provider = params['state'];
//       if (authCode && provider) {
//         this.loading = true;
//         this.authService.externalLogin(authCode, provider).subscribe({
//           next: (response: any) => {
//             if (response.token) {
//               localStorage.setItem('authToken', response.token);
//               this.router.navigate(['/dashboard']);
//             }
//           },
//           error: (error) => {
//             this.loading = false;
//             this.loginError = error.error?.message || 'External login failed';
//             this.setAutoHideError('loginError');
//           }
//         });
//       }
//     });

//     // Clear errors on form value changes
//     this.registerForm.valueChanges.subscribe(() => {
//       this.registerError = '';
//     });
//     this.loginForm.valueChanges.subscribe(() => {
//       this.loginError = '';
//     });
//   }

//   ngOnDestroy(): void {
//     if (this.errorTimeout) {
//       clearTimeout(this.errorTimeout);
//     }
//   }

//   switchToSignIn(event: Event) {
//     event.preventDefault();
//     this.isSignUp = false;
//     this.router.navigate(['/auth/login'], { replaceUrl: true });
//   }

//   switchToSignUp(event: Event) {
//     event.preventDefault();
//     this.isSignUp = true;
//     this.router.navigate(['/auth/register'], { replaceUrl: true });
//   }

//   togglePassword() {
//     this.hidePassword = !this.hidePassword;
//   }

//   toggleConfirmPassword() {
//     this.hideConfirmPassword = !this.hideConfirmPassword;
//   }

//   onRegister() {
//     if (this.registerForm.valid) {
//       this.loading = true;
//       this.authService.register(this.registerForm.value).subscribe({
//         next: (response) => {
//           this.loading = false;
//           this.matSnackBar.open(response.message || 'Registration successful! Please log in.', 'Close', {
//             duration: 5000,
//             horizontalPosition: 'center'
//           });
//           this.isSignUp = false;
//           this.router.navigate(['/auth/login'], {
//             queryParams: { email: this.registerForm.value.email }
//           });
//         },
//         error: (error) => {
//           this.loading = false;
//           this.registerError = error.error?.message || 'Registration failed';
//           this.setAutoHideError('registerError');
//         }
//       });
//     } else {
//       this.registerForm.markAllAsTouched();
//       this.registerError = 'Please fill out all required fields correctly';
//       this.setAutoHideError('registerError');
//     }
//   }

//   onLogin() {
//     if (this.loginForm.valid) {
//       this.loading = true;
//       this.authService.login(this.loginForm.value).subscribe({
//         next: (response) => {
//           this.loading = false;
//           if (response.isSuccess) {
//             this.router.navigate(['/auth/verify-otp'], {
//               queryParams: { email: this.loginForm.value.email }
//             });
//           }
//         },
//         error: (error) => {
//           this.loading = false;
//           if (error.status === 400) {
//             this.loginError = error.error?.errors ? Object.values(error.error.errors).flat().join(' ') : error.error?.message || 'Invalid login attempt';
//           } else if (error.status === 403) {
//             this.loginError = error.error?.message || 'Unauthorized access';
//           } else {
//             this.loginError = error.error?.message || 'Login failed';
//           }
//           this.setAutoHideError('loginError');
//         }
//       });
//     } else {
//       this.loginForm.markAllAsTouched();
//       this.loginError = 'Please fill out all required fields correctly';
//       this.setAutoHideError('loginError');
//     }
//   }

//   loginWithProvider(provider: string) {
//     this.loading = true;
//     this.authService.getExternalLoginUrl(provider).subscribe({
//       next: (response: any) => {
//         window.location.href = response.redirectUrl;
//       },
//       error: (error) => {
//         this.loading = false;
//         this.loginError = error.error?.message || 'External login failed';
//         this.setAutoHideError('loginError');
//       }
//     });
//   }

//   private checkPasswords(group: FormGroup) {
//     const pass = group.get('password')?.value;
//     const confirmPass = group.get('confirmPassword')?.value;
//     return pass === confirmPass ? null : { notSame: true };
//   }

//   private setAutoHideError(errorField: 'registerError' | 'loginError') {
//     if (this.errorTimeout) {
//       clearTimeout(this.errorTimeout);
//     }
//     this.errorTimeout = setTimeout(() => {
//       this[errorField] = '';
//       this.errorTimeout = null;
//     }, 5000);
//   }
// }

// login.component.ts
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';

@Component({
  selector: 'app-login',
  templateUrl: './auth.component.html',
  styleUrls: ['./shared-auth-styles.css'],
  imports : [FormsModule]
})
export class AuthComponent {
  email: string = '';
  password: string = '';
  showPassword: boolean = false;

  constructor(private router: Router) {}

  onLogin(): void {
    // Basic validation
    if (!this.email || !this.password) {
      alert('Please fill in all fields');
      return;
    }

    // Email validation
    const emailPattern = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!emailPattern.test(this.email)) {
      alert('Please enter a valid email address');
      return;
    }

    // Here you would typically call your authentication service
    console.log('Login attempt:', {
      email: this.email,
      password: this.password
    });

    // Simulate login process
    this.authenticateUser();
  }

  private authenticateUser(): void {
    // Simulate API call
    setTimeout(() => {
      // For demo purposes, accept any valid email/password combination
      if (this.email && this.password.length >= 6) {
        console.log('Login successful');
        // Navigate to dashboard or home page
        // this.router.navigate(['/dashboard']);
        alert('Login successful! (This is a demo)');
      } else {
        alert('Invalid credentials. Password must be at least 6 characters.');
      }
    }, 1000);
  }

  loginWithGoogle(): void {
    console.log('Google login clicked');
    // Implement Google OAuth login
    alert('Google login clicked (Demo)');
  }

  loginWithFacebook(): void {
    console.log('Facebook login clicked');
    // Implement Facebook OAuth login
    alert('Facebook login clicked (Demo)');
  }

  loginWithTwitter(): void {
    console.log('Twitter login clicked');
    // Implement Twitter OAuth login
    alert('Twitter login clicked (Demo)');
  }

  togglePasswordVisibility(): void {
    this.showPassword = !this.showPassword;
  }

  // Additional helper methods
  onForgotPassword(): void {
    console.log('Forgot password clicked');
    // Navigate to forgot password page
    // this.router.navigate(['/forgot-password']);
    alert('Forgot password clicked (Demo)');
  }

  onSignUp(): void {
    console.log('Sign up clicked');
    // Navigate to sign up page
    // this.router.navigate(['/signup']);
    alert('Sign up clicked (Demo)');
  }

  // Form validation helpers
  isEmailValid(): boolean {
    const emailPattern = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return emailPattern.test(this.email);
  }

  isPasswordValid(): boolean {
    return this.password.length >= 6;
  }

  isFormValid(): boolean {
    return this.isEmailValid() && this.isPasswordValid();
  }
}