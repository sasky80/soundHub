import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface Preset {
  id: number;
  deviceId: string;
  name: string;
  location: string;
  iconUrl?: string;
  type: string;
  source: string;
  isPresetable: boolean;
}

export interface StorePresetRequest {
  id: number;
  name: string;
  location: string;
  iconUrl?: string;
  type?: string;
  source?: string;
}

@Injectable({ providedIn: 'root' })
export class PresetService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = '/api/devices';

  getPresets(deviceId: string): Observable<Preset[]> {
    return this.http.get<Preset[]>(`${this.apiUrl}/${deviceId}/presets`);
  }

  storePreset(deviceId: string, preset: StorePresetRequest): Observable<Preset> {
    return this.http.post<Preset>(`${this.apiUrl}/${deviceId}/presets`, preset);
  }

  deletePreset(deviceId: string, presetId: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${deviceId}/presets/${presetId}`);
  }

  playPreset(deviceId: string, presetId: number): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${deviceId}/presets/${presetId}/play`, {});
  }
}
