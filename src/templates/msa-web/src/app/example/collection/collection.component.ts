import { AfterViewInit, Component, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { ExampleCollectionDto } from '../contracts';
import { Subscription } from 'rxjs';
import { EventService } from '../event.service';
import { ExampleHttpClient } from '../example.httpclient';
import { MatTableDataSource } from '@angular/material/table';
import { MatPaginator } from '@angular/material/paginator';
import { Router } from '@angular/router';

@Component({
  templateUrl: './collection.component.html',
  styleUrls: ['./collection.component.scss'],
})
export class CollectionComponent implements OnInit, OnDestroy {
  @ViewChild(MatPaginator) paginator!: MatPaginator;

  public dataSource: ExampleCollectionDto[] = []; // = new MatTableDataSource<ExampleCollectionDto>();
  public displayedColumns: string[] = ['position', 'name'];

  private event$: Subscription | undefined;

  constructor(private router: Router, private httpClient: ExampleHttpClient, private eventService: EventService) {}

  ngOnInit(): void {
    this.load();
    this.subscribe();
  }

  // ngAfterViewInit() {
  //   this.dataSource.paginator = this.paginator;
  // }

  ngOnDestroy(): void {
    this.unsubscribe();
  }

  applyFilter(event: Event) {
    const filterValue = (event.target as HTMLInputElement).value;
    //this.dataSource.filter = filterValue.trim().toLowerCase();
  }

  onClick(row: ExampleCollectionDto) {
    this.router.navigate(['/example/details'], {
      queryParams: {
        id: row.position,
      },
    });
  }

  private load() {
    // this.httpClient.getCollection().subscribe((response) => {
    //   this.model = response;
    // });
    this.dataSource = ELEMENT_DATA;
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

const ELEMENT_DATA: ExampleCollectionDto[] = [
  { position: 1, name: 'Hydrogen' },
  { position: 2, name: 'Helium' },
  { position: 3, name: 'Lithium' },
  { position: 4, name: 'Beryllium' },
  { position: 5, name: 'Boron' },
  { position: 6, name: 'Carbon' },
  { position: 7, name: 'Nitrogen' },
  { position: 8, name: 'Oxygen' },
  { position: 9, name: 'Fluorine' },
  { position: 10, name: 'Neon' },
  { position: 11, name: 'Sodium' },
  { position: 12, name: 'Magnesium' },
  { position: 13, name: 'Aluminum' },
  { position: 14, name: 'Silicon' },
  { position: 15, name: 'Phosphorus' },
  { position: 16, name: 'Sulfur' },
  { position: 17, name: 'Chlorine' },
  { position: 18, name: 'Argon' },
  { position: 19, name: 'Potassium' },
  { position: 20, name: 'Calcium' },
];
