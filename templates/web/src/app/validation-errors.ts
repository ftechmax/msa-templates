import { HttpErrorResponse } from '@angular/common/http';
import { FormGroup } from '@angular/forms';

function toControlPath(key: string) {
  return key
    .split('.')
    .map((segment) => segment.charAt(0).toLowerCase() + segment.slice(1))
    .join('.');
}

export function AddValidationErrors(response: HttpErrorResponse, form: FormGroup) {
  const errors = response?.error?.errors;
  if (!errors) {
    return;
  }

  Object.entries(errors).forEach(([key, messages]) => {
    form.get(toControlPath(key))?.setErrors({ fluentValidationError: messages });
  });
}
