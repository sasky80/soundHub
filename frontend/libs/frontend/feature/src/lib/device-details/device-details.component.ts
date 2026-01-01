import { Component, inject, OnInit, OnDestroy, signal, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { DeviceService, Device, DeviceStatus, VolumeInfo } from '@soundhub/frontend/data-access';
import { TranslatePipe } from '@soundhub/frontend/shared';
import { Subject, debounceTime, distinctUntilChanged, takeUntil } from 'rxjs';
import { PresetListComponent } from './preset-list.component';

@Component({
  selector: 'lib-device-details',
  imports: [CommonModule, RouterLink, TranslatePipe, PresetListComponent],
  templateUrl: './device-details.component.html',
  styleUrl: './device-details.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DeviceDetailsComponent implements OnInit, OnDestroy {
  private readonly route = inject(ActivatedRoute);
  private readonly deviceService = inject(DeviceService);
  private readonly destroy$ = new Subject<void>();
  private readonly volumeChange$ = new Subject<number>();

  protected readonly device = signal<Device | null>(null);
  protected readonly status = signal<DeviceStatus | null>(null);
  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);
  protected readonly powerLoading = signal(false);
  protected readonly volumeInfo = signal<VolumeInfo | null>(null);
  protected readonly volumeLoading = signal(false);
  protected readonly volumeValue = signal(0);
  protected readonly muteLoading = signal(false);

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.loadDevice(id);
      this.setupVolumeDebounce(id);
    }
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private setupVolumeDebounce(deviceId: string): void {
    this.volumeChange$
      .pipe(debounceTime(300), distinctUntilChanged(), takeUntil(this.destroy$))
      .subscribe((level) => {
        this.deviceService.setVolume(deviceId, level).subscribe({
          next: () => {
            const current = this.volumeInfo();
            if (current) {
              this.volumeInfo.set({ ...current, targetVolume: level, actualVolume: level });
            }
          },
          error: (err) => console.error('Failed to set volume:', err),
        });
      });
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
      next: (status) => {
        this.status.set(status);
        this.loadVolume(id);
      },
      error: () => {}, // Non-critical, status may not be available
    });
  }

  private loadVolume(id: string): void {
    this.volumeLoading.set(true);
    this.deviceService.getVolume(id).subscribe({
      next: (volumeInfo) => {
        this.volumeInfo.set(volumeInfo);
        this.volumeValue.set(volumeInfo.actualVolume);
        this.volumeLoading.set(false);
      },
      error: () => {
        this.volumeLoading.set(false);
      },
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

  protected onVolumeInput(event: Event): void {
    const input = event.target as HTMLInputElement;
    const level = parseInt(input.value, 10);
    this.volumeValue.set(level);
    this.volumeChange$.next(level);
  }

  protected toggleMute(): void {
    const d = this.device();
    if (!d || this.muteLoading()) return;

    this.muteLoading.set(true);
    this.deviceService.toggleMute(d.id).subscribe({
      next: () => {
        this.loadVolume(d.id);
        this.muteLoading.set(false);
      },
      error: (err) => {
        console.error('Failed to toggle mute:', err);
        this.muteLoading.set(false);
      },
    });
  }
}
