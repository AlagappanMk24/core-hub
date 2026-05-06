import { Component, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import {
  MatDialogModule,
  MatDialogRef,
  MAT_DIALOG_DATA,
  MatDialog,
} from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBarModule, MatSnackBar } from '@angular/material/snack-bar';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { EmailService } from '../../../../../services/email/email.service';
import { InvoiceService } from '../../services/invoice.service';
import { NotificationDialogComponent } from '../../../../../shared/components/notification/notification-dialog.component';
import {
  EmailDialogBaseComponent,
  BaseDialogData,
} from '../../../../../shared/components/email-dialog/email-dialog-base.component';
import { getInvoiceEmailTemplate } from '../../../../../shared/components/email-dialog/email-templates';
import { Nl2brPipe } from '../../../../../shared/pipes/nl2br.pipe';
import { animate, style, transition, trigger } from '@angular/animations';

interface InvoiceDialogData extends BaseDialogData {
  invoiceId: number;
  invoiceNumber: string;
  userEmail?: string;
}

@Component({
  selector: 'app-send-invoice-dialog',
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
    MatCheckboxModule,
    Nl2brPipe,
  ],
  templateUrl: './send-invoice-dialog.component.html',
  styleUrls: ['./send-invoice-dialog.component.css'],
  animations: [
    trigger('dialogFadeIn', [
      transition(':enter', [
        style({ opacity: 0, transform: 'scale(0.95)' }),
        animate('200ms ease-out', style({ opacity: 1, transform: 'scale(1)' })),
      ]),
    ]),
  ],
})

export class SendInvoiceDialogComponent extends EmailDialogBaseComponent {
  templateKeys = [
    'custom',
    'new_invoice',
    'payment_reminder',
    'overdue_notice',
    'payment_received',
    'invoice_copy',
  ];

  constructor(
    dialogRef: MatDialogRef<SendInvoiceDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public override data: InvoiceDialogData,
    emailService: EmailService,
    snackBar: MatSnackBar,
    private invoiceService: InvoiceService,
    private dialog: MatDialog,
  ) {
    super(dialogRef, data, emailService, snackBar);
    this.applyTemplate('custom');
  }

  getTemplateLabel(templateKey: string): string {
    const labels: Record<string, string> = {
      custom: 'Custom',
      new_invoice: 'New Invoice',
      payment_reminder: 'Payment Reminder',
      overdue_notice: 'Overdue Notice',
      payment_received: 'Payment Received',
      invoice_copy: 'Invoice Copy',
    };
    return labels[templateKey] || templateKey;
  }

  applyTemplate(templateKey: string): void {
    const template = getInvoiceEmailTemplate(templateKey);
    if (template) {
      this.emailData.subject = this.replacePlaceholders(template.subject);
      this.emailData.message = this.replacePlaceholders(template.message);
      this.selectedTemplate = templateKey;
    }
  }

  sendEmail(): void {
    if (!this.isFormValid()) return;
    this.isSending = true;
    const payload = {
      to: [this.emailData.to],
      cc: this.emailData.cc ? [this.emailData.cc] : [],
      bcc: this.emailData.bcc ? [this.emailData.bcc] : [],
      subject: this.emailData.subject,
      message: this.emailData.message,
      attachPdf: this.emailData.attachPdf,
      sendCopyToSelf: this.emailData.sendCopyToSelf,
    };
    this.invoiceService.sendInvoice(this.data.invoiceId, payload).subscribe({
      next: () => {
        this.isSending = false;
        this.openDialog(
          'success',
          'Invoice Sent Successfully',
          'Invoice sent successfully!',
          'The invoice has been sent to all specified recipients.',
        );
        this.dialogRef.close(true);
      },
      error: (error) => {
        this.isSending = false;
        this.openDialog(
          'error',
          'Send Failed',
          'Failed to send invoice.',
          error.message || 'Please check the email addresses and try again.',
        );
      },
    });
  }

  private openDialog(
    type: 'success' | 'error',
    title: string,
    message: string,
    submessage: string,
  ): void {
    this.dialog.open(NotificationDialogComponent, {
      width: '400px',
      data: { type, title, message, submessage },
    });
  }
}