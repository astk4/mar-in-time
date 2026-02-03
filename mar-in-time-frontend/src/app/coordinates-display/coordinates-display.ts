import { Component, ViewChildren } from '@angular/core';
import { DirectionSpan } from '../coordinates-display/direction-span/direction-span';
import { App } from '../app';

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

  toDMS(deg: number, padDegrees: number = 2) : string {

    const absValue = Math.abs(deg);
    const degOnly = Math.trunc(absValue);
    
    let result = `${degOnly}`.padStart(padDegrees, '0') + '° ';

    const minutes = (absValue - degOnly) * 60; 
    
    const wholeMinutes = Math.trunc(minutes);
    const wholeMinutesStr = wholeMinutes.toString().padStart(2, '0');
    
    const remMinutesFractionPart = Math.round((minutes - wholeMinutes) * 1000);
    const remMinutesStr = remMinutesFractionPart.toString().padStart(3, '0');

    return result + wholeMinutesStr + '.' + remMinutesStr + "'";
  } 
}
