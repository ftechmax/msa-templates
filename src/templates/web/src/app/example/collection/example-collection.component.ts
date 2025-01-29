import {
  AfterViewInit,
  Component,
  OnDestroy,
  OnInit,
  ViewChild,
} from '@angular/core';
import {
  Subscription,
  catchError,
  map,
  merge,
  of,
  startWith,
  switchMap,
} from 'rxjs';
import { EventService } from '../../status.service';
import { ExampleCollectionDto } from '../contracts';
import { ExampleHttpClient } from '../httpclient';
import { JsonPipe } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { MatTable, MatTableModule } from '@angular/material/table';
//import { ExampleDataSource } from './example.datasource';

@Component({
    selector: 'app-example-collection',
    providers: [ExampleHttpClient],
    templateUrl: './example-collection.component.html',
    styleUrl: './example-collection.component.scss',
    imports: [
        RouterLink,
        JsonPipe,
        MatTableModule,
        MatPaginatorModule,
        MatSortModule,
    ]
})
export class ExampleCollectionComponent
  implements OnInit, OnDestroy, AfterViewInit
{
  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;
  @ViewChild(MatTable) table!: MatTable<ExampleCollectionDto>;
  model = [] as ExampleCollectionDto[];
  event$: Subscription | null = null;
  displayedColumns = ['id', 'name'];
  //dataSource = new ExampleDataSource(this.http);

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

  // ngAfterViewInit(): void {
  //   this.dataSource.sort = this.sort;
  //   this.dataSource.paginator = this.paginator;
  //   this.table.dataSource = this.dataSource;
  // }

  ngAfterViewInit() {
    // If the user changes the sort order, reset back to the first page.
    this.sort.sortChange.subscribe(() => (this.paginator.pageIndex = 0));

    merge(this.sort.sortChange, this.paginator.page)
      .pipe(
        startWith({}),
        switchMap(() => {
          //this.isLoadingResults = true;
          return this.http!.getCollectionPaged(
            this.sort.active,
            this.sort.direction,
            this.paginator.pageIndex,
            this.paginator.pageSize
          ).pipe(catchError(() => of(null)));
        }),
        map((data) => {
          // Flip flag to show that loading has finished.
          // this.isLoadingResults = false;
          // this.isRateLimitReached = data === null;

          if (data === null) {
            return [];
          }

          // Only refresh the result length if there is new data. In case of rate
          // limit errors, we do not want to reset the paginator to zero, as that
          // would prevent users from re-triggering requests.
          //this.resultsLength = data?.length || 0;
          return data;
        })
      )
      .subscribe((data) => (this.model = data));
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
