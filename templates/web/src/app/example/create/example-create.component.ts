import { Component, OnDestroy, OnInit, signal } from '@angular/core';
import { ExampleHttpClient } from '../httpclient';
import { MatFormFieldModule } from '@angular/material/form-field';
import {
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { Subscription } from 'rxjs';
import { Router, RouterLink } from '@angular/router';
import { EventService } from '../../status.service';
import { v4 as uuidv4 } from 'uuid';

import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatSnackBar } from '@angular/material/snack-bar';
import { FaultSnackBarComponent } from '../../core/fault-snackbar/fault-snack-bar.component';
import { DomainFault } from '../../core/contracts';
import { TranslocoDirective } from '@jsverse/transloco';
import { AddValidationErrors } from '../../validation-errors';

@Component({
  selector: 'app-example-create',
  imports: [
    ReactiveFormsModule,
    RouterLink,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    TranslocoDirective,
  ],
  providers: [ExampleHttpClient],
  templateUrl: './example-create.component.html',
  styleUrl: './example-create.component.scss',
})
export class ExampleCreateComponent implements OnInit, OnDestroy {
  private correlationId: string | null = null;

  form = new FormGroup({
    name: new FormControl('', Validators.required),
    description: new FormControl('', Validators.required),
    exampleValueObject: new FormGroup({
      code: new FormControl('aaa', Validators.required),
      value: new FormControl<number | null>(123.45, Validators.required),
    }),
  });

  submitting = signal(false);
  event$: Subscription | null = null;
  fault$: Subscription | null = null;

  constructor(
    private router: Router,
    private http: ExampleHttpClient,
    private eventService: EventService,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit(): void {
    this.event$ = this.eventService.ExampleCreatedEvent.subscribe((data) => {
      if (data.correlationId === this.correlationId) {
        this.router.navigate(['/example', data.id]);
      }
    });

    this.fault$ = this.eventService.CreateExampleFault.subscribe((data) => {
      if (data.correlationId === this.correlationId) {
        console.error(data);
        this.correlationId = null;
        this.submitting.set(false);
        this.openSnackBar(data);
      }
    });
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
      this.form.markAllAsTouched();
      this.submitting.set(false);
      return;
    }

    this.submitting.set(true);
    this.correlationId = uuidv4();

    this.http.create({ ...this.form.value, correlationId: this.correlationId }).subscribe({
      error: (response) => {
        AddValidationErrors(response, this.form);
        this.form.markAllAsTouched();
        this.correlationId = null;
        this.submitting.set(false);
      },
    });
  }
}
