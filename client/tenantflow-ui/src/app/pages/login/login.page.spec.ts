import { TestBed } from '@angular/core/testing';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { Router } from '@angular/router';
import { LoginPage } from './login.page';
import { AuthService } from '../../core/services/auth.service';

describe('LoginPage', () => {
  const router = {
    navigateByUrl: jasmine.createSpy('navigateByUrl').and.returnValue(Promise.resolve(true))
  };

  const auth = {
    login: jasmine.createSpy('login').and.returnValue(Promise.resolve()),
    demoLogin: jasmine.createSpy('demoLogin').and.returnValue(Promise.resolve()),
    isPlatformAdmin: jasmine.createSpy('isPlatformAdmin')
  };

  beforeEach(async () => {
    router.navigateByUrl.calls.reset();
    auth.login.calls.reset();
    auth.demoLogin.calls.reset();
    auth.isPlatformAdmin.calls.reset();

    await TestBed.configureTestingModule({
      imports: [LoginPage],
      providers: [
        provideNoopAnimations(),
        { provide: Router, useValue: router },
        { provide: AuthService, useValue: auth }
      ]
    }).compileComponents();
  });

  it('routes platform admin to /admin', async () => {
    auth.isPlatformAdmin.and.returnValue(true);

    const fixture = TestBed.createComponent(LoginPage);
    await (fixture.componentInstance as any).routeAfterLogin();

    expect(router.navigateByUrl).toHaveBeenCalledWith('/admin');
  });

  it('routes tenant users to /app', async () => {
    auth.isPlatformAdmin.and.returnValue(false);

    const fixture = TestBed.createComponent(LoginPage);
    await (fixture.componentInstance as any).routeAfterLogin();

    expect(router.navigateByUrl).toHaveBeenCalledWith('/app');
  });

  it('shows error when login fails', async () => {
    auth.login.and.returnValue(Promise.reject(new Error('bad creds')));

    const fixture = TestBed.createComponent(LoginPage);
    await fixture.componentInstance.login();

    expect(fixture.componentInstance.error()).toContain('Login failed');
  });
});
