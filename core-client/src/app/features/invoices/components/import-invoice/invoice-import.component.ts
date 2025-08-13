import { Component, Inject } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule, MatDialog } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBarModule } from '@angular/material/snack-bar';
import { InvoiceService } from '../../../../services/invoice/invoice.service';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { animate, style, transition, trigger } from '@angular/animations';
import { NotificationDialogComponent } from '../../../../components/notification/notification-dialog.component';

@Component({
  selector: 'app-invoice-import-dialog',
  template: `
    <div class="dialog-header">
      <h2 mat-dialog-title>Import Invoices</h2>
      <button mat-icon-button class="close-btn" (click)="onCancel()" aria-label="Close dialog" [disabled]="isUploading || isDownloading">
        <mat-icon>close</mat-icon>
      </button>
    </div>
    <mat-dialog-content class="dialog-content">
      <div class="content-wrapper" [class.blurred]="isDownloading || isUploading">
        <div class="import-container">
          <p class="instructions">
            Upload an Excel file (.xlsx) with invoice data. 
            <a href="javascript:void(0)" (click)="downloadTemplate()" class="template-link" matTooltip="Download template">
              Download template
              <mat-icon>file_download</mat-icon>
            </a>
          </p>
          <div class="file-upload-section" [class.dragover]="isDragOver" 
               (dragover)="onDragOver($event)" (dragleave)="onDragLeave($event)" (drop)="onDrop($event)">
            <input
              type="file"
              accept=".xlsx"
              (change)="onFileSelected($event)"
              #fileInput
              class="file-input"
              id="fileInput"
            />
            <label for="fileInput" class="file-label">
              <mat-icon class="upload-icon">cloud_upload</mat-icon>
              <span *ngIf="!selectedFile">Drag & drop or click to select an Excel file</span>
              <span *ngIf="selectedFile" class="selected-file">{{ selectedFile.name }}</span>
            </label>
          </div>
          <div *ngIf="importResult" class="result-container" @fadeIn>
            <div class="result-summary">
              <p class="success" *ngIf="importResult.successCount > 0">
                <mat-icon>check_circle</mat-icon>
                Successfully imported {{ importResult.successCount }} invoice(s).
              </p>
              <p class="error" *ngIf="importResult.errorCount > 0">
                <mat-icon>error</mat-icon>
                Failed to import {{ importResult.errorCount }} invoice(s).
              </p>
            </div>
            <div *ngIf="importResult.errors.length > 0" class="error-details">
              <button mat-button class="toggle-errors" (click)="showErrors = !showErrors">
                {{ showErrors ? 'Hide Errors' : 'Show Errors' }}
                <mat-icon>{{ showErrors ? 'expand_less' : 'expand_more' }}</mat-icon>
              </button>
              <ul *ngIf="showErrors" class="error-list" @expandCollapse>
                <li *ngFor="let error of importResult.errors">{{ error }}</li>
              </ul>
            </div>
          </div>
        </div>
      </div>
      <div *ngIf="isDownloading" class="progress-container" @fadeIn>
        <mat-spinner diameter="24" color="accent"></mat-spinner>
        <span>Downloading template...</span>
      </div>
      <div *ngIf="isUploading" class="progress-container" @fadeIn>
        <mat-spinner diameter="24" color="accent"></mat-spinner>
        <span>Processing file...</span>
      </div>
    </mat-dialog-content>
    <mat-dialog-actions class="dialog-actions">
      <button mat-stroked-button class="cancel-btn" (click)="onCancel()" [disabled]="isUploading || isDownloading">
        Cancel
      </button>
      <button
        mat-raised-button
        color="primary"
        class="upload-btn"
        [disabled]="!selectedFile || isUploading || isDownloading"
        (click)="onUpload()"
        matTooltip="Upload selected file"
      >
        <mat-icon *ngIf="!isUploading">upload</mat-icon>
        <mat-spinner *ngIf="isUploading" diameter="18"></mat-spinner>
        Upload
      </button>
    </mat-dialog-actions>
  `,
  styles: [
    `
      :host {
        font-family: 'Inter', sans-serif;
        display: block;
        background: #f8fafc;
      }

      .dialog-header {
        display: flex;
        justify-content: space-between;
        align-items: center;
        padding: 16px 24px;
        border-bottom: 1px solid #e2e8f0;
        background: #ffffff;
      }

      h2 {
        font-size: 20px;
        font-weight: 700;
        color: #1e293b;
        margin: 0;
      }

      .close-btn {
        color: #8A2BE2;
        transition: all 0.3s ease;
      }

      .close-btn:hover:not(:disabled) {
        background: #f3e8ff;
        transform: scale(1.1);
      }

      .close-btn:disabled {
        opacity: 0.5;
        cursor: not-allowed;
      }

      .dialog-content {
        padding: 24px;
        background: #ffffff;
        max-height: 400px;
        overflow-y: auto;
        position: relative;
      }

      .content-wrapper {
        transition: filter 0.3s ease;
      }

      .content-wrapper.blurred {
        filter: blur(3px);
        pointer-events: none;
      }

      .import-container {
        display: flex;
        flex-direction: column;
        gap: 16px;
      }

      .instructions {
        font-size: 14px;
        color: #64748b;
        margin: 0;
      }

      .template-link {
        color: #8A2BE2;
        text-decoration: none;
        font-weight: 500;
        display: inline-flex;
        align-items: center;
        gap: 4px;
        transition: all 0.3s ease;
      }

      .template-link:hover {
        color: #7c3aed;
        text-decoration: underline;
      }

      .file-upload-section {
        border: 2px dashed #a78bfa;
        border-radius: 12px;
        padding: 24px;
        text-align: center;
        background: #f3e8ff;
        transition: all 0.3s ease;
      }

      .file-upload-section.dragover {
        border-color: #8A2BE2;
        background: #ede9fe;
        transform: scale(1.02);
      }

      .file-input {
        display: none;
      }

      .file-label {
        display: flex;
        flex-direction: column;
        align-items: center;
        gap: 8px;
        cursor: pointer;
        font-size: 14px;
        color: #1e293b;
        font-weight: 500;
      }

      .upload-icon {
        font-size: 32px;
        width: 32px;
        height: 32px;
        color: #8A2BE2;
      }

      .selected-file {
        color: #059669;
        font-weight: 600;
      }

      .progress-container {
        display: flex;
        align-items: center;
        gap: 8px;
        justify-content: center;
        padding: 16px;
        background: rgba(255, 255, 255, 0.9);
        border-radius: 8px;
        position: absolute;
        top: 50%;
        left: 50%;
        transform: translate(-50%, -50%);
        z-index: 10;
        box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
      }

      .progress-container mat-spinner {
          --mdc-circular-progress-active-indicator-color: #8A2BE2 !important;
      }

      .progress-container span {
        color: #1e293b;
        font-size: 14px;
        font-weight: 500;
      }

      .result-container {
        padding: 16px;
        background: #ffffff;
        border: 1px solid #e2e8f0;
        border-radius: 12px;
        animation: fadeIn 0.3s ease;
      }

      .result-summary {
        display: flex;
        flex-direction: column;
        gap: 8px;
      }

      .success, .error {
        display: flex;
        align-items: center;
        gap: 8px;
        font-size: 14px;
        font-weight: 500;
      }

      .success {
        color: #059669;
      }

      .success mat-icon {
        color: #059669;
      }

      .error {
        color: #dc2626;
      }

      .error mat-icon {
        color: #dc2626;
      }

      .error-details {
        margin-top: 12px;
      }

      .toggle-errors {
        color: #8A2BE2;
        font-size: 14px;
        text-transform: none;
        display: flex;
        align-items: center;
        gap: 4px;
      }

      .toggle-errors:hover {
        background: #f3e8ff;
      }

      .error-list {
        margin-top: 8px;
        padding-left: 20px;
        max-height: 150px;
        overflow-y: auto;
        font-size: 13px;
        color: #dc2626;
      }

      .dialog-actions {
        padding: 16px 24px;
        background: #ffffff;
        border-top: 1px solid #e2e8f0;
        justify-content: flex-end;
        gap: 12px;
      }

      .cancel-btn {
        border: 2px solid #a78bfa;
        color: #8A2BE2;
        border-radius: 8px;
        transition: all 0.3s ease;
      }

      .cancel-btn:hover:not(:disabled) {
        background: #f3e8ff;
        border-color: #8A2BE2;
        transform: scale(1.05);
      }

      .cancel-btn:disabled {
        opacity: 0.5;
        cursor: not-allowed;
      }

      .upload-btn {
        background: #8A2BE2;
        color: #ffffff;
        border-radius: 8px;
        display: flex;
        align-items: center;
        gap: 8px;
        transition: all 0.3s ease;
      }

      .upload-btn:hover:not(:disabled) {
        background: #7c3aed;
        transform: scale(1.05);
      }

      .upload-btn:disabled {
        opacity: 0.5;
        cursor: not-allowed;
      }

      @keyframes fadeIn {
        from { opacity: 0; }
        to { opacity: 1; }
      }

      @keyframes expandCollapse {
        from { max-height: 0; opacity: 0; }
        to { max-height: 150px; opacity: 1; }
      }

      @media (max-width: 768px) {
        .dialog-content {
          padding: 16px;
        }

        .file-upload-section {
          padding: 16px;
        }

        .dialog-actions {
          flex-direction: column;
          gap: 8px;
        }

        .cancel-btn, .upload-btn {
          width: 100%;
        }
      }

      @media (max-width: 480px) {
        h2 {
          font-size: 18px;
        }

        .instructions {
          font-size: 12px;
        }

        .file-label {
          font-size: 12px;
        }

        .upload-icon {
          font-size: 24px;
          width: 24px;
          height: 24px;
        }

        .progress-container {
          padding: 12px;
        }

        .progress-container span {
          font-size: 12px;
        }
      }
    `
  ],
  animations: [
    trigger('fadeIn', [
      transition(':enter', [
        style({ opacity: 0 }),
        animate('300ms ease', style({ opacity: 1 }))
      ])
    ]),
    trigger('expandCollapse', [
      transition(':enter', [
        style({ maxHeight: '0px', opacity: 0 }),
        animate('300ms ease', style({ maxHeight: '150px', opacity: 1 }))
      ]),
      transition(':leave', [
        animate('300ms ease', style({ maxHeight: '0px', opacity: 0 }))
      ])
    ])
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
    MatTooltipModule
  ]
})
export class InvoiceImportDialogComponent {
  selectedFile: File | null = null;
  isUploading = false;
  isDownloading = false;
  isDragOver = false;
  showErrors = false;
  importResult: { successCount: number; errorCount: number; errors: string[] } | null = null;

