// features/admin/company-requests/company-requests.component.ts
import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import {
  CompanyRequestService,
  CompanyRequestResponse,
} from '../../services/company-request/company-request.service';
import { MatDialog } from '@angular/material/dialog';
import { NotificationDialogComponent } from '../../components/notification/notification-dialog.component';

@Component({
  selector: 'app-company-requests',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  template: `
    <div class="company-requests-container">
      <!-- Header -->
      <div class="header">
        <div class="header-left">
          <h1>Company Registration Requests</h1>
          <p class="subtitle">
            Manage and review new company registration requests
          </p>
        </div>
        <div class="header-right">
          <div class="stats">
            <div class="stat-item pending">
              <span class="stat-label">Pending</span>
              <span class="stat-value">{{ stats.pending }}</span>
            </div>
            <div class="stat-item approved">
              <span class="stat-label">Approved</span>
              <span class="stat-value">{{ stats.approved }}</span>
            </div>
            <div class="stat-item rejected">
              <span class="stat-label">Rejected</span>
              <span class="stat-value">{{ stats.rejected }}</span>
            </div>
          </div>
        </div>
      </div>

      <!-- Filters -->
      <div class="filters">
        <div class="search-box">
          <i class="fas fa-search"></i>
          <input
            type="text"
            [(ngModel)]="searchTerm"
            (ngModelChange)="onSearch()"
            placeholder="Search by name, email or company..."
          />
        </div>

        <select
          [(ngModel)]="statusFilter"
          (ngModelChange)="loadRequests()"
          class="status-filter"
        >
          <option value="">All Status</option>
          <option value="Pending">Pending</option>
          <option value="Approved">Approved</option>
          <option value="Rejected">Rejected</option>
        </select>
      </div>

      <!-- Requests Table -->
      <div class="table-container">
        <table class="requests-table">
          <thead>
            <tr>
              <th>ID</th>
              <th>Requester</th>
              <th>Email</th>
              <th>Company Name</th>
              <th>Requested</th>
              <th>Status</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            <tr *ngFor="let request of requests">
              <td>#{{ request.id }}</td>
              <td>{{ request.fullName }}</td>
              <td>{{ request.email }}</td>
              <td style='color : #8b5cf6;'> {{ request.companyName }}</td>
              <td>{{ request.requestedAt | date: 'MMM d, yyyy' }}</td>
              <td>
                <span
                  class="status-badge"
                  [class]="request.status.toLowerCase()"
                >
                  {{ request.status }}
                </span>
              </td>
              <td>
                <button
                  class="btn-view"
                  [routerLink]="['/company-requests', request.id]"
                >
                  Review
                </button>
              </td>
            </tr>
            <tr *ngIf="requests.length === 0">
              <td colspan="7" class="no-data">No company requests found</td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- Pagination -->
      <div class="pagination" *ngIf="totalPages > 1">
        <button
          (click)="changePage(currentPage - 1)"
          [disabled]="currentPage === 1"
        >
          Previous
        </button>
        <span>Page {{ currentPage }} of {{ totalPages }}</span>
        <button
          (click)="changePage(currentPage + 1)"
          [disabled]="currentPage === totalPages"
        >
          Next
        </button>
      </div>
    </div>
  `,
  styles: [
    `
      .company-requests-container {
        padding: 30px;
        background: #f8fafc;
        min-height: 100vh;
      }

      .header {
        display: flex;
        justify-content: space-between;
        align-items: center;
        margin-bottom: 30px;
        background: white;
        padding: 20px 30px;
        border-radius: 12px;
        box-shadow: 0 2px 4px rgba(0, 0, 0, 0.02);
      }

      .header-left h1 {
        font-size: 24px;
        color: #1e293b;
        margin: 0 0 5px 0;
      }

      .subtitle {
        color: #64748b;
        margin: 0;
        font-size: 14px;
      }

      .stats {
        display: flex;
        gap: 20px;
      }

      .stat-item {
        text-align: center;
        min-width: 80px;
      }

      .stat-label {
        display: block;
        font-size: 12px;
        color: #64748b;
        margin-bottom: 5px;
      }

      .stat-value {
        font-size: 24px;
        font-weight: 600;
      }

      .stat-item.pending .stat-value {
        color: #f59e0b;
      }
      .stat-item.approved .stat-value {
        color: #10b981;
      }
      .stat-item.rejected .stat-value {
        color: #ef4444;
      }

      .filters {
        display: flex;
        gap: 15px;
        margin-bottom: 20px;
      }

      .search-box {
        flex: 1;
        position: relative;
      }

      .search-box i {
        position: absolute;
        left: 15px;
        top: 50%;
        transform: translateY(-50%);
        color: #94a3b8;
      }

      .search-box input {
        width: 100%;
        padding: 12px 15px 12px 45px;
        border: 1px solid #e2e8f0;
        border-radius: 8px;
        font-size: 14px;
        outline: none;
        transition: all 0.3s;
      }

      .search-box input:focus {
        border-color: #8b5cf6;
        box-shadow: 0 0 0 3px rgba(139, 92, 246, 0.1);
      }

      .status-filter {
        padding: 12px 15px;
        border: 1px solid #e2e8f0;
        border-radius: 8px;
        font-size: 14px;
        outline: none;
        min-width: 150px;
      }

      .table-container {
        background: white;
        border-radius: 12px;
        box-shadow: 0 4px 6px rgba(0, 0, 0, 0.02);
        overflow-x: auto;
      }

      .requests-table {
        width: 100%;
        border-collapse: collapse;
      }

      .requests-table th {
        text-align: left;
        padding: 15px 20px;
        background: #f8fafc;
        color: #475569;
        font-weight: 600;
        font-size: 14px;
        border-bottom: 2px solid #e2e8f0;
      }

      .requests-table td {
        padding: 15px 20px;
        border-bottom: 1px solid #e2e8f0;
        color: #1e293b;
      }

      .status-badge {
        display: inline-block;
        padding: 4px 12px;
        border-radius: 20px;
        font-size: 12px;
        font-weight: 600;
      }

      .status-badge.pending {
        background: #fef3c7;
        color: #92400e;
      }

      .status-badge.approved {
        background: #d1fae5;
        color: #065f46;
      }

      .status-badge.rejected {
        background: #fee2e2;
        color: #991b1b;
      }

      .btn-view {
        padding: 6px 12px;
        background: #8b5cf6;
        color: white;
        border: none;
        border-radius: 6px;
        cursor: pointer;
        font-size: 12px;
        transition: background 0.3s;
      }

      .btn-view:hover {
        background: #7c3aed;
      }

      .no-data {
        text-align: center;
        color: #94a3b8;
        padding: 40px;
      }

      .pagination {
        display: flex;
        justify-content: center;
        align-items: center;
        gap: 20px;
        margin-top: 20px;
      }

      .pagination button {
        padding: 8px 16px;
        border: 1px solid #e2e8f0;
        background: white;
        border-radius: 6px;
        cursor: pointer;
        transition: all 0.3s;
      }

      .pagination button:hover:not(:disabled) {
        background: #8b5cf6;
        color: white;
        border-color: #8b5cf6;
      }

      .pagination button:disabled {
        opacity: 0.5;
        cursor: not-allowed;
      }
    `,
  ],
})
export class CompanyRequestsComponent implements OnInit {
  requests: CompanyRequestResponse[] = [];
  currentPage = 1;
  pageSize = 10;
  totalPages = 1;
  searchTerm = '';
  statusFilter = '';

