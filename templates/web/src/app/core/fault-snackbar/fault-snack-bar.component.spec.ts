import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MAT_SNACK_BAR_DATA, MatSnackBarRef } from '@angular/material/snack-bar';

import { FaultSnackBarComponent } from './fault-snack-bar.component';
import { DomainFault } from '../contracts';

describe('FaultSnackbarComponent', () => {
  let component: FaultSnackBarComponent;
  let fixture: ComponentFixture<FaultSnackBarComponent>;

  const fault: DomainFault = {
    correlationId: 'c2a1f0d4-0000-4000-8000-000000000000',
    message: 'Something went wrong',
    traceId: 'trace-123',
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [FaultSnackBarComponent],
      providers: [
        { provide: MatSnackBarRef, useValue: { dismissWithAction: () => {} } },
        { provide: MAT_SNACK_BAR_DATA, useValue: fault },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(FaultSnackBarComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should show the trace id so it can be passed to the helpdesk', () => {
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.textContent).toContain(fault.traceId);
  });
});
