import { Injectable } from '@angular/core';
import { Subject } from 'rxjs';

export interface Notification {
  id: number;
  message: string;
  type: 'success' | 'error' | 'warning' | 'info';
  duration?: number;
}

@Injectable({ providedIn: 'root' })
export class NotificationService {
  private readonly notifications$ = new Subject<Notification>();
  private idCounter = 0;

  readonly notifications = this.notifications$.asObservable();

  show(message: string, type: Notification['type'] = 'info', duration = 5000): void {
    const notification: Notification = {
      id: ++this.idCounter,
      message,
      type,
      duration
    };
    this.notifications$.next(notification);
  }

  success(message: string, duration?: number): void {
    this.show(message, 'success', duration);
  }

  error(message: string, duration?: number): void {
    this.show(message, 'error', duration);
  }

  warning(message: string, duration?: number): void {
    this.show(message, 'warning', duration);
  }

  info(message: string, duration?: number): void {
    this.show(message, 'info', duration);
  }
}
