import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { SettingsComponent } from './settings.component';

describe('SettingsComponent', () => {
  let component: SettingsComponent;
  let fixture: ComponentFixture<SettingsComponent>;

  beforeEach(async () => {
    localStorage.clear();
    
    await TestBed.configureTestingModule({
      imports: [SettingsComponent],
      providers: [provideRouter([])],
    }).compileComponents();

    fixture = TestBed.createComponent(SettingsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  afterEach(() => {
    localStorage.clear();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should display language options', () => {
    const buttons = fixture.nativeElement.querySelectorAll('.language-btn');
    expect(buttons.length).toBe(2);
  });

  describe('LCD scroll speed settings', () => {
    it('should default to medium speed', () => {
      expect(component['scrollSpeed']()).toBe('medium');
    });

    it('should set scroll speed to slow', () => {
      component['setScrollSpeed']('slow');
      expect(component['scrollSpeed']()).toBe('slow');
    });

    it('should set scroll speed to fast', () => {
      component['setScrollSpeed']('fast');
      expect(component['scrollSpeed']()).toBe('fast');
    });

    it('should persist scroll speed to localStorage', () => {
      component['setScrollSpeed']('slow');
      expect(localStorage.getItem('lcdScrollSpeed')).toBe('slow');
    });

    it('should load scroll speed from localStorage', () => {
      localStorage.setItem('lcdScrollSpeed', 'fast');
      const newComponent = new SettingsComponent();
      expect(newComponent['scrollSpeed']()).toBe('fast');
    });

    it('should show active state on selected speed button', () => {
      component['setScrollSpeed']('slow');
      fixture.detectChanges();

      const slowBtn = Array.from(fixture.nativeElement.querySelectorAll('.option-btn'))
        .find((btn: any) => btn.textContent?.includes('Slow')) as HTMLElement;
      
      expect(slowBtn?.classList.contains('active')).toBe(true);
    });

    it('should apply speed to LCD preview via data attribute', () => {
      component['setScrollSpeed']('fast');
      fixture.detectChanges();

      const preview = fixture.nativeElement.querySelector('.lcd-display-preview');
      expect(preview?.getAttribute('data-lcd-speed')).toBe('fast');
    });
  });

  describe('LCD color theme settings', () => {
    it('should default to green theme', () => {
      expect(component['colorTheme']()).toBe('green');
    });

    it('should set color theme to amber', () => {
      component['setColorTheme']('amber');
      expect(component['colorTheme']()).toBe('amber');
    });

    it('should set color theme to blue', () => {
      component['setColorTheme']('blue');
      expect(component['colorTheme']()).toBe('blue');
    });

    it('should persist color theme to localStorage', () => {
      component['setColorTheme']('amber');
      expect(localStorage.getItem('lcdColorTheme')).toBe('amber');
    });

    it('should load color theme from localStorage', () => {
      localStorage.setItem('lcdColorTheme', 'blue');
      const newComponent = new SettingsComponent();
      expect(newComponent['colorTheme']()).toBe('blue');
    });

    it('should show active state on selected theme button', () => {
      component['setColorTheme']('amber');
      fixture.detectChanges();

      const amberBtn = Array.from(fixture.nativeElement.querySelectorAll('.option-btn'))
        .find((btn: any) => btn.textContent?.includes('Amber')) as HTMLElement;
      
      expect(amberBtn?.classList.contains('active')).toBe(true);
    });

    it('should apply theme to LCD preview via data attribute', () => {
      component['setColorTheme']('blue');
      fixture.detectChanges();

      const preview = fixture.nativeElement.querySelector('.lcd-display-preview');
      expect(preview?.getAttribute('data-lcd-theme')).toBe('blue');
    });

    it('should display color swatches for each theme', () => {
      const swatches = fixture.nativeElement.querySelectorAll('.color-swatch');
      expect(swatches.length).toBe(3);
      
      const classes = Array.from(swatches).map((s: any) => 
        Array.from(s.classList).find(c => c === 'green' || c === 'amber' || c === 'blue')
      );
      expect(classes).toContain('green');
      expect(classes).toContain('amber');
      expect(classes).toContain('blue');
    });
  });

  describe('LCD preview', () => {
    it('should display LCD preview', () => {
      const preview = fixture.nativeElement.querySelector('.lcd-display-preview');
      expect(preview).toBeTruthy();
    });

    it('should show sample text in preview', () => {
      const lcdText = fixture.nativeElement.querySelector('.lcd-display-preview .lcd-text');
      expect(lcdText?.textContent).toContain('KINK FM');
    });
  });
});
