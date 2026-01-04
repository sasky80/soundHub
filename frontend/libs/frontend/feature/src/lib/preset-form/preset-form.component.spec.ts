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
      location: 'https://example.com/jazz',
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
      expect(component['form'].value.location).toBe('https://example.com/jazz');
      expect(component['form'].get('id')?.disabled).toBe(true);
    });
  });

  describe('Form Submission', () => {
    beforeEach(() => {
      fixture.detectChanges();
    });

    it('should save preset with valid form data', () => {
      component['form'].patchValue({
        id: 2,
        name: 'Test Preset',
        location: 'https://example.com/test',
        iconUrl: 'https://example.com/icon.png',
        type: 'stationurl',
        source: 'LOCAL_INTERNET_RADIO',
      });

      component['onSubmit']();

      expect(mockPresetService.storePreset).toHaveBeenCalledWith('test-device', {
        id: 2,
        name: 'Test Preset',
        location: 'https://example.com/test',
        iconUrl: 'https://example.com/icon.png',
        type: 'stationurl',
        source: 'LOCAL_INTERNET_RADIO',
      });
    });

    it('should navigate back after successful save', () => {
      component['form'].patchValue({
        id: 2,
        name: 'Test Preset',
        location: 'https://example.com/test',
      });

      component['onSubmit']();

      expect(mockRouter.navigate).toHaveBeenCalledWith(['/devices', 'test-device']);
    });

    it('should not submit invalid form', () => {
      component['form'].patchValue({
        id: 2,
        name: '',
        location: '',
      });

      component['onSubmit']();

      expect(mockPresetService.storePreset).not.toHaveBeenCalled();
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
