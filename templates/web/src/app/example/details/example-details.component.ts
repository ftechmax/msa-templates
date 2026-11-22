import { Component, Input, OnDestroy, signal } from '@angular/core';
import { ExampleHttpClient } from '../httpclient';
import { RouterLink } from '@angular/router';
import { ExampleDetailsDto } from '../contracts';
import { Subscription, filter, finalize, merge } from 'rxjs';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { EventService } from '../../status.service';

@Component({
  selector: 'app-example-details',
  imports: [
    RouterLink,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
  ],
  providers: [ExampleHttpClient],
  templateUrl: './example-details.component.html',
  styleUrl: './example-details.component.scss',
})
export class ExampleDetailsComponent implements OnDestroy {
  private details$: Subscription | undefined;
  private events$: Subscription;
  private currentId = '';

  model = signal<ExampleDetailsDto | null>(null);
  loading = signal(false);
  error = signal(false);

  @Input()
  set id(value: string) {
    if (!value || value === this.currentId) {
      return;
    }

    this.currentId = value;
    this.loadDetails(value);
  }

  get id() {
    return this.currentId;
  }

  constructor(
    private http: ExampleHttpClient,
    eventService: EventService
  ) {
    this.events$ = merge(
      eventService.ExampleUpdatedEvent,
      eventService.ExampleRemoteCodeSetEvent
    )
      .pipe(filter((event) => event.id === this.currentId))
      .subscribe(() => this.loadDetails(this.currentId, false));
  }

  ngOnDestroy(): void {
    this.details$?.unsubscribe();
    this.events$.unsubscribe();
  }

  private loadDetails(id: string, showSpinner = true) {
    this.details$?.unsubscribe();
    this.loading.set(showSpinner);
    this.error.set(false);

    this.details$ = this.http
      .getDetails(id)
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: (data) => this.model.set(data),
        error: () => {
          this.model.set(null);
          this.error.set(true);
        },
      });
  }
}
