import {
  Component,
  ChangeDetectionStrategy,
  input,
  output,
  signal,
  inject,
  OnInit,
  effect,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { PresetService, Preset, DeviceService } from '@soundhub/frontend/data-access';
import { TranslatePipe } from '@soundhub/frontend/shared';
import { switchMap, of } from 'rxjs';

@Component({
  selector: 'lib-preset-list',
  imports: [CommonModule, TranslatePipe],
  templateUrl: './preset-list.component.html',
  styleUrl: './preset-list.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PresetListComponent implements OnInit {
  private readonly presetService = inject(PresetService);
  private readonly deviceService = inject(DeviceService);
  private readonly router = inject(Router);

  readonly deviceId = input.required<string>();
  readonly isPowerOn = input<boolean>(true);
  readonly presetPlayed = output<number>();

  protected readonly presets = signal<Preset[]>([]);
  protected readonly loading = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly playingPresetId = signal<number | null>(null);

  constructor() {
    effect(() => {
      const id = this.deviceId();
      if (id) {
        this.loadPresets(id);
      }
    });
  }

  ngOnInit(): void {
    // Initial load handled by effect
  }

  private loadPresets(deviceId: string): void {
    this.loading.set(true);
    this.error.set(null);

    this.presetService.getPresets(deviceId).subscribe({
      next: (presets) => {
        this.presets.set(presets);
        this.loading.set(false);
      },
      error: (err) => {
        this.error.set(err.message || 'Failed to load presets');
        this.loading.set(false);
      },
    });
  }

  protected playPreset(preset: Preset): void {
    if (this.playingPresetId()) return;

    this.playingPresetId.set(preset.id);
    const deviceId = this.deviceId();

    // If device is off, power it on first, then play preset
    const playAction$ = this.isPowerOn()
      ? this.presetService.playPreset(deviceId, preset.id)
      : this.deviceService.setPower(deviceId, true).pipe(
          switchMap(() => this.presetService.playPreset(deviceId, preset.id))
        );

    playAction$.subscribe({
      next: () => {
        this.playingPresetId.set(null);
        this.presetPlayed.emit(preset.id);
      },
      error: (err) => {
        console.error('Failed to play preset:', err);
        this.playingPresetId.set(null);
      },
    });
  }

  protected navigateToNewPreset(): void {
    this.router.navigate(['/devices', this.deviceId(), 'presets', 'new']);
  }

  protected navigateToPreset(preset: Preset): void {
    this.router.navigate(['/devices', this.deviceId(), 'presets', preset.id]);
  }

  protected getPresetIcon(preset: Preset): string {
    return preset.iconUrl || '/assets/icons/preset-default.svg';
  }

  protected trackByPresetId(_index: number, preset: Preset): number {
    return preset.id;
  }
}
