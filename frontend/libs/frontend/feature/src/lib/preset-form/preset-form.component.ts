import {
  Component,
  ChangeDetectionStrategy,
  inject,
  OnInit,
  signal,
  computed,
  effect,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { PresetService, Preset, StorePresetRequest } from '@soundhub/frontend/data-access';
import { TranslatePipe } from '@soundhub/frontend/shared';

@Component({
  selector: 'lib-preset-form',
  imports: [CommonModule, RouterLink, ReactiveFormsModule, TranslatePipe],
  templateUrl: './preset-form.component.html',
  styleUrl: './preset-form.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PresetFormComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly fb = inject(FormBuilder);
  private readonly presetService = inject(PresetService);

  protected readonly deviceId = signal<string>('');
  protected readonly presetId = signal<number | null>(null);
  protected readonly isEditMode = computed(() => this.presetId() !== null);
  protected readonly loading = signal(false);
  protected readonly saving = signal(false);
  protected readonly deleting = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly showDeleteConfirm = signal(false);
  protected readonly allPresets = signal<Preset[]>([]);

  /** Whether the current source is LOCAL_INTERNET_RADIO (controls field visibility). */
  protected readonly isLocalRadio = signal(true);

  protected readonly form = this.fb.group({
    id: [1, [Validators.required, Validators.min(1), Validators.max(6)]],
    name: ['', [Validators.required, Validators.maxLength(100)]],
    location: [''],
    streamUrl: [''],
    iconUrl: [''],
    type: ['stationurl'],
    source: ['LOCAL_INTERNET_RADIO'],
  });

  // Available preset slots (1-6 for SoundTouch)
  protected readonly presetSlots = [1, 2, 3, 4, 5, 6];

  constructor() {
    // React to source field changes to toggle LOCAL_INTERNET_RADIO mode
    effect(() => {
      // This effect runs on init; actual form value changes are handled via valueChanges subscription
    });
  }

  ngOnInit(): void {
    const deviceId = this.route.snapshot.paramMap.get('id');
    const presetIdParam = this.route.snapshot.paramMap.get('presetId');

    if (deviceId) {
      this.deviceId.set(deviceId);
      // Load all presets to show slot usage
      this.loadAllPresets(deviceId);
    }

    if (presetIdParam && presetIdParam !== 'new') {
      const presetId = parseInt(presetIdParam, 10);
      if (!isNaN(presetId)) {
        this.presetId.set(presetId);
        this.loadPreset(deviceId!, presetId);
      }
    }

    // Subscribe to source field changes to toggle field visibility and validators
    this.form.get('source')!.valueChanges.subscribe((source) => {
      this.updateSourceMode(source);
    });

    // Apply initial state
    this.updateSourceMode(this.form.get('source')!.value);
  }

  /** Updates form validators and visibility based on whether source is LOCAL_INTERNET_RADIO. */
  private updateSourceMode(source: string | null): void {
    const localRadio = source?.toUpperCase() === 'LOCAL_INTERNET_RADIO';
    this.isLocalRadio.set(localRadio);

    const locationCtrl = this.form.get('location')!;
    const streamUrlCtrl = this.form.get('streamUrl')!;

    if (localRadio) {
      // Stream URL required (must start with http://)
      streamUrlCtrl.setValidators([Validators.required, Validators.pattern(/^https?:\/\/.+/)]);
      locationCtrl.clearValidators();
      locationCtrl.setValue('');
    } else {
      // Location required
      locationCtrl.setValidators([Validators.required]);
      streamUrlCtrl.clearValidators();
      streamUrlCtrl.setValue('');
    }

    locationCtrl.updateValueAndValidity();
    streamUrlCtrl.updateValueAndValidity();
  }

  private loadAllPresets(deviceId: string): void {
    this.presetService.getPresets(deviceId).subscribe({
      next: (presets) => {
        this.allPresets.set(presets);
      },
      error: (err) => {
        console.error('Failed to load presets for slot labels:', err);
      },
    });
  }

  private loadPreset(deviceId: string, presetId: number): void {
    this.loading.set(true);
    this.error.set(null);

    this.presetService.getPresets(deviceId).subscribe({
      next: (presets) => {
        this.allPresets.set(presets);
        const preset = presets.find((p) => p.id === presetId);
        if (preset) {
          this.form.patchValue({
            id: preset.id,
            name: preset.name,
            location: preset.location,
            iconUrl: preset.iconUrl || '',
            type: preset.type,
            source: preset.source,
          });
          // Disable ID field when editing
          this.form.get('id')?.disable();

          // For LOCAL_INTERNET_RADIO presets, try to fetch the stream URL from the station file
          if (preset.source?.toUpperCase() === 'LOCAL_INTERNET_RADIO' && preset.location) {
            this.loadStationFileStreamUrl(preset.location);
          }
        } else {
          this.error.set('Preset not found');
        }
        this.loading.set(false);
      },
      error: (err) => {
        this.error.set(err.message || 'Failed to load preset');
        this.loading.set(false);
      },
    });
  }

  /** Extracts the station filename from a location URL and fetches the stream URL. */
  private loadStationFileStreamUrl(location: string): void {
    try {
      // Extract filename from URL, e.g., "http://host/presets/jazz-fm.json" → "jazz-fm.json"
      const url = new URL(location);
      const pathParts = url.pathname.split('/');
      const filename = pathParts[pathParts.length - 1];

      if (filename && filename.endsWith('.json')) {
        this.presetService.getStationFile(filename).subscribe({
          next: (stationFile) => {
            if (stationFile?.audio?.streamUrl) {
              this.form.patchValue({ streamUrl: stationFile.audio.streamUrl });
            }
          },
          error: () => {
            // Station file not found — leave streamUrl empty, fall back to location
            console.warn('Could not load station file for pre-population:', filename);
          },
        });
      }
    } catch {
      // location is not a valid URL — ignore
    }
  }

  protected getSlotLabel(slotNumber: number): string {
    const preset = this.allPresets().find(p => p.id === slotNumber);
    if (preset) {
      return `Slot ${slotNumber} - ${preset.name}`;
    }
    return `Slot ${slotNumber} - Empty`;
  }

  protected onSubmit(): void {
    if (this.form.invalid || this.saving()) return;

    this.saving.set(true);
    this.error.set(null);

    const formValue = this.form.getRawValue();
    const request: StorePresetRequest = {
      id: formValue.id!,
      name: formValue.name!,
      iconUrl: formValue.iconUrl || undefined,
      type: formValue.type || 'stationurl',
      source: formValue.source || 'LOCAL_INTERNET_RADIO',
      isUpdate: this.isEditMode(),
    };

    if (this.isLocalRadio()) {
      request.streamUrl = formValue.streamUrl || undefined;
      // Location is derived by the backend
    } else {
      request.location = formValue.location || undefined;
    }

    this.presetService.storePreset(this.deviceId(), request).subscribe({
      next: () => {
        this.saving.set(false);
        this.navigateBack();
      },
      error: (err) => {
        if (err.status === 409) {
          this.error.set(err.error?.message || 'A station with this name already exists. Use a different name or edit the existing preset.');
        } else {
          this.error.set(err.error?.message || err.message || 'Failed to save preset');
        }
        this.saving.set(false);
      },
    });
  }

  protected confirmDelete(): void {
    this.showDeleteConfirm.set(true);
  }

  protected cancelDelete(): void {
    this.showDeleteConfirm.set(false);
  }

  protected onDelete(): void {
    const presetId = this.presetId();
    if (!presetId || this.deleting()) return;

    this.deleting.set(true);
    this.error.set(null);
    this.showDeleteConfirm.set(false);

    this.presetService.deletePreset(this.deviceId(), presetId).subscribe({
      next: () => {
        this.deleting.set(false);
        this.navigateBack();
      },
      error: (err) => {
        this.error.set(err.error?.message || err.message || 'Failed to delete preset');
        this.deleting.set(false);
      },
    });
  }

  protected navigateBack(): void {
    this.router.navigate(['/devices', this.deviceId()]);
  }
}
