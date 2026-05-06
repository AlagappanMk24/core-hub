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
import { animate, style, transition, trigger } from '@angular/animations';
import { EmailService } from '../../../../services/email/email.service';
import { NotificationDialogComponent } from '../../../../shared/components/notification/notification-dialog.component';
import {
  EmailDialogBaseComponent,
  BaseDialogData,
} from '../../../../shared/components/email-dialog/email-dialog-base.component';
import { getCustomerEmailTemplate } from '../../../../shared/components/email-dialog/email-templates';
import { Nl2brPipe } from '../../../../shared/pipes/nl2br.pipe';

@Component({
  selector: 'app-send-email-dialog',
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
  templateUrl: './send-email-dialog.component.html',
  styleUrls: ['./send-email-dialog.component.css'],
  animations: [
    trigger('dialogFadeIn', [
      transition(':enter', [
        style({ opacity: 0, transform: 'scale(0.95)' }),
        animate('200ms ease-out', style({ opacity: 1, transform: 'scale(1)' })),
      ]),
    ]),
  ],
})
export class SendEmailDialogComponent extends EmailDialogBaseComponent {
  templateKeys = [
    'custom',
    'statement',
    'payment_reminder',
    'thank_you',
    'welcome',
  ];

  constructor(
    dialogRef: MatDialogRef<SendEmailDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public override data: BaseDialogData,
    emailService: EmailService,
    snackBar: MatSnackBar,
    private dialog: MatDialog,
  ) {
    super(dialogRef, data, emailService, snackBar);
    this.applyTemplate('custom');
  }

  getTemplateLabel(templateKey: string): string {
    const labels: Record<string, string> = {
      custom: 'Custom',
      statement: 'Customer Statement',
      payment_reminder: 'Payment Reminder',
      thank_you: 'Thank You',
      welcome: 'Welcome Email',
    };
    return labels[templateKey] || templateKey;
  }

  applyTemplate(templateKey: string): void {
    const template = getCustomerEmailTemplate(templateKey);
    if (template) {
      this.emailData.subject = this.replacePlaceholders(template.subject);
      this.emailData.message = this.replacePlaceholders(template.message);
      this.selectedTemplate = templateKey;
    }
  }

  sendEmail(): void {
    if (!this.isFormValid()) return;
    this.isSending = true;
    const payload = this.getEmailPayload();
    this.emailService.sendCustomerEmail(payload).subscribe({
      next: () => {
        this.isSending = false;
        this.openDialog(
          'success',
          'Email Sent',
          'Email sent successfully!',
          'The email has been sent to the customer.',
        );
        this.dialogRef.close(true);
      },
      error: (error) => {
        this.isSending = false;
        this.openDialog(
          'error',
          'Send Failed',
          'Failed to send email.',
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