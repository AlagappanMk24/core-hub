import { Component, Input, OnDestroy, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { animate, style, transition, trigger } from '@angular/animations';

export interface LoaderConfig {
  type: 'success' | 'error' | 'warning' | 'info' | 'loading';
  title: string;
  message: string;
  subMessage?: string;
  progress?: number;
  showProgress?: boolean;
  showSteps?: boolean;
  steps?: string[];
  currentStep?: number;
  showTip?: boolean;
  tipMessage?: string;
  duration?: number;
}

@Component({
  selector: 'app-loader',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div
      class="generic-loader-overlay"
      [class.active]="isVisible"
      [class.dark]="isDark"
      [class.light]="!isDark"
    >
      <div
        class="generic-loader-container"
        [class.compact]="isCompact"
        [class.fullscreen]="isFullscreen"
      >
        <!-- Icon Section -->
        <div
          class="loader-icon-section"
          [class.success]="config.type === 'success'"
          [class.error]="config.type === 'error'"
          [class.warning]="config.type === 'warning'"
          [class.info]="config.type === 'info'"
          [class.loading]="config.type === 'loading'"
        >
          <!-- Loading Animation -->
          <div *ngIf="config.type === 'loading'" class="loading-animation">
            <div class="spinner-ring"></div>
            <div class="spinner-ring-inner"></div>
            <div class="spinner-logo">
              <svg width="32" height="32" viewBox="0 0 32 32" fill="none">
                <path
                  d="M10 16L14 20L22 12"
                  stroke="white"
                  stroke-width="2.5"
                  stroke-linecap="round"
                />
                <circle
                  cx="16"
                  cy="16"
                  r="14"
                  stroke="white"
                  stroke-width="2"
                  fill="none"
                />
              </svg>
            </div>
          </div>

          <!-- Success Icon -->
          <div *ngIf="config.type === 'success'" class="success-animation">
            <svg viewBox="0 0 52 52">
              <circle
                class="success-circle"
                cx="26"
                cy="26"
                r="25"
                fill="none"
              />
              <path
                class="success-check"
                fill="none"
                d="M14.1 27.2l7.1 7.2 16.7-16.8"
              />
            </svg>
          </div>

          <!-- Error Icon -->
          <div *ngIf="config.type === 'error'" class="error-animation">
            <svg viewBox="0 0 52 52">
              <circle class="error-circle" cx="26" cy="26" r="25" fill="none" />
              <path
                class="error-cross"
                fill="none"
                d="M16 16l20 20M36 16L16 36"
              />
            </svg>
          </div>

          <!-- Warning Icon -->
          <div *ngIf="config.type === 'warning'" class="warning-animation">
            <svg viewBox="0 0 52 52">
              <circle
                class="warning-circle"
                cx="26"
                cy="26"
                r="25"
                fill="none"
              />
              <path class="warning-mark" fill="none" d="M26 16v12M26 34v2" />
            </svg>
          </div>

          <!-- Info Icon -->
          <div *ngIf="config.type === 'info'" class="info-animation">
            <svg viewBox="0 0 52 52">
              <circle class="info-circle" cx="26" cy="26" r="25" fill="none" />
              <path class="info-i" fill="none" d="M26 22v2M26 28v8" />
              <circle
                class="info-dot"
                cx="26"
                cy="18"
                r="2"
                fill="currentColor"
              />
            </svg>
          </div>
        </div>

        <!-- Content Section -->
        <div class="loader-content-section">
          <h3 class="loader-title">{{ config.title || 'Processing...' }}</h3>
          <p class="loader-message">{{ config.message || 'Please wait...' }}</p>
          <p *ngIf="config.subMessage" class="loader-submessage">
            {{ config.subMessage }}
          </p>

          <!-- Progress Bar -->
          <div *ngIf="config.showProgress" class="loader-progress-section">
            <div class="progress-bar-container">
              <div
                class="progress-bar"
                [style.width]="(config.progress || 0) + '%'"
              >
                <div class="progress-shimmer"></div>
              </div>
            </div>
            <span class="progress-text">{{ config.progress || 0 }}%</span>
          </div>

          <!-- Steps Indicator -->
          <div
            *ngIf="config.showSteps && config.steps?.length"
            class="loader-steps"
          >
            <div
              *ngFor="let step of config.steps; let i = index"
              class="step"
              [class.completed]="i < (config.currentStep || 0)"
              [class.active]="i === (config.currentStep || 0)"
              [class.pending]="i > (config.currentStep || 0)"
            >
              <div class="step-indicator">
                <span *ngIf="i < (config.currentStep || 0)" class="step-check"
                  >✓</span
                >
                <span
                  *ngIf="i === (config.currentStep || 0)"
                  class="step-number"
                  >{{ i + 1 }}</span
                >
                <span
                  *ngIf="i > (config.currentStep || 0)"
                  class="step-number"
                  >{{ i + 1 }}</span
                >
              </div>
              <span class="step-label">{{ step }}</span>
            </div>
          </div>

          <!-- Tip Section -->
          <div *ngIf="config.showTip" class="loader-tip">
            <div class="tip-icon">💡</div>
            <div class="tip-text">{{ config.tipMessage || currentTip }}</div>
          </div>
        </div>

        <!-- Action Buttons -->
        <div *ngIf="showActions" class="loader-actions">
          <button
            *ngIf="showCancelButton"
            class="btn-cancel"
            (click)="onCancelClick()"
          >
            Cancel
          </button>
          <button
            *ngIf="showRetryButton && config.type === 'error'"
            class="btn-retry"
            (click)="onRetryClick()"
          >
            Retry
          </button>
          <button
            *ngIf="showConfirmButton"
            class="btn-confirm"
            (click)="onConfirmClick()"
          >
            OK
          </button>
        </div>
      </div>
    </div>
  `,
  styles: [
    `
      .generic-loader-overlay {
        position: fixed;
        top: 0;
        left: 0;
        right: 0;
        bottom: 0;
        background: rgba(0, 0, 0, 0.5);
        z-index: 10000;
        display: flex;
        align-items: center;
        justify-content: center;
        opacity: 0;
        visibility: hidden;
        transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
      }

      .generic-loader-overlay.active {
        opacity: 1;
        visibility: visible;
      }

      .generic-loader-overlay.dark {
        background: rgba(0, 0, 0, 0.85);
      }

      .generic-loader-container {
        background: white;
        border-radius: 24px;
        padding: 32px 40px;
        max-width: 480px;
        width: 90%;
        box-shadow: 0 20px 40px rgba(0, 0, 0, 0.2);
        transform: scale(0.9);
        transition: transform 0.3s cubic-bezier(0.34, 1.56, 0.64, 1);
      }

      .generic-loader-overlay.active .generic-loader-container {
        transform: scale(1);
      }

      .generic-loader-container.compact {
        padding: 24px 32px;
        max-width: 400px;
      }

      /* Icon Section */
      .loader-icon-section {
        display: flex;
        justify-content: center;
        margin-bottom: 24px;
      }

      .loader-icon-section svg {
        width: 64px;
        height: 64px;
      }

      /* Loading Animation */
      .loading-animation {
        position: relative;
        width: 80px;
        height: 80px;
      }

      .spinner-ring {
        position: absolute;
        top: 0;
        left: 0;
        width: 80px;
        height: 80px;
        border-radius: 50%;
        border: 3px solid transparent;
        border-top-color: #5b21b6;
        border-right-color: #7c3aed;
        animation: spin 1s linear infinite;
      }

      .spinner-ring-inner {
        position: absolute;
        top: 8px;
        left: 8px;
        width: 64px;
        height: 64px;
        border-radius: 50%;
        border: 3px solid transparent;
        border-bottom-color: #8b5cf6;
        border-left-color: #a78bfa;
        animation: spin-reverse 0.8s linear infinite;
      }

      .spinner-logo {
        position: absolute;
        top: 24px;
        left: 24px;
        width: 32px;
        height: 32px;
        background: linear-gradient(135deg, #5b21b6, #7c3aed);
        border-radius: 50%;
        display: flex;
        align-items: center;
        justify-content: center;
      }

      @keyframes spin {
        to {
          transform: rotate(360deg);
        }
      }

      @keyframes spin-reverse {
        to {
          transform: rotate(-360deg);
        }
      }

      /* Success Animation */
      .success-animation svg {
        width: 80px;
        height: 80px;
      }

      .success-circle {
        stroke: #10b981;
        stroke-width: 3;
        stroke-dasharray: 166;
        stroke-dashoffset: 166;
        animation: stroke 0.6s cubic-bezier(0.65, 0, 0.45, 1) forwards;
      }

      .success-check {
        stroke: #10b981;
        stroke-width: 3;
        stroke-dasharray: 48;
        stroke-dashoffset: 48;
        animation: stroke 0.3s cubic-bezier(0.65, 0, 0.45, 1) 0.4s forwards;
      }

      /* Error Animation */
      .error-animation svg {
        width: 80px;
        height: 80px;
      }

      .error-circle {
        stroke: #ef4444;
        stroke-width: 3;
        stroke-dasharray: 166;
        stroke-dashoffset: 166;
        animation: stroke 0.6s cubic-bezier(0.65, 0, 0.45, 1) forwards;
      }

      .error-cross {
        stroke: #ef4444;
        stroke-width: 3;
        stroke-dasharray: 48;
        stroke-dashoffset: 48;
        animation: stroke 0.3s cubic-bezier(0.65, 0, 0.45, 1) 0.4s forwards;
      }

      /* Warning Animation */
      .warning-animation svg {
        width: 80px;
        height: 80px;
      }

      .warning-circle {
        stroke: #f59e0b;
        stroke-width: 3;
        stroke-dasharray: 166;
        stroke-dashoffset: 166;
        animation: stroke 0.6s cubic-bezier(0.65, 0, 0.45, 1) forwards;
      }

      .warning-mark {
        stroke: #f59e0b;
        stroke-width: 3;
        stroke-dasharray: 48;
        stroke-dashoffset: 48;
        animation: stroke 0.3s cubic-bezier(0.65, 0, 0.45, 1) 0.4s forwards;
      }

      /* Info Animation */
      .info-animation svg {
        width: 80px;
        height: 80px;
      }

      .info-circle {
        stroke: #3b82f6;
        stroke-width: 3;
        stroke-dasharray: 166;
        stroke-dashoffset: 166;
        animation: stroke 0.6s cubic-bezier(0.65, 0, 0.45, 1) forwards;
      }

      .info-i {
        stroke: #3b82f6;
        stroke-width: 3;
        stroke-dasharray: 20;
        stroke-dashoffset: 20;
        animation: stroke 0.3s cubic-bezier(0.65, 0, 0.45, 1) 0.4s forwards;
      }

      .info-dot {
        fill: #3b82f6;
        opacity: 0;
        animation: fadeIn 0.3s ease 0.6s forwards;
      }

      @keyframes stroke {
        100% {
          stroke-dashoffset: 0;
        }
      }

      @keyframes fadeIn {
        100% {
          opacity: 1;
        }
      }

      /* Content Section */
      .loader-content-section {
        text-align: center;
      }

      .loader-title {
        font-size: 22px;
        font-weight: 700;
        color: #1f2937;
        margin-bottom: 8px;
        font-family:
          'DM Sans',
          -apple-system,
          BlinkMacSystemFont,
          sans-serif;
      }

      .loader-message {
        font-size: 14px;
        color: #6b7280;
        margin-bottom: 8px;
        line-height: 1.5;
      }

      .loader-submessage {
        font-size: 13px;
        color: #9ca3af;
        margin-bottom: 20px;
      }

      /* Progress Bar */
      .loader-progress-section {
        margin: 20px 0;
        display: flex;
        align-items: center;
        gap: 12px;
      }

      .progress-bar-container {
        flex: 1;
        background: #e5e7eb;
        border-radius: 12px;
        height: 8px;
        overflow: hidden;
      }

      .progress-bar {
        background: linear-gradient(90deg, #5b21b6, #7c3aed, #8b5cf6);
        height: 100%;
        border-radius: 12px;
        position: relative;
        transition: width 0.3s ease;
      }

      .progress-shimmer {
        position: absolute;
        top: 0;
        left: 0;
        right: 0;
        bottom: 0;
        background: linear-gradient(
          90deg,
          transparent,
          rgba(255, 255, 255, 0.3),
          transparent
        );
        animation: shimmer 1.5s infinite;
      }

      @keyframes shimmer {
        0% {
          transform: translateX(-100%);
        }
        100% {
          transform: translateX(100%);
        }
      }

      .progress-text {
        font-size: 14px;
        font-weight: 600;
        color: #5b21b6;
        min-width: 45px;
      }

      /* Steps */
      .loader-steps {
        display: flex;
        justify-content: space-between;
        margin: 20px 0;
        gap: 8px;
      }

      .step {
        flex: 1;
        text-align: center;
      }

      .step-indicator {
        width: 32px;
        height: 32px;
        margin: 0 auto 8px;
        border-radius: 50%;
        display: flex;
        align-items: center;
        justify-content: center;
        font-size: 12px;
        font-weight: 600;
        transition: all 0.3s ease;
      }

      .step.pending .step-indicator {
        background: #e5e7eb;
        color: #9ca3af;
      }

      .step.active .step-indicator {
        background: linear-gradient(135deg, #5b21b6, #7c3aed);
        color: white;
        box-shadow: 0 4px 12px rgba(91, 33, 182, 0.3);
        animation: pulse 0.5s ease;
      }

      .step.completed .step-indicator {
        background: #10b981;
        color: white;
      }

      @keyframes pulse {
        0%,
        100% {
          transform: scale(1);
        }
        50% {
          transform: scale(1.2);
        }
      }

      .step-label {
        font-size: 11px;
        color: #6b7280;
        font-weight: 500;
      }

      .step.active .step-label {
        color: #5b21b6;
        font-weight: 600;
      }

      .step.completed .step-label {
        color: #10b981;
      }

      /* Tip Section */
      .loader-tip {
        display: flex;
        gap: 12px;
        padding: 12px 16px;
        background: #f3f4f6;
        border-radius: 12px;
        margin-top: 20px;
        text-align: left;
      }

      .tip-icon {
        font-size: 18px;
      }

      .tip-text {
        font-size: 12px;
        color: #4b5563;
        line-height: 1.4;
        flex: 1;
      }

      /* Action Buttons */
      .loader-actions {
        display: flex;
        justify-content: center;
        gap: 12px;
        margin-top: 24px;
      }

      .btn-cancel,
      .btn-retry,
      .btn-confirm {
        padding: 10px 20px;
        border-radius: 8px;
        font-size: 14px;
        font-weight: 500;
        cursor: pointer;
        transition: all 0.2s ease;
        border: none;
      }

      .btn-cancel {
        background: #f3f4f6;
        color: #4b5563;
      }

      .btn-cancel:hover {
        background: #e5e7eb;
      }

      .btn-retry {
        background: linear-gradient(135deg, #5b21b6, #7c3aed);
        color: white;
      }

      .btn-retry:hover {
        transform: translateY(-1px);
        box-shadow: 0 4px 12px rgba(91, 33, 182, 0.3);
      }

      .btn-confirm {
        background: #10b981;
        color: white;
      }

      .btn-confirm:hover {
        background: #059669;
      }

      /* Responsive */
      @media (max-width: 640px) {
        .generic-loader-container {
          padding: 24px;
        }

        .loader-title {
          font-size: 18px;
        }

        .step-label {
          font-size: 9px;
        }

        .step-indicator {
          width: 28px;
          height: 28px;
          font-size: 10px;
        }
      }
    `,
  ],
})
export class LoaderComponent implements OnInit, OnDestroy {
  @Input() isVisible: boolean = false;
  @Input() config: LoaderConfig = {
    type: 'loading',
    title: 'Processing',
    message: 'Please wait...',
  };
  @Input() isDark: boolean = false;
  @Input() isCompact: boolean = false;
  @Input() isFullscreen: boolean = false;
  @Input() showActions: boolean = false;
  @Input() showCancelButton: boolean = true;
  @Input() showRetryButton: boolean = true;
  @Input() showConfirmButton: boolean = true;

  @Input() onCancel?: () => void;
  @Input() onRetry?: () => void;
  @Input() onConfirm?: () => void;

  private autoHideTimeout: any;

  public currentTip: string = '';

  ngOnInit(): void {
    this.currentTip = this.getDefaultTip(); // Generate once
    if (this.config?.duration && this.config?.type !== 'loading') {
      this.autoHideTimeout = setTimeout(() => {
        this.isVisible = false;
      }, this.config.duration);
    }
  }

  ngOnDestroy(): void {
    if (this.autoHideTimeout) {
      clearTimeout(this.autoHideTimeout);
    }
  }

  getDefaultTip(): string {
    const tips = [
      'Pro tip: You can save invoices as draft and complete them later.',
      'Tip: Add clear descriptions to invoice items for better understanding.',
      'Did you know? You can attach up to 5 files per invoice.',
      'Tip: Set payment terms to avoid payment delays.',
      'Pro tip: Use the preview feature to review before sending.',
    ];
    return tips[Math.floor(Math.random() * tips.length)];
  }

  onCancelClick(): void {
    if (this.onCancel) {
      this.onCancel();
    }
    this.isVisible = false;
  }

  onRetryClick(): void {
    if (this.onRetry) {
      this.onRetry();
    }
  }

  onConfirmClick(): void {
    if (this.onConfirm) {
      this.onConfirm();
    }
    this.isVisible = false;
  }
}
