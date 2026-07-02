import { Component, OnDestroy, OnInit } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { LayoutComponent } from './layout/layout.component';
import { SseService } from './sse.service';

@Component({
    selector: 'app-root',
    templateUrl: './app.component.html',
    styleUrl: './app.component.scss',
    imports: [RouterOutlet, LayoutComponent]
})
export class AppComponent implements OnInit, OnDestroy {
  title = 'web';

  constructor(private sseService: SseService) {}

  ngOnInit(): void {
    this.sseService.connect();
  }

  ngOnDestroy(): void {
    this.sseService.disconnect();
  }
}
