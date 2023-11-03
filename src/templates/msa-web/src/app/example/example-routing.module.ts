import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { DetailsComponent } from './details/details.component';
import { CollectionComponent } from './collection/collection.component';

const routes: Routes = [
  { path: 'collection', component: CollectionComponent },
  { path: 'details', component: DetailsComponent },
  { path: '', pathMatch: 'full', redirectTo: 'collection' },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class ExampleRoutingModule {}
