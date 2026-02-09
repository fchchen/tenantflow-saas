import { CanActivateFn, Router } from '@angular/router';
import { inject } from '@angular/core';
import { AuthService } from '../services/auth.service';

export const roleGuard: CanActivateFn = (route) => {
  const requiredRoles = (route.data['roles'] as string[] | undefined) ?? [];
  const auth = inject(AuthService);
  const router = inject(Router);

  if (!auth.isAuthenticated()) {
    void router.navigateByUrl('/login');
    return false;
  }

  if (requiredRoles.length === 0 || auth.hasAnyRole(requiredRoles)) {
    return true;
  }

  void router.navigateByUrl('/');
  return false;
};
