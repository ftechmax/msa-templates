import { Component, OnDestroy, OnInit } from '@angular/core';
import { ExampleHttpClient } from '../httpclient';
import { MatFormFieldModule } from '@angular/material/form-field';
import {
  FormControl,
  FormGroup,
  FormsModule,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { Subscription } from 'rxjs';
import { Router } from '@angular/router';
import { EventService } from '../../status.service';
import { v4 as uuidv4 } from 'uuid';
import { CommonModule } from '@angular/common';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatSnackBar } from '@angular/material/snack-bar';
import { FaultSnackBarComponent } from '../../core/fault-snackbar/fault-snack-bar.component';
import { DomainFault } from '../../core/contracts';
import { TranslocoDirective } from '@jsverse/transloco';

@Component({
  selector: 'app-example-create',
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    FormsModule,
    MatButtonModule,
    TranslocoDirective,
  ],
  providers: [ExampleHttpClient],
  templateUrl: './example-create.component.html',
  styleUrl: './example-create.component.scss',
})
export class ExampleCreateComponent implements OnInit, OnDestroy {
  correlationId: string = uuidv4();
  form = new FormGroup({
    correlationId: new FormControl(this.correlationId),
    name: new FormControl('', Validators.required),
    description: new FormControl('', Validators.required),
    exampleValueObject: new FormGroup({
      code: new FormControl('aaa'), //, Validators.required
      value: new FormControl('123.45'), //, Validators.required
    }),
  });

  submitting: boolean = false;
  event$: Subscription | null = null;
  fault$: Subscription | null = null;

  constructor(
    private router: Router,
    private http: ExampleHttpClient,
    private eventService: EventService,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit(): void {
    this.event$ = this.eventService.ExampleCreatedEvent.pipe().subscribe(
      (data) => {
        if (data.correlationId == this.correlationId) {
          this.router.navigate(['/example/details'], {
            queryParams: {
              id: data.id,
            },
          });
        }
      }
    );

    this.fault$ = this.eventService.CreateExampleFault.pipe().subscribe(
      (data) => {
        console.error(data);
        this.openSnackBar(data);
      }
    );
  }

  private openSnackBar(fault: DomainFault) {
    this.snackBar.openFromComponent(FaultSnackBarComponent, {
      data: fault,
    });
  }

  ngOnDestroy(): void {
    this.event$?.unsubscribe();
    this.fault$?.unsubscribe();
  }

  submit() {
    if (!this.form.valid) {
      this.submitting = false;
      return;
    }

    this.submitting = true;

    this.http.create(this.form.value).subscribe({
      error: (response) => {
        //AddValidationErrors(response, this.form);
        this.submitting = false;
      },
    });
  }
}
