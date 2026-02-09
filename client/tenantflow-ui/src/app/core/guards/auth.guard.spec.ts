import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { authGuard } from './auth.guard';
import { AuthService } from '../services/auth.service';

describe('authGuard', () => {
  const router = {
    navigateByUrl: jasmine.createSpy('navigateByUrl').and.returnValue(Promise.resolve(true))
  };

  const auth = {
    isAuthenticated: jasmine.createSpy('isAuthenticated')
  };

  beforeEach(() => {
    router.navigateByUrl.calls.reset();
    auth.isAuthenticated.calls.reset();

    TestBed.configureTestingModule({
      providers: [
        { provide: Router, useValue: router },
        { provide: AuthService, useValue: auth }
      ]
    });
  });

  it('allows navigation for authenticated users', () => {
    auth.isAuthenticated.and.returnValue(true);

    const result = TestBed.runInInjectionContext(() => authGuard({} as never, {} as never));

    expect(result).toBeTrue();
    expect(router.navigateByUrl).not.toHaveBeenCalled();
  });

  it('redirects unauthenticated users to login', () => {
    auth.isAuthenticated.and.returnValue(false);

    const result = TestBed.runInInjectionContext(() => authGuard({} as never, {} as never));

    expect(result).toBeFalse();
    expect(router.navigateByUrl).toHaveBeenCalledWith('/login');
  });
});
