import { Component, inject, signal } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { NotificationService } from './core/services/notification.service';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

interface Toast {
  id: number;
  message: string;
  type: 'success' | 'error' | 'warning' | 'info';
}

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App {
  private readonly notificationService = inject(NotificationService);
  protected readonly toasts = signal<Toast[]>([]);

  constructor() {
    this.notificationService.notifications.pipe(
      takeUntilDestroyed()
    ).subscribe(notification => {
      const toast: Toast = {
        id: notification.id,
        message: notification.message,
        type: notification.type
      };
      this.toasts.update(toasts => [...toasts, toast]);

      setTimeout(() => {
        this.removeToast(toast.id);
      }, notification.duration ?? 5000);
    });
  }

  protected removeToast(id: number): void {
    this.toasts.update(toasts => toasts.filter(t => t.id !== id));
  }
}
