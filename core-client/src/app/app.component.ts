// app.component.ts
import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { OrdersComponent } from './pages/orders/orders.component';
import { DashboardComponent } from "./pages/dashboard/dashboard.component";
import { LayoutComponent } from "./components/layout/layout.component";
@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule,OrdersComponent, DashboardComponent,LayoutComponent],
  templateUrl : './app.component.html',
  styleUrls : ['./app.component.css']
})
export class AppComponent {
  selectedRoute: string = 'dashboard';

  onMenuItemSelected(route: string): void {
    this.selectedRoute = route;
  }
}