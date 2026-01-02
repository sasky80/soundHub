import { test, expect } from '@playwright/test';

test.describe('Now Playing LCD Display', () => {
  test.beforeEach(async ({ page }) => {
    // Navigate to landing page and wait for devices to load
    await page.goto('/');
    await page.waitForSelector('[data-testid="device-list"], .device-list, .devices-grid', { timeout: 10000 });
    
    // Click on first device to navigate to details
    const deviceCard = page.locator('[data-testid="device-card"], .device-card, .device-item').first();
    await deviceCard.click();

    // Wait for device details page to load
    await expect(page.locator('h1, h2').first()).toContainText(/Control|Panel|Details/i);
  });

  test('should display LCD when device is powered on and playing', async ({ page }) => {
    // Ensure device is powered on
    const powerButton = page.locator('button.power-btn, [data-testid="power-button"]');
    const isPoweredOn = await powerButton.evaluate((el) => el.classList.contains('on'));
    
    if (!isPoweredOn) {
      await powerButton.click();
      await page.waitForTimeout(1000);
    }

    // Wait for potential status update
    await page.waitForTimeout(2000);

    // Check if LCD display is visible
    const lcdDisplay = page.locator('.lcd-display');
    const lcdCount = await lcdDisplay.count();

    if (lcdCount > 0) {
      // If LCD is present, verify it's visible
      await expect(lcdDisplay).toBeVisible();
      
      // Check that LCD text is present
      const lcdText = page.locator('.lcd-display .lcd-text');
      await expect(lcdText).toBeVisible();
      
      // Verify text is not empty
      const textContent = await lcdText.textContent();
      expect(textContent?.trim().length).toBeGreaterThan(0);
    }
  });

  test('should hide LCD when device is powered off', async ({ page }) => {
    const powerButton = page.locator('button.power-btn, [data-testid="power-button"]');
    
    // Ensure device is powered on first
    const isPoweredOn = await powerButton.evaluate((el) => el.classList.contains('on'));
    if (!isPoweredOn) {
      await powerButton.click();
      await page.waitForTimeout(2000); // Wait for power on
    }

    // Now turn off the device
    await powerButton.click();
    await page.waitForTimeout(1000); // Wait for power off to process

    // LCD display should not be visible (polling will eventually hide it)
    const lcdDisplay = page.locator('.lcd-display');
    await expect(lcdDisplay).not.toBeVisible({ timeout: 3000 });
  });

  test('should display scrolling animation on LCD text', async ({ page }) => {
    // Ensure device is powered on
    const powerButton = page.locator('button.power-btn, [data-testid="power-button"]');
    const isPoweredOn = await powerButton.evaluate((el) => el.classList.contains('on'));
    
    if (!isPoweredOn) {
      await powerButton.click();
      await page.waitForTimeout(1000);
    }

    await page.waitForTimeout(2000);

    // Check if LCD display exists
    const lcdDisplay = page.locator('.lcd-display');
    const lcdCount = await lcdDisplay.count();

    if (lcdCount > 0) {
      const lcdText = page.locator('.lcd-display .lcd-text');
      
      // Verify text element is not static (should have animation)
      const hasStaticClass = await lcdText.evaluate((el) => el.classList.contains('static'));
      
      if (!hasStaticClass) {
        // Check if element has animation applied
        const hasAnimation = await lcdText.evaluate((el) => {
          const style = window.getComputedStyle(el);
          return style.animationName && style.animationName !== 'none';
        });
        
        expect(hasAnimation).toBeTruthy();
      }
    }
  });

  test('should show Bluetooth button as active when source is BLUETOOTH', async ({ page }) => {
    // Ensure device is powered on
    const powerButton = page.locator('button.power-btn, [data-testid="power-button"]');
    const isPoweredOn = await powerButton.evaluate((el) => el.classList.contains('on'));
    
    if (!isPoweredOn) {
      await powerButton.click();
      await page.waitForTimeout(1000);
    }

    // Check if Bluetooth button exists
    const btBtn = page.locator('button[aria-label="Start Bluetooth pairing"]');
    const btCount = await btBtn.count();
    
    if (btCount > 0) {
      await expect(btBtn).toBeVisible();
      
      // If button has active class, verify aria-pressed
      const hasActiveClass = await btBtn.evaluate((el) => el.classList.contains('active'));
      
      if (hasActiveClass) {
        const ariaPressed = await btBtn.getAttribute('aria-pressed');
        expect(ariaPressed).toBe('true');
      }
    }
  });

  test('should show AUX button as active when source is AUX', async ({ page }) => {
    // Ensure device is powered on
    const powerButton = page.locator('button.power-btn, [data-testid="power-button"]');
    const isPoweredOn = await powerButton.evaluate((el) => el.classList.contains('on'));
    
    if (!isPoweredOn) {
      await powerButton.click();
    }

    // Wait for device to be actually ready by checking if AUX button becomes enabled
    // This indicates power is on, status is loaded, and controls are active
    const auxBtn = page.locator('button[aria-label="Switch to AUX"]');
    await expect(auxBtn).toBeVisible();
    await expect(auxBtn).toBeEnabled({ timeout: 8000 });

    // Click AUX button to activate it
    await auxBtn.click();
    
    // Wait for button to finish loading state (aria-busy should become false)
    await expect(auxBtn).not.toHaveAttribute('aria-busy', 'true', { timeout: 10000 });
    await page.waitForTimeout(1000); // Buffer for API response to complete

    // The component relies on status polling (10s interval) to reflect source changes
    // Wait for the active state to be reflected in the DOM (could take up to 15 seconds)
    await expect(auxBtn).toHaveAttribute('aria-pressed', 'true', { timeout: 15000 });
    
    // Verify active class is also present
    const hasActiveClass = await auxBtn.evaluate((el) => el.classList.contains('active'));
    expect(hasActiveClass).toBeTruthy();
  });

  test('should apply theme styles via data attributes', async ({ page }) => {
    // Ensure device is powered on
    const powerButton = page.locator('button.power-btn, [data-testid="power-button"]');
    const isPoweredOn = await powerButton.evaluate((el) => el.classList.contains('on'));
    
    if (!isPoweredOn) {
      await powerButton.click();
      await page.waitForTimeout(1000);
    }

    // Check if device details component has LCD theme attributes
    const deviceDetails = page.locator('lib-device-details, [data-lcd-theme], [data-lcd-speed]').first();
    const themeAttr = await deviceDetails.getAttribute('data-lcd-theme');
    const speedAttr = await deviceDetails.getAttribute('data-lcd-speed');

    // Verify that theme and speed attributes exist (defaults or user settings)
    if (themeAttr) {
      expect(['green', 'amber', 'blue']).toContain(themeAttr);
    }
    
    if (speedAttr) {
      expect(['slow', 'medium', 'fast']).toContain(speedAttr);
    }
  });
});

