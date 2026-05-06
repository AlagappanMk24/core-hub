import { Component, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatDialogModule, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatSelectModule } from '@angular/material/select';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { animate, style, transition, trigger } from '@angular/animations';
import { CustomerService } from '../../services/customer.service';

export interface FollowUpData {
  customerId: number;
  customerName: string;
}

@Component({
  selector: 'app-schedule-follow-up-dialog',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatDialogModule,
    MatButtonModule,
    MatIconModule,
    MatInputModule,
    MatFormFieldModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatSelectModule,
    MatProgressSpinnerModule,
  ],
  template: `
    <div class="follow-up-dialog" @dialogFadeIn>
      <div class="dialog-header">
        <h2>Schedule Follow-up</h2>
        <button class="close-btn" (click)="onCancel()">
          <mat-icon>close</mat-icon>
        </button>
      </div>
      
      <div class="dialog-content">
        <div class="customer-info">
          <mat-icon>person</mat-icon>
          <span>Customer: <strong>{{ data.customerName }}</strong></span>
        </div>
        
        <div class="form-field">
          <label>Follow-up Date *</label>
          <mat-form-field appearance="outline" class="full-width">
            <mat-label>Select date</mat-label>
            <input matInput [matDatepicker]="picker" [(ngModel)]="followUpDate" [min]="minDate">
            <mat-datepicker-toggle matSuffix [for]="picker"></mat-datepicker-toggle>
            <mat-datepicker #picker></mat-datepicker>
          </mat-form-field>
        </div>
        
        <div class="form-field">
          <label>Priority *</label>
          <select class="form-select" [(ngModel)]="priority">
            <option value="high">🔥 High Priority</option>
            <option value="medium">📌 Medium Priority</option>
            <option value="low">✅ Low Priority</option>
          </select>
        </div>
        
        <div class="form-field">
          <label>Notes (Optional)</label>
          <textarea
            [(ngModel)]="notes"
            placeholder="Add any additional notes..."
            rows="3"
            class="notes-textarea"
          ></textarea>
        </div>
      </div>
      
      <div class="dialog-actions">
        <button class="cancel-btn" (click)="onCancel()" [disabled]="isSaving">Cancel</button>
        <button class="schedule-btn" (click)="scheduleFollowUp()" [disabled]="!followUpDate || isSaving">
          <mat-spinner *ngIf="isSaving" diameter="18"></mat-spinner>
          <mat-icon *ngIf="!isSaving">event</mat-icon>
          {{ isSaving ? 'Scheduling...' : 'Schedule Follow-up' }}
        </button>
      </div>
    </div>
  `,
  styles: [`
    .follow-up-dialog {
      background: white;
      border-radius: 24px;
      overflow: hidden;
      min-width: 450px;
      max-width: 500px;
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
      gap: 8px;
      padding: 12px 16px;
      background: #f8fafc;
      border-radius: 12px;
      margin-bottom: 20px;
      color: #1e293b;
      font-size: 14px;
    }
    
    .form-field {
      margin-bottom: 20px;
    }
    
    .form-field label {
      display: block;
      font-size: 13px;
      font-weight: 600;
      color: #1e293b;
      margin-bottom: 8px;
    }
    
    .full-width {
      width: 100%;
    }
    
    .form-select {
      width: 100%;
      padding: 10px 12px;
      border: 1px solid #e2e8f0;
      border-radius: 12px;
      font-size: 14px;
      font-family: inherit;
      background: white;
      cursor: pointer;
    }
    
    .form-select:focus {
      outline: none;
      border-color: #8b5cf6;
    }
    
    .notes-textarea {
      width: 100%;
      padding: 12px;
      border: 1px solid #e2e8f0;
      border-radius: 12px;
      font-size: 14px;
      font-family: inherit;
      resize: vertical;
    }
    
    .notes-textarea:focus {
      outline: none;
      border-color: #8b5cf6;
    }
    
    .dialog-actions {
      padding: 16px 24px;
      border-top: 1px solid #e2e8f0;
      display: flex;
      justify-content: flex-end;
      gap: 12px;
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
    
    .cancel-btn:hover:not(:disabled) {
      background: #f1f5f9;
    }
    
    .schedule-btn {
      padding: 8px 24px;
      background: #8b5cf6;
      border: none;
      border-radius: 10px;
      font-size: 14px;
      font-weight: 500;
      color: white;
      cursor: pointer;
      display: flex;
      align-items: center;
      gap: 8px;
      transition: all 0.2s ease;
    }
    
    .schedule-btn:hover:not(:disabled) {
      background: #7c3aed;
      transform: translateY(-1px);
    }
    
    .schedule-btn:disabled {
      opacity: 0.5;
      cursor: not-allowed;
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
export class ScheduleFollowUpDialogComponent {
  followUpDate: Date | null = null;
  priority: string = 'medium';
  notes: string = '';
  isSaving = false;
  minDate: Date = new Date();

  constructor(
    public dialogRef: MatDialogRef<ScheduleFollowUpDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: FollowUpData,
    private customerService: CustomerService,
  ) {}

  scheduleFollowUp(): void {
    if (!this.followUpDate) return;
    
    this.isSaving = true;
    const followUpData = {
      date: this.followUpDate,
      priority: this.priority,
      notes: this.notes,
    };
    
    this.customerService.scheduleFollowUp(this.data.customerId, followUpData).subscribe({
      next: () => {
        this.isSaving = false;
        this.dialogRef.close(true);
      },
      error: (error) => {
        console.error('Error scheduling follow-up:', error);
        this.isSaving = false;
        this.dialogRef.close(false);
      },
    });
  }

  onCancel(): void {
    this.dialogRef.close(false);
  }
}