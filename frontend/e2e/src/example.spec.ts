import { test, expect } from '@playwright/test';

test('has title', async ({ page }) => {
  await page.goto('/');

  // Expect app name to be visible
  await expect(page.locator('.app-name').first()).toContainText(/SoundHub/i);
});
