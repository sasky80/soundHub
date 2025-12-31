import { Component, inject, OnInit, signal, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { DeviceService, Device } from '@soundhub/frontend/data-access';
import { LanguageService, TranslatePipe } from '@soundhub/frontend/shared';

@Component({
  selector: 'lib-landing',
  imports: [CommonModule, RouterLink, TranslatePipe],
  templateUrl: './landing.component.html',
  styleUrl: './landing.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LandingComponent implements OnInit {
  private readonly deviceService = inject(DeviceService);
  protected readonly lang = inject(LanguageService);

  protected readonly devices = signal<Device[]>([]);
  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);

  ngOnInit(): void {
    this.loadDevices();
  }

  private loadDevices(): void {
    this.loading.set(true);
    this.error.set(null);

    this.deviceService.getDevices().subscribe({
      next: (devices) => {
        this.devices.set(devices);
        this.loading.set(false);
      },
      error: (err) => {
        this.error.set(err.message || 'Failed to load devices');
        this.loading.set(false);
      },
    });
  }
}
