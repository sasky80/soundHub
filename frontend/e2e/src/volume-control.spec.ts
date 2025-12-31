import { test, expect } from '@playwright/test';

test.describe('Volume Control', () => {
  test.beforeEach(async ({ page }) => {
    // Navigate to landing page and wait for devices to load
    await page.goto('/');
    await page.waitForSelector('[data-testid="device-list"], .device-list, .devices-grid', { timeout: 10000 });
  });

  test('should display volume slider on device details page', async ({ page }) => {
    // Click on first device to navigate to details
    const deviceCard = page.locator('[data-testid="device-card"], .device-card, .device-item').first();
    await deviceCard.click();

    // Wait for device details page to load
    await expect(page.locator('h1, h2').first()).toContainText(/Device|Details/i);

    // Volume slider should be visible
    const volumeSlider = page.locator('input[type="range"].volume-slider, [data-testid="volume-slider"]');
    await expect(volumeSlider).toBeVisible();
  });

  test('should display mute button on device details page', async ({ page }) => {
    // Click on first device to navigate to details
    const deviceCard = page.locator('[data-testid="device-card"], .device-card, .device-item').first();
    await deviceCard.click();

    // Wait for device details page to load
    await expect(page.locator('h1, h2').first()).toContainText(/Device|Details/i);

    // Mute button should be visible
    const muteButton = page.locator('button.mute-btn, [data-testid="mute-button"]');
    await expect(muteButton).toBeVisible();
  });

  test('should show volume value next to slider', async ({ page }) => {
    // Click on first device to navigate to details
    const deviceCard = page.locator('[data-testid="device-card"], .device-card, .device-item').first();
    await deviceCard.click();

    // Wait for device details page to load
    await expect(page.locator('h1, h2').first()).toContainText(/Device|Details/i);

    // Volume value should be visible
    const volumeValue = page.locator('.volume-value, [data-testid="volume-value"]');
    await expect(volumeValue).toBeVisible();
  });

  test('should disable volume controls when device is off', async ({ page }) => {
    // Click on first device to navigate to details
    const deviceCard = page.locator('[data-testid="device-card"], .device-card, .device-item').first();
    await deviceCard.click();

    // Wait for device details page to load
    await expect(page.locator('h1, h2').first()).toContainText(/Device|Details/i);

    // Find and click power button to turn off device
    const powerButton = page.locator('button.power-btn, [data-testid="power-button"]');
    const isPoweredOn = await powerButton.evaluate((el) => el.classList.contains('on'));

    if (isPoweredOn) {
      await powerButton.click();
      await page.waitForTimeout(500); // Wait for state update

      // Volume slider should be disabled
      const volumeSlider = page.locator('input[type="range"].volume-slider, [data-testid="volume-slider"]');
      await expect(volumeSlider).toBeDisabled();

      // Mute button should be disabled
      const muteButton = page.locator('button.mute-btn, [data-testid="mute-button"]');
      await expect(muteButton).toBeDisabled();
    }
  });

  test('should toggle mute state when clicking mute button', async ({ page }) => {
    // Click on first device to navigate to details
    const deviceCard = page.locator('[data-testid="device-card"], .device-card, .device-item').first();
    await deviceCard.click();

    // Wait for device details page to load
    await expect(page.locator('h1, h2').first()).toContainText(/Device|Details/i);

    // Ensure device is on first
    const powerButton = page.locator('button.power-btn, [data-testid="power-button"]');
    const isPoweredOn = await powerButton.evaluate((el) => el.classList.contains('on'));
    if (!isPoweredOn) {
      await powerButton.click();
      await page.waitForTimeout(500);
    }

    // Click mute button
    const muteButton = page.locator('button.mute-btn, [data-testid="mute-button"]');
    const wasMuted = await muteButton.evaluate((el) => el.classList.contains('muted'));
    await muteButton.click();

    // Wait for API response
    await page.waitForTimeout(500);

    // Mute state should have toggled
    const isMutedNow = await muteButton.evaluate((el) => el.classList.contains('muted'));
    expect(isMutedNow).not.toBe(wasMuted);
  });

  test('should be able to adjust volume slider', async ({ page }) => {
    // Click on first device to navigate to details
    const deviceCard = page.locator('[data-testid="device-card"], .device-card, .device-item').first();
    await deviceCard.click();

    // Wait for device details page to load
    await expect(page.locator('h1, h2').first()).toContainText(/Device|Details/i);

    // Ensure device is on first
    const powerButton = page.locator('button.power-btn, [data-testid="power-button"]');
    const isPoweredOn = await powerButton.evaluate((el) => el.classList.contains('on'));
    if (!isPoweredOn) {
      await powerButton.click();
      await page.waitForTimeout(500);
    }

    // Get volume slider and adjust it
    const volumeSlider = page.locator('input[type="range"].volume-slider, [data-testid="volume-slider"]');
    await expect(volumeSlider).toBeEnabled();

    // Set volume to 75
    await volumeSlider.fill('75');

    // Volume value should update
    const volumeValue = page.locator('.volume-value, [data-testid="volume-value"]');
    await expect(volumeValue).toContainText('75');
  });
});
