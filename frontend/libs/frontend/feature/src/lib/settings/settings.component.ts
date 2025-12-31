import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { LanguageService, Language, TranslatePipe } from '@soundhub/frontend/shared';

@Component({
  selector: 'lib-settings',
  standalone: true,
  imports: [CommonModule, RouterLink, TranslatePipe],
  templateUrl: './settings.component.html',
  styleUrl: './settings.component.scss',
})
export class SettingsComponent {
  protected readonly lang = inject(LanguageService);

  protected setLanguage(language: Language): void {
    this.lang.setLanguage(language);
  }
}
