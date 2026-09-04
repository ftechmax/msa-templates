import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { provideRouter } from '@angular/router';
import { TranslocoTestingModule } from '@jsverse/transloco';

import { ExampleCreateComponent } from './example-create.component';

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
});
