import { Injectable, OnDestroy } from '@angular/core';
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';
import { Subject, Observable, BehaviorSubject } from 'rxjs';
import { environment } from '../../environments/environment';
import { Router } from '@angular/router';
import { AuthService } from '../auth/auth.service';

export interface InvoiceNotification {
  invoiceId: number;
  invoiceNumber: string;
  message: string;
  timestamp: Date;
  read: boolean;
}

@Injectable({
  providedIn: 'root',
})
export class NotificationService implements OnDestroy {
  private hubConnection!: HubConnection;
  private notificationsSubject = new BehaviorSubject<InvoiceNotification[]>([]);
  private notificationReceived = new Subject<InvoiceNotification>();
  private destroy$ = new Subject<void>();

  notifications$: Observable<InvoiceNotification[]> =
    this.notificationsSubject.asObservable();
  notificationReceived$: Observable<InvoiceNotification> =
    this.notificationReceived.asObservable();

  constructor(private authService: AuthService, private router: Router) {
    this.setupSignalR();
  }

  private setupSignalR(): void {
    const token = this.authService.getAuthToken();
    if (!token) {
      console.error('No authentication token available for SignalR.');
      return;
    }

    this.hubConnection = new HubConnectionBuilder()
      .withUrl(`${environment.signalRBaseUrl}/notificationHub`, {
        accessTokenFactory: () => token,
      })
      .withAutomaticReconnect([0, 2000, 10000, 30000])
      .build();

    this.hubConnection.on(
      'ReceiveInvoiceNotification',
      (notification: {
        invoiceId: number;
        invoiceNumber: string;
        message: string;
      }) => {
        const newNotification: InvoiceNotification = {
          invoiceId: notification.invoiceId,
          invoiceNumber: notification.invoiceNumber,
          message:
            notification.message ||
            `New invoice ${notification.invoiceNumber} received`, // Fallback message
          timestamp: new Date(),
          read: false,
        };
        const currentNotifications = this.notificationsSubject.value;
        this.notificationsSubject.next(
          [newNotification, ...currentNotifications].slice(0, 10)
        );
        this.notificationReceived.next(newNotification);
      }
    );

    this.hubConnection.onclose((err) => {
      console.error('SignalR connection closed:', err);
    });

    this.hubConnection
      .start()
      .then(() => console.log('SignalR connected successfully'))
      .catch((err) => console.error('SignalR connection error:', err));
  }

  markAsRead(notification: InvoiceNotification): void {
    const notifications = this.notificationsSubject.value.map((n) =>
      n.invoiceId === notification.invoiceId &&
      n.timestamp === notification.timestamp
        ? { ...n, read: true }
        : n
    );
    this.notificationsSubject.next(notifications);
  }

  clearNotifications(): void {
    this.notificationsSubject.next([]);
  }

  get hasNotifications(): boolean {
    const hasUnread = this.notificationsSubject.value.some((n) => !n.read);
    return hasUnread;
  }

  navigateToInvoice(invoiceId: number): void {
    this.router.navigate([`/invoices/view/${invoiceId}`]);
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
    if (this.hubConnection) {
      this.hubConnection
        .stop()
        .catch((err) => console.error('Error stopping SignalR:', err));
    }
  }
}
