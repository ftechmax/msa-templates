import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ExampleUpdateComponent } from './example-update.component';

describe('ExampleUpdateComponent', () => {
  let component: ExampleUpdateComponent;
  let fixture: ComponentFixture<ExampleUpdateComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ExampleUpdateComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(ExampleUpdateComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
