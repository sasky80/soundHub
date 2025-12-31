import { Component, inject, OnInit, signal, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { DeviceService, Device, DeviceStatus } from '@soundhub/frontend/data-access';
import { TranslatePipe } from '@soundhub/frontend/shared';

@Component({
  selector: 'lib-device-details',
  imports: [CommonModule, RouterLink, TranslatePipe],
  templateUrl: './device-details.component.html',
  styleUrl: './device-details.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DeviceDetailsComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly deviceService = inject(DeviceService);

  protected readonly device = signal<Device | null>(null);
  protected readonly status = signal<DeviceStatus | null>(null);
  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);
  protected readonly powerLoading = signal(false);

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.loadDevice(id);
    }
  }

  private loadDevice(id: string): void {
    this.loading.set(true);
    this.error.set(null);

    this.deviceService.getDevice(id).subscribe({
      next: (device) => {
        this.device.set(device);
        this.loading.set(false);
        this.loadStatus(id);
      },
      error: (err) => {
        this.error.set(err.message || 'Failed to load device');
        this.loading.set(false);
      },
    });
  }

  private loadStatus(id: string): void {
    this.deviceService.getDeviceStatus(id).subscribe({
      next: (status) => this.status.set(status),
      error: () => {}, // Non-critical, status may not be available
    });
  }

  protected togglePower(): void {
    const d = this.device();
    const s = this.status();
    if (!d || !s || this.powerLoading()) return;

    this.powerLoading.set(true);
    const newState = !s.powerState;

    this.deviceService.setPower(d.id, newState).subscribe({
      next: () => {
        this.status.set({ ...s, powerState: newState });
        this.powerLoading.set(false);
      },
      error: (err) => {
        console.error('Failed to toggle power:', err);
        this.powerLoading.set(false);
      },
    });
  }
}
