import {
  Component,
  inject,
  OnInit,
  signal,
  computed,
  ChangeDetectionStrategy,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormsModule, ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import {
  DeviceService,
  Device,
  PingResult,
  VendorInfo,
  CreateDeviceRequest,
  UpdateDeviceRequest,
} from '@soundhub/frontend/data-access';
import { TranslatePipe } from '@soundhub/frontend/shared';

interface DeviceWithPingState extends Device {
  pingState: 'idle' | 'pinging' | 'success' | 'error';
  pingLatency?: number;
}

@Component({
  selector: 'lib-device-config',
  imports: [CommonModule, RouterLink, TranslatePipe, FormsModule, ReactiveFormsModule],
  templateUrl: './device-config.component.html',
  styleUrl: './device-config.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DeviceConfigComponent implements OnInit {
  private readonly deviceService = inject(DeviceService);
  private readonly fb = inject(FormBuilder);

  protected readonly devices = signal<DeviceWithPingState[]>([]);
  protected readonly vendors = signal<VendorInfo[]>([]);
  protected readonly networkMask = signal('');
  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);
  protected readonly discovering = signal(false);
  protected readonly discoveryResult = signal<{ discovered: number; new: number } | null>(null);
  protected readonly savingNetworkMask = signal(false);
  protected readonly showDeviceForm = signal(false);
  protected readonly editingDevice = signal<Device | null>(null);
  protected readonly savingDevice = signal(false);
  protected readonly deletingDeviceId = signal<string | null>(null);
  protected readonly showDeleteConfirm = signal<string | null>(null);
  protected readonly selectedCapabilities = signal<boolean[]>([false, false, false, false, false]);

  protected readonly deviceForm = this.fb.group({
    name: ['', [Validators.required, Validators.minLength(1)]],
    ipAddress: ['', [Validators.required]],
    vendor: ['', [Validators.required]],
  });

  protected readonly availableCapabilities = ['power', 'volume', 'presets', 'bluetoothPairing', 'ping'];

  protected readonly isNewDevice = computed(() => this.editingDevice() === null && this.showDeviceForm());

  protected readonly newDevices = computed(() => {
    const fiveMinutesAgo = new Date(Date.now() - 5 * 60 * 1000);
    return this.devices().filter((d) => new Date(d.dateTimeAdded) > fiveMinutesAgo);
  });

  ngOnInit(): void {
    this.loadData();
  }

  private loadData(): void {
    this.loading.set(true);
    this.error.set(null);

    this.deviceService.getDevices().subscribe({
      next: (devices) => {
        this.devices.set(
          devices.map((d) => ({
            ...d,
            pingState: 'idle' as const,
          }))
        );
        this.loading.set(false);
      },
      error: (err) => {
        this.error.set(err.message || 'Failed to load devices');
        this.loading.set(false);
      },
    });

    this.deviceService.getVendors().subscribe({
      next: (vendors) => this.vendors.set(vendors),
      error: () => {}, // Non-critical
    });

    this.deviceService.getNetworkMask().subscribe({
      next: (res) => this.networkMask.set(res.networkMask),
      error: () => {}, // Non-critical
    });
  }

  protected pingDevice(device: DeviceWithPingState): void {
    this.updateDevicePingState(device.id, 'pinging');

    this.deviceService.pingDevice(device.id).subscribe({
      next: (result: PingResult) => {
        this.updateDevicePingState(device.id, result.reachable ? 'success' : 'error', result.latencyMs);
        setTimeout(() => this.updateDevicePingState(device.id, 'idle'), 3000);
      },
      error: () => {
        this.updateDevicePingState(device.id, 'error');
        setTimeout(() => this.updateDevicePingState(device.id, 'idle'), 3000);
      },
    });
  }

  private updateDevicePingState(
    deviceId: string,
    state: 'idle' | 'pinging' | 'success' | 'error',
    latency?: number
  ): void {
    this.devices.update((devices) =>
      devices.map((d) =>
        d.id === deviceId ? { ...d, pingState: state, pingLatency: latency } : d
      )
    );
  }

  protected discoverDevices(): void {
    this.discovering.set(true);
    this.discoveryResult.set(null);

    this.deviceService.discoverDevices().subscribe({
      next: (result) => {
        this.discoveryResult.set({ discovered: result.discovered, new: result.new });
        this.discovering.set(false);
        if (result.new > 0) {
          this.loadData(); // Reload to show new devices
        }
      },
      error: (err) => {
        this.error.set(err.message || 'Discovery failed');
        this.discovering.set(false);
      },
    });
  }

  protected saveNetworkMask(): void {
    const mask = this.networkMask().trim();
    if (!mask) return;

    this.savingNetworkMask.set(true);

    this.deviceService.updateNetworkMask(mask).subscribe({
      next: () => this.savingNetworkMask.set(false),
      error: (err) => {
        this.error.set(err.message || 'Failed to save network mask');
        this.savingNetworkMask.set(false);
      },
    });
  }

  protected openAddDevice(): void {
    this.editingDevice.set(null);
    this.selectedCapabilities.set(this.availableCapabilities.map(() => false));
    this.deviceForm.reset({
      name: '',
      ipAddress: '',
      vendor: this.vendors()[0]?.id || '',
    });
    this.showDeviceForm.set(true);
  }

  protected openEditDevice(device: Device): void {
    this.editingDevice.set(device);
    this.selectedCapabilities.set(
      this.availableCapabilities.map((cap) => device.capabilities.includes(cap))
    );
    this.deviceForm.patchValue({
      name: device.name,
      ipAddress: device.ipAddress,
      vendor: device.vendor,
    });
    this.showDeviceForm.set(true);
  }

  protected closeDeviceForm(): void {
    this.showDeviceForm.set(false);
    this.editingDevice.set(null);
  }

  protected saveDevice(): void {
    if (this.deviceForm.invalid) return;

    this.savingDevice.set(true);
    const formValue = this.deviceForm.value;

    const selectedCapabilities = this.availableCapabilities.filter(
      (_, i) => this.selectedCapabilities()[i]
    );

    if (this.editingDevice()) {
      const request: UpdateDeviceRequest = {
        name: formValue.name || undefined,
        ipAddress: formValue.ipAddress || undefined,
        vendor: formValue.vendor || undefined,
        capabilities: selectedCapabilities.length > 0 ? selectedCapabilities : undefined,
      };

      this.deviceService.updateDevice(this.editingDevice()!.id, request).subscribe({
        next: () => {
          this.savingDevice.set(false);
          this.closeDeviceForm();
          this.loadData();
        },
        error: (err) => {
          this.error.set(err.message || 'Failed to update device');
          this.savingDevice.set(false);
        },
      });
    } else {
      const request: CreateDeviceRequest = {
        name: formValue.name || '',
        ipAddress: formValue.ipAddress || '',
        vendor: formValue.vendor || '',
        capabilities: selectedCapabilities.length > 0 ? selectedCapabilities : undefined,
      };

      this.deviceService.createDevice(request).subscribe({
        next: () => {
          this.savingDevice.set(false);
          this.closeDeviceForm();
          this.loadData();
        },
        error: (err) => {
          this.error.set(err.message || 'Failed to create device');
          this.savingDevice.set(false);
        },
      });
    }
  }

  protected confirmDeleteDevice(deviceId: string): void {
    this.showDeleteConfirm.set(deviceId);
  }

  protected cancelDelete(): void {
    this.showDeleteConfirm.set(null);
  }

  protected deleteDevice(deviceId: string): void {
    this.deletingDeviceId.set(deviceId);

    this.deviceService.deleteDevice(deviceId).subscribe({
      next: () => {
        this.deletingDeviceId.set(null);
        this.showDeleteConfirm.set(null);
        this.loadData();
      },
      error: (err) => {
        this.error.set(err.message || 'Failed to delete device');
        this.deletingDeviceId.set(null);
        this.showDeleteConfirm.set(null);
      },
    });
  }

  protected isNewlyAdded(device: Device): boolean {
    const fiveMinutesAgo = new Date(Date.now() - 5 * 60 * 1000);
    return new Date(device.dateTimeAdded) > fiveMinutesAgo;
  }

  protected hasPingCapability(device: Device): boolean {
    return device.capabilities.includes('ping');
  }

  protected isCapabilitySelected(index: number): boolean {
    return this.selectedCapabilities()[index] ?? false;
  }

  protected toggleCapability(index: number): void {
    this.selectedCapabilities.update((caps) => {
      const newCaps = [...caps];
      newCaps[index] = !newCaps[index];
      return newCaps;
    });
  }

  protected getDeviceCapabilities(): boolean[] {
    if (!this.editingDevice()) return [];
    return this.availableCapabilities.map((cap) =>
      this.editingDevice()!.capabilities.includes(cap)
    );
  }
}
