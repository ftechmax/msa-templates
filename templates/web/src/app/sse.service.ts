import { Injectable } from '@angular/core';
import { Subject } from 'rxjs';
import { EventService } from './status.service';

@Injectable({
  providedIn: 'root',
})
export class SseService {
  constructor(private eventService: EventService) {}

  private eventSource: EventSource | undefined;

  public connect = () => {
    this.eventSource = new EventSource('/api/events');

    this.forward('ExampleCreatedEvent', this.eventService.ExampleCreatedEvent);
    this.forward('ExampleUpdatedEvent', this.eventService.ExampleUpdatedEvent);
    this.forward('ExampleRemoteCodeSetEvent', this.eventService.ExampleRemoteCodeSetEvent);
    this.forward('DomainFault_CreateExampleCommand', this.eventService.CreateExampleFault);
    this.forward('DomainFault_UpdateExampleCommand', this.eventService.UpdateExampleFault);

    this.eventSource.onopen = () => console.log('SSE connection started');

    this.eventSource.onerror = () => {
      if (this.eventSource?.readyState === EventSource.CONNECTING) {
        console.log('SSE connection lost, reconnecting...');
      }
    };
  };

  public disconnect = () => {
    if (this.eventSource) {
      this.eventSource.close();
      this.eventSource = undefined;
    }
  };

  private forward<T>(eventName: string, target: Subject<T>) {
    this.eventSource?.addEventListener(eventName, (event) => {
      target.next(JSON.parse((event as MessageEvent).data));
    });
  }
}
