import { Component, Inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import {
  MatDialogModule,
  MatDialogRef,
  MAT_DIALOG_DATA,
} from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { animate, style, transition, trigger } from '@angular/animations';
import { EmailService } from '../../../services/email/email.service';

export interface BaseEmailData {
  to: string;
  cc: string;
  bcc: string;
  subject: string;
  message: string;
  attachPdf: boolean;
  sendCopyToSelf: boolean;
  from?: string;
}

export interface BaseDialogData {
  customerId: number;
  customerName: string;
  customerEmail: string;
  type: 'statement' | 'invoice' | 'custom';
  invoiceId?: number;
  invoiceNumber?: string;
  invoiceAmount?: number;
  dueDate?: string;
}

@Component({
  selector: 'app-email-dialog-base',
  template: '',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatDialogModule,
    MatButtonModule,
    MatInputModule,
    MatFormFieldModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
  ],
  animations: [
    trigger('dialogFadeIn', [
      transition(':enter', [
        style({ opacity: 0, transform: 'scale(0.95)' }),
        animate('200ms ease-out', style({ opacity: 1, transform: 'scale(1)' })),
      ]),
    ]),
    trigger('fadeIn', [
      transition(':enter', [
        style({ opacity: 0 }),
        animate('300ms ease', style({ opacity: 1 })),
      ]),
    ]),
  ],
})
export abstract class EmailDialogBaseComponent implements OnInit {
  emailData: BaseEmailData = {
    to: '',
    cc: '',
    bcc: '',
    subject: '',
    message: '',
    attachPdf: true,
    sendCopyToSelf: false,
    from: '',
  };

  selectedTemplate: string = '';
  showCc: boolean = false;
  showBcc: boolean = false;
  isSending = false;
  showPreview = false;

  // Template lists to be overridden by child classes
  abstract templateKeys: string[];
  abstract getTemplateLabel(key: string): string;

  protected constructor(
    public dialogRef: MatDialogRef<EmailDialogBaseComponent>,
    @Inject(MAT_DIALOG_DATA) public data: BaseDialogData,
    protected emailService: EmailService,
    protected snackBar: MatSnackBar
  ) {
    this.emailData.to = data.customerEmail;
  }

  ngOnInit(): void {
    this.loadFromEmail();
  }

  protected loadFromEmail(): void {
    this.emailService.getEmailSettings().subscribe({
      next: (settings) => {
        this.emailData.from = settings.fromEmail;
      },
      error: (err) => {
        console.error('Error fetching email settings:', err);
      },
    });
  }

  abstract applyTemplate(templateKey: string): void;

  protected replacePlaceholders(text: string): string {
    const placeholders: Record<string, string> = {
      '{{customerName}}': this.data.customerName,
      '{{date}}': new Date().toLocaleDateString(),
      '{{invoiceNumber}}': this.data.invoiceNumber || '',
      '{{amount}}': this.data.invoiceAmount ? `$${this.data.invoiceAmount.toFixed(2)}` : '$0.00',
      '{{dueDate}}': this.data.dueDate || new Date(Date.now() + 7 * 86400000).toLocaleDateString(),
      '{{companyName}}': this.getCompanyName(),
      '{{bankName}}': 'Your Bank Name',
      '{{accountNumber}}': 'XXXX-XXXX-XXXX',
    };

    Object.entries(placeholders).forEach(([key, value]) => {
      text = text.replace(new RegExp(key, 'g'), value);
    });

    return text;
  }

  protected getCompanyName(): string {
    return 'CoreInvoice';
  }

  togglePreview(): void {
    if (!this.isFormValid()) {
      this.snackBar.open('Please fill in required fields (To, Subject, Message)', 'Close', {
        duration: 3000,
      });
      return;
    }
    this.showPreview = true;
  }

  isFormValid(): boolean {
    return !!(
      this.emailData.to &&
      this.emailData.to.includes('@') &&
      this.emailData.subject?.trim() &&
      this.emailData.message?.trim()
    );
  }

  abstract sendEmail(): void;

  onCancel(): void {
    this.dialogRef.close({ sent: false });
  }

  getEmailPayload(): any {
    return {
      to: this.emailData.to,
      cc: this.emailData.cc,
      bcc: this.emailData.bcc,
      subject: this.emailData.subject,
      body: this.emailData.message,
      attachPdf: this.emailData.attachPdf,
      sendCopyToSelf: this.emailData.sendCopyToSelf,
      customerId: this.data.customerId,
      invoiceId: this.data.invoiceId,
      type: this.selectedTemplate,
    };
  }
}