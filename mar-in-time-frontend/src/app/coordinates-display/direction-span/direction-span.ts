import { Component, Input } from '@angular/core';

@Component({
  selector: 'direction-span',
  imports: [],
  templateUrl: './direction-span.html',
  styleUrl: './direction-span.css',
})
export class DirectionSpan {

  posText: string = "";
  negText: string = "";
  resultText: string = this.posText;

  posColor: string = "";
  negColor: string = "";
  resultColor: string = this.posColor;

  @Input()
  set positiveText(val: string) {
    this.posText = val;
  }

  @Input()
  set negativeText(val: string) {
    this.negText = val;
  }

  @Input()
  set positiveColor(val: string) {
    this.posColor = val;
 }

  @Input()
  set negativeColor(val: string) {
    this.negColor = val;
  }

  @Input()
  set direction(val: number) {
    this.resultText = val >= 0 ? this.posText : this.negText;
    this.resultColor = val >= 0 ? this.posColor : this.negColor;
  }
}
