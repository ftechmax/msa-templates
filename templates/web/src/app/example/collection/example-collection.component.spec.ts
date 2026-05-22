import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ExampleCollectionComponent } from './example-collection.component';

describe('ExampleCollectionComponent', () => {
  let component: ExampleCollectionComponent;
  let fixture: ComponentFixture<ExampleCollectionComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ExampleCollectionComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(ExampleCollectionComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