  constructor(
    private invoiceService: InvoiceService,
    private dialog: MatDialog,
    public dialogRef: MatDialogRef<InvoiceImportDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: any
  ) {
    this.dialogRef.updateSize('600px');
  }

  downloadTemplate(): void {
    this.isDownloading = true;
    this.invoiceService.downloadImportTemplate().subscribe({
      next: (blob) => {
        const downloadUrl = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = downloadUrl;
        link.download = 'invoice-import-template.xlsx';
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
        window.URL.revokeObjectURL(downloadUrl);
        this.isDownloading = false;
          this.openDialog(
          'success',
          'Template Downloaded Successfully',
          'Template downloaded successfully!',
          'The Excel template file has been saved to your downloads folder. Use this template to format your invoice data correctly before importing.'
        );
      },
      error: (error) => {
        this.isDownloading = false;
       this.openDialog(
          'error',
          'Download Failed',
          'Failed to download template. Please try again.',
          'The template file could not be downloaded due to a system error. Please check your internet connection and try again.'
        );
        console.error('Template download error:', error);
      }
    });
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.handleFile(input.files?.[0]);
    input.value = '';
  }

  onDragOver(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    if (!this.isUploading && !this.isDownloading) {
      this.isDragOver = true;
    }
  }

  onDragLeave(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.isDragOver = false;
  }

