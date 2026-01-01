import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideRouter, ActivatedRoute } from '@angular/router';
import { of } from 'rxjs';
import { DeviceDetailsComponent } from './device-details.component';
import { DeviceService } from '@soundhub/frontend/data-access';

describe('DeviceDetailsComponent', () => {
  let component: DeviceDetailsComponent;
  let fixture: ComponentFixture<DeviceDetailsComponent>;
  let httpMock: HttpTestingController;

  const mockDevice = {
    id: 'test-device-id',
    name: 'Test Speaker',
    ipAddress: '192.168.1.100',
    vendor: 'bose-soundtouch',
    capabilities: ['power', 'volume'],
    dateTimeAdded: new Date().toISOString(),
  };

  const mockStatus = {
    isOnline: true,
    powerState: true,
    volume: 50,
    currentSource: 'SPOTIFY',
  };

  const mockVolumeInfo = {
    targetVolume: 50,
    actualVolume: 50,
    isMuted: false,
  };

  const mockPresets: unknown[] = [];

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DeviceDetailsComponent],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([]),
        {
          provide: ActivatedRoute,
          useValue: {
            snapshot: {
              paramMap: {
                get: () => 'test-device-id',
              },
            },
          },
        },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(DeviceDetailsComponent);
    component = fixture.componentInstance;
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  describe('volume controls', () => {
    beforeEach(() => {
      fixture.detectChanges();
      // Flush initial HTTP requests
      httpMock.expectOne('/api/devices/test-device-id').flush(mockDevice);
      httpMock.expectOne('/api/devices/test-device-id/status').flush(mockStatus);
      httpMock.expectOne('/api/devices/test-device-id/volume').flush(mockVolumeInfo);
      fixture.detectChanges();
      httpMock.expectOne('/api/devices/test-device-id/presets').flush(mockPresets);
    });

    it('should load volume info after status loads', () => {
      expect(component['volumeInfo']()).toEqual(mockVolumeInfo);
      expect(component['volumeValue']()).toBe(50);
    });

    it('should have volume slider disabled when device is off', () => {
      component['status'].set({ ...mockStatus, powerState: false });
      fixture.detectChanges();

      const slider = fixture.nativeElement.querySelector('.volume-slider');
      expect(slider?.disabled).toBe(true);
    });

    it('should have volume slider enabled when device is on', () => {
      fixture.detectChanges();

      const slider = fixture.nativeElement.querySelector('.volume-slider');
      expect(slider?.disabled).toBe(false);
    });

    it('should have mute button disabled when device is off', () => {
      component['status'].set({ ...mockStatus, powerState: false });
      fixture.detectChanges();

      const muteBtn = fixture.nativeElement.querySelector('.mute-btn');
      expect(muteBtn?.disabled).toBe(true);
    });

    it('should reflect muted state on mute button', () => {
      component['volumeInfo'].set({ ...mockVolumeInfo, isMuted: true });
      fixture.detectChanges();

      const muteBtn = fixture.nativeElement.querySelector('.mute-btn');
      expect(muteBtn?.classList.contains('muted')).toBe(true);
    });

    it('should update power state when preset powers on device', () => {
      component['status'].set({ ...mockStatus, powerState: false });

      component['onPresetPowerStateChanged'](true);

      expect(component['status']()?.powerState).toBe(true);
    });

    it('should ignore redundant power state updates', () => {
      component['status'].set({ ...mockStatus, powerState: true });

      component['onPresetPowerStateChanged'](true);

      expect(component['status']()?.powerState).toBe(true);
    });

    it('should call toggleMute when mute button clicked', () => {
      fixture.detectChanges();
      const muteBtn = fixture.nativeElement.querySelector('.mute-btn');
      muteBtn?.click();

      const req = httpMock.expectOne('/api/devices/test-device-id/mute');
      expect(req.request.method).toBe('POST');
      req.flush(null);

      // Expect volume refetch
      httpMock.expectOne('/api/devices/test-device-id/volume').flush({ ...mockVolumeInfo, isMuted: true });
    });

    it('should debounce volume changes', () => {
      jest.useFakeTimers();
      fixture.detectChanges();

      // Simulate multiple rapid slider changes
      component['onVolumeInput']({ target: { value: '60' } } as unknown as Event);
      component['onVolumeInput']({ target: { value: '65' } } as unknown as Event);
      component['onVolumeInput']({ target: { value: '70' } } as unknown as Event);

      // Before debounce, no request
      jest.advanceTimersByTime(200);
      httpMock.expectNone('/api/devices/test-device-id/volume');

      // After debounce, only one request with final value
      jest.advanceTimersByTime(150);
      const req = httpMock.expectOne('/api/devices/test-device-id/volume');
      expect(req.request.body).toEqual({ level: 70 });
      req.flush(null);
      jest.useRealTimers();
    });
  });
});
