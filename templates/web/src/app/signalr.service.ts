import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { EventService } from './status.service';
import { ExampleCreatedEvent } from './example/contracts';
import { HttpClient } from '@angular/common/http';
import { DomainFault } from './core/contracts';

@Injectable({
  providedIn: 'root',
})
export class SignalRService {
  constructor(private eventService: EventService, private http: HttpClient) {}

  private hubConnection: signalR.HubConnection | undefined;

  public startConnection = () => {
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(`/api-hub`)
      .build();
    this.hubConnection
      .start()
      .then(() => console.log('SignalR connection started'))
      .catch((err) =>
        console.log('Error while starting SignalR connection: ' + err)
      );
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
      this.hubConnection.on(
        'ExampleCreatedEvent',
        (data: ExampleCreatedEvent) => {
          that.eventService.ExampleCreatedEvent.next(data);
        }
      );

      this.hubConnection.on(
        'DomainFault_CreateExampleCommand',
        (data: DomainFault) => {
          that.eventService.CreateExampleFault.next(data);
        }
      );
    }
  };
}
