import { Component, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CoordinatesDisplay } from '../coordinates-display/coordinates-display';
import { MapToUiService } from '../map-to-ui-service';
import { App } from '../app';
import { PointSummary } from '../point-summary/point-summary';
import { FetcherService } from '../fetcher-service';

@Component({
  selector: 'side-info',
  imports: [CommonModule, CoordinatesDisplay, PointSummary],
  templateUrl: './side-info.html',
  styleUrl: './side-info.css',
})
export class SideInfo {
  isOpen: boolean = false;

  @ViewChild(CoordinatesDisplay) coordDisplay!: CoordinatesDisplay;
  @ViewChild(PointSummary) pointSummary!: PointSummary;

  constructor(private mapToUiService: MapToUiService, 
              private fetcherService: FetcherService) 
  { 
    this.mapToUiService.details$.subscribe((details) => {
      if (!details) { return; }
      
      this.coordDisplay.lat = App.roundTo(details.lat, 6);
      this.coordDisplay.lng = App.roundTo(details.lng, 6);

      const url = `https://localhost:7120/spatial/point?lng=${details.lng}&lat=${details.lat}`;
      this.pointSummary.atSea = null

      this.fetcherService.getItem(url).subscribe((data) => 
      {
        this.pointSummary.setData(data);
      });
    });
  }

  onToggle() {
    this.isOpen = !this.isOpen;
  }
}
