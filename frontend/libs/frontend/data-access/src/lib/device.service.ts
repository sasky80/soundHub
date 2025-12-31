import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface Device {
  id: string;
  name: string;
  ipAddress: string;
  vendor: string;
  port: number;
  isOnline: boolean;
  powerState: boolean;
  volume: number;
  capabilities: string[];
  lastSeen: string;
}

export interface DeviceStatus {
  isOnline: boolean;
  powerState: boolean;
  volume: number;
  currentSource?: string;
}

export interface PowerRequest {
  on: boolean;
}

@Injectable({ providedIn: 'root' })
export class DeviceService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = '/api/devices';

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
}
