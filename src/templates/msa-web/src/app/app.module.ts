import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { DashboardComponent } from './dashboard/dashboard.component';
import { HttpClientModule } from '@angular/common/http';
import { TranslocoRootModule } from './transloco-root.module';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatListModule } from '@angular/material/list';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { MatButtonModule } from '@angular/material/button';

interface Config {
  apiUrl: string;
}

declare var appConfig: Config; // Captures the config from config.js
var config: Config;

(() => {
  if (typeof appConfig === 'undefined') {
    config = {
      apiUrl: 'http://example.kube.local',
    } as Config;
  } else {
    config = appConfig;
    config.apiUrl = config.apiUrl.replace(/\/$/, '');
  }
})();

@NgModule({
  declarations: [AppComponent, DashboardComponent],
  imports: [
    BrowserModule,
    AppRoutingModule,
    BrowserAnimationsModule,
    HttpClientModule,
    TranslocoRootModule,
    MatToolbarModule,
    MatSidenavModule,
    MatListModule,
    MatIconModule,
    MatMenuModule,
    MatButtonModule,
  ],
  providers: [{ provide: 'BASE_API_URL', useValue: config.apiUrl }],
  bootstrap: [AppComponent],
})
export class AppModule {}
