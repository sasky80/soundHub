import { Route } from '@angular/router';
import {
  LandingComponent,
  SettingsComponent,
  DeviceConfigComponent,
  DeviceDetailsComponent,
} from '@soundhub/frontend/feature';

export const appRoutes: Route[] = [
  { path: '', component: LandingComponent },
  { path: 'settings', component: SettingsComponent },
  { path: 'settings/devices', component: DeviceConfigComponent },
  { path: 'devices/:id', component: DeviceDetailsComponent },
];
