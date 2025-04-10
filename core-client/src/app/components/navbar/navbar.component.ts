// import { Component, inject } from '@angular/core';
// import { MatButtonModule } from '@angular/material/button';
// import { MatToolbarModule } from '@angular/material/toolbar';
// import { MatIconModule } from '@angular/material/icon';
// import { AuthService } from '../../services/auth.service';
// import { Router, RouterLink } from '@angular/router';

// @Component({
//   selector: 'app-navbar',
//   standalone: true,
//   imports: [MatToolbarModule, MatButtonModule, MatIconModule, RouterLink],
//   templateUrl: './navbar.component.html',
//   styleUrl: './navbar.component.css',
// })
// export class NavbarComponent {
//   authService = inject(AuthService);
//   router = inject(Router);

//   isLoggedIn(){
//     return this.authService.isLoggedIn();
//   }
// }

import { Component } from '@angular/core';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatMenuModule } from '@angular/material/menu';
import { MatSidenavModule } from '@angular/material/sidenav';
import { RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { NavModule } from '@coreui/angular';

@Component({
  selector: 'app-navbar',
  templateUrl: './navbar.component.html',
  styleUrls: ['./navbar.component.css'],
  imports: [
    NavModule,
    MatToolbarModule,
    MatIconModule,
    MatButtonModule,
    MatMenuModule,
    MatSidenavModule,
    RouterLink,
    CommonModule
  ],
})
export class NavbarComponent {
  isLoggedIn = true; // Change this based on authentication
  userName = 'John Doe';
  isDarkMode = false;

  toggleSidenav(sidenav: any) {
    sidenav.toggle();
  }

  toggleDarkMode() {
    this.isDarkMode = !this.isDarkMode;
  }

  logout() {
    console.log('User logged out');
    this.isLoggedIn = false;
  }
}
