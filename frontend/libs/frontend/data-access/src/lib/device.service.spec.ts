import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { DeviceService, Device, PingResult, DiscoveryResult, VendorInfo } from './device.service';

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
        capabilities: ['power', 'volume'],
        dateTimeAdded: new Date().toISOString(),
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

  it('should ping device', () => {
    const mockResult: PingResult = { reachable: true, latencyMs: 42 };

    service.pingDevice('1').subscribe((result) => {
      expect(result).toEqual(mockResult);
    });

    const req = httpMock.expectOne('/api/devices/1/ping');
    expect(req.request.method).toBe('GET');
    req.flush(mockResult);
  });

  it('should discover devices', () => {
    const mockResult: DiscoveryResult = {
      discovered: 3,
      new: 1,
      devices: [],
    };

    service.discoverDevices().subscribe((result) => {
      expect(result).toEqual(mockResult);
    });

    const req = httpMock.expectOne('/api/devices/discover');
    expect(req.request.method).toBe('POST');
    req.flush(mockResult);
  });

  it('should get vendors', () => {
    const mockVendors: VendorInfo[] = [
      { id: 'bose', name: 'Bose SoundTouch' },
    ];

    service.getVendors().subscribe((vendors) => {
      expect(vendors).toEqual(mockVendors);
    });

    const req = httpMock.expectOne('/api/vendors');
    expect(req.request.method).toBe('GET');
    req.flush(mockVendors);
  });

  it('should get network mask', () => {
    service.getNetworkMask().subscribe((result) => {
      expect(result.networkMask).toBe('192.168.1.0/24');
    });

    const req = httpMock.expectOne('/api/config/network-mask');
    expect(req.request.method).toBe('GET');
    req.flush({ networkMask: '192.168.1.0/24' });
  });

  it('should update network mask', () => {
    service.updateNetworkMask('192.168.1.0/24').subscribe();

    const req = httpMock.expectOne('/api/config/network-mask');
    expect(req.request.method).toBe('PUT');
    expect(req.request.body).toEqual({ networkMask: '192.168.1.0/24' });
    req.flush(null);
  });

  it('should create device', () => {
    const newDevice = {
      name: 'New Device',
      ipAddress: '192.168.1.50',
      vendor: 'bose',
    };

    service.createDevice(newDevice).subscribe();

    const req = httpMock.expectOne('/api/devices');
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(newDevice);
    req.flush({ ...newDevice, id: '123', capabilities: [], dateTimeAdded: new Date().toISOString() });
  });

  it('should delete device', () => {
    service.deleteDevice('1').subscribe();

    const req = httpMock.expectOne('/api/devices/1');
    expect(req.request.method).toBe('DELETE');
    req.flush(null);
  });
});
