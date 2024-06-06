import { Injectable } from '@angular/core';
import { Subject } from 'rxjs';
import { ExampleCreatedEvent, ExampleUpdatedEvent } from './example/contracts';


@Injectable({
  providedIn: 'root',
})
export class EventService {
  // Example
  ExampleCreatedEvent: Subject<ExampleCreatedEvent> = new Subject<ExampleCreatedEvent>();
  ExampleUpdatedEvent: Subject<ExampleUpdatedEvent> = new Subject<ExampleUpdatedEvent>();
}
