import { Injectable } from '@angular/core';
import { EventService } from './status.service';
import { ExampleCreatedEvent } from './example/contracts';
import { DomainFault } from './core/contracts';

@Injectable({
  providedIn: 'root',
})
export class SseService {
  constructor(private eventService: EventService) {}

  private eventSource: EventSource | undefined;

  public connect = () => {
    this.eventSource = new EventSource('/api/events');

    this.eventSource.addEventListener('ExampleCreatedEvent', (event) => {
      const data: ExampleCreatedEvent = JSON.parse(event.data);
      this.eventService.ExampleCreatedEvent.next(data);
    });

    this.eventSource.addEventListener('DomainFault_CreateExampleCommand', (event) => {
      const data: DomainFault = JSON.parse(event.data);
      this.eventService.CreateExampleFault.next(data);
    });

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
}
