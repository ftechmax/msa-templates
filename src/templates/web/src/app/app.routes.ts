import { Routes } from '@angular/router';
import { PageNotFoundComponent } from './core/page-not-found/page-not-found.component';

export const routes: Routes = [
  {path: 'example', loadChildren: () => import('./example/routes')},
  {path:'', pathMatch: 'full', redirectTo: '/example'},
  {path: '**', component:PageNotFoundComponent}
];
