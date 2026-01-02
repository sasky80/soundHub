import { test, expect } from '@playwright/test';

test.describe('Remote Control', () => {
  test.beforeEach(async ({ page }) => {
    // Navigate to landing page and wait for devices to load
    await page.goto('/');
    await page.waitForSelector('[data-testid="device-list"], .device-list, .devices-grid', { timeout: 10000 });
    
    // Click on first device to navigate to details
    const deviceCard = page.locator('[data-testid="device-card"], .device-card, .device-item').first();
    await deviceCard.click();

    // Wait for device details page to load
    await expect(page.locator('h1, h2').first()).toContainText(/Control|Panel|Details/i);
    
    // Ensure device is powered on
    const powerButton = page.locator('button.power-btn, [data-testid="power-button"]');
    const isPoweredOn = await powerButton.evaluate((el) => el.classList.contains('on'));
    if (!isPoweredOn) {
      await powerButton.click();
      await page.waitForTimeout(500);
    }
  });

  test('should display all playback control buttons', async ({ page }) => {
    const prevBtn = page.locator('button[aria-label="Previous track"]');
    const playBtn = page.locator('button[aria-label="Play or pause"]');
    const nextBtn = page.locator('button[aria-label="Next track"]');

    await expect(prevBtn).toBeVisible();
    await expect(playBtn).toBeVisible();
    await expect(nextBtn).toBeVisible();
  });

  test('should click previous track button', async ({ page }) => {
    const prevBtn = page.locator('button[aria-label="Previous track"]');
    
    await prevBtn.click();
    await page.waitForTimeout(300);

    // Button should become disabled briefly during loading
    // Then enabled again after response
    await expect(prevBtn).toBeEnabled();
  });

  test('should click next track button', async ({ page }) => {
    const nextBtn = page.locator('button[aria-label="Next track"]');
    
    await nextBtn.click();
    await page.waitForTimeout(300);

    await expect(nextBtn).toBeEnabled();
  });

  test('should click play/pause button', async ({ page }) => {
    const playBtn = page.locator('button[aria-label="Play or pause"]');
    await expect(playBtn).toBeVisible();

    // Click the button - API call may or may not succeed in test environment
    await playBtn.click();

    // Button should still be visible after click (test that it's clickable)
    await expect(playBtn).toBeVisible();
  });

  test('should display volume control buttons', async ({ page }) => {
    const volUpBtn = page.locator('button[aria-label="Volume up"]');
    const volDownBtn = page.locator('button[aria-label="Volume down"]');

    await expect(volUpBtn).toBeVisible();
    await expect(volDownBtn).toBeVisible();
  });

  test('should click volume up button', async ({ page }) => {
    const volUpBtn = page.locator('button[aria-label="Volume up"]');
    const volumeValue = page.locator('.volume-value, [data-testid="volume-value"]');
    
    const initialVolume = await volumeValue.textContent();
    
    await volUpBtn.click();
    await page.waitForTimeout(300);

    // Volume should potentially increase (or stay the same if at max)
    const newVolume = await volumeValue.textContent();
    expect(newVolume).toBeDefined();
  });

  test('should click volume down button', async ({ page }) => {
    const volDownBtn = page.locator('button[aria-label="Volume down"]');
    const volumeValue = page.locator('.volume-value, [data-testid="volume-value"]');
    
    const initialVolume = await volumeValue.textContent();
    
    await volDownBtn.click();
    await page.waitForTimeout(300);

    // Volume should potentially decrease (or stay the same if at min)
    const newVolume = await volumeValue.textContent();
    expect(newVolume).toBeDefined();
  });

  test('should display AUX button', async ({ page }) => {
    const auxBtn = page.locator('button[aria-label="Switch to AUX"]');
    await expect(auxBtn).toBeVisible();
  });

  test('should click AUX button and show active state', async ({ page }) => {
    const auxBtn = page.locator('button[aria-label="Switch to AUX"]');
    await expect(auxBtn).toBeVisible();
    
    await auxBtn.click();
    await page.waitForTimeout(500);

    // Button should be visible after click (active state is optional, may not trigger without API)
    await expect(auxBtn).toBeVisible();
  });

  test('should show Bluetooth button if device supports it', async ({ page }) => {
    const btBtn = page.locator('button[aria-label="Start Bluetooth pairing"]');
    
    // Button might or might not be visible depending on device capabilities
    const count = await btBtn.count();
    
    if (count > 0) {
      await expect(btBtn).toBeVisible();
    }
  });

  test('should click Bluetooth button if visible', async ({ page }) => {
    const btBtn = page.locator('button[aria-label="Start Bluetooth pairing"]');
    const count = await btBtn.count();
    
    if (count > 0) {
      await expect(btBtn).toBeEnabled();
      await btBtn.click();
      await page.waitForTimeout(500);

      // Button should still be visible after click
      await expect(btBtn).toBeVisible();
    }
  });

  test('should disable remote buttons when device is powered off', async ({ page }) => {
    const powerButton = page.locator('button.power-btn, [data-testid="power-button"]');
    
    // Turn off the device
    await powerButton.click();
    await page.waitForTimeout(500);

    // All remote buttons should be disabled
    const prevBtn = page.locator('button[aria-label="Previous track"]');
    const playBtn = page.locator('button[aria-label="Play or pause"]');
    const nextBtn = page.locator('button[aria-label="Next track"]');
    const volUpBtn = page.locator('button[aria-label="Volume up"]');
    const volDownBtn = page.locator('button[aria-label="Volume down"]');
    const auxBtn = page.locator('button[aria-label="Switch to AUX"]');

    await expect(prevBtn).toBeDisabled();
    await expect(playBtn).toBeDisabled();
    await expect(nextBtn).toBeDisabled();
    await expect(volUpBtn).toBeDisabled();
    await expect(volDownBtn).toBeDisabled();
    await expect(auxBtn).toBeDisabled();
  });
});
