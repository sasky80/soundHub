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
    'presets.title': 'Presets',
    'presets.play': 'Play',
    'presets.edit': 'Edit',
    'presets.addNew': 'Add Preset',
    'presets.powerOffHint': 'Turn on the device to play presets',    'presets.willPowerOn': 'Will turn on device and play',    'presetForm.back': 'Back to Device',
    'presetForm.createTitle': 'New Preset',
    'presetForm.editTitle': 'Edit Preset',
    'presetForm.slot': 'Preset Slot',
    'presetForm.slotNumber': 'Slot',
    'presetForm.slotRequired': 'Please select a preset slot',
    'presetForm.name': 'Name',
    'presetForm.nameRequired': 'Please enter a preset name',
    'presetForm.location': 'Stream URL',
    'presetForm.locationRequired': 'Please enter a stream URL',
    'presetForm.locationHint': 'The URL of the audio stream or station',
    'presetForm.iconUrl': 'Icon URL',
    'presetForm.iconUrlHint': 'Optional URL for the preset icon (64x64 recommended)',
    'presetForm.advancedOptions': 'Advanced Options (SoundTouch)',
    'presetForm.type': 'Type',
    'presetForm.typeHint': 'Content type (e.g., stationurl, uri)',
    'presetForm.source': 'Source',
    'presetForm.sourceHint': 'Source identifier (e.g., LOCAL_INTERNET_RADIO)',
    'presetForm.deleteConfirmTitle': 'Delete Preset?',
    'presetForm.deleteConfirmMessage': 'Are you sure you want to delete this preset? This action cannot be undone.',
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
    'presets.title': 'Presety',
    'presets.play': 'Odtwórz',
    'presets.edit': 'Edytuj',
    'presets.addNew': 'Dodaj preset',
    'presets.powerOffHint': 'Włącz urządzenie, aby odtwarzać presety',
    'presets.willPowerOn': 'Włączy urządzenie i odtworzy',
    'presetForm.back': 'Powrót do urządzenia',
    'presetForm.createTitle': 'Nowy preset',
    'presetForm.editTitle': 'Edytuj preset',
    'presetForm.slot': 'Slot presetu',
    'presetForm.slotNumber': 'Slot',
    'presetForm.slotRequired': 'Wybierz slot presetu',
    'presetForm.name': 'Nazwa',
    'presetForm.nameRequired': 'Podaj nazwę presetu',
    'presetForm.location': 'URL strumienia',
    'presetForm.locationRequired': 'Podaj URL strumienia',
    'presetForm.locationHint': 'URL strumienia audio lub stacji',
    'presetForm.iconUrl': 'URL ikony',
    'presetForm.iconUrlHint': 'Opcjonalny URL ikony presetu (zalecane 64x64)',
    'presetForm.advancedOptions': 'Opcje zaawansowane (SoundTouch)',
    'presetForm.type': 'Typ',
    'presetForm.typeHint': 'Typ zawartości (np. stationurl, uri)',
    'presetForm.source': 'Źródło',
    'presetForm.sourceHint': 'Identyfikator źródła (np. LOCAL_INTERNET_RADIO)',
    'presetForm.deleteConfirmTitle': 'Usunąć preset?',
    'presetForm.deleteConfirmMessage': 'Czy na pewno chcesz usunąć ten preset? Tej operacji nie można cofnąć.',
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
