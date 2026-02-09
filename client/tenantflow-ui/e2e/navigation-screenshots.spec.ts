import fs from 'node:fs';
import path from 'node:path';
import { expect, test } from '@playwright/test';

const screenshotsDir = path.resolve(__dirname, '../../../docs/screenshots');

const platformAdminAuth = {
  token: '',
  expiresAtUtc: new Date(Date.now() + 60 * 60 * 1000).toISOString(),
  tenantId: 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa',
  userId: '11111111-1111-1111-1111-111111111111',
  email: 'platform@tenantflow.dev',
  roles: ['PlatformAdmin']
};

const tenantAdminAuth = {
  token: '',
  expiresAtUtc: new Date(Date.now() + 60 * 60 * 1000).toISOString(),
  tenantId: 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb',
  userId: '22222222-2222-2222-2222-222222222222',
  email: 'admin@acme.dev',
  roles: ['TenantAdmin']
};

function ensureScreenshotsDir(): void {
  fs.mkdirSync(screenshotsDir, { recursive: true });
}

test('capture navigation screenshots for demo', async ({ page }) => {
  ensureScreenshotsDir();

  await page.goto('/');
  await expect(page.getByTestId('home-title')).toBeVisible();
  await page.screenshot({ path: path.join(screenshotsDir, '01-home.png'), fullPage: true });

  await page.goto('/login');
  await expect(page.getByTestId('login-title')).toBeVisible();
  await page.screenshot({ path: path.join(screenshotsDir, '02-login.png'), fullPage: true });

  await page.route('**/api/v1/admin/tenants', async (route) => {
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify([
        { id: 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb', name: 'Acme Logistics', slug: 'acme', isActive: true, userCount: 2 },
        { id: 'cccccccc-cccc-cccc-cccc-cccccccccccc', name: 'Globex Manufacturing', slug: 'globex', isActive: true, userCount: 1 }
      ])
    });
  });

  await page.goto('/login');
  await page.evaluate((auth) => {
    sessionStorage.setItem('tenantflow_auth', JSON.stringify(auth));
  }, platformAdminAuth);
  await page.goto('/admin');
  await expect(page.getByText('Platform Admin Portal')).toBeVisible();
  await page.screenshot({ path: path.join(screenshotsDir, '03-admin-portal.png'), fullPage: true });

  await page.route('**/api/v1/tenant/users', async (route) => {
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify([
        { userId: '33333333-3333-3333-3333-333333333333', email: 'user@acme.dev', displayName: 'Acme User', role: 'TenantUser', isActive: true }
      ])
    });
  });

  await page.route('**/api/v1/tenant/feature-flags', async (route) => {
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify([
        { id: 'f1f1f1f1-1111-1111-1111-111111111111', key: 'quote.create', isEnabled: true, rolloutPercent: 100 },
        { id: 'f2f2f2f2-2222-2222-2222-222222222222', key: 'tenant.user.invite', isEnabled: true, rolloutPercent: 100 }
      ])
    });
  });

  await page.route('**/api/v1/quotes', async (route) => {
    if (route.request().method() === 'GET') {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify([
          { id: 'q1q1q1q1-1111-1111-1111-111111111111', quoteNumber: 'Q-20260209120000-101', customerName: 'Contoso', premium: 1250 }
        ])
      });
      return;
    }

    await route.continue();
  });

  await page.goto('/login');
  await page.evaluate((auth) => {
    sessionStorage.setItem('tenantflow_auth', JSON.stringify(auth));
  }, tenantAdminAuth);
  await page.goto('/app');
  await expect(page.getByText('Tenant Portal')).toBeVisible();
  await page.screenshot({ path: path.join(screenshotsDir, '04-tenant-portal.png'), fullPage: true });
});
