import { Component, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { MainMap } from './main-map/main-map';
import { SideInfo } from './side-info/side-info';
import { patchIconPaths } from './main-map/leaflet-logic/main_map.js';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, MainMap, SideInfo],
  templateUrl: './app.html',
})
export class App {
  protected readonly title = signal('mar-in-time-frontend');

  static roundTo(num: number, decimals: number): number {
    const pow = Math.pow(10, decimals);
    return Math.round((num+Number.EPSILON) * pow) / pow;
  }

  ngOnInit() {
    patchIconPaths();
  }
}
