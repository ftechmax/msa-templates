import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ExampleCreateComponent } from './example-create.component';

describe('ExampleCreateComponent', () => {
  let component: ExampleCreateComponent;
  let fixture: ComponentFixture<ExampleCreateComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ExampleCreateComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(ExampleCreateComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
