import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import {
  HttpTestingController,
  provideHttpClientTesting,
} from '@angular/common/http/testing';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { provideRouter, Router } from '@angular/router';
import { MatSnackBar } from '@angular/material/snack-bar';
import { vi } from 'vitest';

import { ExampleUpdateComponent } from './example-update.component';
import { EventService } from '../../status.service';

describe('ExampleUpdateComponent', () => {
  let component: ExampleUpdateComponent;
  let fixture: ComponentFixture<ExampleUpdateComponent>;
  let http: HttpTestingController;
  let events: EventService;

  const details = {
    id: 'example-1',
    name: 'Name',
    description: 'Description',
    exampleValueObject: { code: 'abc', value: 1.5 },
    remoteCode: null,
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ExampleUpdateComponent],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideNoopAnimations(),
        provideRouter([]),
      ],
    }).compileComponents();

    http = TestBed.inject(HttpTestingController);
    events = TestBed.inject(EventService);
    fixture = TestBed.createComponent(ExampleUpdateComponent);
    component = fixture.componentInstance;
    component.id = details.id;
    fixture.detectChanges();
    http.expectOne('/api/example/example-1').flush(details);
  });

  function submit() {
    component.submit();
    const request = http.expectOne('/api/example/example-1');
    expect(request.request.method).toBe('PUT');
    request.flush(null, { status: 202, statusText: 'Accepted' });
    return request.request.body.correlationId as string;
  }

  it('should load the current values into the form', () => {
    expect(component.form.value.description).toBe('Description');
    expect(component.form.value.exampleValueObject?.code).toBe('abc');
  });

  it('should navigate to details when its update succeeds', () => {
    const navigate = vi.spyOn(TestBed.inject(Router), 'navigate').mockResolvedValue(true);
    const correlationId = submit();

    events.ExampleUpdatedEvent.next({ correlationId: 'other', id: details.id });
    expect(navigate).not.toHaveBeenCalled();

    events.ExampleUpdatedEvent.next({ correlationId, id: details.id });
    expect(navigate).toHaveBeenCalledWith(['/example', details.id]);
  });

  it('should show a snackbar and allow a retry when its update faults', () => {
    const open = vi.spyOn(TestBed.inject(MatSnackBar), 'openFromComponent');
    const correlationId = submit();

    events.UpdateExampleFault.next({ correlationId: 'other', message: 'x', traceId: 't' });
    expect(open).not.toHaveBeenCalled();
    expect(component.submitting()).toBe(true);

    events.UpdateExampleFault.next({ correlationId, message: 'x', traceId: 't' });
    expect(open).toHaveBeenCalledOnce();
    expect(component.submitting()).toBe(false);
  });
});
