import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { LanguageService, Language, TranslatePipe } from '@soundhub/frontend/shared';

type LcdScrollSpeed = 'slow' | 'medium' | 'fast';
type LcdColorTheme = 'green' | 'amber' | 'blue';

@Component({
  selector: 'lib-settings',
  standalone: true,
  imports: [CommonModule, RouterLink, TranslatePipe],
  templateUrl: './settings.component.html',
  styleUrl: './settings.component.scss',
})
export class SettingsComponent {
  protected readonly lang = inject(LanguageService);
  
  protected readonly scrollSpeed = signal<LcdScrollSpeed>(this.getScrollSpeed());
  protected readonly colorTheme = signal<LcdColorTheme>(this.getColorTheme());

  protected setLanguage(language: Language): void {
    this.lang.setLanguage(language);
  }

  protected setScrollSpeed(speed: LcdScrollSpeed): void {
    this.scrollSpeed.set(speed);
    localStorage.setItem('lcdScrollSpeed', speed);
  }

  protected setColorTheme(theme: LcdColorTheme): void {
    this.colorTheme.set(theme);
    localStorage.setItem('lcdColorTheme', theme);
  }

  private getScrollSpeed(): LcdScrollSpeed {
    const stored = localStorage.getItem('lcdScrollSpeed');
    return (stored as LcdScrollSpeed) || 'medium';
  }

  private getColorTheme(): LcdColorTheme {
    const stored = localStorage.getItem('lcdColorTheme');
    return (stored as LcdColorTheme) || 'green';
  }
}
