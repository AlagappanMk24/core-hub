import { Component, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogModule, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDividerModule } from '@angular/material/divider';
import { animate, style, transition, trigger } from '@angular/animations';
import { Customer } from '../../services/models/customer.model';

export interface QuickActionsData {
  customer: Customer;
}

@Component({
  selector: 'app-quick-actions-dialog',
  standalone: true,
  imports: [
    CommonModule,
    MatDialogModule,
    MatButtonModule,
    MatIconModule,
    MatDividerModule,
  ],
  template: `
    <div class="quick-actions-dialog" @dialogFadeIn>
      <div class="dialog-header">
        <h2>Quick Actions</h2>
        <button class="close-btn" (click)="onClose()">
          <mat-icon>close</mat-icon>
        </button>
      </div>
      
      <div class="dialog-content">
        <div class="customer-info">
          <div class="customer-avatar" [style.background]="getAvatarColor(data.customer.name)">
            {{ getInitials(data.customer.name) }}
          </div>
          <div class="customer-details">
            <h3>{{ data.customer.name }}</h3>
            <p>{{ data.customer.email }}</p>
          </div>
        </div>
        
        <mat-divider></mat-divider>
        
        <div class="actions-list">
          <button class="action-item" (click)="selectAction('createInvoice')">
            <mat-icon class="action-icon purple">receipt</mat-icon>
            <div class="action-info">
              <span class="action-title">Create Invoice</span>
              <span class="action-desc">Generate a new invoice for this customer</span>
            </div>
            <mat-icon class="arrow-icon">chevron_right</mat-icon>
          </button>
          
          <button class="action-item" (click)="selectAction('sendEmail')">
            <mat-icon class="action-icon blue">email</mat-icon>
            <div class="action-info">
              <span class="action-title">Send Email</span>
              <span class="action-desc">Compose and send an email</span>
            </div>
            <mat-icon class="arrow-icon">chevron_right</mat-icon>
          </button>
          
          <button class="action-item" (click)="selectAction('viewStatement')">
            <mat-icon class="action-icon green">description</mat-icon>
            <div class="action-info">
              <span class="action-title">View Statement</span>
              <span class="action-desc">Generate customer statement</span>
            </div>
            <mat-icon class="arrow-icon">chevron_right</mat-icon>
          </button>
          
          <button class="action-item" (click)="selectAction('addNote')">
            <mat-icon class="action-icon orange">note_add</mat-icon>
            <div class="action-info">
              <span class="action-title">Add Note</span>
              <span class="action-desc">Add a private note about this customer</span>
            </div>
            <mat-icon class="arrow-icon">chevron_right</mat-icon>
          </button>
          
          <button class="action-item" (click)="selectAction('scheduleFollowUp')">
            <mat-icon class="action-icon red">event</mat-icon>
            <div class="action-info">
              <span class="action-title">Schedule Follow-up</span>
              <span class="action-desc">Set a reminder for follow-up</span>
            </div>
            <mat-icon class="arrow-icon">chevron_right</mat-icon>
          </button>
        </div>
      </div>
      
      <div class="dialog-actions">
        <button class="cancel-btn" (click)="onClose()">Cancel</button>
      </div>
    </div>
  `,
  styles: [`
    .quick-actions-dialog {
      background: white;
      border-radius: 24px;
      overflow: hidden;
      min-width: 500px;
      max-width: 450px;
    }
    
    .dialog-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 20px 24px;
      background: linear-gradient(135deg, #f8fafc 0%, #ffffff 100%);
      border-bottom: 1px solid #e2e8f0;
    }
    
    .dialog-header h2 {
      margin: 0;
      font-size: 20px;
      font-weight: 600;
      color: #1e293b;
    }
    
    .close-btn {
      width: 32px;
      height: 32px;
      border-radius: 8px;
      border: none;
      background: transparent;
      cursor: pointer;
      display: flex;
      align-items: center;
      justify-content: center;
      color: #64748b;
      transition: all 0.2s ease;
    }
    
    .close-btn:hover {
      background: #f1f5f9;
    }
    
    .dialog-content {
      padding: 24px;
    }
    
    .customer-info {
      display: flex;
      align-items: center;
      gap: 16px;
      margin-bottom: 20px;
    }
    
    .customer-avatar {
      width: 56px;
      height: 56px;
      border-radius: 28px;
      display: flex;
      align-items: center;
      justify-content: center;
      font-size: 20px;
      font-weight: 600;
      color: white;
    }
    
    .customer-details h3 {
      margin: 0 0 4px 0;
      font-size: 18px;
      font-weight: 600;
      color: #1e293b;
    }
    
    .customer-details p {
      margin: 0;
      font-size: 13px;
      color: #64748b;
    }
    
    .actions-list {
      display: flex;
      flex-direction: column;
      gap: 8px;
      margin-top: 16px;
    }
    
    .action-item {
      display: flex;
      align-items: center;
      gap: 16px;
      padding: 14px 16px;
      border: none;
      background: #f8fafc;
      border-radius: 12px;
      cursor: pointer;
      transition: all 0.2s ease;
      text-align: left;
      width: 100%;
    }
    
    .action-item:hover {
      background: #f1f5f9;
      transform: translateX(4px);
    }
    
    .action-icon {
      font-size: 24px;
      width: 24px;
      height: 24px;
    }
    
    .action-icon.purple { color: #8b5cf6; }
    .action-icon.blue { color: #3b82f6; }
    .action-icon.green { color: #10b981; }
    .action-icon.orange { color: #f59e0b; }
    .action-icon.red { color: #ef4444; }
    
    .action-info {
      flex: 1;
    }
    
    .action-title {
      display: block;
      font-size: 14px;
      font-weight: 600;
      color: #1e293b;
      margin-bottom: 2px;
    }
    
    .action-desc {
      font-size: 12px;
      color: #64748b;
    }
    
    .arrow-icon {
      color: #94a3b8;
      font-size: 18px;
      width: 18px;
      height: 18px;
    }
    
    .dialog-actions {
      padding: 16px 24px;
      border-top: 1px solid #e2e8f0;
      display: flex;
      justify-content: flex-end;
    }
    
    .cancel-btn {
      padding: 8px 20px;
      border: 1px solid #e2e8f0;
      background: white;
      border-radius: 10px;
      font-size: 14px;
      font-weight: 500;
      color: #64748b;
      cursor: pointer;
      transition: all 0.2s ease;
    }
    
    .cancel-btn:hover {
      background: #f1f5f9;
    }
  `],
  animations: [
    trigger('dialogFadeIn', [
      transition(':enter', [
        style({ opacity: 0, transform: 'scale(0.95)' }),
        animate('200ms ease-out', style({ opacity: 1, transform: 'scale(1)' })),
      ]),
    ]),
  ],
})
export class QuickActionsDialogComponent {
  constructor(
    public dialogRef: MatDialogRef<QuickActionsDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: QuickActionsData,
  ) {}

  getInitials(name: string): string {
    return name
      .split(' ')
      .map(n => n.charAt(0).toUpperCase())
      .join('')
      .substring(0, 2);
  }

  getAvatarColor(name: string): string {
    const colors = ['#8b5cf6', '#3b82f6', '#10b981', '#f59e0b', '#ef4444'];
    const index = name.split('').reduce((sum, char) => sum + char.charCodeAt(0), 0) % colors.length;
    return colors[index];
  }

  selectAction(action: string): void {
    this.dialogRef.close({ action });
  }

  onClose(): void {
    this.dialogRef.close();
  }
}