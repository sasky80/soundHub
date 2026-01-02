import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface Device {
  id: string;
  name: string;
  ipAddress: string;
  vendor: string;
  capabilities: string[];
  dateTimeAdded: string;
}

export interface NowPlayingInfo {
  stationName?: string;
  artist?: string;
  track?: string;
  album?: string;
  source?: string;
  playStatus?: string;
  artUrl?: string;
}

export interface DeviceStatus {
  isOnline: boolean;
  powerState: boolean;
  volume: number;
  currentSource?: string;
  nowPlaying?: NowPlayingInfo;
}

export interface PowerRequest {
  on: boolean;
}

export interface PingResult {
  reachable: boolean;
  latencyMs: number;
}

export interface DiscoveryResult {
  discovered: number;
  new: number;
  devices: Device[];
}

export interface VendorInfo {
  id: string;
  name: string;
}

export interface NetworkMaskResponse {
  networkMask: string;
}

export interface CreateDeviceRequest {
  name: string;
  ipAddress: string;
  vendor: string;
  capabilities?: string[];
}

export interface UpdateDeviceRequest {
  name?: string;
  ipAddress?: string;
  vendor?: string;
  capabilities?: string[];
}

export interface VolumeInfo {
  targetVolume: number;
  actualVolume: number;
  isMuted: boolean;
}

export interface SetVolumeRequest {
  level: number;
}

export interface PressKeyRequest {
  key: string;
}

@Injectable({ providedIn: 'root' })
export class DeviceService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = '/api/devices';
  private readonly configUrl = '/api/config';

  getDevices(): Observable<Device[]> {
    return this.http.get<Device[]>(this.apiUrl);
  }

  getDevice(id: string): Observable<Device> {
    return this.http.get<Device>(`${this.apiUrl}/${id}`);
  }

  getDeviceStatus(id: string): Observable<DeviceStatus> {
    return this.http.get<DeviceStatus>(`${this.apiUrl}/${id}/status`);
  }

  setPower(id: string, on: boolean): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${id}/power`, { on });
  }

  pingDevice(id: string): Observable<PingResult> {
    return this.http.get<PingResult>(`${this.apiUrl}/${id}/ping`);
  }

  discoverDevices(): Observable<DiscoveryResult> {
    return this.http.post<DiscoveryResult>(`${this.apiUrl}/discover`, {});
  }

  getNetworkMask(): Observable<NetworkMaskResponse> {
    return this.http.get<NetworkMaskResponse>(`${this.configUrl}/network-mask`);
  }

  updateNetworkMask(networkMask: string): Observable<void> {
    return this.http.put<void>(`${this.configUrl}/network-mask`, { networkMask });
  }

  getVendors(): Observable<VendorInfo[]> {
    return this.http.get<VendorInfo[]>('/api/vendors');
  }

  createDevice(device: CreateDeviceRequest): Observable<Device> {
    return this.http.post<Device>(this.apiUrl, device);
  }

  updateDevice(id: string, device: UpdateDeviceRequest): Observable<Device> {
    return this.http.put<Device>(`${this.apiUrl}/${id}`, device);
  }

  deleteDevice(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  getVolume(id: string): Observable<VolumeInfo> {
    return this.http.get<VolumeInfo>(`${this.apiUrl}/${id}/volume`);
  }

  setVolume(id: string, level: number): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${id}/volume`, { level } as SetVolumeRequest);
  }

  toggleMute(id: string): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${id}/mute`, {});
  }

  pressKey(id: string, key: string): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${id}/key`, { key } as PressKeyRequest);
  }

  enterBluetoothPairing(id: string): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${id}/bluetooth/enter-pairing`, {});
  }
}
