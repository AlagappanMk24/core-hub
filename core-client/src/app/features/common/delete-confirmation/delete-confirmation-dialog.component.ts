import { Component, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

export interface DeleteDialogData {
  title?: string;
  message?: string;
  itemName?: string;
  confirmText?: string;
  cancelText?: string;
}

@Component({
  selector: 'app-delete-confirmation-dialog',
  template: `
    <div class="delete-dialog-container">
      <!-- Close Button
      <button mat-icon-button class="close-btn" (click)="onCancel()">
        <mat-icon>close</mat-icon>
      </button> -->

      <!-- Icon -->
      <div class="icon-container">
        <mat-icon class="delete-icon">close</mat-icon>
      </div>

      <!-- Title -->
      <h2 class="dialog-title">{{ data.title || 'Are you sure?' }}</h2>

      <!-- Message -->
      <p class="dialog-message">
        {{ data.message || 'Do you really want to delete these records? This process cannot be undone.' }}
        <span *ngIf="data.itemName" class="item-name">{{ data.itemName }}</span>
      </p>

      <!-- Action Buttons -->
      <div class="dialog-actions">
        <button mat-button class="cancel-btn" (click)="onCancel()">
          {{ data.cancelText || 'Cancel' }}
        </button>
        <button mat-raised-button class="delete-btn" (click)="onConfirm()">
          {{ data.confirmText || 'Delete' }}
        </button>
      </div>
    </div>
  `,
  styles: [`
    .delete-dialog-container {
      position: relative;
      padding: 32px;
      text-align: center;
      min-width: 400px;
      background: white;
      border-radius: 16px;
      box-shadow: 0 10px 25px rgba(0, 0, 0, 0.1);
    }

    .icon-container {
      display: flex;
      align-items: center;
      justify-content: center;
      width: 80px;
      height: 80px;
      margin: 0 auto 24px;
      background: #fef2f2;
      border: 3px solid #fecaca;
      border-radius: 50%;
    }

    .delete-icon {
      font-size: 40px;
      width: 40px;
      height: 40px;
      color: #ef4444;
    }

    .dialog-title {
      font-size: 24px;
      font-weight: 600;
      color: #1f2937;
      margin: 0 0 16px;
      line-height: 1.3;
    }

    .dialog-message {
      font-size: 16px;
      color: #6b7280;
      line-height: 1.5;
      margin: 0 0 32px;
      max-width: 340px;
      margin-left: auto;
      margin-right: auto;
    }

    .item-name {
      font-weight: 600;
      color: #374151;
    }

    .dialog-actions {
      display: flex;
      gap: 12px;
      justify-content: center;
    }

    .cancel-btn {
      padding: 12px 24px;
      border: none;
      background: #f3f4f6;
      color: #6b7280;
      font-size: 16px;
      font-weight: 500;
      border-radius: 8px;
      cursor: pointer;
      transition: all 0.2s ease;
      min-width: 100px;
    }

    .cancel-btn:hover {
      background: #e5e7eb;
      color: #374151;
    }

    .delete-btn {
      padding: 12px 24px;
      border: none;
      background: #f15e5e;
      color: white;
      font-size: 16px;
      font-weight: 500;
      border-radius: 8px;
      cursor: pointer;
      transition: all 0.2s ease;
      min-width: 100px;
      box-shadow: 0 2px 4px rgba(239, 68, 68, 0.2);
    }

    .delete-btn:hover {
      background: #ee3535;
      transform: translateY(-1px);
      box-shadow: 0 4px 8px rgba(239, 68, 68, 0.3);
    }

    .delete-btn:active {
      transform: translateY(0);
    }

    @media (max-width: 480px) {
      .delete-dialog-container {
        min-width: 320px;
        padding: 24px;
      }

      .icon-container {
        width: 60px;
        height: 60px;
        margin-bottom: 20px;
      }

      .delete-icon {
        font-size: 30px;
        width: 30px;
        height: 30px;
      }

      .dialog-title {
        font-size: 20px;
        margin-bottom: 12px;
      }

      .dialog-message {
        font-size: 14px;
        margin-bottom: 24px;
      }

      .dialog-actions {
        flex-direction: column;
      }

      .cancel-btn,
      .delete-btn {
        width: 100%;
        margin: 0;
      }
    }
  `],
  standalone: true,
  imports: [
    CommonModule,
    MatDialogModule,
    MatButtonModule,
    MatIconModule
  ]
})
export class DeleteConfirmationDialogComponent {
  constructor(
    public dialogRef: MatDialogRef<DeleteConfirmationDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: DeleteDialogData
  ) {}

  onCancel(): void {
    this.dialogRef.close(false);
  }

  onConfirm(): void {
    this.dialogRef.close(true);
  }
}