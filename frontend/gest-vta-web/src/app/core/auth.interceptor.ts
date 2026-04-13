import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { AuthService } from './auth.service';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(AuthService);
  const isLogin = req.url.includes('/api/auth/login');

  if (isLogin) {
    return next(req);
  }

  const session = auth.session();
  if (session?.accessToken && auth.isTokenExpiredNow()) {
    auth.logout();
    return throwError(() => new HttpErrorResponse({ status: 401, statusText: 'Sesión expirada' }));
  }

  const token = session?.accessToken;
  const authReq = token ? req.clone({ setHeaders: { Authorization: `Bearer ${token}` } }) : req;

  return next(authReq).pipe(
    catchError((err: HttpErrorResponse) => {
      if (err.status === 401 && !isLogin) {
        auth.logout();
      }
      return throwError(() => err);
    }),
  );
};
