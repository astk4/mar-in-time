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

  posClass: string = "";
  negClass: string = "";
  resultClass: string = this.posClass;

  @Input()
  set positiveText(val: string) {
    this.posText = val;
  }

  @Input()
  set negativeText(val: string) {
    this.negText = val;
  }

  @Input()
  set positiveClass(val: string) {
    this.posClass = val;
 }

  @Input()
  set negativeClass(val: string) {
    this.negClass = val;
  }

  @Input()
  set direction(val: number) {
    this.resultText = val >= 0 ? this.posText : this.negText;
    this.resultClass = val >= 0 ? this.posClass : this.negClass;
  }
}
