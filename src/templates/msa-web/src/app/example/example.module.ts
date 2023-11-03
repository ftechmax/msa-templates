import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { ExampleRoutingModule } from './example-routing.module';
import { CollectionComponent } from './collection/collection.component';
import { DetailsComponent } from './details/details.component';
import { CreateComponent } from './create/create.component';

@NgModule({
  declarations: [CollectionComponent, DetailsComponent, CreateComponent],
  imports: [CommonModule, ExampleRoutingModule],
})
export class ExampleModule {}
