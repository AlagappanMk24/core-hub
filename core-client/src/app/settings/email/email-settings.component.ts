import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { InvoiceService } from '../../services/invoice/invoice.service';
import { NotificationDialogComponent } from '../../components/notification/notification-dialog.component';
import { EmailService } from '../../services/email/email.service';

interface EmailSettings {
  fromEmail: string;
}

@Component({
  selector: 'app-email-settings',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    RouterModule,
    MatProgressSpinnerModule,
    MatDialogModule,
  ],
  template: `
    <div class="container mx-auto p-6 max-w-2xl">
      <h1 class="text-2xl font-bold text-purple-800 mb-6">Email Settings</h1>
      <form class="bg-white p-6 rounded-lg shadow-lg border border-purple-200" #emailForm="ngForm">
        <div class="mb-4 flex flex-col">
          <label class="block text-sm font-medium text-purple-700 mb-2" for="fromEmail">
            From Email Address
          </label>
          <input
            type="email"
            id="fromEmail"
            [(ngModel)]="emailSettings.fromEmail"
            name="fromEmail"
            class="w-full p-3 border border-purple-300 rounded-lg focus:ring-2 focus:ring-purple-500 focus:border-purple-500 transition placeholder-gray-400 placeholder:italic"
            placeholder="e.g., yourname@company.com"
            required
            pattern="[a-z0-9._%+-]+@[a-z0-9.-]+\\.[a-z]{2,}$"
            #fromEmailInput="ngModel"
          />
          <div *ngIf="fromEmailInput.invalid && (fromEmailInput.dirty || fromEmailInput.touched)" class="text-red-500 text-sm mt-1">
            <span *ngIf="fromEmailInput.errors?.['required']">Email is required.</span>
            <span *ngIf="fromEmailInput.errors?.['pattern']">Please enter a valid email address.</span>
          </div>
        </div>
        <div class="action-buttons flex justify-end gap-3">
          <button
            type="button"
            (click)="cancel()"
            class="px-4 py-2 bg-gray-300 text-gray-700 rounded-lg hover:bg-gray-400 transition disabled:opacity-50 disabled:cursor-not-allowed"
            [disabled]="isSaving"
          >
            Cancel
          </button>
          <button
            type="button"
            (click)="saveSettings()"
            class="px-4 py-2 bg-purple-600 text-white rounded-lg hover:bg-purple-700 transition flex items-center justify-center gap-2 disabled:opacity-70 disabled:cursor-not-allowed"
            [disabled]="emailForm.invalid || isSaving"
          >
            <mat-spinner *ngIf="isSaving" [diameter]="20"></mat-spinner>
            <span [class.hidden]="isSaving">Save</span>
            <span *ngIf="isSaving" class="ml-1">Saving...</span>
          </button>
        </div>
      </form>
    </div>
  `,
  styles: [
    `
      @import 'https://cdn.jsdelivr.net/npm/tailwindcss@2.2.19/dist/tailwind.min.css';

      form {
        transition: all 0.3s ease-in-out;
      }

      input:focus {
        outline: none;
      }

      .action-buttons {
        display: flex;
        gap: 12px;
        margin-top: 24px;
        justify-content: flex-end;
      }

      .action-buttons button .ml-1 {
        margin-left: 0.25rem;
      }

      @media (max-width: 768px) {
        .action-buttons {
          flex-direction: column-reverse;
          gap: 8px;
        }
      }
    `,
  ],
})
export class EmailSettingsComponent implements OnInit {
  emailSettings: EmailSettings = {
    fromEmail: '',
  };
  isSaving = false;

  constructor(
    private emailService: EmailService,
    private dialog: MatDialog
  ) {}

  ngOnInit(): void {
    this.loadEmailSettings();
  }

  loadEmailSettings(): void {
    this.emailService.getEmailSettings().subscribe({
      next: (settings: EmailSettings) => {
        this.emailSettings = settings;
      },
      error: (err) => {
      console.error('Error fetching email settings:', err);
      this.openDialog(
        'error',
        'Error',
        'Failed to load email settings.',
        'Email configuration could not be retrieved. Please check your connection and try again.'
      );
    },
    });
  }

  saveSettings(): void {
    if (this.validateSettings()) {
      this.isSaving = true;
      this.emailService.saveEmailSettings(this.emailSettings).subscribe({
        next: () => {
          this.isSaving = false;
            this.openDialog(
          'success',
          'Success',
          'Email settings saved successfully!',
          'Your email configuration has been updated. All future invoices will use these settings for delivery.'
        );
        },
        error: (err) => {
          this.isSaving = false;
          console.error('Error saving email settings:', err);
            this.openDialog(
          'error',
          'Error',
          'Failed to save email settings.',
          'Your email configuration could not be saved. Please verify your settings and try again.'
        );
        },
      });
    }
  }

  cancel(): void {
    if (!this.isSaving) {
      this.loadEmailSettings();
    }
  }

  validateSettings(): boolean {
    if (!this.emailSettings.fromEmail.trim()) {
     this.openDialog(
      'error',
      'Required Field',
      'Email address is required.',
      'Please enter a valid email address to use as the sender for all invoice communications.'
    );
      return false;
    }
    const emailPattern = /[a-z0-9._%+-]+@[a-z0-9.-]+\.[a-z]{2,}$/;
    if (!emailPattern.test(this.emailSettings.fromEmail)) {
     this.openDialog(
      'error',
      'Invalid Format',
      'Please enter a valid email address.',
      'The email address format is incorrect. Please use a valid format like example@company.com.'
    );
      return false;
    }
    return true;
  }

  openDialog(type: 'success' | 'error', title: string, message: string, submessage : string): void {
    this.dialog.open(NotificationDialogComponent, {
      width: '400px',
      data: { type, title, message, submessage },
    });
  }
}