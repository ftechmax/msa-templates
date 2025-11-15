import { ComponentFixture, TestBed } from '@angular/core/testing';

import { FaultSnackBarComponent } from './fault-snack-bar.component';

describe('FaultSnackbarComponent', () => {
  let component: FaultSnackBarComponent;
  let fixture: ComponentFixture<FaultSnackBarComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [FaultSnackBarComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(FaultSnackBarComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