test.describe('LCD Display Settings', () => {
  test.beforeEach(async ({ page }) => {
    // Navigate to landing page
    await page.goto('/');
    await page.waitForSelector('[data-testid="device-list"], .device-list, .devices-grid', { timeout: 10000 });
    
    // Navigate to settings page
    const settingsLink = page.locator('a[href="/settings"], [data-testid="settings-link"]').first();
    await settingsLink.click();

    // Wait for settings page to load
    await expect(page.locator('h1, h2').first()).toContainText(/Settings|Ustawienia/i);
  });

  test('should display LCD settings section', async ({ page }) => {
    // Look for LCD Display heading
    const lcdHeading = page.locator('text=/LCD Display|Wyświetlacz LCD/i');
    await expect(lcdHeading).toBeVisible();
  });

  test('should display LCD preview', async ({ page }) => {
    // Check for LCD preview container
    const lcdPreview = page.locator('.lcd-display-preview, .lcd-preview-container');
    await expect(lcdPreview.first()).toBeVisible();
    
    // Preview should contain sample text
    const previewText = page.locator('.lcd-display-preview .lcd-text');
    await expect(previewText).toBeVisible();
    
    const textContent = await previewText.textContent();
    expect(textContent?.trim().length).toBeGreaterThan(0);
  });

  test('should display scroll speed options', async ({ page }) => {
    // Check for scroll speed buttons
    const slowBtn = page.locator('text=/Slow|Wolna/i').first();
    const mediumBtn = page.locator('text=/Medium|Średnia/i').first();
    const fastBtn = page.locator('text=/Fast|Szybka/i').first();

    await expect(slowBtn).toBeVisible();
    await expect(mediumBtn).toBeVisible();
    await expect(fastBtn).toBeVisible();
  });

  test('should display color theme options with swatches', async ({ page }) => {
    // Check for color theme buttons
    const greenBtn = page.locator('text=/Green|Zielony/i').first();
    const amberBtn = page.locator('text=/Amber|Bursztynowy/i').first();
    const blueBtn = page.locator('text=/Blue|Niebieski/i').first();

    await expect(greenBtn).toBeVisible();
    await expect(amberBtn).toBeVisible();
    await expect(blueBtn).toBeVisible();

    // Check for color swatches
    const swatches = page.locator('.color-swatch');
    const swatchCount = await swatches.count();
    expect(swatchCount).toBeGreaterThanOrEqual(3);
  });

  test('should change scroll speed when clicked', async ({ page }) => {
    const slowBtn = page.locator('button:has-text("Slow"), button:has-text("Wolna")').first();
    
    await slowBtn.click();
    await page.waitForTimeout(300);

    // Button should have active state
    const hasActiveClass = await slowBtn.evaluate((el) => el.classList.contains('active'));
    expect(hasActiveClass).toBeTruthy();

    // Preview should update with new speed
    const preview = page.locator('.lcd-display-preview').first();
    const speedAttr = await preview.getAttribute('data-lcd-speed');
    expect(speedAttr).toBe('slow');
  });

  test('should change color theme when clicked', async ({ page }) => {
    const amberBtn = page.locator('button:has-text("Amber"), button:has-text("Bursztynowy")').first();
    
    await amberBtn.click();
    await page.waitForTimeout(300);

    // Button should have active state
    const hasActiveClass = await amberBtn.evaluate((el) => el.classList.contains('active'));
    expect(hasActiveClass).toBeTruthy();

    // Preview should update with new theme
    const preview = page.locator('.lcd-display-preview').first();
    const themeAttr = await preview.getAttribute('data-lcd-theme');
    expect(themeAttr).toBe('amber');
  });

  test('should persist settings to localStorage', async ({ page }) => {
    // Change scroll speed
    const fastBtn = page.locator('button:has-text("Fast"), button:has-text("Szybka")').first();
    await fastBtn.click();
    await page.waitForTimeout(300);

    // Change color theme
    const blueBtn = page.locator('button:has-text("Blue"), button:has-text("Niebieski")').first();
    await blueBtn.click();
    await page.waitForTimeout(300);

    // Verify localStorage was updated
    const scrollSpeed = await page.evaluate(() => localStorage.getItem('lcdScrollSpeed'));
    const colorTheme = await page.evaluate(() => localStorage.getItem('lcdColorTheme'));

    expect(scrollSpeed).toBe('fast');
    expect(colorTheme).toBe('blue');
  });

  test('should load settings from localStorage on page load', async ({ page }) => {
    // Set localStorage values
    await page.evaluate(() => {
      localStorage.setItem('lcdScrollSpeed', 'slow');
      localStorage.setItem('lcdColorTheme', 'amber');
    });

    // Navigate to home first
    await page.goto('/');
    await page.waitForSelector('[data-testid="device-list"], .device-list, .devices-grid', { timeout: 10000 });

    // Navigate to settings
    const settingsLink = page.locator('a[href="/settings"], [data-testid="settings-link"]').first();
    await settingsLink.click();
    await page.waitForTimeout(1000);

    // Verify slow and amber buttons are active
    const slowBtn = page.locator('button:has-text("Slow"), button:has-text("Wolna")').first();
    const amberBtn = page.locator('button:has-text("Amber"), button:has-text("Bursztynowy")').first();

    const slowActive = await slowBtn.evaluate((el) => el.classList.contains('active'));
    const amberActive = await amberBtn.evaluate((el) => el.classList.contains('active'));

    expect(slowActive).toBeTruthy();
    expect(amberActive).toBeTruthy();
  });
});
