import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { environment } from 'src/environments/environment';
import { TranslocoRootModule } from './transloco-root.module';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';

interface Config {
  apiUrl: string;
  internationalizationUrl: string;
}

declare var appConfig: Config;
var config: Config;

(() => {
  if (typeof appConfig === 'undefined') {
    config = environment as unknown as Config;
  } else {
    config = appConfig;
  }
  config.apiUrl = config.apiUrl.replace(/\/$/, '');
  config.internationalizationUrl = config.internationalizationUrl.replace(
    /\/$/,
    ''
  );
})();

@NgModule({
  declarations: [AppComponent],
  imports: [
    BrowserModule,
    AppRoutingModule,
    FontAwesomeModule, // See: https://fontawesome.com/v5/docs/web/use-with/angular
    TranslocoRootModule, // See: https://ngneat.github.io/transloco/
  ],
  providers: [
    { provide: 'BASE_API_URL', useValue: config.apiUrl },
    {
      provide: 'INTERNATIONALIZATION_API_URL',
      useValue: config.internationalizationUrl,
    },
  ],
  bootstrap: [AppComponent],
})
export class AppModule {}
