import { Component, ViewChildren } from '@angular/core';
import { DirectionSpan } from '../coordinates-display/direction-span/direction-span';

@Component({
  selector: 'coordinates-display',
  imports: [DirectionSpan],
  templateUrl: './coordinates-display.html',
  styleUrl: './coordinates-display.css',
})
export class CoordinatesDisplay {
  lat: number | null = null;
  lng: number | null = null;
  @ViewChildren(DirectionSpan) directionSpans!: DirectionSpan[];

  getIsNorth() : boolean {
    return this.lat !== null && this.lat >= 0;
  }

  getIsEast() : boolean {
    return this.lng !== null && this.lng >= 0;
  }

  getAbs(val: number) : number {
    return Math.abs(val);
  }

  getSign(val: number) : number {
    return val >= 0 ? 1 : -1;
  }
}
