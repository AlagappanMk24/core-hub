// import { Component, OnInit } from '@angular/core';
// import { CommonModule } from '@angular/common';
// import { Router, NavigationEnd } from '@angular/router';
// import { filter } from 'rxjs/operators';

// interface MenuItem {
//   label: string;
//   icon: string;
//   route: string;
//   active: boolean;
//   hasSubmenu?: boolean;
//   submenuOpen?: boolean;
//   subItems?: MenuItem[];
// }

// @Component({
//   selector: 'app-sidebar',
//   standalone: true,
//   imports: [CommonModule],
//   templateUrl: './sidebar.component.html',
//   styleUrls: ['./sidebar.component.css'],
// })

// export class SidebarComponent implements OnInit {
//   menuItems: MenuItem[] = [
//     { label: 'Dashboard', icon: 'fas fa-th-large', route: '/dashboard', active: true },
//     { label: 'Invoices', icon: 'fas fa-file-invoice', route: '/invoices', active: false },
//      {
//       label: 'Settings',
//       icon: 'fas fa-cog',
//       route: '/settings',
//       active: false,
//       hasSubmenu: true,
//       submenuOpen: false,
//       subItems: [
//         { label: 'Invoice Settings', icon: 'fas fa-file-invoice-dollar', route: '/settings/invoice', active: false },
//        { label: 'Email Settings', icon: 'fas fa-envelope', route: '/settings/email', active: false },
//       ],
//     },
//   ];

//   constructor(private router: Router) {}

//   ngOnInit(): void {
//     // Sync active state with current route
//     this.router.events
//       .pipe(filter((event) => event instanceof NavigationEnd))
//       .subscribe((event: NavigationEnd) => {
//         this.updateActiveMenuItem(event.urlAfterRedirects);
//       });

//     // Set initial active state based on current route
//     this.updateActiveMenuItem(this.router.url);
//   }

//  toggleSubmenu(item: MenuItem): void {
//     if (item.hasSubmenu) {
//       item.submenuOpen = !item.submenuOpen;
//     }
//   }

//   onMenuItemClick(item: MenuItem): void {
//     if (item.hasSubmenu) {
//       this.toggleSubmenu(item);
//     } else {
//       this.menuItems.forEach((menuItem) => {
//         menuItem.active = false;
//         if (menuItem.subItems) {
//           menuItem.subItems.forEach((subItem) => (subItem.active = false));
//         }
//       });
//       item.active = true;
//       this.router.navigate([item.route]).catch((err) => {
//         console.error('Navigation error:', err);
//       });
//     }
//   }

//    private updateActiveMenuItem(currentUrl: string): void {
//     this.menuItems.forEach((item) => {
//       if (item.hasSubmenu && item.subItems) {
//         item.submenuOpen = item.subItems.some((subItem) => currentUrl.startsWith(subItem.route));
//         item.active = item.submenuOpen;
//         item.subItems.forEach((subItem) => {
//           subItem.active = currentUrl === subItem.route || currentUrl.startsWith(subItem.route + '/');
//         });
//       } else {
//         item.active = currentUrl === item.route || currentUrl.startsWith(item.route + '/');
//       }
//     });
//   }
// }

import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, NavigationEnd } from '@angular/router';
import { filter } from 'rxjs/operators';
import { AuthService } from '../../services/auth/auth.service';

interface MenuItem {
  label: string;
  icon: string;
  route: string;
  active: boolean;
  hasSubmenu?: boolean;
  submenuOpen?: boolean;
  subItems?: MenuItem[];
  requiredRole?: string; // Optional role requirement for menu items
}

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './sidebar.component.html',
  styleUrls: ['./sidebar.component.css'],
})
export class SidebarComponent implements OnInit {
  user: { fullName: string; roles: string[] } | null = null;
  menuItems: MenuItem[] = [
    {
      label: 'Dashboard',
      icon: 'fas fa-th-large',
      route: '/dashboard', // Will be overridden for customers
      active: true,
    },
    {
      label: 'Invoices',
      icon: 'fas fa-file-invoice',
      route: '/invoices',
      active: false,
    },
    {
      label: 'Customers',
      icon: 'fas fa-users',
      route: '/customers',
      active: false,
      requiredRole: 'Admin', // Users can view (read-only) if endpoint allows
    },
     {
      label: 'Payments',
      icon: 'fas fa-credit-card',
      route: '/payments',
      active: false,
    },
    {
      label: 'Reports',
      icon: 'fas fa-chart-bar',
      route: '/reports',
      active: false,
      requiredRole: 'Admin',
    },
    {
      label: 'Account',
      icon: 'fas fa-user',
      route: '/account',
      active: false,
      hasSubmenu: true,
      submenuOpen: false,
      subItems: [
        {
          label: 'Profile',
          icon: 'fas fa-user-edit',
          route: '/account/profile',
          active: false,
        },
        {
          label: 'Change Password',
          icon: 'fas fa-key',
          route: '/account/change-password',
          active: false,
        },
        {
          label: 'Users',
          icon: 'fas fa-user-shield',
          route: '/account/users',
          active: false,
          requiredRole: 'Admin',
        },
      ],
    },
     {
      label: 'Support',
      icon: 'fas fa-headset',
      route: '/support',
      active: false,
    },
    {
      label: 'Settings',
      icon: 'fas fa-cog',
      route: '/settings',
      active: false,
      hasSubmenu: true,
      submenuOpen: false,
      subItems: [
        {
          label: 'Invoice Settings',
          icon: 'fas fa-file-invoice-dollar',
          route: '/settings/invoice',
          active: false,
          requiredRole: 'Admin',
        },
        {
          label: 'Email Settings',
          icon: 'fas fa-envelope',
          route: '/settings/email',
          active: false,
          requiredRole: 'Admin',
        },
        {
          label: 'Company Settings',
          icon: 'fas fa-building',
          route: '/settings/company',
          active: false,
          requiredRole: 'Admin',
        },
      ],
    },
  ];

