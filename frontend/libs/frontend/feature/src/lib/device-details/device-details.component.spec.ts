import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideRouter, ActivatedRoute } from '@angular/router';
import { DeviceDetailsComponent } from './device-details.component';

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

  describe('remote control buttons', () => {
    beforeEach(() => {
      fixture.detectChanges();
      httpMock.expectOne('/api/devices/test-device-id').flush(mockDevice);
      httpMock.expectOne('/api/devices/test-device-id/status').flush(mockStatus);
      httpMock.expectOne('/api/devices/test-device-id/volume').flush(mockVolumeInfo);
      fixture.detectChanges();
      httpMock.expectOne('/api/devices/test-device-id/presets').flush(mockPresets);
      fixture.detectChanges();
    });

    it('should render remote control buttons', () => {
      const prevBtn = fixture.nativeElement.querySelector('button[aria-label="Previous track"]');
      const playBtn = fixture.nativeElement.querySelector('button[aria-label="Play or pause"]');
      const nextBtn = fixture.nativeElement.querySelector('button[aria-label="Next track"]');
      const volUpBtn = fixture.nativeElement.querySelector('button[aria-label="Volume up"]');
      const volDownBtn = fixture.nativeElement.querySelector('button[aria-label="Volume down"]');
      const auxBtn = fixture.nativeElement.querySelector('button[aria-label="Switch to AUX"]');

      expect(prevBtn).toBeTruthy();
      expect(playBtn).toBeTruthy();
      expect(nextBtn).toBeTruthy();
      expect(volUpBtn).toBeTruthy();
      expect(volDownBtn).toBeTruthy();
      expect(auxBtn).toBeTruthy();
    });

    it('should hide Bluetooth button if capability not present', () => {
      const btBtn = fixture.nativeElement.querySelector('button[aria-label="Start Bluetooth pairing"]');
      expect(btBtn).toBeNull();
    });

    it('should show Bluetooth button if capability is present', () => {
      component['device'].set({
        ...mockDevice,
        capabilities: [...mockDevice.capabilities, 'bluetoothPairing'],
      });
      fixture.detectChanges();

      const btBtn = fixture.nativeElement.querySelector('button[aria-label="Start Bluetooth pairing"]');
      expect(btBtn).toBeTruthy();
    });

    it('should disable buttons when device is off', () => {
      component['status'].set({ ...mockStatus, powerState: false });
      fixture.detectChanges();

      const prevBtn = fixture.nativeElement.querySelector('button[aria-label="Previous track"]');
      const playBtn = fixture.nativeElement.querySelector('button[aria-label="Play or pause"]');
      const nextBtn = fixture.nativeElement.querySelector('button[aria-label="Next track"]');
      const volUpBtn = fixture.nativeElement.querySelector('button[aria-label="Volume up"]');
      const volDownBtn = fixture.nativeElement.querySelector('button[aria-label="Volume down"]');
      const auxBtn = fixture.nativeElement.querySelector('button[aria-label="Switch to AUX"]');

      expect(prevBtn?.disabled).toBe(true);
      expect(playBtn?.disabled).toBe(true);
      expect(nextBtn?.disabled).toBe(true);
      expect(volUpBtn?.disabled).toBe(true);
      expect(volDownBtn?.disabled).toBe(true);
      expect(auxBtn?.disabled).toBe(true);
    });

    it('should enable buttons when device is on', () => {
      const prevBtn = fixture.nativeElement.querySelector('button[aria-label="Previous track"]');
      const playBtn = fixture.nativeElement.querySelector('button[aria-label="Play or pause"]');
      const nextBtn = fixture.nativeElement.querySelector('button[aria-label="Next track"]');

      expect(prevBtn?.disabled).toBe(false);
      expect(playBtn?.disabled).toBe(false);
      expect(nextBtn?.disabled).toBe(false);
    });

    it('should call pressKey with correct key name when buttons clicked', () => {
      const prevBtn = fixture.nativeElement.querySelector('button[aria-label="Previous track"]');
      const nextBtn = fixture.nativeElement.querySelector('button[aria-label="Next track"]');
      const auxBtn = fixture.nativeElement.querySelector('button[aria-label="Switch to AUX"]');

      prevBtn?.click();
      let req = httpMock.expectOne('/api/devices/test-device-id/key');
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual({ key: 'PREV_TRACK' });
      req.flush(null);

      nextBtn?.click();
      req = httpMock.expectOne('/api/devices/test-device-id/key');
      expect(req.request.body).toEqual({ key: 'NEXT_TRACK' });
      req.flush(null);

      auxBtn?.click();
      req = httpMock.expectOne('/api/devices/test-device-id/key');
      expect(req.request.body).toEqual({ key: 'AUX_INPUT' });
      req.flush(null);
    });

    it('should toggle play/pause icon based on play state', () => {
      const getIconSrc = () =>
        fixture.nativeElement
          .querySelector('button[aria-label="Play or pause"] img')
          ?.getAttribute('src');

      component['isPlaying'].set(false);
      fixture.detectChanges();
      expect(getIconSrc()).toContain('remote-play.svg');

      component['isPlaying'].set(true);
      fixture.detectChanges();
      expect(getIconSrc()).toContain('remote-pause.svg');
    });

    it('should update isPlaying signal when play/pause button clicked', () => {
      component['isPlaying'].set(false);
      const playBtn = fixture.nativeElement.querySelector('button[aria-label="Play or pause"]');

      playBtn?.click();
      const req = httpMock.expectOne('/api/devices/test-device-id/key');
      req.flush(null);

      expect(component['isPlaying']()).toBe(true);
    });

    it('should display loading state during key press', () => {
      component['keyLoading'].set('PREV_TRACK');
      fixture.detectChanges();

      const prevBtn = fixture.nativeElement.querySelector('button[aria-label="Previous track"]');
      expect(prevBtn?.disabled).toBe(true);
    });

    it('should display loading state during Bluetooth pairing', () => {
      component['device'].set({
        ...mockDevice,
        capabilities: [...mockDevice.capabilities, 'bluetoothPairing'],
      });
      component['pairingLoading'].set(true);
      fixture.detectChanges();

      const btBtn = fixture.nativeElement.querySelector('button[aria-label="Start Bluetooth pairing"]');
      expect(btBtn?.disabled).toBe(true);
    });

    it('should call enterBluetoothPairing when Bluetooth button clicked', () => {
      component['device'].set({
        ...mockDevice,
        capabilities: [...mockDevice.capabilities, 'bluetoothPairing'],
      });
      fixture.detectChanges();

      const btBtn = fixture.nativeElement.querySelector('button[aria-label="Start Bluetooth pairing"]');
      btBtn?.click();

      const req = httpMock.expectOne('/api/devices/test-device-id/bluetooth/enter-pairing');
      expect(req.request.method).toBe('POST');
      req.flush(null);
    });

    it('should display pairing message after Bluetooth pairing started', () => {
      component['device'].set({
        ...mockDevice,
        capabilities: [...mockDevice.capabilities, 'bluetoothPairing'],
      });
      fixture.detectChanges();

      const btBtn = fixture.nativeElement.querySelector('button[aria-label="Start Bluetooth pairing"]');
      btBtn?.click();

      const req = httpMock.expectOne('/api/devices/test-device-id/bluetooth/enter-pairing');
      req.flush(null);
      fixture.detectChanges();

      const message = fixture.nativeElement.querySelector('.pairing-message');
      expect(message?.textContent).toContain('Bluetooth pairing started');
    });

    it('should set active state on AUX button after successful key press', () => {
      const auxBtn = fixture.nativeElement.querySelector('button[aria-label="Switch to AUX"]');
      auxBtn?.click();

      const req = httpMock.expectOne('/api/devices/test-device-id/key');
      req.flush(null);
      fixture.detectChanges();

      expect(auxBtn?.classList.contains('active')).toBe(true);
    });

    it('should display error message when key press fails', () => {
      const auxBtn = fixture.nativeElement.querySelector('button[aria-label="Switch to AUX"]');
      auxBtn?.click();

      const req = httpMock.expectOne('/api/devices/test-device-id/key');
      req.error(new ProgressEvent('error'));
      fixture.detectChanges();

      const message = fixture.nativeElement.querySelector('.remote-message');
      expect(message?.textContent).toContain('Action failed');
    });
  });

  describe('Now Playing LCD display', () => {
    beforeEach(() => {
      fixture.detectChanges();
      httpMock.expectOne('/api/devices/test-device-id').flush(mockDevice);
      httpMock.expectOne('/api/devices/test-device-id/status').flush(mockStatus);
      httpMock.expectOne('/api/devices/test-device-id/volume').flush(mockVolumeInfo);
      fixture.detectChanges();
      httpMock.expectOne('/api/devices/test-device-id/presets').flush(mockPresets);
    });

    it('should format Now Playing text with all fields present', () => {
      const nowPlaying = {
        stationName: 'KINK FM',
        artist: 'Jungle Brothers',
        track: 'Straight Out the Jungle',
        album: 'Straight Out the Jungle',
        source: 'LOCAL_INTERNET_RADIO',
        playStatus: 'PLAY_STATE',
        artUrl: undefined,
      };

      const formatted = component['formatNowPlaying'](nowPlaying);
      expect(formatted).toBe('KINK FM: Jungle Brothers, Straight Out the Jungle');
    });

    it('should format Now Playing text with missing artist', () => {
      const nowPlaying = {
        stationName: 'KINK FM',
        artist: undefined,
        track: 'Straight Out the Jungle',
        album: undefined,
        source: 'LOCAL_INTERNET_RADIO',
        playStatus: 'PLAY_STATE',
        artUrl: undefined,
      };

      const formatted = component['formatNowPlaying'](nowPlaying);
      expect(formatted).toBe('KINK FM: Straight Out the Jungle');
    });

    it('should format Now Playing text with missing track', () => {
      const nowPlaying = {
        stationName: 'KINK FM',
        artist: 'Jungle Brothers',
        track: undefined,
        album: undefined,
        source: 'LOCAL_INTERNET_RADIO',
        playStatus: 'PLAY_STATE',
        artUrl: undefined,
      };

      const formatted = component['formatNowPlaying'](nowPlaying);
      expect(formatted).toBe('KINK FM: Jungle Brothers');
    });

    it('should format Now Playing text with only station name', () => {
      const nowPlaying = {
        stationName: 'KINK FM',
        artist: undefined,
        track: undefined,
        album: undefined,
        source: 'LOCAL_INTERNET_RADIO',
        playStatus: 'PLAY_STATE',
        artUrl: undefined,
      };

      const formatted = component['formatNowPlaying'](nowPlaying);
      expect(formatted).toBe('KINK FM');
    });

    it('should display LCD when device is on and nowPlaying exists', () => {
      const statusWithNowPlaying = {
        ...mockStatus,
        powerState: true,
        nowPlaying: {
          stationName: 'KINK FM',
          artist: 'Jungle Brothers',
          track: 'Straight Out the Jungle',
          album: undefined,
          source: 'LOCAL_INTERNET_RADIO',
          playStatus: 'PLAY_STATE',
          artUrl: undefined,
        },
      };

      component['status'].set(statusWithNowPlaying);
      fixture.detectChanges();

      const lcdDisplay = fixture.nativeElement.querySelector('.lcd-display');
      expect(lcdDisplay).toBeTruthy();
      expect(lcdDisplay?.textContent).toContain('KINK FM: Jungle Brothers, Straight Out the Jungle');
    });

    it('should hide LCD when device is off', () => {
      const statusWithNowPlaying = {
        ...mockStatus,
        powerState: false,
        nowPlaying: {
          stationName: 'KINK FM',
          artist: 'Jungle Brothers',
          track: 'Straight Out the Jungle',
          album: undefined,
          source: 'LOCAL_INTERNET_RADIO',
          playStatus: 'PLAY_STATE',
          artUrl: undefined,
        },
      };

      component['status'].set(statusWithNowPlaying);
      fixture.detectChanges();

      const lcdDisplay = fixture.nativeElement.querySelector('.lcd-display');
      expect(lcdDisplay).toBeFalsy();
    });

    it('should show no playback message when device is on but no nowPlaying', () => {
      component['status'].set({ ...mockStatus, powerState: true, nowPlaying: undefined });
      fixture.detectChanges();

      const lcdDisplay = fixture.nativeElement.querySelector('.lcd-display');
      const lcdText = lcdDisplay?.querySelector('.lcd-text.static');
      expect(lcdDisplay).toBeTruthy();
      expect(lcdText).toBeTruthy();
    });
  });

  describe('Bluetooth button active state', () => {
    beforeEach(() => {
      fixture.detectChanges();
      httpMock.expectOne('/api/devices/test-device-id').flush({
        ...mockDevice,
        capabilities: [...mockDevice.capabilities, 'bluetoothPairing'],
      });
      httpMock.expectOne('/api/devices/test-device-id/status').flush(mockStatus);
      httpMock.expectOne('/api/devices/test-device-id/volume').flush(mockVolumeInfo);
      fixture.detectChanges();
      httpMock.expectOne('/api/devices/test-device-id/presets').flush(mockPresets);
    });

    it('should show Bluetooth button as active when source is BLUETOOTH', () => {
      component['status'].set({ ...mockStatus, currentSource: 'BLUETOOTH' });
      fixture.detectChanges();

      expect(component['isBluetoothActive']()).toBe(true);
      const btBtn = fixture.nativeElement.querySelector('button[aria-label="Start Bluetooth pairing"]');
      expect(btBtn?.classList.contains('active')).toBe(true);
      expect(btBtn?.getAttribute('aria-pressed')).toBe('true');
    });

    it('should not show Bluetooth button as active when source is not BLUETOOTH', () => {
      component['status'].set({ ...mockStatus, currentSource: 'SPOTIFY' });
      fixture.detectChanges();

      expect(component['isBluetoothActive']()).toBe(false);
      const btBtn = fixture.nativeElement.querySelector('button[aria-label="Start Bluetooth pairing"]');
      expect(btBtn?.classList.contains('active')).toBe(false);
      expect(btBtn?.getAttribute('aria-pressed')).toBe('false');
    });
  });

  describe('AUX button active state', () => {
    beforeEach(() => {
      fixture.detectChanges();
      httpMock.expectOne('/api/devices/test-device-id').flush(mockDevice);
      httpMock.expectOne('/api/devices/test-device-id/status').flush(mockStatus);
      httpMock.expectOne('/api/devices/test-device-id/volume').flush(mockVolumeInfo);
      fixture.detectChanges();
      httpMock.expectOne('/api/devices/test-device-id/presets').flush(mockPresets);
    });

    it('should show AUX button as active when source is AUX', () => {
      component['status'].set({ ...mockStatus, currentSource: 'AUX' });
      fixture.detectChanges();

      expect(component['isAuxActive']()).toBe(true);
      const auxBtn = fixture.nativeElement.querySelector('button[aria-label="Switch to AUX"]');
      expect(auxBtn?.classList.contains('active')).toBe(true);
      expect(auxBtn?.getAttribute('aria-pressed')).toBe('true');
    });

    it('should show AUX button as active when source is AUX_INPUT', () => {
      component['status'].set({ ...mockStatus, currentSource: 'AUX_INPUT' });
      fixture.detectChanges();

      expect(component['isAuxActive']()).toBe(true);
      const auxBtn = fixture.nativeElement.querySelector('button[aria-label="Switch to AUX"]');
      expect(auxBtn?.classList.contains('active')).toBe(true);
    });

    it('should not show AUX button as active when source is different', () => {
      component['status'].set({ ...mockStatus, currentSource: 'SPOTIFY' });
      fixture.detectChanges();

      expect(component['isAuxActive']()).toBe(false);
      const auxBtn = fixture.nativeElement.querySelector('button[aria-label="Switch to AUX"]');
      expect(auxBtn?.classList.contains('active')).toBe(false);
    });
  });
});
