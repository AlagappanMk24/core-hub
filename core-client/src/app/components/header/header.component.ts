// header.component.ts
// import { Component, Input, Output, EventEmitter } from '@angular/core';
// import { CommonModule } from '@angular/common';
// import { FormsModule } from '@angular/forms';

// @Component({
//   selector: 'app-header',
//   standalone: true,
//   imports: [CommonModule, FormsModule],
//   templateUrl: './header.component.html',
//   styleUrls: ['./header.component.css'],
// })
// export class HeaderComponent {
//   @Input() userName: string = 'David Greyhenak';
//   @Input() userAvatar: string = 'https://images.unsplash.com/photo-1472099645785-5658abf4ff4e?w=40&h=40&fit=crop&crop=face';
//   @Input() hasNotifications: boolean = true;
  
//   @Output() themeChanged = new EventEmitter<string>();

//   searchQuery: string = '';

//   onThemeChange(theme: string): void {
//     this.themeChanged.emit(theme);
//   }
// }

import { Component, Input, Output, EventEmitter, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth/auth.service';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [FormsModule, CommonModule],
  templateUrl: './header.component.html',
  styleUrls: ['./header.component.css'],
})
export class HeaderComponent {
  authService = inject(AuthService);
  router = inject(Router);

  @Input() userName: string = 'Guest';
  @Input() userAvatar: string = 'https://images.unsplash.com/photo-1472099645785-5658abf4ff4e?w=40&h=40&fit=crop&crop=face';
  @Input() hasNotifications: boolean = true; // Added @Input()
  searchQuery: string = '';
  isDropdownOpen: boolean = false;

  @Output() themeChanged = new EventEmitter<string>();

  constructor() {
    const user = this.authService.getUserDetail();
    if (user) {
      this.userName = user.fullName || user.email || 'User';
      // Optionally fetch avatar from backend or use a default
    }
  }

  toggleDropdown(): void {
    this.isDropdownOpen = !this.isDropdownOpen;
  }

  logout(): void {
    this.authService.logout();
    this.isDropdownOpen = false;
    this.router.navigate(['/auth/login']);
  }

  onThemeChange(theme: string): void {
    this.themeChanged.emit(theme);
  }
}