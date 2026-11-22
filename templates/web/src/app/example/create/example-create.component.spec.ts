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
import { TranslocoTestingModule } from '@jsverse/transloco';

import { ExampleCreateComponent } from './example-create.component';
import { EventService } from '../../status.service';

describe('ExampleCreateComponent', () => {
  let component: ExampleCreateComponent;
  let fixture: ComponentFixture<ExampleCreateComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        ExampleCreateComponent,
        TranslocoTestingModule.forRoot({
          langs: { en: {} },
          translocoConfig: { availableLangs: ['en'], defaultLang: 'en' },
        }),
      ],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideNoopAnimations(),
        provideRouter([]),
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(ExampleCreateComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should not submit an empty form', () => {
    component.submit();
    expect(component.form.valid).toBe(false);
    expect(component.submitting()).toBe(false);
  });

  function submit() {
    component.form.patchValue({ name: 'Name', description: 'Description' });
    component.submit();
    const request = TestBed.inject(HttpTestingController).expectOne('/api/example/');
    request.flush(null, { status: 202, statusText: 'Accepted' });
    return request.request.body.correlationId as string;
  }

  it('should navigate to the details route when its create succeeds', () => {
    const navigate = vi.spyOn(TestBed.inject(Router), 'navigate').mockResolvedValue(true);
    const correlationId = submit();

    TestBed.inject(EventService).ExampleCreatedEvent.next({ correlationId, id: 'example-1' });

    expect(navigate).toHaveBeenCalledWith(['/example', 'example-1']);
  });

  it('should show a snackbar and allow a retry only for its own fault', () => {
    const open = vi.spyOn(TestBed.inject(MatSnackBar), 'openFromComponent');
    const events = TestBed.inject(EventService);
    const correlationId = submit();

    events.CreateExampleFault.next({ correlationId: 'other', message: 'x', traceId: 't' });
    expect(open).not.toHaveBeenCalled();
    expect(component.submitting()).toBe(true);

    events.CreateExampleFault.next({ correlationId, message: 'x', traceId: 't' });
    expect(open).toHaveBeenCalledOnce();
    expect(component.submitting()).toBe(false);
  });
});
