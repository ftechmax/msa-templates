import { Inject, Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';

@Injectable({
  providedIn: 'root',
})
export class SignalRService {
  constructor(@Inject('BASE_API_URL') private hubApiUrl: string) {
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(`${this.hubApiUrl}/api-hub`)
      .build();
  }

  public hubConnection: signalR.HubConnection;

  public startConnection() {
    this.hubConnection
      .start()
      .catch((err) =>
        console.log(
          'Error while starting SignalR generator API connection: ' + err
        )
      );
  }

  public stopConnection() {
    this.hubConnection.stop();
  }
}
