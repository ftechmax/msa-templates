import { Injectable } from '@angular/core';
import { Subject } from 'rxjs';
import { IExampleCreatedEvent, IExampleUpdatedEvent } from './contracts';
import { HubConnection } from '@microsoft/signalr';
import { SignalRService } from '../core/signalr.service';

@Injectable()
export class EventService {
  public ExampleCreatedEvent = new Subject<IExampleCreatedEvent>();
  public ExampleUpdatedEvent = new Subject<IExampleUpdatedEvent>();

  constructor(signalrService: SignalRService) {
    this.register(signalrService.hubConnection);
  }

  register(hubConnection: HubConnection) {
    let that = this;

    hubConnection.on('IExampleCreatedEvent', (data) => {
      that.ExampleCreatedEvent.next(data);
    });

    hubConnection.on('IExampleUpdatedEvent', (data) => {
      that.ExampleUpdatedEvent.next(data);
    });
  }
}
