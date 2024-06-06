import { Component, OnDestroy, OnInit } from '@angular/core';
import { Subscription } from 'rxjs';
import { EventService } from '../../status.service';
import { ExampleCollectionDto } from '../contracts';
import { ExampleHttpClient } from '../httpclient';
import { ExampleCreateComponent } from '../create/example-create.component';

@Component({
  selector: 'app-example-collection',
  standalone: true,
  imports: [],
  templateUrl: './example-collection.component.html',
  styleUrl: './example-collection.component.scss'
})
export class ExampleCollectionComponent implements OnInit, OnDestroy {
  model = [] as ExampleCollectionDto[];
  event$: Subscription | null = null;

  constructor(private http: ExampleHttpClient, private eventService: EventService) {}

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
    this.http.getCollection().subscribe((response) => {
      this.model = response;
    });
  }

  onCreate() {
    // const dialogRef = this.dialog.open(ExampleCreateComponent);

    // dialogRef.afterClosed().subscribe(() => {});
  }
}
