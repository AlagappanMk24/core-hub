import { Component, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  MatDialogRef,
  MAT_DIALOG_DATA,
  MatDialogContent,
  MatDialogActions,
} from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-notification-dialog',
  template: `
    <div class="dialog-container">
      <button mat-icon-button class="close-btn" (click)="dialogRef.close()">
        <mat-icon>close</mat-icon>
      </button>
      <!-- Icon -->
      <div class="icon-container">
        <div class="icon-circle" [ngClass]="data.type">
          <mat-icon class="status-icon">{{ getIcon() }}</mat-icon>
        </div>
      </div>

      <!-- Title -->
      <h2 class="dialog-title" [ngClass]="data.type">
        {{ data.title || (data.type === 'success' ? 'Success' : 'Error') }}
      </h2>

      <!-- Message -->
      <mat-dialog-content class="dialog-content">
        <p class="dialog-message">{{ data.message }}</p>
        <p class="dialog-submessage" *ngIf="data.submessage">
          {{ data.submessage }}
        </p>
      </mat-dialog-content>

      <!-- Actions -->
      <mat-dialog-actions class="dialog-actions">
        <button
          mat-flat-button
          class="action-btn"
          [ngClass]="data.type"
          (click)="dialogRef.close(true)"
        >
          {{
            data.buttonText || (data.type === 'success' ? 'Ok' : 'Retry')
          }}
        </button>
      </mat-dialog-actions>
    </div>
  `,
  styles: [
    `
      .dialog-container {
        position: relative;
        background: #ffffff;
        border-radius: 24px;
        box-shadow: 0 25px 70px rgba(0, 0, 0, 0.12);
        padding: 48px 40px 40px;
        text-align: center;
        min-width: 380px;
        max-width: 440px;
        font-family:
          -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
      }

      .close-btn {
        position: absolute;
        top: 16px;
        right: 16px;
        // color: #9ca3af;
        transition: all 0.2s ease;
      }

      .close-btn:hover {
        // color: #4b5563;
        // background-color: rgba(0, 0, 0, 0.05);
      }

      .icon-container {
        margin-bottom: 24px;
        display: flex;
        justify-content: center;
      }

      .icon-circle {
        width: 92px;
        height: 92px;
        border-radius: 50%;
        display: flex;
        align-items: center;
        justify-content: center;
        border: 5px solid;
        background: #ffffff;
        box-shadow: 0 10px 30px rgba(0, 0, 0, 0.08);
      }

      .icon-circle.success {
        border-color: #22c55e;
      }

      .icon-circle.error {
        border-color: #ef4444;
      }

      .status-icon {
        font-size: 46px !important;
        width: 46px !important;
        height: 46px !important;
      }

      .icon-circle.success .status-icon {
        color: #22c55e;
      }

      .icon-circle.error .status-icon {
        color: #ef4444;
      }

      .dialog-title {
        font-size: 24px;
        font-weight: 700;
        margin: 0 0 16px 0;
        color: #1f2937;
        letter-spacing: -0.02em;
      }

      .dialog-title.success {
        color: #166534;
      }

      .dialog-title.error {
        color: #991b1b;
      }

      .dialog-content {
        padding: 0 0 32px 0;
        margin: 0;
      }

      .dialog-message {
        font-size: 17px;
        line-height: 1.55;
        color: #374151;
        font-weight: 500;
        margin: 0;
      }

      .dialog-submessage {
        font-size: 14.5px;
        color: #6b7280;
        line-height: 1.5;
        margin: 12px 0 0 0;
      }

      .dialog-actions {
        padding: 0;
        margin: 0;
        justify-content: center;
      }

      .action-btn {
        font-size: 16px;
        font-weight: 600;
        padding: 14px 48px;
        border-radius: 12px;
        min-width: 160px;
        text-transform: none;
        letter-spacing: 0.3px;
        box-shadow: 0 4px 14px rgba(0, 0, 0, 0.08);
        transition: all 0.25s cubic-bezier(0.4, 0, 0.2, 1);
      }

      .action-btn.success {
        background-color: #22c55e;
        color: white;
      }

      .action-btn.success:hover {
        background-color: #16a34a;
        transform: translateY(-2px);
        box-shadow: 0 8px 20px rgba(34, 197, 94, 0.35);
      }

      .action-btn.error {
        background-color: #ef4444;
        color: white;
      }

      .action-btn.error:hover {
        background-color: #dc2626;
        transform: translateY(-2px);
        box-shadow: 0 8px 20px rgba(239, 68, 68, 0.35);
      }

      /* Material overrides */
      :host ::ng-deep .mat-mdc-button {
        box-shadow: none !important;
      }

      @media (max-width: 480px) {
        .dialog-container {
          padding: 40px 28px 32px;
          min-width: 340px;
          border-radius: 20px;
        }

        .icon-circle {
          width: 80px;
          height: 80px;
        }

        .status-icon {
          font-size: 40px !important;
        }

        .dialog-title {
          font-size: 22px;
        }
        .close-btn {
          top: 8px;
          right: 8px;
        } /* Move X closer on mobile */
      }
    `,
  ],
  standalone: true,
  imports: [
    CommonModule,
    MatButtonModule,
    MatIconModule,
    MatDialogContent,
    MatDialogActions,
  ],
})
export class NotificationDialogComponent {
  constructor(
    public dialogRef: MatDialogRef<NotificationDialogComponent>,
    @Inject(MAT_DIALOG_DATA)
    public data: {
      type: 'success' | 'error';
      title?: string;
      message: string;
      submessage?: string;
      buttonText?: string;
    },
  ) {}

  getIcon(): string {
    return this.data.type === 'success' ? 'check_circle' : 'error_outline';
  }
}
