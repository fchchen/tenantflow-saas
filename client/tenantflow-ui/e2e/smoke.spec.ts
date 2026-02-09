import { expect, test } from '@playwright/test';

test('home page renders primary navigation', async ({ page }) => {
  await page.goto('/');

  await expect(page.getByTestId('brand-link')).toBeVisible();
  await expect(page.getByTestId('admin-link')).toBeVisible();
  await expect(page.getByTestId('tenant-link')).toBeVisible();
  await expect(page.getByTestId('home-title')).toHaveText('TenantFlow SaaS');
});

test('login page shows sign in controls and demo actions', async ({ page }) => {
  await page.goto('/login');

  await expect(page.getByTestId('login-title')).toHaveText('Sign In');
  await expect(page.getByTestId('email-input')).toBeVisible();
  await expect(page.getByTestId('password-input')).toBeVisible();
  await expect(page.getByTestId('login-button')).toBeVisible();
  await expect(page.getByTestId('demo-platform-admin-button')).toBeVisible();
});
