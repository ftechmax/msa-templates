import { Component, Input, OnDestroy, OnInit, signal } from '@angular/core';
import {
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Router, RouterLink } from '@angular/router';
import { Subscription } from 'rxjs';
import { v4 as uuidv4 } from 'uuid';
import { DomainFault } from '../../core/contracts';
import { FaultSnackBarComponent } from '../../core/fault-snackbar/fault-snack-bar.component';
import { EventService } from '../../status.service';
import { AddValidationErrors } from '../../validation-errors';
import { ExampleHttpClient } from '../httpclient';

@Component({
  selector: 'app-example-update',
  imports: [
    ReactiveFormsModule,
    RouterLink,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
  ],
  providers: [ExampleHttpClient],
  templateUrl: './example-update.component.html',
  styleUrl: './example-update.component.scss',
})
export class ExampleUpdateComponent implements OnInit, OnDestroy {
  @Input() id: string = '';

  private correlationId: string | null = null;
  private subscriptions = new Subscription();

  form = new FormGroup({
    description: new FormControl('', Validators.required),
    exampleValueObject: new FormGroup({
      code: new FormControl('', Validators.required),
      value: new FormControl<number | null>(null, Validators.required),
    }),
  });

  name = signal('');
  loadFailed = signal(false);
  submitting = signal(false);

  constructor(
    private router: Router,
    private http: ExampleHttpClient,
    private eventService: EventService,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit(): void {
    this.subscriptions.add(
      this.eventService.ExampleUpdatedEvent.subscribe((data) => {
        if (data.correlationId === this.correlationId) {
          this.router.navigate(['/example', this.id]);
        }
      })
    );

    this.subscriptions.add(
      this.eventService.UpdateExampleFault.subscribe((data) => {
        if (data.correlationId === this.correlationId) {
          console.error(data);
          this.correlationId = null;
          this.submitting.set(false);
          this.openSnackBar(data);
        }
      })
    );

    if (this.id) {
      this.subscriptions.add(
        this.http.getDetails(this.id).subscribe({
          next: (data) => {
            this.name.set(data.name);
            this.form.patchValue({
              description: data.description,
              exampleValueObject: data.exampleValueObject,
            });
          },
          error: () => this.loadFailed.set(true),
        })
      );
    }
  }

  ngOnDestroy(): void {
    this.subscriptions.unsubscribe();
  }

  submit() {
    if (!this.form.valid) {
      this.form.markAllAsTouched();
      return;
    }

    this.submitting.set(true);
    this.correlationId = uuidv4();

    this.http
      .update(this.id, { ...this.form.value, correlationId: this.correlationId })
      .subscribe({
        error: (response) => {
          AddValidationErrors(response, this.form);
          this.form.markAllAsTouched();
          this.correlationId = null;
          this.submitting.set(false);
        },
      });
  }

  private openSnackBar(fault: DomainFault) {
    this.snackBar.openFromComponent(FaultSnackBarComponent, {
      data: fault,
    });
  }
}
