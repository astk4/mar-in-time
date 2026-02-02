import { Component } from '@angular/core';
import { initMainMap, addMarkers, registerOnMapClickCallback } 
  from './leaflet-logic/main_map.js';
import { MapToUiService } from '../map-to-ui-service.js';

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
    registerOnMapClickCallback(this.onMapClick.bind(this));
  }

  constructor(private mapToUiService: MapToUiService) {}

  onMapClick(lat: number, lng: number) {
    this.mapToUiService.send({ lat, lng });
  }
}
