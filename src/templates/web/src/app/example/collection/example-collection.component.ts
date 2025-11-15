import {
  AfterViewInit,
  Component,
  OnDestroy,
  signal,
  ViewChild,
} from '@angular/core';
import {
  Subject,
  Subscription,
  catchError,
  debounceTime,
  map,
  merge,
  of,
  startWith,
  switchMap,
} from 'rxjs';
import { EventService } from '../../status.service';
import { ExampleCollectionDto } from '../contracts';
import { ExampleHttpClient } from '../httpclient';
import { Router, RouterLink } from '@angular/router';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { MatTableModule } from '@angular/material/table';
import { MatProgressSpinner } from '@angular/material/progress-spinner';
import { DatePipe, NgIf } from '@angular/common';
import { MatIcon } from '@angular/material/icon';

@Component({
  selector: 'app-example-collection',
  providers: [ExampleHttpClient],
  templateUrl: './example-collection.component.html',
  styleUrl: './example-collection.component.scss',
  imports: [
    NgIf,
    DatePipe,
    RouterLink,
    MatTableModule,
    MatPaginatorModule,
    MatSortModule,
    MatProgressSpinner,
    MatIcon,
  ],
})
export class ExampleCollectionComponent implements AfterViewInit, OnDestroy {
  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  displayedColumns = ['name', 'created', 'actions'];
  model = [] as ExampleCollectionDto[];
  event$: Subscription | null = null;
  data$: Subscription | undefined;
  spinner = signal(false);

  private isLoading = new Subject<boolean>();
  private debouncedLoading$: Subscription;

  constructor(
    private readonly router: Router,
    private readonly http: ExampleHttpClient,
    private readonly eventService: EventService
  ) {
    this.debouncedLoading$ = this.isLoading
      .pipe(debounceTime(200))
      .subscribe((loading) => {
        if (loading) {
          this.spinner.set(true);
        } else {
          this.spinner.set(false);
        }
      });
  }

  ngAfterViewInit(): void {
    this.sort.sortChange.subscribe(() => (this.paginator.pageIndex = 0));

    this.data$ = merge(
      this.sort.sortChange,
      this.paginator.page,
      this.eventService.ExampleCreatedEvent
    )
      .pipe(
        startWith([]),
        switchMap(() => {
          this.isLoading.next(true);
          return this.http
            .getCollectionPaged(
              this.sort.active,
              this.sort.direction,
              this.paginator.pageIndex,
              this.paginator.pageSize
            )
            .pipe(catchError(() => of(null)));
        }),
        map((data) => {
          this.isLoading.next(false);

          if (data === null) {
            return [];
          }

          return data;
        })
      )
      .subscribe((data) => (this.model = data));
  }

  ngOnDestroy(): void {
    this.event$?.unsubscribe();
    this.data$?.unsubscribe();
    this.isLoading?.complete();
    this.debouncedLoading$?.unsubscribe();
  }

  onCreate() {
    this.router.navigate(['/example', 'create']);
  }
}
