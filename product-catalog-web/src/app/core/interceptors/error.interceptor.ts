import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { NotificationService } from '../services/notification.service';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const notificationService = inject(NotificationService);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      let message = 'An unexpected error occurred.';

      if (error.error instanceof ErrorEvent) {
        message = error.error.message;
      } else {
        switch (error.status) {
          case 0:
            message = 'Unable to connect to the server. Please check your network connection.';
            break;
          case 400:
            message = error.error?.message || 'Invalid request. Please check your input.';
            break;
          case 404:
            message = 'The requested resource was not found.';
            break;
          case 409:
            message = error.error?.message || 'A conflict occurred. The resource may already exist.';
            break;
          case 422:
            message = error.error?.message || 'Validation failed. Please check your input.';
            break;
          case 500:
            message = 'A server error occurred. Please try again later.';
            break;
          default:
            message = error.error?.message || `Error ${error.status}: ${error.statusText}`;
        }
      }

      notificationService.error(message);
      return throwError(() => error);
    })
  );
};
