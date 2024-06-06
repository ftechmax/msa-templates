import { Route } from '@angular/router';
import { ExampleCollectionComponent } from './collection/example-collection.component';
import { ExampleCreateComponent } from './create/example-create.component';
import { ExampleUpdateComponent } from './update/example-update.component';
import { ExampleDetailsComponent } from './details/example-details.component';

export default [
    {path: '', component: ExampleCollectionComponent},
    {path: 'create', component: ExampleCreateComponent},
    {path: ':id', component: ExampleDetailsComponent},
    {path: 'update/:id', component: ExampleUpdateComponent},
] satisfies Route[];
