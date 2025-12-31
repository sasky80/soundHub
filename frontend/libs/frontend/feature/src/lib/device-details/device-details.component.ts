import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { DeviceService, Device } from '@soundhub/frontend/data-access';
import { TranslatePipe } from '@soundhub/frontend/shared';

@Component({
  selector: 'lib-device-details',
  standalone: true,
  imports: [CommonModule, RouterLink, TranslatePipe],
  templateUrl: './device-details.component.html',
  styleUrl: './device-details.component.scss',
})
export class DeviceDetailsComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly deviceService = inject(DeviceService);

  protected readonly device = signal<Device | null>(null);
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
      },
      error: (err) => {
        this.error.set(err.message || 'Failed to load device');
        this.loading.set(false);
      },
    });
  }

  protected togglePower(): void {
    const d = this.device();
    if (!d || this.powerLoading()) return;

    this.powerLoading.set(true);
    const newState = !d.powerState;

    this.deviceService.setPower(d.id, newState).subscribe({
      next: () => {
        this.device.set({ ...d, powerState: newState });
        this.powerLoading.set(false);
      },
      error: (err) => {
        console.error('Failed to toggle power:', err);
        this.powerLoading.set(false);
      },
    });
  }
}
