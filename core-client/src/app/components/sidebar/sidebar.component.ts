import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, NavigationEnd } from '@angular/router';
import { filter } from 'rxjs/operators';

interface MenuItem {
  label: string;
  icon: string;
  route: string;
  active: boolean;
  hasSubmenu?: boolean;
  submenuOpen?: boolean;
}

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './sidebar.component.html',
  styleUrls: ['./sidebar.component.css'],
})

export class SidebarComponent implements OnInit {
  menuItems: MenuItem[] = [
    { label: 'Dashboard', icon: 'fas fa-th-large', route: '/dashboard', active: true },
    { label: 'Customers', icon: 'fas fa-layers', route: '/customers', active: false, hasSubmenu: true },
    { label: 'Categories', icon: 'fas fa-star', route: '/categories', active: false },
    { label: 'Products', icon: 'fas fa-edit', route: '/products', active: false },
    { label: 'Orders', icon: 'fas fa-chart-bar', route: '/orders', active: false },
    { label: 'Invoices', icon: 'fas fa-file-icon', route: '/invoices', active: false },
    { label: 'Tables', icon: 'fas fa-table', route: '/tables', active: false },
    { label: 'Sample Pages', icon: 'fas fa-file-alt', route: '/sample-pages', active: false, hasSubmenu: true },
    { label: 'Projects', icon: 'fas fa-briefcase', route: '/projects', active: false },
  ];

  constructor(private router: Router) {}

  ngOnInit(): void {
    // Sync active state with current route
    this.router.events
      .pipe(filter((event) => event instanceof NavigationEnd))
      .subscribe((event: NavigationEnd) => {
        this.updateActiveMenuItem(event.urlAfterRedirects);
      });

    // Set initial active state based on current route
    this.updateActiveMenuItem(this.router.url);
  }

  onMenuItemClick(item: MenuItem): void {
    // Reset all active states
    this.menuItems.forEach((menuItem) => (menuItem.active = false));
    // Set clicked item as active
    item.active = true;
    // Navigate to the route
    this.router.navigate([item.route]).catch((err) => {
      console.error('Navigation error:', err);
    });
  }

  private updateActiveMenuItem(currentUrl: string): void {
    this.menuItems.forEach((item) => {
      item.active = currentUrl === item.route || currentUrl.startsWith(item.route + '/');
    });
  }
}