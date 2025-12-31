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
    'deviceConfig.networkConfig': 'Network Configuration',
    'deviceConfig.networkMask': 'Network Mask (CIDR)',
    'deviceConfig.saveNetworkMask': 'Save',
    'deviceConfig.discoverDevices': 'Discover Devices',
    'deviceConfig.discovering': 'Discovering...',
    'deviceConfig.discovered': 'Found',
    'deviceConfig.newDevices': 'New',
    'deviceConfig.devicesTitle': 'Devices',
    'deviceConfig.addDevice': 'Add Device',
    'deviceConfig.editDevice': 'Edit Device',
    'deviceConfig.deviceName': 'Device Name',
    'deviceConfig.ipAddress': 'IP Address / Hostname',
    'deviceConfig.vendor': 'Vendor',
    'deviceConfig.capabilities': 'Capabilities',
    'deviceConfig.ping': 'Ping',
    'deviceConfig.edit': 'Edit',
    'deviceConfig.delete': 'Delete',
    'deviceConfig.new': 'NEW',
    'deviceConfig.confirmDelete': 'Confirm Delete',
    'deviceConfig.confirmDeleteMessage': 'Are you sure you want to delete this device? This action cannot be undone.',
    'deviceDetails.title': 'Device Details',
    'deviceDetails.power': 'Power',
    'deviceDetails.powerOn': 'On',
    'deviceDetails.powerOff': 'Off',
    'deviceDetails.back': 'Back',
    'deviceDetails.volume': 'Volume',
    'deviceDetails.mute': 'Mute',
    'deviceDetails.unmute': 'Unmute',
    'deviceDetails.volumeDisabled': 'Volume control unavailable when device is off',
    'common.loading': 'Loading...',
    'common.saving': 'Saving...',
    'common.deleting': 'Deleting...',
    'common.save': 'Save',
    'common.cancel': 'Cancel',
    'common.delete': 'Delete',
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
    'deviceConfig.networkConfig': 'Konfiguracja sieci',
    'deviceConfig.networkMask': 'Maska sieci (CIDR)',
    'deviceConfig.saveNetworkMask': 'Zapisz',
    'deviceConfig.discoverDevices': 'Wykryj urządzenia',
    'deviceConfig.discovering': 'Wyszukiwanie...',
    'deviceConfig.discovered': 'Znaleziono',
    'deviceConfig.newDevices': 'Nowe',
    'deviceConfig.devicesTitle': 'Urządzenia',
    'deviceConfig.addDevice': 'Dodaj urządzenie',
    'deviceConfig.editDevice': 'Edytuj urządzenie',
    'deviceConfig.deviceName': 'Nazwa urządzenia',
    'deviceConfig.ipAddress': 'Adres IP / Nazwa hosta',
    'deviceConfig.vendor': 'Producent',
    'deviceConfig.capabilities': 'Możliwości',
    'deviceConfig.ping': 'Ping',
    'deviceConfig.edit': 'Edytuj',
    'deviceConfig.delete': 'Usuń',
    'deviceConfig.new': 'NOWE',
    'deviceConfig.confirmDelete': 'Potwierdź usunięcie',
    'deviceConfig.confirmDeleteMessage': 'Czy na pewno chcesz usunąć to urządzenie? Tej operacji nie można cofnąć.',
    'deviceDetails.title': 'Szczegóły urządzenia',
    'deviceDetails.power': 'Zasilanie',
    'deviceDetails.powerOn': 'Włączone',
    'deviceDetails.powerOff': 'Wyłączone',
    'deviceDetails.back': 'Powrót',
    'deviceDetails.volume': 'Głośność',
    'deviceDetails.mute': 'Wycisz',
    'deviceDetails.unmute': 'Wyłącz wyciszenie',
    'deviceDetails.volumeDisabled': 'Sterowanie głośnością niedostępne gdy urządzenie jest wyłączone',
    'common.loading': 'Ładowanie...',
    'common.saving': 'Zapisywanie...',
    'common.deleting': 'Usuwanie...',
    'common.save': 'Zapisz',
    'common.cancel': 'Anuluj',
    'common.delete': 'Usuń',
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
