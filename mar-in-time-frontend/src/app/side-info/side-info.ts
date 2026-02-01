import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'side-info',
  imports: [CommonModule],
  templateUrl: './side-info.html',
  styleUrl: './side-info.css',
})
export class SideInfo {
  isOpen: boolean = false;

  onToggle() {
    this.isOpen = !this.isOpen;
  }
}
