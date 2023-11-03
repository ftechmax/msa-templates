import { Component, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Subscription } from 'rxjs';
import { EventService } from '../event.service';
import { ExampleHttpClient } from '../example.httpclient';
import { ExampleDetailsDto } from '../contracts';

@Component({
  templateUrl: './details.component.html',
  styleUrls: ['./details.component.scss'],
})
export class DetailsComponent implements OnInit, OnDestroy {
  public model = {} as ExampleDetailsDto;
  private id: string | null;

  events$: Subscription[] = [];

  constructor(
    private route: ActivatedRoute,
    private httpClient: ExampleHttpClient,
    private eventService: EventService
  ) {
    this.id = this.route.snapshot.queryParamMap.get('id');
  }

  ngOnInit(): void {
    if (!this.id) {
      return;
    }

    this.load(this.id);
    this.subscribe(this.id);
  }

  ngOnDestroy(): void {
    this.unsubscribe();
  }

  private load(id: string) {
    this.httpClient.getDetails(id).subscribe((response) => {
      this.model = response;
    });
  }

  private subscribe(id: string) {
    this.events$.push(
      this.eventService.ExampleUpdatedEvent.pipe().subscribe((data) => {
        if (data.id == id) {
          this.load(id);
        }
      })
    );
  }

  private unsubscribe() {
    this.events$.forEach((i) => {
      i?.unsubscribe();
    });
  }
}
