import { Component, OnInit } from '@angular/core';
import { HttpClient, HttpClientModule } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { Router } from '@angular/router';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { MatPaginator, MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatSelectChange, MatSelectModule } from '@angular/material/select';
import { ViewChild } from '@angular/core';
import { AuthService } from '../../../services/auth/auth.service';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';

interface Invoice {
  id: number;
  invoiceNumber: string;
  customer: { name: string };
  totalAmount: number;
  dueDate: string;
  status: string;
}

@Component({
  selector: 'app-customer-dashboard',
  template: `
    <div class="container p-6">
      <h1 class="text-2xl font-bold text-purple-600 mb-4">Customer Dashboard</h1>
      <div class="mb-4">
        <mat-form-field appearance="outline">
          <mat-label>Filter by Status</mat-label>
          <mat-select [(ngModel)]="selectedStatus" (selectionChange)="onStatusChange($event)">
            <mat-option value="">All</mat-option>
            <mat-option value="Sent">Sent</mat-option>
            <mat-option value="Viewed">Viewed</mat-option>
            <mat-option value="Paid">Paid</mat-option>
            <mat-option value="Overdue">Overdue</mat-option>
          </mat-select>
        </mat-form-field>
      </div>
      <div class="table-container">
        <table mat-table [dataSource]="dataSource" class="mat-elevation-z2 w-full">
          <ng-container matColumnDef="invoiceNumber">
            <th mat-header-cell *matHeaderCellDef>Invoice Number</th>
            <td mat-cell *matCellDef="let invoice">{{ invoice.invoiceNumber }}</td>
          </ng-container>
          <ng-container matColumnDef="customerName">
            <th mat-header-cell *matHeaderCellDef>Customer</th>
            <td mat-cell *matCellDef="let invoice">{{ invoice.customer.name }}</td>
          </ng-container>
          <ng-container matColumnDef="totalAmount">
            <th mat-header-cell *matHeaderCellDef>Total Amount</th>
            <!-- <td mat-cell *matCellDef="let invoice">{{ invoice.totalAmount | currency }}</td> -->
          </ng-container>
          <ng-container matColumnDef="dueDate">
            <th mat-header-cell *matHeaderCellDef>Due Date</th>
            <!-- <td mat-cell *matCellDef="let invoice">{{ invoice.dueDate | date }}</td> -->
          </ng-container>
          <ng-container matColumnDef="status">
            <th mat-header-cell *matHeaderCellDef>Status</th>
            <td mat-cell *matCellDef="let invoice">{{ invoice.status }}</td>
          </ng-container>
          <ng-container matColumnDef="actions">
            <th mat-header-cell *matHeaderCellDef>Actions</th>
            <td mat-cell *matCellDef="let invoice">
              <button mat-icon-button (click)="viewInvoice(invoice.id)">
                <mat-icon>visibility</mat-icon>
              </button>
              <!-- <button mat-icon-button (click)="downloadPdf(invoice.id)">
                <mat-icon>download</mat-icon>
              </button> -->
            </td>
          </ng-container>
          <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
          <tr mat-row *matRowDef="let row; columns: displayedColumns;"></tr>
        </table>
      </div>
      <mat-paginator
        #paginator
        [pageSizeOptions]="[5, 10, 20]"
        [pageSize]="pageSize"
        [pageIndex]="pageIndex"
        [length]="totalCount"
        (page)="onPageChange($event)"
        showFirstLastButtons
      ></mat-paginator>
    </div>
  `,
  styles: [
    `
      .container {
        max-width: 1200px;
        margin: 0 auto;
      }
      .table-container {
        overflow-x: auto;
      }
      table {
        width: 100%;
        border-collapse: collapse;
      }
      th, td {
        padding: 12px;
        text-align: left;
      }
      th {
        background-color: #f5f5f5;
      }
    `
  ],
  imports :[
    MatTableModule,
    MatPaginatorModule,
    MatSelectModule,
    MatFormFieldModule,
    MatIconModule,
    MatButtonModule,
    FormsModule,
    HttpClientModule
  ]
})
export class CustomerDashboardComponent implements OnInit {
  dataSource = new MatTableDataSource<Invoice>([]);
  displayedColumns: string[] = ['invoiceNumber', 'customerName', 'totalAmount', 'dueDate', 'status', 'actions'];
  pageIndex = 0;
  pageSize = 10;
  totalCount = 0;
  selectedStatus = '';
  @ViewChild(MatPaginator) paginator!: MatPaginator;

  constructor(
    private http: HttpClient,
    private authService: AuthService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.checkAuthAndLoad();
  }

  private checkAuthAndLoad(): void {
    if (!this.authService.isAuthenticated() || 
        (!this.authService.hasRole('Customer') && !this.authService.hasRole('Admin'))) {
      this.router.navigate(['/auth/login']);
      return;
    }
    // this.loadInvoices();
  }

  // loadInvoices(): void {
  //   const params: any = {
  //     pageNumber: this.pageIndex + 1,
  //     pageSize: this.pageSize
  //   };
  //   if (this.selectedStatus) {
  //     params.status = this.selectedStatus;
  //   }

  //   this.http.get(`${environment.apiUrl}/api/customer-dashboard/invoices`, { 
  //     params,
  //     headers: { Authorization: `Bearer ${this.authService.getAuthToken()}` }
  //   })
  //     .subscribe({
  //       next: (response: any) => {
  //         this.dataSource.data = response.items.map((invoice: any) => ({
  //           ...invoice,
  //           status: new Date(invoice.dueDate) < new Date() && invoice.status !== 'Paid' 
  //             ? 'Overdue' 
  //             : invoice.status
  //         }));
  //         this.totalCount = response.totalCount;
  //         this.pageIndex = response.pageNumber - 1;
  //         this.pageSize = response.pageSize;
  //       },
  //       error: (err) => {
  //         console.error('Error loading invoices:', err);
  //         if (err.status === 401) {
  //           this.authService.logout();
  //           this.router.navigate(['/auth/login']);
  //         }
  //       }
  //     });
  // }

  onPageChange(event: PageEvent): void {
    this.pageIndex = event.pageIndex;
    this.pageSize = event.pageSize;
    // this.loadInvoices();
  }

  onStatusChange(event: MatSelectChange): void {
    this.selectedStatus = event.value;
    this.pageIndex = 0;
    // this.loadInvoices();
  }

  viewInvoice(id: number): void {
    this.router.navigate([`/invoices/view/${id}`]);
  }

}