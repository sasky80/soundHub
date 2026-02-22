import { ComponentFixture, TestBed } from '@angular/core/testing';
import { PresetFormComponent } from './preset-form.component';
import { PresetService } from '@soundhub/frontend/data-access';
import { ActivatedRoute, Router } from '@angular/router';
import { ReactiveFormsModule } from '@angular/forms';
import { of, throwError, Subject } from 'rxjs';

describe('PresetFormComponent', () => {
  let component: PresetFormComponent;
  let fixture: ComponentFixture<PresetFormComponent>;
  let mockPresetService: jest.Mocked<PresetService>;
  let mockRouter: jest.Mocked<Router>;
  let mockActivatedRoute: Partial<ActivatedRoute>;

  const mockPresets = [
    {
      id: 1,
      deviceId: 'test-device',
      name: 'Radio Jazz',
      location: 'http://host/presets/radio-jazz.json',
      iconUrl: 'https://example.com/jazz.png',
      type: 'stationurl',
      source: 'LOCAL_INTERNET_RADIO',
      isPresetable: true,
    },
    {
      id: 3,
      deviceId: 'test-device',
      name: 'Morning News',
      location: 'https://example.com/news',
      type: 'stationurl',
      source: 'LOCAL_INTERNET_RADIO',
      isPresetable: true,
    },
  ];

  beforeEach(async () => {
    mockPresetService = {
      getPresets: jest.fn().mockReturnValue(of(mockPresets)),
      storePreset: jest.fn().mockReturnValue(of(mockPresets[0])),
      deletePreset: jest.fn().mockReturnValue(of(void 0)),
      getStationFile: jest.fn().mockReturnValue(of({ name: 'Radio Jazz', audio: { streamUrl: 'http://jazz.stream/live' } })),
    } as unknown as jest.Mocked<PresetService>;

    mockRouter = {
      navigate: jest.fn(),
      createUrlTree: jest.fn().mockReturnValue({}),
      serializeUrl: jest.fn().mockReturnValue(''),
      events: new Subject(),
    } as unknown as jest.Mocked<Router>;

    mockActivatedRoute = {
      snapshot: {
        paramMap: {
          get: jest.fn((key: string) => {
            if (key === 'id') return 'test-device';
            if (key === 'presetId') return 'new';
            return null;
          }),
        },
      } as any,
    };

    await TestBed.configureTestingModule({
      imports: [PresetFormComponent, ReactiveFormsModule],
      providers: [
        { provide: PresetService, useValue: mockPresetService },
        { provide: Router, useValue: mockRouter },
        { provide: ActivatedRoute, useValue: mockActivatedRoute },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(PresetFormComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  describe('Slot Dropdown Labels', () => {
    beforeEach(() => {
      fixture.detectChanges();
    });

    it('should show "Slot X - PresetName" for occupied slots', () => {
      expect(component['getSlotLabel'](1)).toBe('Slot 1 - Radio Jazz');
      expect(component['getSlotLabel'](3)).toBe('Slot 3 - Morning News');
    });

    it('should show "Slot X - Empty" for unoccupied slots', () => {
      expect(component['getSlotLabel'](2)).toBe('Slot 2 - Empty');
      expect(component['getSlotLabel'](4)).toBe('Slot 4 - Empty');
      expect(component['getSlotLabel'](5)).toBe('Slot 5 - Empty');
      expect(component['getSlotLabel'](6)).toBe('Slot 6 - Empty');
    });

    it('should load all presets on initialization for slot labels', () => {
      expect(mockPresetService.getPresets).toHaveBeenCalledWith('test-device');
      expect(component['allPresets']()).toEqual(mockPresets);
    });
  });

  describe('Edit Mode', () => {
    it('should be in create mode when presetId is "new"', () => {
      fixture.detectChanges();
      expect(component['isEditMode']()).toBe(false);
      expect(component['presetId']()).toBe(null);
    });

    it('should be in edit mode when presetId is a number', () => {
      mockActivatedRoute.snapshot!.paramMap.get = jest.fn((key: string) => {
        if (key === 'id') return 'test-device';
        if (key === 'presetId') return '1';
        return null;
      });

      fixture.detectChanges();

      expect(component['isEditMode']()).toBe(true);
      expect(component['presetId']()).toBe(1);
    });

    it('should load preset data when editing existing preset', () => {
      mockActivatedRoute.snapshot!.paramMap.get = jest.fn((key: string) => {
        if (key === 'id') return 'test-device';
        if (key === 'presetId') return '1';
        return null;
      });

      fixture.detectChanges();

      expect(component['form'].value.name).toBe('Radio Jazz');
      expect(component['form'].get('id')?.disabled).toBe(true);
    });

    it('should fetch station file stream URL for LOCAL_INTERNET_RADIO preset in edit mode', () => {
      mockActivatedRoute.snapshot!.paramMap.get = jest.fn((key: string) => {
        if (key === 'id') return 'test-device';
        if (key === 'presetId') return '1';
        return null;
      });

      fixture.detectChanges();

      expect(mockPresetService.getStationFile).toHaveBeenCalledWith('radio-jazz.json');
      expect(component['form'].value.streamUrl).toBe('http://jazz.stream/live');
    });
  });

  describe('LOCAL_INTERNET_RADIO conditional fields', () => {
    beforeEach(() => {
      fixture.detectChanges();
    });

    it('should default to LOCAL_INTERNET_RADIO mode', () => {
      expect(component['isLocalRadio']()).toBe(true);
    });

    it('should show streamUrl field and hide location when source is LOCAL_INTERNET_RADIO', () => {
      component['form'].get('source')!.setValue('LOCAL_INTERNET_RADIO');
      expect(component['isLocalRadio']()).toBe(true);

      // streamUrl should be required
      component['form'].get('streamUrl')!.setValue('');
      expect(component['form'].get('streamUrl')!.valid).toBe(false);

      // location should not be required
      expect(component['form'].get('location')!.valid).toBe(true);
    });

    it('should show location field and hide streamUrl when source is not LOCAL_INTERNET_RADIO', () => {
      component['form'].get('source')!.setValue('SPOTIFY');
      expect(component['isLocalRadio']()).toBe(false);

      // location should be required
      component['form'].get('location')!.setValue('');
      expect(component['form'].get('location')!.valid).toBe(false);

      // streamUrl should not be required
      expect(component['form'].get('streamUrl')!.valid).toBe(true);
    });

    it('should validate streamUrl starts with http://', () => {
      component['form'].get('streamUrl')!.setValue('ftp://bad-protocol');
      expect(component['form'].get('streamUrl')!.valid).toBe(false);

      component['form'].get('streamUrl')!.setValue('http://valid.stream/live');
      expect(component['form'].get('streamUrl')!.valid).toBe(true);

      component['form'].get('streamUrl')!.setValue('https://also-valid.stream/live');
      expect(component['form'].get('streamUrl')!.valid).toBe(true);
    });
  });

  describe('Form Submission', () => {
    beforeEach(() => {
      fixture.detectChanges();
    });

    it('should save LOCAL_INTERNET_RADIO preset with streamUrl instead of location', () => {
      component['form'].patchValue({
        id: 2,
        name: 'Test Radio',
        streamUrl: 'http://test.stream/live',
        iconUrl: 'https://example.com/icon.png',
        type: 'stationurl',
        source: 'LOCAL_INTERNET_RADIO',
      });

      component['onSubmit']();

      expect(mockPresetService.storePreset).toHaveBeenCalledWith('test-device', {
        id: 2,
        name: 'Test Radio',
        streamUrl: 'http://test.stream/live',
        iconUrl: 'https://example.com/icon.png',
        type: 'stationurl',
        source: 'LOCAL_INTERNET_RADIO',
        isUpdate: false,
      });
    });

    it('should save non-LOCAL_INTERNET_RADIO preset with location', () => {
      component['form'].get('source')!.setValue('SPOTIFY');
      component['form'].patchValue({
        id: 2,
        name: 'Test Playlist',
        location: 'spotify:playlist:abc',
        type: 'stationurl',
      });

      component['onSubmit']();

      expect(mockPresetService.storePreset).toHaveBeenCalledWith('test-device', {
        id: 2,
        name: 'Test Playlist',
        location: 'spotify:playlist:abc',
        iconUrl: undefined,
        type: 'stationurl',
        source: 'SPOTIFY',
        isUpdate: false,
      });
    });

    it('should navigate back after successful save', () => {
      component['form'].patchValue({
        id: 2,
        name: 'Test Preset',
        streamUrl: 'http://test.stream/live',
      });

      component['onSubmit']();

      expect(mockRouter.navigate).toHaveBeenCalledWith(['/devices', 'test-device']);
    });

    it('should not submit invalid form', () => {
      component['form'].patchValue({
        id: 2,
        name: '',
        streamUrl: '',
      });

      component['onSubmit']();

      expect(mockPresetService.storePreset).not.toHaveBeenCalled();
    });

    it('should handle 409 Conflict with user-friendly error', () => {
      mockPresetService.storePreset.mockReturnValue(
        throwError(() => ({ status: 409, error: { message: "Station file 'test' already exists." } }))
      );

      component['form'].patchValue({
        id: 2,
        name: 'Duplicate Station',
        streamUrl: 'http://test.stream/live',
      });

      component['onSubmit']();

      expect(component['error']()).toBe("Station file 'test' already exists.");
    });
  });

  describe('Delete Preset', () => {
    beforeEach(() => {
      mockActivatedRoute.snapshot!.paramMap.get = jest.fn((key: string) => {
        if (key === 'id') return 'test-device';
        if (key === 'presetId') return '1';
        return null;
      });
      fixture.detectChanges();
    });

    it('should show delete confirmation dialog', () => {
      expect(component['showDeleteConfirm']()).toBe(false);
      
      component['confirmDelete']();
      
      expect(component['showDeleteConfirm']()).toBe(true);
    });

    it('should cancel delete and hide dialog', () => {
      component['showDeleteConfirm'].set(true);
      
      component['cancelDelete']();
      
      expect(component['showDeleteConfirm']()).toBe(false);
    });

    it('should delete preset and navigate back', () => {
      component['onDelete']();

      expect(mockPresetService.deletePreset).toHaveBeenCalledWith('test-device', 1);
      expect(mockRouter.navigate).toHaveBeenCalledWith(['/devices', 'test-device']);
    });

    it('should handle delete error', () => {
      mockPresetService.deletePreset.mockReturnValue(
        throwError(() => ({ error: { message: 'Delete failed' } }))
      );

      component['onDelete']();

      expect(component['error']()).toBe('Delete failed');
    });
  });
});
