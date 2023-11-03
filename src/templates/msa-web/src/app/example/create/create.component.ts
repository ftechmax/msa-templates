import { Component, OnDestroy, OnInit } from '@angular/core';
import { Subscription } from 'rxjs';
import { EventService } from '../event.service';
import { ExampleHttpClient } from '../example.httpclient';
import { v4 as uuidv4 } from 'uuid';
import { FormGroup, FormControl, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { addValidationErrors } from 'src/app/core/validation-errors';

@Component({
  templateUrl: './create.component.html',
  styleUrls: ['./create.component.scss'],
})
export class CreateComponent implements OnInit, OnDestroy {
  public form: FormGroup | undefined;
  public submitting = false;
  private correlationId: string;
  private event$: Subscription | undefined;

  constructor(
    private router: Router,
    private httpClient: ExampleHttpClient,
    private eventService: EventService
  ) {
    this.correlationId = uuidv4();
  }

  ngOnInit(): void {
    this.load();
    this.subscribe();
  }

  ngOnDestroy(): void {
    this.unsubscribe();
  }

  public submit() {
    if (!this.form || !this.form.valid) {
      this.submitting = false;
      return;
    }

    this.submitting = true;

    this.httpClient.create(this.form.value).subscribe({
      error: (response) => {
        addValidationErrors(response, this.form!);
        this.submitting = false;
      },
    });
  }

  private load() {
    this.form = new FormGroup({
      correlationId: new FormControl(this.correlationId),
      name: new FormControl('', Validators.required),
      description: new FormControl('', Validators.required),
    });
  }

  private subscribe() {
    this.event$ = this.eventService.ExampleCreatedEvent.pipe().subscribe(
      (data) => {
        if (data.correlationId != this.correlationId) {
          return;
        }

        this.router.navigate(['/example/details'], {
          queryParams: {
            id: data.id,
          },
        });
      }
    );
  }

  private unsubscribe() {
    this.event$?.unsubscribe();
  }
}
