import { Component, OnDestroy, OnInit } from '@angular/core';
import { Subscription } from 'rxjs';
import { EventService } from '../../status.service';
import { ExampleCollectionDto } from '../contracts';
import { ExampleHttpClient } from '../httpclient';
import { ExampleCreateComponent } from '../create/example-create.component';
import { JsonPipe } from '@angular/common';
import { Router, RouterLink } from '@angular/router';

@Component({
  selector: 'app-example-collection',
  standalone: true,
  imports: [RouterLink, JsonPipe],
  providers: [ExampleHttpClient],
  templateUrl: './example-collection.component.html',
  styleUrl: './example-collection.component.scss',
})
export class ExampleCollectionComponent implements OnInit, OnDestroy {
  model = [] as ExampleCollectionDto[];
  event$: Subscription | null = null;

  constructor(
    private readonly router: Router,
    private readonly http: ExampleHttpClient,
    private readonly eventService: EventService
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
    this.http.getCollection().subscribe((response) => {
      this.model = response;
    });
  }

  onCreate() {
    // const dialogRef = this.dialog.open(ExampleCreateComponent);
    // dialogRef.afterClosed().subscribe(() => {});
    this.router.navigate(['/example', 'create']);
  }
}
