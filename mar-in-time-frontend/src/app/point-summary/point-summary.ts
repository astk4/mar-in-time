import { Component } from '@angular/core';

@Component({
  selector: 'point-summary',
  imports: [],
  templateUrl: './point-summary.html',
  styleUrl: './point-summary.css',
})
export class PointSummary {
  atSea: boolean | null = null;
  eezObject: any = null;

  setData(data: any) {
    this.atSea = data.atSea;
    this.eezObject = data.exclusiveEconomicZone;
  }
}
