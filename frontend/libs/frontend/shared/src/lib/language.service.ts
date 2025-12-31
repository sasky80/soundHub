import { Injectable, signal, computed } from '@angular/core';

export type Language = 'en' | 'pl';

export interface Translations {
  [key: string]: string;
}

const TRANSLATIONS: Record<Language, Translations> = {
  en: {
    'app.title': 'SoundHub',
    'landing.title': 'Your Devices',
    'landing.noDevices': 'No devices configured',
    'landing.settings': 'Settings',
    'settings.title': 'Settings',
    'settings.language': 'Language',
    'settings.languageEn': 'English',
    'settings.languagePl': 'Polish',
    'settings.deviceConfig': 'Device Configuration',
    'settings.back': 'Back',
    'deviceConfig.title': 'Device Configuration',
    'deviceConfig.noDevices': 'No devices configured',
    'deviceDetails.title': 'Device Details',
    'deviceDetails.power': 'Power',
    'deviceDetails.powerOn': 'On',
    'deviceDetails.powerOff': 'Off',
    'deviceDetails.back': 'Back',
    'common.loading': 'Loading...',
    'common.error': 'An error occurred',
  },
  pl: {
    'app.title': 'SoundHub',
    'landing.title': 'Twoje urządzenia',
    'landing.noDevices': 'Brak skonfigurowanych urządzeń',
    'landing.settings': 'Ustawienia',
    'settings.title': 'Ustawienia',
    'settings.language': 'Język',
    'settings.languageEn': 'Angielski',
    'settings.languagePl': 'Polski',
    'settings.deviceConfig': 'Konfiguracja urządzeń',
    'settings.back': 'Powrót',
    'deviceConfig.title': 'Konfiguracja urządzeń',
    'deviceConfig.noDevices': 'Brak skonfigurowanych urządzeń',
    'deviceDetails.title': 'Szczegóły urządzenia',
    'deviceDetails.power': 'Zasilanie',
    'deviceDetails.powerOn': 'Włączone',
    'deviceDetails.powerOff': 'Wyłączone',
    'deviceDetails.back': 'Powrót',
    'common.loading': 'Ładowanie...',
    'common.error': 'Wystąpił błąd',
  },
};

const STORAGE_KEY = 'soundhub-language';

@Injectable({ providedIn: 'root' })
export class LanguageService {
  private readonly currentLanguage = signal<Language>(this.loadLanguage());

  readonly language = this.currentLanguage.asReadonly();

  readonly translations = computed(() => TRANSLATIONS[this.currentLanguage()]);

  setLanguage(lang: Language): void {
    this.currentLanguage.set(lang);
    localStorage.setItem(STORAGE_KEY, lang);
  }

  translate(key: string): string {
    return this.translations()[key] ?? key;
  }

  private loadLanguage(): Language {
    const stored = localStorage.getItem(STORAGE_KEY);
    if (stored === 'en' || stored === 'pl') {
      return stored;
    }
    return 'en';
  }
}
