// import { DataSource } from '@angular/cdk/collections';
// import { MatPaginator } from '@angular/material/paginator';
// import { MatSort } from '@angular/material/sort';
// import { catchError, map } from 'rxjs/operators';
// import { Observable, of as observableOf, merge } from 'rxjs';
// import { ExampleCollectionDto } from '../contracts';
// import { ExampleHttpClient } from '../httpclient';

// // TODO: replace this with real data from your application
// // const EXAMPLE_DATA: ExampleCollectionDto[] = [
// //   { id: '', name: 'Hydrogen' },
// //   { id: '', name: 'Helium' },
// //   { id: '', name: 'Lithium' },
// //   { id: '', name: 'Beryllium' },
// //   { id: '', name: 'Boron' },
// //   { id: '', name: 'Carbon' },
// //   { id: '', name: 'Nitrogen' },
// //   { id: '', name: 'Oxygen' },
// //   { id: '', name: 'Fluorine' },
// //   { id: '', name: 'Neon' },
// //   { id: '', name: 'Sodium' },
// //   { id: '', name: 'Magnesium' },
// //   { id: '', name: 'Aluminum' },
// //   { id: '', name: 'Silicon' },
// //   { id: '', name: 'Phosphorus' },
// //   { id: '', name: 'Sulfur' },
// //   { id: '', name: 'Chlorine' },
// //   { id: '', name: 'Argon' },
// //   { id: '', name: 'Potassium' },
// //   { id: '', name: 'Calcium' },
// // ];

// /**
//  * Data source for the Table view. This class should
//  * encapsulate all logic for fetching and manipulating the displayed data
//  * (including sorting, pagination, and filtering).
//  */
// export class ExampleDataSource extends DataSource<ExampleCollectionDto> {
//   data: ExampleCollectionDto[] = []; // EXAMPLE_DATA;
//   paginator: MatPaginator | undefined;
//   sort: MatSort | undefined;

//   constructor(private http: ExampleHttpClient) {
//     super();
//   }

//   /**
//    * Connect this data source to the table. The table will only update when
//    * the returned stream emits new items.
//    * @returns A stream of the items to be rendered.
//    */
//   connect(): Observable<ExampleCollectionDto[]> {
//     if (this.paginator && this.sort) {
//       // Combine everything that affects the rendered data into one update
//       // stream for the data-table to consume.
//       return merge(
//         //observableOf(this.data),
//         this.paginator.page,
//         this.sort.sortChange
//       ).pipe(
//         map(() => {
//           //return this.getPagedData(this.getSortedData([...this.data]));
//           return this.http.getCollectionPaged(
//             this.sort.active,
//             this.sort.direction,
//             this.paginator.pageIndex,
//             this.paginator.pageSize
//           ).pipe(catchError(() => observableOf(null)));
//         })
//       );
//     } else {
//       throw Error(
//         'Please set the paginator and sort on the data source before connecting.'
//       );
//     }
//   }

//   /**
//    *  Called when the table is being destroyed. Use this function, to clean up
//    * any open connections or free any held resources that were set up during connect.
//    */
//   disconnect(): void {}

//   // /**
//   //  * Paginate the data (client-side). If you're using server-side pagination,
//   //  * this would be replaced by requesting the appropriate data from the server.
//   //  */
//   // private getPagedData(data: ExampleCollectionDto[]): ExampleCollectionDto[] {
//   //   if (this.paginator) {
//   //     const startIndex = this.paginator.pageIndex * this.paginator.pageSize;
//   //     return data.splice(startIndex, this.paginator.pageSize);
//   //   } else {
//   //     return data;
//   //   }
//   // }

//   // /**
//   //  * Sort the data (client-side). If you're using server-side sorting,
//   //  * this would be replaced by requesting the appropriate data from the server.
//   //  */
//   // private getSortedData(data: ExampleCollectionDto[]): ExampleCollectionDto[] {
//   //   if (!this.sort || !this.sort.active || this.sort.direction === '') {
//   //     return data;
//   //   }

//   //   return data.sort((a, b) => {
//   //     const isAsc = this.sort?.direction === 'asc';
//   //     switch (this.sort?.active) {
//   //       case 'name':
//   //         return compare(a.name, b.name, isAsc);
//   //       case 'id':
//   //         return compare(+a.id, +b.id, isAsc);
//   //       default:
//   //         return 0;
//   //     }
//   //   });
//   // }
// }

// /** Simple sort comparator for example ID/Name columns (for client-side sorting). */
// function compare(
//   a: string | number,
//   b: string | number,
//   isAsc: boolean
// ): number {
//   return (a < b ? -1 : 1) * (isAsc ? 1 : -1);
// }
