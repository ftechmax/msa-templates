import { Component, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import {
  MAT_SNACK_BAR_DATA,
  MatSnackBarAction,
  MatSnackBarActions,
  MatSnackBarLabel,
  MatSnackBarRef,
} from '@angular/material/snack-bar';
import { DomainFault } from '../contracts';

@Component({
    selector: 'app-fault-snackbar',
    imports: [
        MatButtonModule,
        MatSnackBarLabel,
        MatSnackBarActions,
        MatSnackBarAction,
    ],
    templateUrl: './fault-snack-bar.component.html',
    styleUrl: './fault-snack-bar.component.scss'
})
export class FaultSnackBarComponent {
  snackBarRef = inject(MatSnackBarRef);
  data = inject(MAT_SNACK_BAR_DATA) as DomainFault;
}
