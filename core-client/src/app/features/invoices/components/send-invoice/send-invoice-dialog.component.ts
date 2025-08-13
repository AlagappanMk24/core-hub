import { Component, Inject } from '@angular/core';
import {
  MatDialogRef,
  MAT_DIALOG_DATA,
  MatDialogModule,
  MatDialog,
} from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { animate, style, transition, trigger } from '@angular/animations';
import { EmailService } from '../../../../services/email/email.service';
import { InvoiceService } from '../../../../services/invoice/invoice.service';
import { NotificationDialogComponent } from '../../../../components/notification/notification-dialog.component';

@Component({
  selector: 'app-send-invoice-dialog',
  template: `
    <div
      class="dialog-container bg-white rounded-xl shadow-2xl overflow-hidden"
      @dialogFadeIn
    >
      <div class="dialog-header flex justify-between items-center p-6 bg-white">
        <h2 class="text-xl font-bold text-purple-600">
          Send Invoice #{{ data.invoiceNumber }}
        </h2>
        <button
          class="text-gray-600 hover:text-purple-600 transition-transform duration-300 transform hover:scale-110 disabled:opacity-50 disabled:cursor-not-allowed"
          (click)="onCancel()"
          aria-label="Close dialog"
          [disabled]="isSending"
        >
          <mat-icon>close</mat-icon>
        </button>
      </div>
      <div
        class="dialog-content p-6 max-h-[60vh] overflow-y-auto relative bg-white"
      >
        <div
          class="content-wrapper transition-all duration-300"
          [class.blur-sm]="isSending"
        >
          <form
            class="email-form flex flex-col gap-5 p-4 bg-gray-50 rounded-lg shadow-sm"
            #emailForm="ngForm"
          >
            <!-- From Field -->
            <div class="form-row flex items-start gap-4">
              <label
                class="form-label w-32 text-sm font-semibold text-gray-800 flex items-center mt-2"
                >From</label
              >
              <div class="flex-1 flex flex-col">
                <input
                  type="email"
                  id="from-input"
                  class="p-2 border border-gray-300 rounded-lg text-gray-800 focus:outline-none focus:ring-2 focus:ring-purple-500 focus:border-transparent bg-white placeholder-gray-400 cursor-no-drop"
                  [(ngModel)]="emailData.from"
                  name="from"
                  readonly
                />
              </div>
            </div>

            <!-- To Fields -->
            <div
              *ngFor="
                let recipient of emailData.to;
                let i = index;
                trackBy: trackByIndex
              "
              class="form-row flex items-center gap-4"
            >
              <label
                class="form-label w-32 text-sm font-semibold text-gray-800 flex items-center"
                [ngClass]="{ invisible: i > 0 }"
                >To</label
              >
              <div class="flex-1 flex items-center gap-2">
                <input
                  type="email"
                  id="to-input-{{ i }}"
                  class="flex-1 p-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-purple-500 focus:border-transparent bg-white text-gray-800 placeholder-gray-400"
                  [ngClass]="{ 'bg-gray-100 cursor-not-allowed': i === 0 }"
                  [(ngModel)]="emailData.to[i]"
                  name="to-{{ i }}"
                  required
                  pattern="[a-z0-9._%+-]+@[a-z0-9.-]+\\.[a-z]{2,}$"
                  placeholder="Recipient's email address"
                  #toInput="ngModel"
                />
                <button
                  *ngIf="i > 0"
                  type="button"
                  class="text-red-500 hover:text-red-600 transition-transform duration-300 transform hover:scale-110"
                  (click)="removeRecipient(i, 'to')"
                  aria-label="Remove recipient"
                >
                  <mat-icon>remove_circle</mat-icon>
                </button>
              </div>
                <div
                  *ngIf="toInput.invalid && (toInput.dirty || toInput.touched)"
                  class="text-red-500 text-sm"
                >
                  <span *ngIf="toInput.errors?.['required']">Email is required.</span>
                  <span *ngIf="toInput.errors?.['pattern']">Invalid email format.</span>
                </div>
            </div>

            <!-- Add To Recipient Button -->
            <div class="form-row flex items-center gap-4">
              <div class="w-32"></div>
              <button
                type="button"
                class="text-purple-600 hover:text-purple-700 text-base font-medium flex items-center gap-1 transition-colors duration-300"
                (click)="addRecipient('to')"
              >
                <mat-icon class="text-base">add</mat-icon>
                Add To Recipient
              </button>
            </div>

            <!-- CC Fields -->
            <div
              *ngFor="
                let ccRecipient of emailData.cc;
                let i = index;
                trackBy: trackByIndex
              "
              class="form-row flex items-center gap-4"
            >
              <label
                class="form-label w-32 text-sm font-semibold text-gray-800 flex items-center"
                [ngClass]="{ invisible: i > 0 }"
                >CC</label
              >
              <div class="flex-1 flex items-center gap-2">
                <input
                  type="email"
                  id="cc-input-{{ i }}"
                  class="flex-1 p-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-purple-500 focus:border-transparent bg-white text-gray-800 placeholder-gray-400"
                  [(ngModel)]="emailData.cc[i]"
                  name="cc-{{ i }}"
                  pattern="[a-z0-9._%+-]+@[a-z0-9.-]+\\.[a-z]{2,}$"
                  placeholder="CC email address"
                  #ccInput="ngModel"
                />
                <button
                  type="button"
                  class="text-red-500 hover:text-red-600 transition-transform duration-300 transform hover:scale-110"
                  (click)="removeRecipient(i, 'cc')"
                  aria-label="Remove CC recipient"
                >
                  <mat-icon>remove_circle</mat-icon>
                </button>
                <div
                  *ngIf="ccInput.invalid && (ccInput.dirty || ccInput.touched)"
                  class="text-red-500 text-sm mt-1"
                >
                  <span *ngIf="ccInput.errors?.['pattern']"
                    >Invalid email format.</span
                  >
                </div>
              </div>
            </div>
            
            <!-- CC Field for SendCopyToSelf -->
            <div
              *ngIf="emailData.sendCopyToSelf"
              class="form-row flex items-center gap-4"
            >
              <label
                class="form-label w-32 text-sm font-semibold text-gray-800 flex items-center"
                [ngClass]="{ invisible: emailData.cc.length > 0 }"
                >CC</label
              >
              <div class="flex-1 flex flex-col gap-1">
                <div class="flex items-center gap-2">
                  <input
                    type="email"
                    id="cc-self-input"
                    class="flex-1 p-2 border border-gray-300 rounded-lg text-gray-800 bg-gray-100 cursor-not-allowed"
                    [value]="emailData.from"
                    readonly
                    placeholder="Your email (auto-added)"
                  />
                  <button
                    type="button"
                    class="text-gray-400 cursor-not-allowed"
                    disabled
                    aria-label="Cannot remove self CC"
                  >
                    <mat-icon>remove_circle</mat-icon>
                  </button>
                </div>
              </div>
            </div>

            <!-- Add CC Recipient Button -->
            <div class="form-row flex items-center gap-4">
              <div class="w-32"></div>
              <button
                type="button"
                class="text-purple-600 hover:text-purple-700 text-base font-medium flex items-center gap-1 transition-colors duration-300"
                (click)="addRecipient('cc')"
              >
                <mat-icon class="text-base">add</mat-icon>
                Add CC Recipient
              </button>
            </div>

            <!-- Subject Field -->
            <div class="form-row flex items-start gap-4">
              <label
                class="form-label w-32 text-sm font-semibold text-gray-800 flex items-center mt-2"
                >Subject</label
              >
              <div class="flex-1 flex flex-col">
                <input
                  type="text"
                  id="subject-input"
                  class="p-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-purple-500 focus:border-transparent bg-white text-gray-800 placeholder-gray-400"
                  [(ngModel)]="emailData.subject"
                  name="subject"
                  required
                  placeholder="Invoice #{{ data.invoiceNumber }}"
                  #subjectInput="ngModel"
                />
                <div
                  *ngIf="
                    subjectInput.invalid &&
                    (subjectInput.dirty || subjectInput.touched)
                  "
                  class="text-red-500 text-sm mt-1"
                >
                  <span *ngIf="subjectInput.errors?.['required']"
                    >Subject is required.</span
                  >
                </div>
              </div>
            </div>

            <!-- Message Field -->
            <div class="form-row flex items-start gap-4">
              <label
                class="form-label w-32 text-sm font-semibold text-gray-800 flex items-center mt-2"
                >Message</label
              >
              <div class="flex-1 flex flex-col">
                <textarea
                  id="message-input"
                  class="p-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-purple-500 focus:border-transparent bg-white text-gray-800 placeholder-gray-400 resize-none"
                  [(ngModel)]="emailData.message"
                  name="message"
                  rows="5"
                  required
                  placeholder="Dear Customer,&#10;Please find attached your invoice.&#10;Thank you for your business!"
                  #messageInput="ngModel"
                ></textarea>
                <div
                  *ngIf="
                    messageInput.invalid &&
                    (messageInput.dirty || messageInput.touched)
                  "
                  class="text-red-500 text-sm mt-1"
                >
                  <span *ngIf="messageInput.errors?.['required']"
                    >Message is required.</span
                  >
                </div>
              </div>
            </div>

            <!-- Checkbox Group -->
            <div class="checkbox-group flex flex-col gap-3 mt-3">
              <div class="form-row flex items-center gap-4">
                <div class="form-label-spacer w-32"></div>
                <mat-checkbox
                  class="text-gray-800 text-sm hover:scale-105 transition-transform duration-300 violet-checkbox"
                  [(ngModel)]="emailData.attachPdf"
                  name="attachPdf"
                  labelPosition="after"
                >
                  Attach a PDF copy of this invoice
                </mat-checkbox>
              </div>
              <div class="form-row flex items-center gap-4">
                <div class="form-label-spacer w-32"></div>
                <mat-checkbox
                  class="text-gray-800 text-sm hover:scale-105 transition-transform duration-300 violet-checkbox"
                  [(ngModel)]="emailData.sendCopyToSelf"
                  name="sendCopyToSelf"
                  labelPosition="after"
                >
                  Send a copy to myself
                </mat-checkbox>
              </div>
            </div>
          </form>
        </div>
        <div
          *ngIf="isSending"
          class="progress-container flex items-center gap-3 justify-center p-4 bg-white/95 rounded-lg absolute top-1/2 left-1/2 transform -translate-x-1/2 -translate-y-1/2 z-10 shadow-lg"
          @fadeIn
          aria-live="polite"
        >
          <mat-spinner diameter="24" class="text-purple-500"></mat-spinner>
          <span class="text-gray-800 text-sm font-medium"
            >Sending invoice...</span
          >
        </div>
      </div>
      <div
        class="dialog-actions p-6 bg-white border-t border-gray-200 flex justify-end gap-3"
      >
        <button
          class="cancel-btn px-4 py-2 border-2 border-purple-400 text-purple-600 rounded-lg text-sm font-medium hover:bg-purple-50 hover:border-purple-500 hover:scale-105 transition-all duration-300 disabled:opacity-50 disabled:cursor-not-allowed"
          (click)="onCancel()"
          [disabled]="isSending"
        >
          Cancel
        </button>
        <button
          class="send-btn px-4 py-2 bg-purple-600 text-white rounded-lg text-sm font-medium flex items-center gap-2 hover:bg-purple-700 hover:scale-105 transition-all duration-300 disabled:opacity-50 disabled:cursor-not-allowed"
          [ngClass]="{ 'animate-pulse': emailForm.valid && !isSending }"
          [disabled]="emailForm.invalid || isSending"
          (click)="onSend()"
          matTooltip="Send invoice"
        >
          <mat-icon *ngIf="!isSending">send</mat-icon>
          <mat-spinner *ngIf="isSending" diameter="18"></mat-spinner>
          Send
        </button>
      </div>
    </div>
  `,
  styles: [
    `
      .violet-checkbox {
        --mdc-checkbox-selected-checkmark-color: #ffffff;
        --mdc-checkbox-selected-icon-color: #8a2be2;
        --mdc-checkbox-unselected-icon-color: #64748b;
      }
    `,
  ],

  animations: [
    trigger('dialogFadeIn', [
      transition(':enter', [
        style({ opacity: 0, transform: 'translateY(20px)' }),
        animate(
          '300ms ease-out',
          style({ opacity: 1, transform: 'translateY(0)' })
        ),
      ]),
    ]),
    trigger('fadeIn', [
      transition(':enter', [
        style({ opacity: 0 }),
        animate('300ms ease', style({ opacity: 1 })),
      ]),
    ]),
  ],
  standalone: true,
  imports: [
    CommonModule,
    MatDialogModule,
    MatButtonModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
    FormsModule,
    MatIconModule,
    MatTooltipModule,
    MatCheckboxModule,
  ],
})
export class SendInvoiceDialogComponent {
  emailData = {
    from: '',
    to: [''],
    cc: [] as string[],
    subject: '',
    message: `Dear Customer,\nPlease find attached your invoice.\nThank you for your business!`,
    attachPdf: true,
    sendCopyToSelf: false,
  };
  isSending = false;

