// sidebar.component.ts
import { Component, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';

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

export class SidebarComponent {
  @Output() menuItemSelected = new EventEmitter<string>();

  menuItems: MenuItem[] = [
    { label: 'Dashboard', icon: 'fas fa-th-large', route: 'dashboard', active: true },
    { label: 'Customers', icon: 'fas fa-layers', route: 'customers', active: false, hasSubmenu: true },
    { label: 'Categories', icon: 'fas fa-star', route: 'categories', active: false },
    { label: 'Products', icon: 'fas fa-edit', route: 'products', active: false },
    { label: 'Orders', icon: 'fas fa-chart-bar', route: 'orders', active: false },
    { label: 'Tables', icon: 'fas fa-table', route: 'tables', active: false },
    { label: 'Sample Pages', icon: 'fas fa-file-alt', route: 'sample-pages', active: false, hasSubmenu: true },
    { label: 'Projects', icon: 'fas fa-briefcase', route: 'projects', active: false }
  ];

  onMenuItemClick(item: MenuItem): void {
    // Reset all active states
    this.menuItems.forEach(menuItem => menuItem.active = false);
    // Set clicked item as active
    item.active = true;
    // Emit the selected route
    this.menuItemSelected.emit(item.route);
  }
}