import { ApplicationConfig, provideZoneChangeDetection, isDevMode } from '@angular/core';
import { provideRouter, withComponentInputBinding } from '@angular/router';
import { routes } from './app.routes';
import { provideHttpClient } from '@angular/common/http';
import { TranslocoHttpLoader } from './transloco-loader';
import { provideTransloco } from '@jsverse/transloco';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';

export const appConfig: ApplicationConfig = {
  providers: [provideZoneChangeDetection({ eventCoalescing: true }), provideRouter(routes, withComponentInputBinding()), provideHttpClient(), provideTransloco({
        config: {
          availableLangs: ['en'],
          defaultLang: 'en',
          reRenderOnLangChange: false,
          prodMode: !isDevMode(),
        },
        loader: TranslocoHttpLoader
      }), provideAnimationsAsync()]
};
