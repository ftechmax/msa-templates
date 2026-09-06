import { HttpErrorResponse } from '@angular/common/http';
import { FormControl, FormGroup } from '@angular/forms';

import { AddValidationErrors } from './validation-errors';

describe('AddValidationErrors', () => {
  let form: FormGroup;

  function badRequest(errors: unknown) {
    return new HttpErrorResponse({ status: 400, error: { errors } });
  }

  beforeEach(() => {
    form = new FormGroup({
      name: new FormControl(''),
      exampleValueObject: new FormGroup({
        code: new FormControl(''),
        value: new FormControl(''),
      }),
    });
  });

  it('should set the errors of a top level control', () => {
    const messages = ["'Name' must not be empty."];

    AddValidationErrors(badRequest({ Name: messages }), form);

    expect(form.get('name')?.errors).toEqual({
      fluentValidationError: messages,
    });
  });

  it('should set the errors of a nested control', () => {
    const messages = ["'Code' must not be empty."];

    AddValidationErrors(
      badRequest({ 'ExampleValueObject.Code': messages }),
      form
    );

    expect(form.get('exampleValueObject.code')?.errors).toEqual({
      fluentValidationError: messages,
    });
  });

  it('should ignore a key without a matching control', () => {
    AddValidationErrors(badRequest({ Missing: ['nope'] }), form);

    expect(form.valid).toBeTruthy();
  });

  it('should ignore a response without validation errors', () => {
    const response = new HttpErrorResponse({
      status: 500,
      error: { title: 'An error occurred.' },
    });

    expect(() => AddValidationErrors(response, form)).not.toThrow();
    expect(form.valid).toBeTruthy();
  });
});
