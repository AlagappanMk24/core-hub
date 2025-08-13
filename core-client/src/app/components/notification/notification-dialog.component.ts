// import { Component, Inject } from '@angular/core';
// import { CommonModule } from '@angular/common';
// import { MatDialogRef, MAT_DIALOG_DATA, MatDialogContent, MatDialogActions } from '@angular/material/dialog';
// import { MatButtonModule } from '@angular/material/button';
// import { MatIconModule } from '@angular/material/icon';

// @Component({
//   selector: 'app-notification-dialog',
//   template: `
//     <div class="dialog-container">
//       <div class="icon-container">
//         <div class="icon-circle" [ngClass]="data.type">
//           <mat-icon class="status-icon">{{ data.type === 'error' ? 'close' : 'check' }}</mat-icon>
//         </div>
//       </div>
      
//       <h2 class="dialog-title" [ngClass]="data.type">{{ data.title }}</h2>
      
//       <mat-dialog-content class="dialog-content">
//         <p class="dialog-message">{{ data.message }}</p>
//       </mat-dialog-content>
      
//       <mat-dialog-actions class="dialog-actions">
//         <button 
//           mat-raised-button 
//           class="action-btn" 
//           [ngClass]="data.type"
//           (click)="dialogRef.close()">
//           {{ data.type === 'error' ? 'Try Again' : 'Ok' }}
//         </button>
//       </mat-dialog-actions>
//     </div>
//   `,
//   styles: [`
//     .dialog-container {
//       padding: 40px 30px 30px;
//       text-align: center;
//       background: #ffffff;
//       border-radius: 12px;
//       box-shadow: 0 10px 40px rgba(0, 0, 0, 0.1);
//       min-width: 320px;
//       max-width: 400px;
//       position: relative;
//     }

//     .icon-container {
//       margin-bottom: 24px;
//       display: flex;
//       justify-content: center;
//     }

//     .icon-circle {
//       width: 80px;
//       height: 80px;
//       border-radius: 50%;
//       display: flex;
//       align-items: center;
//       justify-content: center;
//       border: 3px solid;
//       position: relative;
//     }

//     .icon-circle.success {
//       border-color: #e5e5e5;
//       background: #ffffff;
//     }

//     .icon-circle.error {
//       border-color: #e5e5e5;
//       background: #ffffff;
//     }

//     .status-icon {
//       font-size: 36px;
//       width: 36px;
//       height: 36px;
//     }

//     .icon-circle.success .status-icon {
//       color: #22c55e;
//     }

//     .icon-circle.error .status-icon {
//       color: #ef4444;
//     }

//     .dialog-title {
//       font-size: 28px;
//       font-weight: 400;
//       margin: 0 0 16px 0;
//       letter-spacing: -0.5px;
//     }

//     .dialog-title.success {
//       color: #22c55e;
//     }

//     .dialog-title.error {
//       color: #ef4444;
//     }

//     .dialog-content {
//       margin-bottom: 32px;
//       padding: 0;
//     }

//     .dialog-message {
//       font-size: 16px;
//       color: #9ca3af;
//       line-height: 1.4;
//       margin: 0;
//       font-weight: 400;
//     }

//     .dialog-actions {
//       display: flex;
//       justify-content: center;
//       padding: 0;
//       margin: 0;
//     }

//     .action-btn {
//       font-size: 16px;
//       font-weight: 500;
//       border-radius: 25px;
//       padding: 12px 32px;
//       border: none;
//       min-width: 120px;
//       text-transform: none;
//       letter-spacing: 0.5px;
//     }

//     .action-btn.success {
//       background-color: #22c55e;
//       color: #ffffff;
//     }

//     .action-btn.success:hover {
//       background-color: #16a34a;
//     }

//     .action-btn.error {
//       background-color: #ef4444;
//       color: #ffffff;
//     }

//     .action-btn.error:hover {
//       background-color: #dc2626;
//     }

//     /* Remove material button ripple effect styling conflicts */
//     .action-btn:focus {
//       box-shadow: none;
//     }

//     @media (max-width: 480px) {
//       .dialog-container {
//         padding: 30px 20px 20px;
//         min-width: 280px;
//       }
      
//       .icon-circle {
//         width: 70px;
//         height: 70px;
//       }
      
//       .status-icon {
//         font-size: 32px;
//         width: 32px;
//         height: 32px;
//       }
      
//       .dialog-title {
//         font-size: 24px;
//       }
      
//       .dialog-message {
//         font-size: 14px;
//       }
      
//       .action-btn {
//         font-size: 14px;
//         padding: 10px 28px;
//       }
//     }
//   `],
//   standalone: true,
//   imports: [CommonModule, MatButtonModule, MatIconModule, MatDialogContent, MatDialogActions]
// })
// export class NotificationDialogComponent {
//   constructor(
//     public dialogRef: MatDialogRef<NotificationDialogComponent>,
//     @Inject(MAT_DIALOG_DATA) public data: { type: 'success' | 'error', title: string, message: string }
//   ) {}
// }

