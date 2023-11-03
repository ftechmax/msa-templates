import { NgModule } from '@angular/core';
import { HeaderComponent } from './header.component';
import { MatButtonModule } from '@angular/material/button';
import { CommonModule } from '@angular/common';

@NgModule({
  declarations: [HeaderComponent],
  imports: [
    CommonModule,
    //FontAwesomeModule, // See: https://fontawesome.com/v5/docs/web/use-with/angular
    //TranslocoRootModule, // See: https://ngneat.github.io/transloco/
    MatButtonModule,
  ],
  exports: [HeaderComponent],
})
export class HeaderModule {}
