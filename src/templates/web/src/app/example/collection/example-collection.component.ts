import {
  AfterViewInit,
  Component,
  OnDestroy,
  OnInit,
  ViewChild,
} from '@angular/core';
import { Subscription } from 'rxjs';
import { EventService } from '../../status.service';
import { ExampleCollectionDto } from '../contracts';
import { ExampleHttpClient } from '../httpclient';
import { JsonPipe } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { MatTable, MatTableModule } from '@angular/material/table';
import { ExampleDataSource } from './example.datasource';

@Component({
  selector: 'app-example-collection',
  standalone: true,
  providers: [ExampleHttpClient],
  templateUrl: './example-collection.component.html',
  styleUrl: './example-collection.component.scss',
  imports: [
    RouterLink,
    JsonPipe,
    MatTableModule,
    MatPaginatorModule,
    MatSortModule,
  ],
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
  dataSource = new ExampleDataSource();

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

  ngAfterViewInit(): void {
    this.dataSource.sort = this.sort;
    this.dataSource.paginator = this.paginator;
    this.table.dataSource = this.dataSource;
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
