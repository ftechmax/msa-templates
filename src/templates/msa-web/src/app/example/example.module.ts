import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { ExampleRoutingModule } from './example-routing.module';
import { CollectionComponent } from './collection/collection.component';
import { DetailsComponent } from './details/details.component';
import { CreateComponent } from './create/create.component';
import { ExampleHttpClient } from './example.httpclient';
import { ExampleEventService } from './example-event.service';

@NgModule({
  declarations: [CollectionComponent, DetailsComponent, CreateComponent],
  imports: [CommonModule, ExampleRoutingModule],
  providers: [ExampleHttpClient, ExampleEventService],
})
export class ExampleModule {}