import { Component, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogContent, MatDialogActions } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-notification-dialog',
  template: `
    <div class="dialog-container">
      <div class="icon-container">
        <div class="icon-circle" [ngClass]="data.type">
          <mat-icon class="status-icon">{{ data.type === 'error' ? 'sentiment_very_dissatisfied' : 'check' }}</mat-icon>
        </div>
      </div>
      
      <h2 class="dialog-title" [ngClass]="data.type">{{ data.title || (data.type === 'error' ? 'ERROR!' : 'SUCCESS') }}</h2>
      
      <mat-dialog-content class="dialog-content">
        <p class="dialog-message">{{ data.message }}</p>
        <p class="dialog-submessage" *ngIf="data.submessage">{{ data.submessage }}</p>
      </mat-dialog-content>
      
      <mat-dialog-actions class="dialog-actions">
        <button 
          mat-raised-button 
          class="action-btn" 
          [ngClass]="data.type"
          (click)="dialogRef.close()">
          {{ data.type === 'error' ? 'Try Again' : 'Ok' }}
        </button>
      </mat-dialog-actions>
    </div>
  `,
  styles: [`
    .dialog-container {
      padding: 40px 30px 35px;
      text-align: center;
      background: #ffffff;
      border-radius: 20px;
      box-shadow: 0 20px 60px rgba(0, 0, 0, 0.15);
      min-width: 380px;
      max-width: 420px;
      position: relative;
      font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif;
    }

    .icon-container {
      margin-bottom: 20px;
      display: flex;
      justify-content: center;
    }

    .icon-circle {
      width: 80px;
      height: 80px;
      border-radius: 50%;
      display: flex;
      align-items: center;
      justify-content: center;
      border: 4px solid;
      position: relative;
    }

    .icon-circle.success {
      border-color: #22c55e;
      background: #ffffff;
    }

    .icon-circle.error {
      border-color: #ef4444;
      background: #ffffff;
    }

    .status-icon {
      font-size: 40px !important;
      width: 40px !important;
      height: 40px !important;
      font-weight: 600;
    }

    .icon-circle.success .status-icon {
      color: #22c55e;
    }

    .icon-circle.error .status-icon {
      color: #ef4444;
    }

    .dialog-title {
      font-size: 20px;
      font-weight: 700;
      margin: 0 0 20px 0;
      letter-spacing: 0.5px;
      text-transform: uppercase;
    }

    .dialog-title.success {
      color: #22c55e;
    }

    .dialog-title.error {
      color: #ef4444;
    }

    .dialog-content {
      margin-bottom: 30px;
      padding: 0;
    }

    .dialog-message {
      font-size: 16px;
      color: #374151;
      line-height: 1.5;
      margin: 0 0 8px 0;
      font-weight: 500;
    }

    .dialog-submessage {
      font-size: 14px;
      color: #9ca3af;
      line-height: 1.4;
      margin: 0;
      font-weight: 400;
    }

    .dialog-actions {
      display: flex;
      justify-content: center;
      padding: 0;
      margin: 0;
    }

    .action-btn {
      font-size: 16px;
      font-weight: 600;
      border-radius: 8px;
      padding: 14px 40px;
      border: none;
      min-width: 140px;
      text-transform: none;
      letter-spacing: 0.3px;
      box-shadow: none;
      transition: all 0.2s ease;
    }

    .action-btn.success {
      background-color: #22c55e;
      color: #ffffff;
    }

    .action-btn.success:hover {
      background-color: #16a34a;
      transform: translateY(-1px);
      box-shadow: 0 4px 12px rgba(34, 197, 94, 0.3);
    }

    .action-btn.error {
      background-color: #ef4444;
      color: #ffffff;
    }

    .action-btn.error:hover {
      background-color: #dc2626;
      transform: translateY(-1px);
      box-shadow: 0 4px 12px rgba(239, 68, 68, 0.3);
    }

    /* Remove material button ripple effect styling conflicts */
    .action-btn:focus {
      box-shadow: none;
      outline: 2px solid transparent;
    }

    .action-btn:active {
      transform: translateY(0);
    }

    /* Material button override */
    :host ::ng-deep .mat-mdc-raised-button:not(:disabled) {
      box-shadow: none;
    }

    :host ::ng-deep .mat-mdc-raised-button:hover:not(:disabled) {
      box-shadow: none;
    }

    @media (max-width: 480px) {
      .dialog-container {
        padding: 35px 25px 30px;
        min-width: 320px;
        max-width: 350px;
      }
      
      .icon-circle {
        width: 70px;
        height: 70px;
      }
      
      .status-icon {
        font-size: 36px !important;
        width: 36px !important;
        height: 36px !important;
      }
      
      .dialog-title {
        font-size: 20px;
        margin-bottom: 16px;
      }
      
      .dialog-message {
        font-size: 15px;
      }

      .dialog-submessage {
        font-size: 13px;
      }
      
      .action-btn {
        font-size: 15px;
        padding: 12px 32px;
        min-width: 120px;
      }
    }

    @media (max-width: 360px) {
      .dialog-container {
        padding: 30px 20px 25px;
        min-width: 280px;
      }
      
      .dialog-title {
        font-size: 18px;
      }
      
      .dialog-message {
        font-size: 14px;
      }

      .dialog-submessage {
        font-size: 12px;
      }
      
      .action-btn {
        font-size: 14px;
        padding: 10px 28px;
      }
    }
  `],
  standalone: true,
  imports: [CommonModule, MatButtonModule, MatIconModule, MatDialogContent, MatDialogActions]
})
export class NotificationDialogComponent {
  constructor(
    public dialogRef: MatDialogRef<NotificationDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: { 
      type: 'success' | 'error', 
      title?: string, 
      message: string,
      submessage?: string 
    }
  ) {}
}