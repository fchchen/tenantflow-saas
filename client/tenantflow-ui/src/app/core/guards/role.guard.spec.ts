import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { roleGuard } from './role.guard';
import { AuthService } from '../services/auth.service';

describe('roleGuard', () => {
  const router = {
    navigateByUrl: jasmine.createSpy('navigateByUrl').and.returnValue(Promise.resolve(true))
  };

  const auth = {
    isAuthenticated: jasmine.createSpy('isAuthenticated'),
    hasAnyRole: jasmine.createSpy('hasAnyRole')
  };

  beforeEach(() => {
    router.navigateByUrl.calls.reset();
    auth.isAuthenticated.calls.reset();
    auth.hasAnyRole.calls.reset();

    TestBed.configureTestingModule({
      providers: [
        { provide: Router, useValue: router },
        { provide: AuthService, useValue: auth }
      ]
    });
  });

  it('allows when authenticated and role is present', () => {
    auth.isAuthenticated.and.returnValue(true);
    auth.hasAnyRole.and.returnValue(true);
    const route = { data: { roles: ['PlatformAdmin'] } } as never;

    const result = TestBed.runInInjectionContext(() => roleGuard(route, {} as never));

    expect(result).toBeTrue();
    expect(router.navigateByUrl).not.toHaveBeenCalled();
  });

  it('redirects to root when role is missing', () => {
    auth.isAuthenticated.and.returnValue(true);
    auth.hasAnyRole.and.returnValue(false);
    const route = { data: { roles: ['PlatformAdmin'] } } as never;

    const result = TestBed.runInInjectionContext(() => roleGuard(route, {} as never));

    expect(result).toBeFalse();
    expect(router.navigateByUrl).toHaveBeenCalledWith('/');
  });
});
