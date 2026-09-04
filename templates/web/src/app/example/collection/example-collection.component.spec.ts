import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { provideRouter } from '@angular/router';

import { ExampleCollectionComponent } from './example-collection.component';

describe('ExampleCollectionComponent', () => {
  let component: ExampleCollectionComponent;
  let fixture: ComponentFixture<ExampleCollectionComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ExampleCollectionComponent],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideNoopAnimations(),
        provideRouter([]),
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(ExampleCollectionComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should start with an empty collection', () => {
    expect(component.model()).toEqual([]);
  });
});
