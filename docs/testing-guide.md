# Testing Guide

## Overview

This guide covers testing practices for the SoundHub project, including unit tests (Jest), E2E tests (Playwright), and common patterns for mocking Angular dependencies.

## Frontend Testing

### Running Tests

```bash
# Run all unit tests in the workspace
npx nx run-many --target=test --all

# Run tests for all affected projects (based on git changes)
npx nx affected --target=test

# Run all tests in a specific library
npx nx test <library-name>

# Run specific test files
npx nx test <library-name> --testFile="component.spec.ts"

# Run tests in watch mode
npx nx test <library-name> --watch

# Run tests with coverage
npx nx test <library-name> --coverage

# Run E2E tests
npx nx e2e e2e

# Run E2E tests in headed mode (see browser)
npx nx e2e e2e --headed
```

### Unit Testing with Jest

#### Component Testing Structure

```typescript
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { YourComponent } from './your.component';

describe('YourComponent', () => {
  let component: YourComponent;
  let fixture: ComponentFixture<YourComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [YourComponent], // Standalone components
      providers: [
        // Provide mocks here
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(YourComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
```

### Common Mocking Patterns

#### Mocking Angular Router

**⚠️ Important:** When testing components with `RouterLink` directives, the Router mock must include all methods and observables that RouterLink depends on:

```typescript
import { Router } from '@angular/router';
import { Subject } from 'rxjs';

let mockRouter: jest.Mocked<Partial<Router>>;

beforeEach(async () => {
  mockRouter = {
    navigate: jest.fn(),
    createUrlTree: jest.fn().mockReturnValue({}),
    serializeUrl: jest.fn().mockReturnValue(''),
    events: new Subject(), // Required for RouterLink
  } as unknown as jest.Mocked<Router>;

  await TestBed.configureTestingModule({
    imports: [YourComponent],
    providers: [
      { provide: Router, useValue: mockRouter },
    ],
  }).compileComponents();
});
```

**Required Router methods:**
- `navigate()` - For programmatic navigation
- `createUrlTree()` - Used by RouterLink directive to create URL trees
- `serializeUrl()` - Used by RouterLink to serialize URLs
- `events` - Observable stream of router events (use `Subject` for testing)

**Common Router Testing Scenarios:**

```typescript
// Test navigation was called
it('should navigate on action', () => {
  component.goToPage();
  expect(mockRouter.navigate).toHaveBeenCalledWith(['/path', 'param']);
});

// Test RouterLink in template (requires full mock)
it('should render router links', () => {
  fixture.detectChanges();
  const link = fixture.nativeElement.querySelector('a[routerLink]');
  expect(link).toBeTruthy();
});
```

#### Mocking ActivatedRoute

```typescript
import { ActivatedRoute } from '@angular/router';

let mockActivatedRoute: Partial<ActivatedRoute>;

beforeEach(async () => {
  mockActivatedRoute = {
    snapshot: {
      paramMap: {
        get: jest.fn((key: string) => {
          if (key === 'id') return 'test-id';
          return null;
        }),
      },
    },
  } as any;

  await TestBed.configureTestingModule({
    imports: [YourComponent],
    providers: [
      { provide: ActivatedRoute, useValue: mockActivatedRoute },
    ],
  }).compileComponents();
});
```

#### Mocking HTTP Services

```typescript
import { of, throwError } from 'rxjs';

let mockService: jest.Mocked<YourService>;

beforeEach(async () => {
  mockService = {
    getData: jest.fn().mockReturnValue(of({ id: 1, name: 'Test' })),
    postData: jest.fn().mockReturnValue(of(void 0)),
  } as unknown as jest.Mocked<YourService>;

  await TestBed.configureTestingModule({
    imports: [YourComponent],
    providers: [
      { provide: YourService, useValue: mockService },
    ],
  }).compileComponents();
});

// Test success case
it('should load data', () => {
  component.loadData();
  expect(mockService.getData).toHaveBeenCalled();
  expect(component.data()).toBe('Test');
});

// Test error case
it('should handle error', () => {
  mockService.getData.mockReturnValue(throwError(() => new Error('Failed')));
  component.loadData();
  expect(component.error()).toBeTruthy();
});
```

#### Testing Angular Signals

```typescript
it('should update signal value', () => {
  expect(component.count()).toBe(0);
  
  component.increment();
  
  expect(component.count()).toBe(1);
});

it('should react to computed signals', () => {
  component.count.set(5);
  
  expect(component.doubleCount()).toBe(10);
});
```

#### Testing Forms

