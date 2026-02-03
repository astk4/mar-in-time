import { Component, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CoordinatesDisplay } from '../coordinates-display/coordinates-display';
import { MapToUiService } from '../map-to-ui-service';
import { App } from '../app';

@Component({
  selector: 'side-info',
  imports: [CommonModule, CoordinatesDisplay],
  templateUrl: './side-info.html',
  styleUrl: './side-info.css',
})
export class SideInfo {
  isOpen: boolean = false;

  @ViewChild(CoordinatesDisplay) coordDisplay!: CoordinatesDisplay;

  constructor(private mapToUiService: MapToUiService) 
  { 
    this.mapToUiService.details$.subscribe((details) => {
      if (!details) { return; }
      
      this.coordDisplay.lat = App.roundTo(details.lat, 6);
      this.coordDisplay.lng = App.roundTo(details.lng, 6);
    });
  }

  onToggle() {
    this.isOpen = !this.isOpen;
  }
}
