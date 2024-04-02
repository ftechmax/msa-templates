import {
  provideTransloco,
  TranslocoModule
} from '@jsverse/transloco';
import { isDevMode, NgModule } from '@angular/core';
import { TranslocoHttpLoader } from './transloco-loader';

@NgModule({
  exports: [ TranslocoModule ],
  providers: [
      provideTransloco({
        config: {
          availableLangs: ['en'],
          defaultLang: 'en',
          reRenderOnLangChange: false,
          prodMode: !isDevMode(),
        },
        loader: TranslocoHttpLoader
      }),
  ],
})
export class TranslocoRootModule {}
