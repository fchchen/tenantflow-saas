import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { AdminDashboardPage } from './admin-dashboard.page';

describe('AdminDashboardPage', () => {
  let httpMock: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AdminDashboardPage, HttpClientTestingModule],
      providers: [provideNoopAnimations()]
    }).compileComponents();

    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('loads tenants on init', async () => {
    const fixture = TestBed.createComponent(AdminDashboardPage);
    fixture.detectChanges();

    const req = httpMock.expectOne('http://localhost:5000/api/v1/admin/tenants');
    expect(req.request.method).toBe('GET');
    req.flush([{ id: '1', name: 'Acme', slug: 'acme', isActive: true, userCount: 2 }]);

    await fixture.whenStable();
    expect(fixture.componentInstance.tenants().length).toBe(1);
  });

  it('does not call API when tenant fields are empty', async () => {
    const fixture = TestBed.createComponent(AdminDashboardPage);

    await fixture.componentInstance.createTenant();

    httpMock.expectNone('http://localhost:5000/api/v1/admin/tenants');
    expect(fixture.componentInstance.tenants().length).toBe(0);
  });
});
