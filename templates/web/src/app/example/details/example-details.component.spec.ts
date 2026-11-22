import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ExampleDetailsComponent } from './example-details.component';
import { provideHttpClient } from '@angular/common/http';
import {
  HttpTestingController,
  provideHttpClientTesting,
} from '@angular/common/http/testing';
import { provideRouter } from '@angular/router';
import { EventService } from '../../status.service';

describe('ExampleDetailsComponent', () => {
  let component: ExampleDetailsComponent;
  let fixture: ComponentFixture<ExampleDetailsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ExampleDetailsComponent],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([]),
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(ExampleDetailsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should reload only for events about its own example', () => {
    const http = TestBed.inject(HttpTestingController);
    const events = TestBed.inject(EventService);
    component.id = 'example-1';
    http.expectOne('/api/example/example-1').flush({});

    events.ExampleUpdatedEvent.next({ correlationId: 'c', id: 'other' });
    http.expectNone('/api/example/example-1');

    events.ExampleRemoteCodeSetEvent.next({ correlationId: 'c', id: 'example-1' });
    http.expectOne('/api/example/example-1').flush({});

    events.ExampleUpdatedEvent.next({ correlationId: 'c', id: 'example-1' });
    http.expectOne('/api/example/example-1').flush({});
  });
});
