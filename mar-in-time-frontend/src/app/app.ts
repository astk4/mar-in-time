import { Component, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { MainMap } from './main-map/main-map';
import { patchIconPaths } from './main-map/leaflet-logic/main_map.js';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, MainMap],
  templateUrl: './app.html'
})
export class App {
  protected readonly title = signal('mar-in-time-frontend');

  ngOnInit() {
    patchIconPaths();
  }
}
