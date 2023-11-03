import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ExampleRoutingModule } from './example-routing.module';
import { CollectionComponent } from './collection/collection.component';
import { DetailsComponent } from './details/details.component';
import { CreateComponent } from './create/create.component';
import { ExampleHttpClient } from './example.httpclient';
import { EventService } from './event.service';
import { TranslocoModule } from '@ngneat/transloco';
import { ReactiveFormsModule } from '@angular/forms';
import { HeaderModule } from '../core/header/header.module';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import { MatPaginatorModule } from '@angular/material/paginator';
import { MatTableModule } from '@angular/material/table';

@NgModule({
  declarations: [CollectionComponent, DetailsComponent, CreateComponent],
  imports: [CommonModule, ExampleRoutingModule, TranslocoModule, ReactiveFormsModule, HeaderModule, MatCardModule, MatButtonModule, MatInputModule, MatIconModule, MatTableModule, MatPaginatorModule],
  providers: [ExampleHttpClient, EventService],
})
export class ExampleModule {}
