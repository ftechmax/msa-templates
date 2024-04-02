import { NgModule, isDevMode } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { environment } from 'src/environments/environment';
import { TranslocoRootModule } from './transloco-root.module';
import { HttpClientModule } from '@angular/common/http';

interface Config {
  production: boolean;
  apiUrl: string;
}

declare var appConfig: Config;
var config: Config;

(() => {
  if (typeof appConfig === 'undefined') {
    config = environment as unknown as Config;
  } else {
    config = appConfig;
  }

  if (!isDevMode()) {
    const hostname = window.location.hostname;
    const protocol = window.location.protocol;

    // The following captures the domain name from the hostname which is useful for setting other API URLs
    // const regex = new RegExp('^([a-z0-9|-]+).{1}((?:[a-z0-9|-]+.?)+[a-z]+)$', 'gi');
    // const match = regex.exec(hostname);
    // const domain = match![2];

    config.apiUrl = `${protocol}//${hostname}`;
  } else {
    config.apiUrl = config.apiUrl.replace(/\/$/, '');
  }
})();

@NgModule({
  declarations: [AppComponent],
  imports: [
    BrowserModule,
    AppRoutingModule,
    HttpClientModule,
    TranslocoRootModule, // See: https://jsverse.github.io/transloco/
  ],
  providers: [
    { provide: 'BASE_API_URL', useValue: config.apiUrl },
  ],
  bootstrap: [AppComponent],
})
export class AppModule {}
