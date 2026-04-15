import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { LoaderConfig } from '../../components/loader/loader.component';

@Injectable({
  providedIn: 'root'
})
export class LoaderService {
  private loaderSubject = new BehaviorSubject<{
    visible: boolean;
    config: LoaderConfig;
    showActions: boolean;
    showCancelButton?: boolean;
    showRetryButton?: boolean;
    showConfirmButton?: boolean;
    onCancel?: () => void;
    onRetry?: () => void;
    onConfirm?: () => void;
  }>({
    visible: false,
    config: { type: 'loading', title: 'Processing', message: 'Please wait...' },
    showActions: false
  });

  loaderState$ = this.loaderSubject.asObservable();

  /**
   * Show loading spinner
   */
  showLoading(title: string = 'Processing', message: string = 'Please wait...', options?: Partial<LoaderConfig>): void {
    this.loaderSubject.next({
      visible: true,
      config: {
        type: 'loading',
        title,
        message,
        showProgress: options?.showProgress || false,
        showSteps: options?.showSteps || false,
        steps: options?.steps,
        showTip: options?.showTip !== false,
        tipMessage: options?.tipMessage,
        ...options
      },
      showActions: false
    });
  }

  /**
   * Show success message
   */
  showSuccess(title: string, message: string, duration: number = 3000, onConfirm?: () => void): void {
    this.loaderSubject.next({
      visible: true,
      config: {
        type: 'success',
        title,
        message,
        duration,
        showTip: false
      },
      showActions: true,
      showCancelButton: false,
      showRetryButton: false,
      showConfirmButton: true,
      onConfirm
    });

    setTimeout(() => {
      this.hide();
    }, duration);
  }

  /**
   * Show error message
   */
  showError(title: string, message: string, onRetry?: () => void, onCancel?: () => void): void {
    this.loaderSubject.next({
      visible: true,
      config: {
        type: 'error',
        title,
        message,
        showTip: true,
        tipMessage: 'Please check your connection and try again.'
      },
      showActions: true,
      showCancelButton: true,
      showRetryButton: true,
      showConfirmButton: false,
      onRetry,
      onCancel
    });
  }

  /**
   * Show warning message
   */
  showWarning(title: string, message: string, onConfirm?: () => void, onCancel?: () => void): void {
    this.loaderSubject.next({
      visible: true,
      config: {
        type: 'warning',
        title,
        message,
        showTip: false
      },
      showActions: true,
      showCancelButton: true,
      showRetryButton: false,
      showConfirmButton: true,
      onConfirm,
      onCancel
    });
  }

  /**
   * Show info message
   */
  showInfo(title: string, message: string, duration: number = 3000): void {
    this.loaderSubject.next({
      visible: true,
      config: {
        type: 'info',
        title,
        message,
        duration,
        showTip: false
      },
      showActions: false
    });

    setTimeout(() => {
      this.hide();
    }, duration);
  }

  /**
   * Show progress loader
   */
  showProgress(title: string, message: string, steps: string[], onComplete?: () => void): void {
    this.loaderSubject.next({
      visible: true,
      config: {
        type: 'loading',
        title,
        message,
        showProgress: true,
        progress: 0,
        showSteps: true,
        steps,
        currentStep: 0,
        showTip: true
      },
      showActions: false,
      onConfirm: onComplete
    });
  }

  /**
   * Update progress
   */
  updateProgress(progress: number, currentStep?: number, message?: string): void {
    const current = this.loaderSubject.value;
    if (current.visible) {
      this.loaderSubject.next({
        ...current,
        config: {
          ...current.config,
          progress,
          currentStep: currentStep !== undefined ? currentStep : Math.floor(progress / (100 / (current.config.steps?.length || 1))),
          message: message || current.config.message
        }
      });
    }
  }

  /**
   * Update message
   */
  updateMessage(message: string, subMessage?: string): void {
    const current = this.loaderSubject.value;
    if (current.visible) {
      this.loaderSubject.next({
        ...current,
        config: {
          ...current.config,
          message,
          subMessage
        }
      });
    }
  }

  /**
   * Hide loader
   */
  hide(): void {
    this.loaderSubject.next({
      visible: false,
      config: { type: 'loading', title: '', message: '' },
      showActions: false
    });
  }
}