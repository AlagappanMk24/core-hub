// layout.component.ts
import { Component, EventEmitter, inject, OnInit, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HeaderComponent } from '../header/header.component';
import { AuthService } from '../../services/auth/auth.service';
import { SidebarComponent } from '../sidebar/sidebar.component';
import { RouterOutlet } from '@angular/router';
import { NotificationService } from '../../services/notification/notification.service';

@Component({
  selector: 'app-layout',
  standalone: true,
  imports: [CommonModule, SidebarComponent, HeaderComponent, RouterOutlet],
  templateUrl: './layout.component.html',
  styleUrls: ['./layout.component.css'],
})
export class LayoutComponent implements OnInit {
  isSidebarCollapsed = false;
  currentUser = {
    name: 'Guest',
    avatar: 'https://images.unsplash.com/photo-1472099645785-5658abf4ff4e?w=40&h=40&fit=crop&crop=face'
  };

  private authService = inject(AuthService);
   private notificationService = inject(NotificationService);

  ngOnInit(): void {
    this.loadCurrentUser();
  }

  loadCurrentUser(): void {
    const userDetails = this.authService.getUserDetail();
    if (userDetails) {
      this.currentUser.name = userDetails.fullName || userDetails.email || 'User';
    } else {
      this.currentUser.name = 'David Greyhenak'; 
    }
  }

  // onMenuItemSelected(route: string): void {
  //   this.menuItemSelected.emit(route);
  // }

  onThemeChanged(theme: string): void {
    // Here you would implement theme switching logic
    // this.themeService.setTheme(theme);
  }

  toggleSidebar(): void {
    this.isSidebarCollapsed = !this.isSidebarCollapsed;
  }
}