  constructor(
    private emailService: EmailService,
    private invoiceService: InvoiceService,
    private dialog: MatDialog,
    public dialogRef: MatDialogRef<SendInvoiceDialogComponent>,
    @Inject(MAT_DIALOG_DATA)
    public data: {
      invoiceId: number;
      invoiceNumber: string;
      customerEmail?: string;
      userEmail?: string;
    }
  ) {
    this.emailData.subject = `Invoice #${this.data.invoiceNumber}`;
    this.emailData.to = [this.data.customerEmail || ''];
    this.emailService.getEmailSettings().subscribe({
      next: (settings) => {
        this.emailData.from = settings.fromEmail || 'alagappanmk98@gmail.com';
      },
      error: (err) => {
        console.error('Error fetching email settings:', err);
         this.openDialog(
      'error',
      'Email Settings Error',
      'Failed to load default email address.',
      'Could not retrieve your default email settings. Please check your internet connection or contact support if the issue persists.'
    );
      },
    });
    this.dialogRef.updateSize('600px');
  }

  trackByIndex(index: number): number {
    return index;
  }

  addRecipient(type: 'to' | 'cc'): void {
    if (type === 'to') {
      this.emailData.to.push('');
    } else {
      this.emailData.cc.push('');
    }
  }

  removeRecipient(index: number, type: 'to' | 'cc'): void {
    if (type === 'to' && index > 0) {
      this.emailData.to.splice(index, 1);
    } else if (type === 'cc') {
      this.emailData.cc.splice(index, 1);
    }
  }