  stats = {
    pending: 0,
    approved: 0,
    rejected: 0,
  };
  private dialog = inject(MatDialog);
  constructor(
    private companyRequestService: CompanyRequestService,
      private router: Router,
  ) {}

  ngOnInit(): void {
    this.loadRequests();
  }

  loadRequests(): void {
    this.companyRequestService
      .getAllRequests({
        page: this.currentPage,
        pageSize: this.pageSize,
        status: this.statusFilter,
        search: this.searchTerm,
      })
      .subscribe({
        next: (response) => {
          this.requests = response.requests;
          this.totalPages = Math.ceil(response.totalCount / this.pageSize);
          this.stats = {
            pending: response.pendingCount,
            approved: response.approvedCount,
            rejected: response.rejectedCount,
          };
        },
          error: (error) => {
          console.error('Error loading requests:', error);
          if (error.status === 401) {
            this.dialog.open(NotificationDialogComponent, {
              data: {
                type: 'error',
                title: 'Unauthorized!',
                message: 'Please login again.',
              },
              width: '450px',
              disableClose: true,
            }).afterClosed().subscribe(() => {
              this.router.navigate(['/auth/login']);
            });
          } else {
            this.dialog.open(NotificationDialogComponent, {
              data: {
                type: 'error',
                title: 'Error!',
                message: 'Failed to load company requests.',
              },
              width: '450px',
              disableClose: true,
            });
          }
        },
      });
  }

  onSearch(): void {
    this.currentPage = 1;
    this.loadRequests();
  }

  changePage(page: number): void {
    this.currentPage = page;
    this.loadRequests();
  }
}
