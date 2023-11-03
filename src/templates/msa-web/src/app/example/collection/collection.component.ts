import { Component, OnDestroy, OnInit } from '@angular/core';
import { ExampleCollectionDto } from '../example-contracts';
import { Subscription } from 'rxjs';
import { EventService } from '../event.service';
import { ExampleHttpClient } from '../example.httpclient';

@Component({
  templateUrl: './collection.component.html',
  styleUrls: ['./collection.component.scss'],
})
export class CollectionComponent implements OnInit, OnDestroy {
  public model = [] as ExampleCollectionDto[];
  private event$: Subscription | undefined;

  constructor(
    private apiHttpClient: ExampleHttpClient,
    private eventService: EventService
  ) {}

  ngOnInit(): void {
    this.load();
    this.event$ = this.eventService.ExampleCreatedEvent.pipe().subscribe(() => {
      this.load();
    });
  }

  ngOnDestroy(): void {
    this.event$?.unsubscribe();
  }

  private load() {
    this.apiHttpClient.getCollection().subscribe((response) => {
      this.model = response;
    });
  }
}
