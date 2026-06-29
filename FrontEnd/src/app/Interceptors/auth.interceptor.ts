import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { ApiService } from '../services/api';
import { catchError, finalize } from 'rxjs/operators';
import { throwError } from 'rxjs';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const apiService = inject(ApiService);

  apiService.isLoading.set(true);
  apiService.errorMessage.set('');

  return next(req).pipe(
    catchError((err) => {
      const msg = err.error?.detail || err.error?.message || 'Error occurred';
      apiService.errorMessage.set(msg);
      return throwError(() => err);
    }),
    finalize(() => apiService.isLoading.set(false))
  );
};