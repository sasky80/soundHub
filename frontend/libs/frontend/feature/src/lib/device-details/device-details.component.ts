import { Component, inject, OnInit, OnDestroy, signal, ChangeDetectionStrategy, computed, HostBinding } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { DeviceService, Device, DeviceStatus, VolumeInfo, NowPlayingInfo } from '@soundhub/frontend/data-access';
import { TranslatePipe } from '@soundhub/frontend/shared';
import { Subject, interval, debounceTime, distinctUntilChanged, takeUntil, switchMap, startWith } from 'rxjs';
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
  private readonly statusPolling$ = new Subject<void>();
  private readonly POLL_INTERVAL = 10000; // 10 seconds

  @HostBinding('attr.data-lcd-speed')
  protected get lcdSpeed(): string {
    return localStorage.getItem('lcdScrollSpeed') ?? 'medium';
  }

  @HostBinding('attr.data-lcd-theme')
  protected get lcdTheme(): string {
    return localStorage.getItem('lcdColorTheme') ?? 'green';
  }

  protected readonly device = signal<Device | null>(null);
  protected readonly status = signal<DeviceStatus | null>(null);
  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);
  protected readonly powerLoading = signal(false);
  protected readonly volumeInfo = signal<VolumeInfo | null>(null);
  protected readonly volumeLoading = signal(false);
  protected readonly volumeValue = signal(0);
  protected readonly muteLoading = signal(false);
  protected readonly keyLoading = signal<string | null>(null);
  protected readonly pairingLoading = signal(false);
  protected readonly isPlaying = signal(false);
  protected readonly pairingMessage = signal<string | null>(null);
  protected readonly remoteMessage = signal<string | null>(null);
  protected readonly shouldScroll = signal(false);

  protected readonly isPowerOn = computed(() => this.status()?.powerState ?? false);
  protected readonly bluetoothSupported = computed(() =>
    this.device()?.capabilities?.includes('bluetoothPairing') ?? false
  );
  protected readonly isBluetoothActive = computed(() => 
    this.status()?.currentSource === 'BLUETOOTH'
  );
  protected readonly isAuxActive = computed(() => {
    const source = this.status()?.currentSource;
    return source === 'AUX' || source === 'AUX_INPUT';
  });

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.loadDevice(id);
      this.setupVolumeDebounce(id);
    }
  }

  ngOnDestroy(): void {
    this.stopPolling();
    this.destroy$.next();
    this.destroy$.complete();
  }

  private startPolling(deviceId: string): void {
    this.statusPolling$.next(); // Stop any existing polling
    
    interval(this.POLL_INTERVAL)
      .pipe(
        startWith(0), // Immediate first poll
        switchMap(() => this.deviceService.getDeviceStatus(deviceId)),
        takeUntil(this.statusPolling$),
        takeUntil(this.destroy$)
      )
      .subscribe({
        next: (status) => {
          this.status.set(status);
        },
        error: (err) => console.error('Polling error:', err),
      });
  }

  private stopPolling(): void {
    this.statusPolling$.next();
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
        
        // Start polling if device is powered on
        if (status.powerState) {
          this.startPolling(id);
        }
      },
      error: () => {}, // Non-critical, status may not be available
    });
  }

  private loadVolume(id: string, showLoading = true): void {
    if (showLoading) {
      this.volumeLoading.set(true);
    }
    this.deviceService.getVolume(id).subscribe({
      next: (volumeInfo) => {
        this.volumeInfo.set(volumeInfo);
        this.volumeValue.set(volumeInfo.actualVolume);
        if (showLoading) {
          this.volumeLoading.set(false);
        }
      },
      error: () => {
        if (showLoading) {
          this.volumeLoading.set(false);
        }
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
        
        // Start or stop polling based on new power state
        if (newState) {
          this.startPolling(d.id);
        } else {
          this.stopPolling();
        }
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

  protected pressKey(keyName: string): void {
    const d = this.device();
    if (!d || this.keyLoading()) return;

    this.keyLoading.set(keyName);
    this.remoteMessage.set(null);
    this.deviceService.pressKey(d.id, keyName).subscribe({
      next: () => {
        if (keyName === 'PLAY_PAUSE') {
          this.isPlaying.update((current) => !current);
        }
        if (keyName === 'VOLUME_UP' || keyName === 'VOLUME_DOWN') {
          this.loadVolume(d.id, false);
        }
        this.keyLoading.set(null);
      },
      error: (err) => {
        console.error('Failed to send key:', err);
        this.remoteMessage.set('Action failed. Please try again.');
        this.keyLoading.set(null);
      },
    });
  }

  protected isKeyInFlight(key: string): boolean {
    return this.keyLoading() === key;
  }

  protected startBluetoothPairing(): void {
    const d = this.device();
    if (!d || this.pairingLoading()) return;

    this.pairingLoading.set(true);

    this.deviceService.enterBluetoothPairing(d.id).subscribe({
      next: () => {
        this.pairingLoading.set(false);
      },
      error: (err) => {
        console.error('Failed to start Bluetooth pairing:', err);
        this.pairingLoading.set(false);
      },
    });
  }

  protected formatNowPlaying(nowPlaying: NowPlayingInfo): string {
    const parts: string[] = [];
    
    if (nowPlaying.stationName) {
      parts.push(nowPlaying.stationName);
    }
    
    const trackInfo: string[] = [];
    if (nowPlaying.artist) {
      trackInfo.push(nowPlaying.artist);
    }
    if (nowPlaying.track) {
      trackInfo.push(nowPlaying.track);
    }
    
    if (trackInfo.length > 0) {
      if (parts.length > 0) {
        parts.push(': ');
      }
      parts.push(trackInfo.join(', '));
    }
    
    return parts.length > 0 ? parts.join('') : '---';
  }

  protected onPresetPowerStateChanged(newState: boolean): void {
    const currentStatus = this.status();
    if (!currentStatus || currentStatus.powerState === newState) {
      return;
    }

    this.status.set({ ...currentStatus, powerState: newState });
  }
}
