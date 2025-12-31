import { TestBed } from '@angular/core/testing';
import { LanguageService } from './language.service';

describe('LanguageService', () => {
  let service: LanguageService;

  beforeEach(() => {
    localStorage.clear();
    TestBed.configureTestingModule({});
    service = TestBed.inject(LanguageService);
  });

  afterEach(() => {
    localStorage.clear();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should default to English', () => {
    expect(service.language()).toBe('en');
  });

  it('should translate keys', () => {
    const title = service.translate('app.title');
    expect(title).toBe('SoundHub');
  });

  it('should switch to Polish', () => {
    service.setLanguage('pl');
    expect(service.language()).toBe('pl');
    expect(service.translate('landing.title')).toBe('Twoje urządzenia');
  });

  it('should persist language to localStorage', () => {
    service.setLanguage('pl');
    expect(localStorage.getItem('soundhub-language')).toBe('pl');
  });

  it('should return key for unknown translations', () => {
    const unknown = service.translate('unknown.key');
    expect(unknown).toBe('unknown.key');
  });
});
