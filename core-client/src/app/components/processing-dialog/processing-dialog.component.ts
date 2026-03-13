import { Component, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

@Component({
  selector: 'app-processing-dialog',
  standalone: true,
  imports: [CommonModule, MatProgressSpinnerModule],
  template: `
    <div class="swal-processing-container">
      <div class="swal-loader"></div>

      <h2 class="swal-title">
        {{ data.title || 'Processing...' }}
      </h2>

      <p class="swal-text">
        {{ data.message || 'Please wait while we process your request.' }}
      </p>
    </div>
  `,
  styles: [
    `
      .swal-processing-container {
        display: flex;
        flex-direction: column;
        align-items: center;
        justify-content: center;
        text-align: center;
        padding: 30px 30px 32px;
        min-width: 360px;
      }

      /* SWEETALERT EXACT SPINNER */

      .swal-loader {
        border: 4px solid #f3f3f3;
        border-top: 4px solid #3085d6;
        border-radius: 50%;
        width: 45px;
        height: 45px;
        animation: swal-spin 1s linear infinite;
        margin-bottom: 18px;
      }

      @keyframes swal-spin {
        0% {
          transform: rotate(0deg);
        }
        100% {
          transform: rotate(360deg);
        }
      }

      .swal-title {
        font-size: 22px;
        font-weight: 600;
        color: #545454;
        margin: 0 0 10px 0;
      }

      .swal-text {
        font-size: 15px;
        color: #6e6e6e;
        margin: 0;
        line-height: 1.4;
      }
    `,
  ],
})
export class ProcessingDialogComponent {
  constructor(
    @Inject(MAT_DIALOG_DATA)
    public data: { title?: string; message?: string },
  ) {}
}