  onSend(): void {
    if (this.isSending) return;

    this.isSending = true;
    const uniqueCc = [...new Set(this.emailData.cc.filter((email) => email.trim() !== ''))];
    const payload = {
      ...this.emailData,
      to: this.emailData.to.filter((email) => email.trim() !== ''),
      cc: this.emailData.sendCopyToSelf
        ? [...uniqueCc.filter(email => email !== this.emailData.from), this.emailData.from]
        : uniqueCc,
    };
    console.log('Sending payload:', JSON.stringify(payload, null, 2));
    this.invoiceService.sendInvoice(this.data.invoiceId, payload).subscribe({
      next: () => {
        this.isSending = false;
         this.openDialog(
        'success',
        'Invoice Sent Successfully',
        'Invoice sent successfully!',
        'The invoice has been sent to all specified recipients. They will receive it in their email shortly along with any attachments you selected.'
      );
        this.dialogRef.close(true);
      },
      error: (error) => {
        this.isSending = false;
         this.openDialog(
        'error',
        'Send Failed',
        'Failed to send invoice. Please try again.',
        'The invoice could not be sent due to a system error. Please check the email addresses and your internet connection, then try again.'
      );
        console.error('Send invoice error:', error);
      },
    });
  }

  onCancel(): void {
    this.dialogRef.close();
  }

  openDialog(type: 'success' | 'error', title: string, message: string, submessage: string): void {
  this.dialog.open(NotificationDialogComponent, {
    width: '400px',
    data: { type, title, message, submessage },
  });
}
}
