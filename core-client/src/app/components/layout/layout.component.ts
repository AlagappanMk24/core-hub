// layout.component.ts
import { Component, EventEmitter, inject, OnInit, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HeaderComponent } from '../header/header.component';
import { AuthService } from '../../services/auth.service';
import { SidebarComponent } from '../sidebar/sidebar.component';
import { RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-layout',
  standalone: true,
  imports: [CommonModule, SidebarComponent, HeaderComponent, RouterOutlet],
  templateUrl: './layout.component.html',
  styleUrls: ['./layout.component.css'],
})
export class LayoutComponent implements OnInit {
  isSidebarCollapsed = false;
  hasNotifications = true;

  currentUser = {
    name: 'Guest',
    avatar: 'https://images.unsplash.com/photo-1472099645785-5658abf4ff4e?w=40&h=40&fit=crop&crop=face'
  };

  private authService = inject(AuthService);

  ngOnInit(): void {
    this.loadCurrentUser();
  }

  loadCurrentUser(): void {
    const userDetails = this.authService.getUserDetail();
    if (userDetails) {
      this.currentUser.name = userDetails.fullName || userDetails.email || 'User';
      // You can also update the avatar if your userDetails contains an avatar URL
      // this.currentUser.avatar = userDetails.avatarUrl || this.currentUser.avatar;
    } else {
      this.currentUser.name = 'David Greyhenak'; // Default name matching the design
    }
  }

  // onMenuItemSelected(route: string): void {
  //   this.menuItemSelected.emit(route);
  // }

  onThemeChanged(theme: string): void {
    console.log('Theme changed to:', theme);
    // Here you would implement theme switching logic
    // this.themeService.setTheme(theme);
  }

  toggleSidebar(): void {
    this.isSidebarCollapsed = !this.isSidebarCollapsed;
  }
}