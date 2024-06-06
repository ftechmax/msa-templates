import { Inject, Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { EventService } from './status.service';
import { ExampleCreatedEvent } from './example/contracts';

@Injectable({
  providedIn: 'root',
})
export class SignalRService {
  constructor(@Inject('BASE_API_URL') private hubApiUrl: string, private eventService: EventService) {}

  private hubConnection: signalR.HubConnection | undefined;

  public startConnection = () => {
    this.hubConnection = new signalR.HubConnectionBuilder().withUrl(`${this.hubApiUrl}/api-hub`).build();
    this.hubConnection
      .start()
      .then(() => console.log('SignalR generator API connection started'))
      .catch((err) => console.log('Error while starting SignalR generator API connection: ' + err));
  };

  public stopConnection = () => {
    if (this.hubConnection) {
      this.hubConnection.stop().finally(() => {
        this.hubConnection = undefined;
      });
    }
  };

  public addStatusListener = () => {
    var that = this;
    if (this.hubConnection) {
      // Example
      this.hubConnection.on('ExampleCreatedEvent', (data: ExampleCreatedEvent) => {
        that.eventService.CustomerCreatedEvent.next(data);
      });

    }
  };
}
