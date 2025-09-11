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
import { InvoiceNotification, NotificationService } from '../../services/notification/notification.service';
import { Subject, takeUntil } from 'rxjs';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [FormsModule, CommonModule],
  templateUrl: './header.component.html',
  styleUrls: ['./header.component.css'],
})
export class HeaderComponent {
  authService = inject(AuthService);
  notificationService = inject(NotificationService);
  router = inject(Router);

  @Input() userName: string = 'Guest';
  @Input() userAvatar: string =
    'https://images.unsplash.com/photo-1472099645785-5658abf4ff4e?w=40&h=40&fit=crop&crop=face';

  searchQuery: string = '';
  isDropdownOpen: boolean = false;
  isNotificationDropdownOpen: boolean = false;
  notifications: InvoiceNotification[] = [];
  private destroy$ = new Subject<void>();

  @Output() themeChanged = new EventEmitter<string>();

  constructor() {
    const user = this.authService.getUserDetail();
    if (user) {
      this.userName = user.fullName || user.email || 'User';
    }
  }

  ngOnInit(): void {
    this.notificationService.notifications$
      .pipe(takeUntil(this.destroy$))
      .subscribe((notifications) => {
        console.log('Notifications in HeaderComponent:', notifications); // Debug
        this.notifications = notifications;
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  toggleDropdown(): void {
    this.isDropdownOpen = !this.isDropdownOpen;
    this.isNotificationDropdownOpen = false; // Close notification dropdown when user dropdown is toggled
  }

  toggleNotificationDropdown(): void {
    this.isNotificationDropdownOpen = !this.isNotificationDropdownOpen;
    this.isDropdownOpen = false; // Close user dropdown when notification dropdown is toggled
    if (this.isNotificationDropdownOpen) {
      // Optionally mark all notifications as read when opening the dropdown
      this.notifications.forEach((n) => {
        if (!n.read) {
          this.notificationService.markAsRead(n);
        }
      });
    }
  }

  viewInvoice(notification: InvoiceNotification): void {
    this.notificationService.markAsRead(notification);
    this.notificationService.navigateToInvoice(notification.invoiceId);
    this.isNotificationDropdownOpen = false;
  }
  clearNotifications(): void {
    this.notificationService.clearNotifications();
    this.isNotificationDropdownOpen = false;
  }
  logout(): void {
    this.authService.logout();
    this.isDropdownOpen = false;
    this.router.navigate(['/auth/login']);
  }

  onThemeChange(theme: string): void {
    this.themeChanged.emit(theme);
  }
  get hasNotifications(): boolean {
    return this.notificationService.hasNotifications;
  }
    get unreadCount(): number {
    return this.notifications.filter((n) => !n.read).length;
  }
}
