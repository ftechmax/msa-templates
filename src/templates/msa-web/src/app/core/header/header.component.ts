import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';

@Component({
  selector: 'app-header',
  templateUrl: './header.component.html',
  styleUrls: ['./header.component.scss'],
})
export class HeaderComponent implements OnInit {
  @Input() titleText: string | undefined;
  @Input() buttonText: string | undefined;
  @Input() enabled: boolean = true;
  @Output() open: EventEmitter<any> = new EventEmitter();

  constructor() {}

  ngOnInit(): void {}
  openDialog() {
    this.open.emit(null);
  }
}
