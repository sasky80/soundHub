import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { PresetService, Preset, StorePresetRequest } from './preset.service';

describe('PresetService', () => {
  let service: PresetService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });

    service = TestBed.inject(PresetService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should fetch presets for a device', () => {
    const deviceId = 'device-1';
    const mockPresets: Preset[] = [
      {
        id: 1,
        deviceId,
        name: 'Morning Jazz',
        location: 'http://example.com/jazz',
        iconUrl: 'http://example.com/jazz.png',
        type: 'stationurl',
        source: 'LOCAL_INTERNET_RADIO',
        isPresetable: true,
      },
    ];

    service.getPresets(deviceId).subscribe((presets) => {
      expect(presets).toEqual(mockPresets);
    });

    const req = httpMock.expectOne(`/api/devices/${deviceId}/presets`);
    expect(req.request.method).toBe('GET');
    req.flush(mockPresets);
  });

  it('should store a preset for a device', () => {
    const deviceId = 'device-1';
    const payload: StorePresetRequest = {
      id: 2,
      name: 'Chillhop',
      location: 'http://example.com/chillhop',
      iconUrl: undefined,
      type: 'stationurl',
      source: 'LOCAL_INTERNET_RADIO',
    };

    const storedPreset: Preset = {
      id: payload.id,
      deviceId,
      name: payload.name,
      location: payload.location,
      iconUrl: payload.iconUrl,
      type: payload.type!,
      source: payload.source!,
      isPresetable: true,
    };

    service.storePreset(deviceId, payload).subscribe((preset) => {
      expect(preset).toEqual(storedPreset);
    });

    const req = httpMock.expectOne(`/api/devices/${deviceId}/presets`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(payload);
    req.flush(storedPreset);
  });

  it('should delete a preset', () => {
    const deviceId = 'device-1';
    const presetId = 3;

    service.deletePreset(deviceId, presetId).subscribe((response) => {
      expect(response).toBeUndefined();
    });

    const req = httpMock.expectOne(`/api/devices/${deviceId}/presets/${presetId}`);
    expect(req.request.method).toBe('DELETE');
    req.flush(null);
  });

  it('should play a preset', () => {
    const deviceId = 'device-1';
    const presetId = 4;

    service.playPreset(deviceId, presetId).subscribe((response) => {
      expect(response).toBeUndefined();
    });

    const req = httpMock.expectOne(`/api/devices/${deviceId}/presets/${presetId}/play`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({});
    req.flush(null);
  });
});
