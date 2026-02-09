import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { Router } from '@angular/router';
import { AuthService } from './auth.service';

describe('AuthService', () => {
  let service: AuthService;
  let httpMock: HttpTestingController;
  const router = {
    navigateByUrl: jasmine.createSpy('navigateByUrl').and.returnValue(Promise.resolve(true))
  };

  beforeEach(() => {
    sessionStorage.clear();
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [{ provide: Router, useValue: router }]
    });

    service = TestBed.inject(AuthService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
    sessionStorage.clear();
  });

  it('stores auth payload without persisting JWT token', async () => {
    const loginPromise = service.login({ email: 'admin@acme.dev', password: 'Pass123$' });

    const req = httpMock.expectOne('http://localhost:5000/api/v1/auth/login');
    expect(req.request.method).toBe('POST');
    req.flush({
      token: 'token-1',
      expiresAtUtc: new Date().toISOString(),
      tenantId: 'tenant-1',
      userId: 'user-1',
      email: 'admin@acme.dev',
      roles: ['TenantAdmin']
    });

    await loginPromise;
    expect(service.isAuthenticated()).toBeTrue();
    expect(service.token()).toBe('');

    const stored = JSON.parse(sessionStorage.getItem('tenantflow_auth') ?? '{}') as { token?: string };
    expect(stored.token).toBe('');
  });

  it('calls demo endpoint for demo persona', async () => {
    const demoPromise = service.demoLogin('tenant-admin');

    const req = httpMock.expectOne('http://localhost:5000/api/v1/auth/demo');
    expect(req.request.method).toBe('POST');
    expect(req.request.body.persona).toBe('tenant-admin');
    req.flush({
      token: 'token-2',
      expiresAtUtc: new Date().toISOString(),
      tenantId: 'tenant-2',
      userId: 'user-2',
      email: 'admin@acme.dev',
      roles: ['TenantAdmin']
    });

    await demoPromise;
    expect(service.isAuthenticated()).toBeTrue();
  });

  it('calls logout endpoint and clears session', () => {
    sessionStorage.setItem('tenantflow_auth', '{}');

    service.logout();

    const req = httpMock.expectOne('http://localhost:5000/api/v1/auth/logout');
    expect(req.request.method).toBe('POST');
    req.flush(null);

    expect(sessionStorage.getItem('tenantflow_auth')).toBeNull();
    expect(router.navigateByUrl).toHaveBeenCalledWith('/login');
  });
});
