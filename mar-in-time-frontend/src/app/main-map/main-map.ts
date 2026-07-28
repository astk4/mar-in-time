import { Component } from '@angular/core';
import { initMainMap, addMarkers, registerOnMapClickCallback, setOsConfigData } 
  from './leaflet-logic/main_map.js';
import { initSignalR } from './signalr-connection.js'
import { MapToUiService } from '../map-to-ui-service.js';
import { ConfigDataService } from '../config-service.js';

@Component({
  selector: 'main-map',
  imports: [],
  templateUrl: './main-map.html',
  styleUrl: './main-map.css',
})
export class MainMap {

  ngAfterViewInit() {
    initSignalR();
    initMainMap();
    addMarkers();
    registerOnMapClickCallback(this.onMapClick.bind(this));
  }

  constructor(private mapToUiService: MapToUiService, private configService: ConfigDataService) {
    setOsConfigData(this.configService.configData);
  }

  onMapClick(lat: number, lng: number) {
    this.mapToUiService.send({ lat, lng });
  }
}
