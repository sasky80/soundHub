import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideRouter } from '@angular/router';
import { DeviceConfigComponent } from './device-config.component';
import { Device, VendorInfo } from '@soundhub/frontend/data-access';

describe('DeviceConfigComponent', () => {
  let component: DeviceConfigComponent;
  let fixture: ComponentFixture<DeviceConfigComponent>;
  let httpMock: HttpTestingController;

  const mockDevices: Device[] = [
    {
      id: '1',
      name: 'Living Room Speaker',
      ipAddress: '192.168.1.10',
      vendor: 'bose-soundtouch',
      capabilities: ['power', 'volume', 'ping'],
      dateTimeAdded: new Date().toISOString(),
    },
    {
      id: '2',
      name: 'Kitchen Speaker',
      ipAddress: '192.168.1.11',
      vendor: 'bose-soundtouch',
      capabilities: ['power', 'volume'],
      dateTimeAdded: new Date(Date.now() - 10 * 60 * 1000).toISOString(), // 10 mins ago
    },
  ];

  const mockVendors: VendorInfo[] = [
    { id: 'bose-soundtouch', name: 'Bose SoundTouch' },
  ];

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DeviceConfigComponent],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([]),
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(DeviceConfigComponent);
    component = fixture.componentInstance;
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  function flushInitialRequests(): void {
    httpMock.expectOne('/api/devices').flush(mockDevices);
    httpMock.expectOne('/api/vendors').flush(mockVendors);
    httpMock.expectOne('/api/config/network-mask').flush({ networkMask: '192.168.1.0/24' });
  }

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should load devices on init', fakeAsync(() => {
    fixture.detectChanges();
    flushInitialRequests();
    tick();

    expect(component['devices']().length).toBe(2);
    expect(component['devices']()[0].name).toBe('Living Room Speaker');
  }));

  it('should load vendors on init', fakeAsync(() => {
    fixture.detectChanges();
    flushInitialRequests();
    tick();

    expect(component['vendors']().length).toBe(1);
    expect(component['vendors']()[0].id).toBe('bose-soundtouch');
  }));

  it('should load network mask on init', fakeAsync(() => {
    fixture.detectChanges();
    flushInitialRequests();
    tick();

    expect(component['networkMask']()).toBe('192.168.1.0/24');
  }));

  it('should ping device and update state', fakeAsync(() => {
    fixture.detectChanges();
    flushInitialRequests();
    tick();

    const device = component['devices']()[0];
    component['pingDevice'](device);

    const pingReq = httpMock.expectOne('/api/devices/1/ping');
    expect(pingReq.request.method).toBe('GET');
    pingReq.flush({ reachable: true, latencyMs: 42 });
    tick();

    const updatedDevice = component['devices']().find((d) => d.id === '1');
    expect(updatedDevice?.pingState).toBe('success');
    expect(updatedDevice?.pingLatency).toBe(42);
  }));

  it('should show ping button only for devices with ping capability', fakeAsync(() => {
    fixture.detectChanges();
    flushInitialRequests();
    tick();

    const deviceWithPing = component['devices']()[0];
    const deviceWithoutPing = component['devices']()[1];

    expect(component['hasPingCapability'](deviceWithPing)).toBe(true);
    expect(component['hasPingCapability'](deviceWithoutPing)).toBe(false);
  }));

  it('should highlight newly added devices', fakeAsync(() => {
    fixture.detectChanges();
    flushInitialRequests();
    tick();

    const newDevice = component['devices']()[0]; // Added just now
    const oldDevice = component['devices']()[1]; // Added 10 mins ago

    expect(component['isNewlyAdded'](newDevice)).toBe(true);
    expect(component['isNewlyAdded'](oldDevice)).toBe(false);
  }));

  it('should discover devices and reload on success', fakeAsync(() => {
    fixture.detectChanges();
    flushInitialRequests();
    tick();

    component['discoverDevices']();

    const discoverReq = httpMock.expectOne('/api/devices/discover');
    expect(discoverReq.request.method).toBe('POST');
    discoverReq.flush({ discovered: 3, new: 1, devices: [] });
    tick();

    expect(component['discoveryResult']()).toEqual({ discovered: 3, new: 1 });

    // Should trigger reload
    httpMock.expectOne('/api/devices').flush(mockDevices);
    httpMock.expectOne('/api/vendors').flush(mockVendors);
    httpMock.expectOne('/api/config/network-mask').flush({ networkMask: '192.168.1.0/24' });
  }));

  it('should update network mask', fakeAsync(() => {
    fixture.detectChanges();
    flushInitialRequests();
    tick();

    component['networkMask'].set('10.0.0.0/8');
    component['saveNetworkMask']();

    const updateReq = httpMock.expectOne('/api/config/network-mask');
    expect(updateReq.request.method).toBe('PUT');
    expect(updateReq.request.body).toEqual({ networkMask: '10.0.0.0/8' });
    updateReq.flush(null);
    tick();

    expect(component['savingNetworkMask']()).toBe(false);
  }));

  it('should open add device form', fakeAsync(() => {
    fixture.detectChanges();
    flushInitialRequests();
    tick();

    component['openAddDevice']();

    expect(component['showDeviceForm']()).toBe(true);
    expect(component['editingDevice']()).toBeNull();
    expect(component['isNewDevice']()).toBe(true);
  }));

  it('should open edit device form with device data', fakeAsync(() => {
    fixture.detectChanges();
    flushInitialRequests();
    tick();

    const device = mockDevices[0];
    component['openEditDevice'](device);

    expect(component['showDeviceForm']()).toBe(true);
    expect(component['editingDevice']()).toEqual(device);
    expect(component['isNewDevice']()).toBe(false);
    expect(component['deviceForm'].value.name).toBe('Living Room Speaker');
  }));

  it('should create new device', fakeAsync(() => {
    fixture.detectChanges();
    flushInitialRequests();
    tick();

    component['openAddDevice']();
    component['deviceForm'].patchValue({
      name: 'New Speaker',
      ipAddress: '192.168.1.50',
      vendor: 'bose-soundtouch',
    });

    component['saveDevice']();

    const createReq = httpMock.expectOne('/api/devices');
    expect(createReq.request.method).toBe('POST');
    expect(createReq.request.body.name).toBe('New Speaker');
    createReq.flush({ ...mockDevices[0], id: 'new-id' });
    tick();

    expect(component['showDeviceForm']()).toBe(false);

    // Should reload devices
    httpMock.expectOne('/api/devices').flush(mockDevices);
    httpMock.expectOne('/api/vendors').flush(mockVendors);
    httpMock.expectOne('/api/config/network-mask').flush({ networkMask: '192.168.1.0/24' });
  }));

  it('should delete device after confirmation', fakeAsync(() => {
    fixture.detectChanges();
    flushInitialRequests();
    tick();

    component['confirmDeleteDevice']('1');
    expect(component['showDeleteConfirm']()).toBe('1');

    component['deleteDevice']('1');

    const deleteReq = httpMock.expectOne('/api/devices/1');
    expect(deleteReq.request.method).toBe('DELETE');
    deleteReq.flush(null);
    tick();

    expect(component['showDeleteConfirm']()).toBeNull();

    // Should reload devices
    httpMock.expectOne('/api/devices').flush(mockDevices);
    httpMock.expectOne('/api/vendors').flush(mockVendors);
    httpMock.expectOne('/api/config/network-mask').flush({ networkMask: '192.168.1.0/24' });
  }));

  it('should cancel delete confirmation', fakeAsync(() => {
    fixture.detectChanges();
    flushInitialRequests();
    tick();

    component['confirmDeleteDevice']('1');
    expect(component['showDeleteConfirm']()).toBe('1');

    component['cancelDelete']();
    expect(component['showDeleteConfirm']()).toBeNull();
  }));

  it('should toggle capability selection', fakeAsync(() => {
    fixture.detectChanges();
    flushInitialRequests();
    tick();

    component['openAddDevice']();

    expect(component['isCapabilitySelected'](0)).toBe(false);

    component['toggleCapability'](0);
    expect(component['isCapabilitySelected'](0)).toBe(true);

    component['toggleCapability'](0);
    expect(component['isCapabilitySelected'](0)).toBe(false);
  }));
});
