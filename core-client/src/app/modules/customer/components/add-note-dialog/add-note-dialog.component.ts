import { Component, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatDialogModule, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { animate, style, transition, trigger } from '@angular/animations';
import { CustomerService } from '../../services/customer.service';

export interface AddNoteData {
  customerId: number;
}

@Component({
  selector: 'app-add-note-dialog',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatDialogModule,
    MatButtonModule,
    MatIconModule,
    MatInputModule,
    MatFormFieldModule,
    MatProgressSpinnerModule,
  ],
  template: `
    <div class="add-note-dialog" @dialogFadeIn>
      <div class="dialog-header">
        <h2>Add Note</h2>
        <button class="close-btn" (click)="onCancel()">
          <mat-icon>close</mat-icon>
        </button>
      </div>
      
      <div class="dialog-content">
        <div class="note-info">
          <mat-icon class="info-icon">info</mat-icon>
          <span>Add a private note about this customer. Notes are only visible to staff.</span>
        </div>
        
        <div class="form-field">
          <label>Note Content *</label>
          <textarea
            [(ngModel)]="noteContent"
            placeholder="Enter your note here..."
            rows="6"
            class="note-textarea"
            maxlength="500"
          ></textarea>
          <div class="character-count">{{ noteContent.length }}/500</div>
        </div>
      </div>
      
      <div class="dialog-actions">
        <button class="cancel-btn" (click)="onCancel()" [disabled]="isSaving">Cancel</button>
        <button class="save-btn" (click)="saveNote()" [disabled]="!noteContent.trim() || isSaving">
          <mat-spinner *ngIf="isSaving" diameter="18"></mat-spinner>
          <mat-icon *ngIf="!isSaving">save</mat-icon>
          {{ isSaving ? 'Saving...' : 'Save Note' }}
        </button>
      </div>
    </div>
  `,
  styles: [`
    .add-note-dialog {
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
    
    .note-info {
      display: flex;
      align-items: center;
      gap: 12px;
      padding: 12px 16px;
      background: #f0fdf4;
      border-radius: 12px;
      margin-bottom: 20px;
      color: #166534;
      font-size: 13px;
    }
    
    .info-icon {
      color: #22c55e;
    }
    
    .form-field {
      margin-bottom: 16px;
    }
    
    .form-field label {
      display: block;
      font-size: 13px;
      font-weight: 600;
      color: #1e293b;
      margin-bottom: 6px;
    }
    
    .note-textarea {
      width: 100%;
      padding: 12px;
      border: 1px solid #e2e8f0;
      border-radius: 12px;
      font-size: 14px;
      font-family: inherit;
      resize: vertical;
      transition: all 0.2s ease;
    }
    
    .note-textarea:focus {
      outline: none;
      border-color: #8b5cf6;
      box-shadow: 0 0 0 3px rgba(139, 92, 246, 0.1);
    }
    
    .character-count {
      text-align: right;
      font-size: 11px;
      color: #94a3b8;
      margin-top: 4px;
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
    
    .save-btn {
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
    
    .save-btn:hover:not(:disabled) {
      background: #7c3aed;
      transform: translateY(-1px);
    }
    
    .save-btn:disabled {
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
export class AddNoteDialogComponent {
  noteContent = '';
  isSaving = false;

  constructor(
    public dialogRef: MatDialogRef<AddNoteDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: AddNoteData,
    private customerService: CustomerService,
  ) {}

  saveNote(): void {
    if (!this.noteContent.trim()) return;
    
    this.isSaving = true;
    this.customerService.addCustomerNote(this.data.customerId, this.noteContent).subscribe({
      next: () => {
        this.isSaving = false;
        this.dialogRef.close(true);
      },
      error: (error) => {
        console.error('Error saving note:', error);
        this.isSaving = false;
        this.dialogRef.close(false);
      },
    });
  }

  onCancel(): void {
    this.dialogRef.close(false);
  }
}