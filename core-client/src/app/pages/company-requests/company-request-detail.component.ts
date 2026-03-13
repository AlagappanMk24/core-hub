// features/admin/company-requests/company-request-detail.component.ts
import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import {
  CompanyRequestService,
  CompanyRequestResponse,
} from '../../services/company-request/company-request.service';
import { MatDialog } from '@angular/material/dialog';
import { NotificationDialogComponent } from '../../components/notification/notification-dialog.component';
import { ProcessingDialogComponent } from '../../components/processing-dialog/processing-dialog.component';

@Component({
  selector: 'app-company-request-detail',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  template: `
    <div class="request-detail-container" *ngIf="request">
      <!-- Header with back button -->
      <div class="header">
        <button class="back-btn" [routerLink]="['/company-requests']">
          ← Back to Requests
        </button>
        <div class="status-badge" [class]="request.status.toLowerCase()">
          {{ request.status }}
        </div>
      </div>

      <!-- Main Content -->
      <div class="request-card">
        <div class="card-header">
          <h2>Company Registration Request #{{ request.id }}</h2>
          <p class="request-date">
            Requested on
            {{ request.requestedAt | date: 'MMMM d, yyyy hh:mm a' }}
          </p>
        </div>

        <div class="card-body">
          <!-- Requester Information -->
          <div class="info-section">
            <h3>Requester Information</h3>
            <div class="info-grid">
              <div class="info-item">
                <span class="label">Full Name</span>
                <span class="value">{{ request.fullName }}</span>
              </div>
              <div class="info-item">
                <span class="label">Email Address</span>
                <span class="value">{{ request.email }}</span>
              </div>
              <div class="info-item full-width">
                <span class="label">Requested Company</span>
                <span class="value company-name">{{
                  request.companyName
                }}</span>
              </div>
            </div>
          </div>

          <!-- Processing Information (if already processed) -->
          <div class="info-section" *ngIf="request.status !== 'Pending'">
            <h3>Processing Information</h3>
            <div class="info-grid">
              <div class="info-item">
                <span class="label">Processed By</span>
                <span class="value">{{ request.processedBy }}</span>
              </div>
              <div class="info-item">
                <span class="label">Processed At</span>
                <span class="value">{{
                  request.processedAt | date: 'MMMM d, yyyy hh:mm a'
                }}</span>
              </div>
              <div class="info-item full-width" *ngIf="request.rejectionReason">
                <span class="label">Rejection Reason</span>
                <span class="value rejection-reason">{{
                  request.rejectionReason
                }}</span>
              </div>
            </div>
          </div>

          <!-- Action Buttons (for pending requests) -->
          <div class="action-section" *ngIf="request.status === 'Pending'">
            <h3>Take Action</h3>

            <div class="action-buttons">
              <!-- Approve Button -->
              <button class="btn-approve" (click)="showApproveConfirm = true">
                <i class="fas fa-check-circle"></i>
                Approve Request
              </button>

              <!-- Reject Button -->
              <button class="btn-reject" (click)="showRejectForm = true">
                <i class="fas fa-times-circle"></i>
                Reject Request
              </button>
            </div>

            <!-- Approve Confirmation Modal -->
            <div class="modal" *ngIf="showApproveConfirm">
              <div class="modal-content">
                <h3>Confirm Approval</h3>
                <p>Are you sure you want to approve this company request?</p>
                <p class="warning">
                  This will create a new company and grant the user access.
                </p>

                <div class="modal-actions">
                  <button
                    class="btn-cancel"
                    (click)="showApproveConfirm = false"
                  >
                    Cancel
                  </button>
                  <button
                    class="btn-confirm-approve"
                    (click)="confirmApprove()"
                  >
                    Yes, Approve
                  </button>
                </div>
              </div>
            </div>

            <!-- Reject Form Modal -->
            <div class="modal" *ngIf="showRejectForm">
              <div class="modal-content">
                <h3>Reject Request</h3>
                <p>Please provide a reason for rejection:</p>

                <textarea
                  [(ngModel)]="rejectionReason"
                  placeholder="Enter rejection reason..."
                  rows="4"
                ></textarea>

                <div class="modal-actions">
                  <button class="btn-cancel" (click)="cancelReject()">
                    Cancel
                  </button>
                  <button
                    class="btn-confirm-reject"
                    (click)="confirmReject()"
                    [disabled]="!rejectionReason"
                  >
                    Submit Rejection
                  </button>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [
    `
      .request-detail-container {
        padding: 30px;
        background: #f8fafc;
        min-height: 100vh;
      }

      .header {
        display: flex;
        justify-content: space-between;
        align-items: center;
        margin-bottom: 20px;
      }

      .back-btn {
        background: white;
        border: 1px solid #e2e8f0;
        padding: 10px 20px;
        border-radius: 8px;
        color: #64748b;
        cursor: pointer;
        transition: all 0.3s;
        text-decoration: none;
      }

      .back-btn:hover {
        background: #f8fafc;
        color: #8b5cf6;
        border-color: #8b5cf6;
      }

      .status-badge {
        padding: 8px 16px;
        border-radius: 30px;
        font-weight: 600;
        font-size: 14px;
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

      .request-card {
        background: white;
        border-radius: 16px;
        box-shadow: 0 4px 20px rgba(0, 0, 0, 0.05);
        overflow: hidden;
      }

      .card-header {
        padding: 25px 30px;
        background: linear-gradient(135deg, #8b5cf6, #6d28d9);
        color: white;
      }

      .card-header h2 {
        margin: 0 0 5px 0;
        font-size: 24px;
      }

      .request-date {
        margin: 0;
        opacity: 0.9;
        font-size: 14px;
      }

      .card-body {
        padding: 30px;
      }

      .info-section {
        margin-bottom: 30px;
        padding-bottom: 30px;
        border-bottom: 1px solid #e2e8f0;
      }

      .info-section:last-child {
        border-bottom: none;
        padding-bottom: 0;
        margin-bottom: 0;
      }

      .info-section h3 {
        color: #1e293b;
        margin: 0 0 20px 0;
        font-size: 18px;
      }

      .info-grid {
        display: grid;
        grid-template-columns: repeat(2, 1fr);
        gap: 20px;
      }

      .info-item {
        display: flex;
        flex-direction: column;
      }

      .info-item.full-width {
        grid-column: span 2;
      }

      .label {
        font-size: 12px;
        color: #64748b;
        margin-bottom: 5px;
        text-transform: uppercase;
        letter-spacing: 0.5px;
      }

      .value {
        font-size: 16px;
        color: #1e293b;
        font-weight: 500;
      }

      .company-name {
        font-size: 18px;
        font-weight: 600;
        color: #8b5cf6;
      }

      .rejection-reason {
        color: #991b1b;
        background: #fee2e2;
        padding: 10px;
        border-radius: 8px;
      }

      .action-section {
        margin-top: 20px;
      }

      .action-buttons {
        display: flex;
        gap: 15px;
        margin-top: 15px;
      }

      .btn-approve,
      .btn-reject {
        flex: 1;
        padding: 15px;
        border: none;
        border-radius: 10px;
        font-size: 16px;
        font-weight: 600;
        cursor: pointer;
        display: flex;
        align-items: center;
        justify-content: center;
        gap: 10px;
        transition: all 0.3s;
      }

      .btn-approve {
        background: #10b981;
        color: white;
      }

      .btn-approve:hover {
        background: #059669;
        transform: translateY(-2px);
        box-shadow: 0 4px 12px rgba(16, 185, 129, 0.3);
      }

      .btn-reject {
        background: #ef4444;
        color: white;
      }

      .btn-reject:hover {
        background: #dc2626;
        transform: translateY(-2px);
        box-shadow: 0 4px 12px rgba(239, 68, 68, 0.3);
      }

      /* Modal Styles */
      .modal {
        position: fixed;
        top: 0;
        left: 0;
        right: 0;
        bottom: 0;
        background: rgba(0, 0, 0, 0.5);
        display: flex;
        align-items: center;
        justify-content: center;
        z-index: 1000;
      }

      .modal-content {
        background: white;
        border-radius: 16px;
        padding: 30px;
        max-width: 500px;
        width: 90%;
      }

      .modal-content h3 {
        margin: 0 0 15px 0;
        color: #1e293b;
      }

      .modal-content p {
        color: #64748b;
        margin-bottom: 20px;
      }

      .warning {
        color: #991b1b;
        background: #fee2e2;
        padding: 10px;
        border-radius: 8px;
        font-size: 14px;
      }

      textarea {
        width: 100%;
        padding: 12px;
        border: 1px solid #e2e8f0;
        border-radius: 8px;
        margin-bottom: 20px;
        resize: vertical;
      }

      .modal-actions {
        display: flex;
        gap: 10px;
        justify-content: flex-end;
      }

      .btn-cancel {
        padding: 10px 20px;
        background: #e2e8f0;
        border: none;
        border-radius: 6px;
        cursor: pointer;
      }

      .btn-confirm-approve {
        padding: 10px 20px;
        background: #10b981;
        color: white;
        border: none;
        border-radius: 6px;
        cursor: pointer;
      }

      .btn-confirm-reject {
        padding: 10px 20px;
        background: #ef4444;
        color: white;
        border: none;
        border-radius: 6px;
        cursor: pointer;
      }

      .btn-confirm-reject:disabled {
        opacity: 0.5;
        cursor: not-allowed;
      }

      @media (max-width: 768px) {
        .info-grid {
          grid-template-columns: 1fr;
        }

        .info-item.full-width {
          grid-column: span 1;
        }

        .action-buttons {
          flex-direction: column;
        }
      }

      @keyframes slideIn {
        from {
          transform: translateY(-30px);
          opacity: 0;
        }
        to {
          transform: translateY(0);
          opacity: 1;
        }
      }

      @keyframes slideOut {
        from {
          transform: translateY(0);
          opacity: 1;
        }
        to {
          transform: translateY(-30px);
          opacity: 0;
        }
      }

      /* Custom SweetAlert2 animations */
      ::ng-deep .swal2-popup {
        animation: slideIn 0.3s ease-out;
      }

      ::ng-deep .swal2-popup.swal2-hide {
        animation: slideOut 0.3s ease-in !important;
      }
    `,
  ],
})
export class CompanyRequestDetailComponent implements OnInit {
  request: CompanyRequestResponse | null = null;
  requestId: number;

  showApproveConfirm = false;
  showRejectForm = false;
  rejectionReason = '';
  isProcessing = false;

  private dialog = inject(MatDialog);
  private processingDialogRef: any;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private companyRequestService: CompanyRequestService,
  ) {
    this.requestId = this.route.snapshot.params['id'];
  }

  ngOnInit(): void {
    this.loadRequest();
  }

  loadRequest(): void {
    this.companyRequestService.getRequestById(this.requestId).subscribe({
      next: (response) => {
        this.request = response;
      },
      error: (error) => {
        if (error.status === 401) {
          this.showNotification(
            'error',
            'Unauthorized!',
            'You are not authorized. Please login again.',
          )
            .afterClosed()
            .subscribe(() => {
              this.router.navigate(['/auth/login']);
            });
        } else {
          this.showNotification(
            'error',
            'Error!',
            error.error?.message || 'Failed to load request details.',
          )
            .afterClosed()
            .subscribe(() => {
              this.router.navigate(['/company-requests']);
            });
        }
      },
    });
  }

  confirmApprove(): void {
    // Close the approval confirmation modal
    this.showApproveConfirm = false;
    this.openProcessingDialog('Please wait while we approve the request.');

    this.approveRequest();
  }

  approveRequest(): void {
    this.companyRequestService.approveRequest(this.requestId).subscribe({
      next: () => {
        this.closeProcessingDialog();
        this.showNotification(
          'success',
          'Approved!',
          'Company request has been approved successfully!',
          'The requester has been notified and the company has been created.',
        )
          .afterClosed()
          .subscribe(() => {
            this.router.navigate(['/company-requests']);
          });
      },
      error: (error) => {
        this.closeProcessingDialog();
        this.showNotification(
          'error',
          'Approval Failed!',
          error.error?.message ||
            'Failed to approve request. Please try again.',
        );
      },
    });
  }

  confirmReject(): void {
    if (!this.rejectionReason) {
      this.showNotification(
        'error',
        'Reason Required!',
        'Please provide a reason for rejection.',
      );
      return;
    }
    // Close the reject form modal
    this.showRejectForm = false;

    this.openProcessingDialog('Please wait while we reject the request.');

    this.rejectRequest();
  }

  rejectRequest(): void {
    this.companyRequestService
      .rejectRequest(this.requestId, this.rejectionReason)
      .subscribe({
        next: (response) => {
          this.closeProcessingDialog();
          this.showNotification(
            'success',
            'Rejected!',
            'Company request has been rejected.',
            'The requester has been notified with the rejection reason.',
          )
            .afterClosed()
            .subscribe(() => {
              this.router.navigate(['/company-requests']);
            });
        },
        error: (error) => {
          this.closeProcessingDialog();
          this.showNotification(
            'error',
            'Rejection Failed!',
            error.error?.message ||
              'Failed to reject request. Please try again.',
          );
        },
      });
  }

  cancelReject(): void {
    this.showRejectForm = false;
    this.rejectionReason = '';
    // Navigate back to requests list when cancel is clicked
    this.router.navigate(['/company-requests']);
  }

  private showNotification(
    type: 'success' | 'error',
    title: string,
    message: string,
    submessage?: string,
  ) {
    return this.dialog.open(NotificationDialogComponent, {
      data: {
        type,
        title,
        message,
        submessage,
      },
      width: '420px',
      disableClose: true,
      panelClass: 'notification-dialog-panel',
    });
  }

  openProcessingDialog(message: string) {
    this.processingDialogRef = this.dialog.open(ProcessingDialogComponent, {
      disableClose: true,
      panelClass: 'processing-dialog',
      data: {
        title: 'Processing...',
        message: message,
      },
    });
  }
  closeProcessingDialog() {
    if (this.processingDialogRef) {
      this.processingDialogRef.close();
      this.processingDialogRef = null;
    }
  }
}