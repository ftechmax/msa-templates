import { Injectable } from '@angular/core';
import { Subject } from 'rxjs';
import { IExampleCreatedEvent } from './example-contracts';
import { HubConnection } from '@microsoft/signalr';
import { SignalRService } from '../core/signalr.service';

@Injectable({
  providedIn: 'root',
})
export class ExampleEventService {
  public ExampleCreatedEvent: Subject<IExampleCreatedEvent>;

  constructor(signalrService: SignalRService) {
    this.ExampleCreatedEvent = new Subject();

    this.register(signalrService.hubConnection);
  }

  register(hubConnection: HubConnection) {
    let that = this;
    hubConnection.on('IExampleCreatedEvent', (data) => {
      that.ExampleCreatedEvent.next(data);
    });
  }
}
