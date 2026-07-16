import { Component, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { MainMap } from './main-map/main-map';
import { SideInfo } from './side-info/side-info';
import { patchIconPaths } from './main-map/leaflet-logic/main_map.js';
import { ConfigDataService } from './config-service';

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

  constructor(private cfds: ConfigDataService) { }

  ngOnInit() {
    patchIconPaths();
    console.log("Containerization detected:", this.cfds.configData.inContainer);
  }
}