  constructor(private router: Router, private authService: AuthService) {}

  ngOnInit(): void {
    // Get user details
    this.user = this.authService.getUserDetail();

     // Override Dashboard route for Customer role
    if (this.user?.roles.includes('Customer') && !this.user?.roles.includes('Admin')) {
      const dashboardItem = this.menuItems.find((item) => item.label === 'Dashboard');
      if (dashboardItem) {
        dashboardItem.route = '/customer-dashboard';
      }
    }

    // Filter menu items based on user roles
    this.filterMenuItems();

    // Sync active state with current route
    this.router.events
      .pipe(filter((event) => event instanceof NavigationEnd))
      .subscribe((event: NavigationEnd) => {
        this.updateActiveMenuItem(event.urlAfterRedirects);
      });

    // Set initial active state based on current route
    this.updateActiveMenuItem(this.router.url);
  }

filterMenuItems(): void {
    const userRoles = this.user?.roles || [];
    this.menuItems = this.menuItems
      .map((item) => {
        if (item.hasSubmenu && item.subItems) {
          const filteredSubItems = item.subItems.filter(
            (subItem) => !subItem.requiredRole || userRoles.includes(subItem.requiredRole)
          );
          return {
            ...item,
            subItems: filteredSubItems,
            // Only show parent menu if it has sub-items or no required role
            requiredRole: item.requiredRole && filteredSubItems.length === 0 ? item.requiredRole : undefined,
          };
        }
        return item;
      })
      .filter(
        (item) =>
          (!item.requiredRole || userRoles.includes(item.requiredRole)) &&
          (!item.hasSubmenu || (item.subItems && item.subItems.length > 0))
      );

    // Special case for Customers: Remove Customers and Reports
    if (userRoles.includes('Customer') && !userRoles.includes('Admin') && !userRoles.includes('User')) {
      this.menuItems = this.menuItems.filter(
        (item) => item.label !== 'Customers' && item.label !== 'Reports'
      );
    }

    // Special case for Users: Make Customers read-only (if endpoint exists)
    if (userRoles.includes('User') && !userRoles.includes('Admin')) {
      const customersItem = this.menuItems.find((item) => item.label === 'Customers');
      if (customersItem) {
        customersItem.route = '/customers/view'; // Hypothetical read-only route
      }
    }
  }


  toggleSubmenu(item: MenuItem): void {
    if (item.hasSubmenu) {
      item.submenuOpen = !item.submenuOpen;
    }
  }

  onMenuItemClick(item: MenuItem): void {
    if (item.hasSubmenu) {
      this.toggleSubmenu(item);
    } else {
      this.menuItems.forEach((menuItem) => {
        menuItem.active = false;
        if (menuItem.subItems) {
          menuItem.subItems.forEach((subItem) => (subItem.active = false));
        }
      });
      item.active = true;
      this.router.navigate([item.route]).catch((err) => {
        console.error('Navigation error:', err);
      });
    }
  }

  private updateActiveMenuItem(currentUrl: string): void {
    this.menuItems.forEach((item) => {
      if (item.hasSubmenu && item.subItems) {
        item.submenuOpen = item.subItems.some((subItem) => currentUrl.startsWith(subItem.route));
        item.active = item.submenuOpen;
        item.subItems.forEach((subItem) => {
          subItem.active = currentUrl === subItem.route || currentUrl.startsWith(subItem.route + '/');
        });
      } else {
        item.active = currentUrl === item.route || currentUrl.startsWith(item.route + '/');
      }
    });
  }

   logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }  
  getInitials(fullName: string): string {
    if (!fullName) return '??';
    const names = fullName.trim().split(/\s+/);
    if (names.length === 0) return '??';
    if (names.length === 1) return names[0].charAt(0).toUpperCase();
    return `${names[0].charAt(0)}${names[names.length - 1].charAt(0)}`.toUpperCase();
  }


}