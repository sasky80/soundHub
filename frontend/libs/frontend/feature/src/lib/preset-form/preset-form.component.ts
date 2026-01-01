import {
  Component,
  ChangeDetectionStrategy,
  inject,
  OnInit,
  signal,
  computed,
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

  protected readonly form = this.fb.group({
    id: [1, [Validators.required, Validators.min(1), Validators.max(6)]],
    name: ['', [Validators.required, Validators.maxLength(100)]],
    location: ['', [Validators.required]],
    iconUrl: [''],
    type: ['stationurl'],
    source: ['LOCAL_INTERNET_RADIO'],
  });

  // Available preset slots (1-6 for SoundTouch)
  protected readonly presetSlots = [1, 2, 3, 4, 5, 6];

  ngOnInit(): void {
    const deviceId = this.route.snapshot.paramMap.get('id');
    const presetIdParam = this.route.snapshot.paramMap.get('presetId');

    if (deviceId) {
      this.deviceId.set(deviceId);
    }

    if (presetIdParam && presetIdParam !== 'new') {
      const presetId = parseInt(presetIdParam, 10);
      if (!isNaN(presetId)) {
        this.presetId.set(presetId);
        this.loadPreset(deviceId!, presetId);
      }
    }
  }

  private loadPreset(deviceId: string, presetId: number): void {
    this.loading.set(true);
    this.error.set(null);

    this.presetService.getPresets(deviceId).subscribe({
      next: (presets) => {
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

  protected onSubmit(): void {
    if (this.form.invalid || this.saving()) return;

    this.saving.set(true);
    this.error.set(null);

    const formValue = this.form.getRawValue();
    const request: StorePresetRequest = {
      id: formValue.id!,
      name: formValue.name!,
      location: formValue.location!,
      iconUrl: formValue.iconUrl || undefined,
      type: formValue.type || 'stationurl',
      source: formValue.source || 'LOCAL_INTERNET_RADIO',
    };

    this.presetService.storePreset(this.deviceId(), request).subscribe({
      next: () => {
        this.saving.set(false);
        this.navigateBack();
      },
      error: (err) => {
        this.error.set(err.error?.message || err.message || 'Failed to save preset');
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
