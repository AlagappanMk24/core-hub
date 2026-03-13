// layout.component.ts
import { Component, OnInit, HostListener } from '@angular/core';
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
  isMobileView = false;
  currentUser = {
    name: 'Guest',
    avatar:
      'https://images.unsplash.com/photo-1472099645785-5658abf4ff4e?w=40&h=40&fit=crop&crop=face',
  };

  constructor(private authService: AuthService) {
    this.checkScreenSize();
  }

  ngOnInit(): void {
    this.loadCurrentUser();
  }

  @HostListener('window:resize')
  checkScreenSize(): void {
    this.isMobileView = window.innerWidth <= 1024;
    if (this.isMobileView) {
      this.isSidebarCollapsed = true;
    }
  }
  loadCurrentUser(): void {
    const userDetails = this.authService.getUserDetail();
    if (userDetails) {
      this.currentUser.name =
        userDetails.fullName || userDetails.email || 'User';
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

  onSidebarCollapsed(collapsed: boolean): void {
    this.isSidebarCollapsed = collapsed;
  }

  toggleSidebar(): void {
    this.isSidebarCollapsed = !this.isSidebarCollapsed;
  }
}
