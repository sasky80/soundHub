import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { APP_BASE_HREF } from '@angular/common';

export const apiBasePathInterceptor: HttpInterceptorFn = (req, next) => {
  const baseHref = inject(APP_BASE_HREF, { optional: true }) ?? '/';

  if (req.url.startsWith('/api/')) {
    const basePath = baseHref.endsWith('/') ? baseHref.slice(0, -1) : baseHref;
    const cloned = req.clone({ url: `${basePath}${req.url}` });
    return next(cloned);
  }

  return next(req);
};
