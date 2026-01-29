import { Component } from '@angular/core';
import { initMainMap, addMarkers } from './leaflet-logic/main_map.js';

@Component({
  selector: 'main-map',
  imports: [],
  templateUrl: './main-map.html',
  styleUrl: './main-map.css',
})
export class MainMap {
  ngAfterViewInit() {
    initMainMap();
    addMarkers();
  }
}