```typescript
import { ReactiveFormsModule } from '@angular/forms';

beforeEach(async () => {
  await TestBed.configureTestingModule({
    imports: [YourComponent, ReactiveFormsModule],
  }).compileComponents();
});

it('should validate form', () => {
  const form = component.form;
  
  form.patchValue({ name: '', email: 'invalid' });
  expect(form.valid).toBe(false);
  
  form.patchValue({ name: 'John', email: 'john@example.com' });
  expect(form.valid).toBe(true);
});

it('should submit valid form', () => {
  const spy = jest.spyOn(component, 'onSubmit');
  component.form.patchValue({ name: 'John' });
  
  component.onSubmit();
  
  expect(spy).toHaveBeenCalled();
  expect(mockService.save).toHaveBeenCalledWith({ name: 'John' });
});
```

### E2E Testing with Playwright

#### Test Structure

```typescript
import { test, expect } from '@playwright/test';

test.describe('Feature Name', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/');
  });

  test('should perform user action', async ({ page }) => {
    await page.click('button[aria-label="Action"]');
    await expect(page.locator('.result')).toBeVisible();
  });
});
```

#### Best Practices

1. **Use semantic selectors**: Prefer `role`, `aria-label`, `data-testid` over CSS classes
2. **Wait for conditions**: Use Playwright's auto-waiting and explicit waits
3. **Test user flows**: Focus on complete user journeys, not implementation details
4. **Keep tests independent**: Each test should be runnable in isolation

```typescript
// Good: semantic selector
await page.click('button[aria-label="Add Preset"]');

// Avoid: implementation-specific selector
await page.click('.preset-list__add-btn');

// Good: wait for condition
await expect(page.locator('.preset-item')).toHaveCount(3);

// Avoid: arbitrary timeouts
await page.waitForTimeout(1000);
```

## Backend Testing

### .NET Unit Tests (xUnit)

```bash
# Run all tests
dotnet test

# Run tests with coverage
dotnet test /p:CollectCoverage=true
```

#### Test Structure

```csharp
public class ServiceTests
{
    private readonly Mock<IDependency> _mockDependency;
    private readonly Service _service;

    public ServiceTests()
    {
        _mockDependency = new Mock<IDependency>();
        _service = new Service(_mockDependency.Object);
    }

    [Fact]
    public async Task Method_Should_ReturnExpectedResult()
    {
        // Arrange
        _mockDependency.Setup(x => x.GetData())
            .ReturnsAsync(new Data());

        // Act
        var result = await _service.ProcessData();

        // Assert
        Assert.NotNull(result);
        _mockDependency.Verify(x => x.GetData(), Times.Once);
    }
}
```

## Troubleshooting

### Common Issues

#### "Cannot read properties of undefined (reading 'subscribe')"

**Cause:** RouterLink directive trying to access `router.events` which is undefined.

**Solution:** Add `events: new Subject()` to your Router mock (see Mocking Angular Router section).

#### "this.router.createUrlTree is not a function"

**Cause:** RouterLink directive requires `createUrlTree` method.

**Solution:** Add `createUrlTree: jest.fn().mockReturnValue({})` to your Router mock.

#### "this.router.serializeUrl is not a function"

**Cause:** RouterLink directive requires `serializeUrl` method.

**Solution:** Add `serializeUrl: jest.fn().mockReturnValue('')` to your Router mock.

#### Tests Pass Locally But Fail in CI

- Ensure all dependencies are installed
- Check for timing-sensitive tests (use proper waiting strategies)
- Verify environment variables are set correctly
- Check for hardcoded paths or URLs

## Best Practices

### General Testing Principles

1. **Test behavior, not implementation**: Focus on what the component does, not how it does it
2. **Keep tests simple**: One assertion per test when possible
3. **Use descriptive names**: Test names should describe the expected behavior
4. **Arrange-Act-Assert**: Structure tests with clear setup, action, and verification phases
5. **Avoid test interdependence**: Each test should run independently

### Angular-Specific

1. **Use TestBed for integration tests**: Test components with their dependencies
2. **Mock external dependencies**: Services, HTTP, Router, etc.
3. **Test signals reactivity**: Verify computed values update correctly
4. **Test template interactions**: Use `fixture.detectChanges()` and query elements
5. **Verify lifecycle hooks**: Test `ngOnInit`, `ngOnDestroy`, etc.

### Code Coverage

- Aim for meaningful coverage, not just high percentages
- Focus on critical paths and business logic
- Don't test framework code or simple getters/setters
- Use coverage reports to identify untested edge cases

```bash
# Generate coverage report
npx nx test feature --coverage
```

## Resources

- [Jest Documentation](https://jestjs.io/)
- [Angular Testing Guide](https://angular.dev/guide/testing)
- [Playwright Documentation](https://playwright.dev/)
- [xUnit Documentation](https://xunit.net/)
