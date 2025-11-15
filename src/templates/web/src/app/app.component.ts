import { Component, OnDestroy, OnInit } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { LayoutComponent } from './layout/layout.component';
import { SignalRService } from './signalr.service';

@Component({
    selector: 'app-root',
    templateUrl: './app.component.html',
    styleUrl: './app.component.scss',
    imports: [RouterOutlet, LayoutComponent]
})
export class AppComponent implements OnInit, OnDestroy {
  title = 'web';

  constructor(private signalrService: SignalRService) {}

  ngOnInit(): void {
    this.signalrService.startConnection();
    this.signalrService.addStatusListener();
  }

  ngOnDestroy(): void {
    this.signalrService.stopConnection();
  }
}
