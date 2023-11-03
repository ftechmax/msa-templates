import { Component, OnDestroy, OnInit } from '@angular/core';
import { ExampleCollectionDto } from '../contracts';
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
    private httpClient: ExampleHttpClient,
    private eventService: EventService
  ) {}

  ngOnInit(): void {
    this.load();
    this.subscribe();
  }

  ngOnDestroy(): void {
    this.unsubscribe();
  }

  private load() {
    this.httpClient.getCollection().subscribe((response) => {
      this.model = response;
    });
  }

  private subscribe() {
    this.event$ = this.eventService.ExampleCreatedEvent.pipe().subscribe(() => {
      this.load();
    });
  }

  private unsubscribe() {
    this.event$?.unsubscribe();
  }
}
