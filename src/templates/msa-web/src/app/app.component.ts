import { AfterViewInit, Component, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { SignalRService } from './core/signalr.service';
import { MatSidenavContainer } from '@angular/material/sidenav';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss'],
})
export class AppComponent implements OnInit, OnDestroy, AfterViewInit {
  @ViewChild(MatSidenavContainer) sidenavContainer: MatSidenavContainer | undefined;

  constructor(private signalrService: SignalRService) {}

  ngOnInit(): void {
    this.signalrService.startConnection();
  }

  ngOnDestroy(): void {
    this.signalrService.stopConnection();
  }

  ngAfterViewInit() {
    this.sidenavContainer!.scrollable.elementScrolled().subscribe(() => {
      /* react to scrolling */
    });
  }
}
