import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { DeviceService, Device } from './device.service';

describe('DeviceService', () => {
  let service: DeviceService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(DeviceService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should get devices', () => {
    const mockDevices: Device[] = [
      {
        id: '1',
        name: 'Test Device',
        ipAddress: '192.168.1.1',
        vendor: 'bose',
        port: 8090,
        isOnline: true,
        powerState: true,
        volume: 50,
        capabilities: [],
        lastSeen: new Date().toISOString(),
      },
    ];

    service.getDevices().subscribe((devices) => {
      expect(devices).toEqual(mockDevices);
    });

    const req = httpMock.expectOne('/api/devices');
    expect(req.request.method).toBe('GET');
    req.flush(mockDevices);
  });

  it('should set power', () => {
    service.setPower('1', true).subscribe();

    const req = httpMock.expectOne('/api/devices/1/power');
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({ on: true });
    req.flush(null);
  });
});
