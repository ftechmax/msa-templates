import { HttpClient } from '@angular/common/http';
import {
  TRANSLOCO_LOADER,
  Translation,
  TranslocoLoader,
  TRANSLOCO_CONFIG,
  translocoConfig,
  TranslocoModule,
} from '@ngneat/transloco';
import { Inject, Injectable, NgModule } from '@angular/core';
import { environment } from '../environments/environment';
import { TranslocoMessageFormatModule } from '@ngneat/transloco-messageformat';

@Injectable({ providedIn: 'root' })
export class TranslocoHttpLoader implements TranslocoLoader {
  constructor(
    private http: HttpClient,
    @Inject('INTERNATIONALIZATION_API_URL') private baseUrl: string
  ) {}

  getTranslation(langPath: string) {
    return this.http.get<Translation>(
      `${this.baseUrl}/i18n/safety-generator/${langPath}.json`
    );
  }
}

@NgModule({
  exports: [TranslocoModule],
  imports: [TranslocoMessageFormatModule.forRoot()],
  providers: [
    {
      provide: TRANSLOCO_CONFIG,
      useValue: translocoConfig({
        availableLangs: ['en'],
        defaultLang: 'en',
        reRenderOnLangChange: false,
        prodMode: environment.production,
      }),
    },
    { provide: TRANSLOCO_LOADER, useClass: TranslocoHttpLoader },
  ],
})
export class TranslocoRootModule {}
