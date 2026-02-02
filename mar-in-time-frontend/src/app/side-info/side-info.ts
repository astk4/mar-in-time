import { Component, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CoordinatesDisplay } from '../coordinates-display/coordinates-display';
import { MapToUiService } from '../map-to-ui-service';

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
      
      this.coordDisplay.lat = this.roundTo(details.lat, 6);
      this.coordDisplay.lng = this.roundTo(details.lng, 6);
    });
  }

  private roundTo(num: number, decimals: number): number {
    const pow = Math.pow(10, decimals);
    return Math.round((num+Number.EPSILON) * pow) / pow;
  }

  onToggle() {
    this.isOpen = !this.isOpen;
  }
}
