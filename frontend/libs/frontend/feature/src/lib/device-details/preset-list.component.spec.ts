import { ComponentFixture, TestBed } from '@angular/core/testing';
import { PresetListComponent } from './preset-list.component';
import { PresetService, DeviceService } from '@soundhub/frontend/data-access';
import { Router } from '@angular/router';
import { of, throwError } from 'rxjs';
import { signal } from '@angular/core';

describe('PresetListComponent', () => {
  let component: PresetListComponent;
  let fixture: ComponentFixture<PresetListComponent>;
  let mockPresetService: jest.Mocked<PresetService>;
  let mockDeviceService: jest.Mocked<DeviceService>;
  let mockRouter: jest.Mocked<Router>;

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
      id: 2,
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
      playPreset: jest.fn().mockReturnValue(of(void 0)),
    } as unknown as jest.Mocked<PresetService>;

    mockDeviceService = {
      setPower: jest.fn().mockReturnValue(of(void 0)),
    } as unknown as jest.Mocked<DeviceService>;

    mockRouter = {
      navigate: jest.fn(),
    } as unknown as jest.Mocked<Router>;

    await TestBed.configureTestingModule({
      imports: [PresetListComponent],
      providers: [
        { provide: PresetService, useValue: mockPresetService },
        { provide: DeviceService, useValue: mockDeviceService },
        { provide: Router, useValue: mockRouter },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(PresetListComponent);
    component = fixture.componentInstance;
    fixture.componentRef.setInput('deviceId', 'test-device');
    fixture.componentRef.setInput('isPowerOn', true);
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  describe('Edit Mode', () => {
    it('should initialize with edit mode disabled', () => {
      expect(component['editMode']()).toBe(false);
    });

    it('should toggle edit mode when toggleEditMode is called', () => {
      expect(component['editMode']()).toBe(false);
      
      component['toggleEditMode']();
      expect(component['editMode']()).toBe(true);
      
      component['toggleEditMode']();
      expect(component['editMode']()).toBe(false);
    });

    it('should exit edit mode when exitEditMode is called', () => {
      component['editMode'].set(true);
      expect(component['editMode']()).toBe(true);
      
      component['exitEditMode']();
      expect(component['editMode']()).toBe(false);
    });

    it('should navigate to preset details only in edit mode', () => {
      const preset = mockPresets[0];
      
      // Normal mode - should not navigate
      component['editMode'].set(false);
      component['navigateToPreset'](preset);
      expect(mockRouter.navigate).not.toHaveBeenCalled();
      
      // Edit mode - should navigate
      component['editMode'].set(true);
      component['navigateToPreset'](preset);
      expect(mockRouter.navigate).toHaveBeenCalledWith(['/devices', 'test-device', 'presets', 1]);
    });

    it('should play preset in normal mode when name is clicked', () => {
      const preset = mockPresets[0];
      component['editMode'].set(false);
      
      component['onPresetNameClick'](preset);
      
      expect(mockPresetService.playPreset).toHaveBeenCalledWith('test-device', 1);
      expect(mockRouter.navigate).not.toHaveBeenCalled();
    });

    it('should navigate to preset details in edit mode when name is clicked', () => {
      const preset = mockPresets[0];
      component['editMode'].set(true);
      
      component['onPresetNameClick'](preset);
      
      expect(mockRouter.navigate).toHaveBeenCalledWith(['/devices', 'test-device', 'presets', 1]);
      expect(mockPresetService.playPreset).not.toHaveBeenCalled();
    });
  });

  describe('Preset Loading', () => {
    it('should load presets on initialization', () => {
      fixture.detectChanges();
      
      expect(mockPresetService.getPresets).toHaveBeenCalledWith('test-device');
      expect(component['presets']()).toEqual(mockPresets);
      expect(component['loading']()).toBe(false);
    });

    it('should handle preset loading error', () => {
      mockPresetService.getPresets.mockReturnValue(
        throwError(() => new Error('Failed to load'))
      );
      
      fixture.detectChanges();
      
      expect(component['error']()).toBe('Failed to load');
      expect(component['loading']()).toBe(false);
    });
  });

  describe('Play Preset', () => {
    beforeEach(() => {
      fixture.detectChanges();
    });

    it('should play preset when device is powered on', () => {
      const preset = mockPresets[0];
      
      component['playPreset'](preset);
      
      expect(mockPresetService.playPreset).toHaveBeenCalledWith('test-device', 1);
      expect(component['playingPresetId']()).toBe(null);
    });

    it('should power on device before playing preset when device is off', () => {
      fixture.componentRef.setInput('isPowerOn', false);
      const preset = mockPresets[0];
      
      component['playPreset'](preset);
      
      expect(mockDeviceService.setPower).toHaveBeenCalledWith('test-device', true);
      expect(mockPresetService.playPreset).toHaveBeenCalledWith('test-device', 1);
    });

    it('should not play preset if another preset is already playing', () => {
      component['playingPresetId'].set(2);
      const preset = mockPresets[0];
      
      component['playPreset'](preset);
      
      expect(mockPresetService.playPreset).not.toHaveBeenCalled();
    });
  });

  describe('Navigation', () => {
    it('should navigate to new preset form', () => {
      component['navigateToNewPreset']();
      
      expect(mockRouter.navigate).toHaveBeenCalledWith(['/devices', 'test-device', 'presets', 'new']);
    });
  });
});