  onDrop(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    if (!this.isUploading && !this.isDownloading) {
      this.isDragOver = false;
      this.handleFile(event.dataTransfer?.files[0]);
    }
  }

  private handleFile(file: File | undefined): void {
    if (file && file.name.endsWith('.xlsx')) {
      this.selectedFile = file;
      this.importResult = null;
    } else {
     this.openDialog(
        'error',
        'Invalid File Format',
        'Please select a valid Excel file (.xlsx)',
        'Only Excel files with .xlsx extension are supported for import. Please choose the correct file format and try again.'
      );
      this.selectedFile = null;
    }
  }

  onUpload(): void {
    if (!this.selectedFile) return;

    this.isUploading = true;
    this.importResult = null;

    this.invoiceService.importInvoices(this.selectedFile).subscribe({
      next: (result) => {
        this.isUploading = false;
        this.importResult = result;
          if (result.successCount > 0 && result.errorCount === 0) {
          // All imports successful
          this.openDialog(
            'success',
            'Import Completed Successfully',
            `Successfully imported ${result.successCount} invoice(s)`,
            'All invoices have been imported successfully and are now available in your invoice list. You can view and manage them immediately.'
          );
        } else if (result.successCount > 0 && result.errorCount > 0) {
          // Partial success
          this.openDialog(
            'success',
            'Import Partially Completed',
            `Successfully imported ${result.successCount} invoice(s)`,
            `${result.errorCount} invoice(s) failed to import due to data issues. Please check the error details below and fix the data in your file before retrying.`
          );
          this.showErrors = true;
        } else if (result.successCount === 0 && result.errorCount > 0) {
          // All failed
          this.openDialog(
            'error',
            'Import Failed',
            `Failed to import ${result.errorCount} invoice(s)`,
            'No invoices could be imported due to data validation errors. Please review the error details below, fix the issues in your file, and try again.'
          );
          this.showErrors = true;
        }
      },
      error: (error) => {
        this.isUploading = false;
        this.openDialog(
          'error',
          'Import Process Failed',
          'Failed to import invoices. Please try again.',
          'The import process encountered a system error and could not be completed. Please check your file format, ensure it follows the template structure, and try again.'
        );
        console.error('Import error:', error);
      }
    });
  }

  openDialog(type: 'success' | 'error', title: string, message: string, submessage: string): void {
    this.dialog.open(NotificationDialogComponent, {
      width: '400px',
      data: { type, title, message, submessage },
    });
  }
  
  onCancel(): void {
    this.dialogRef.close();
  }
}