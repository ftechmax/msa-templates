import { FormGroup } from '@angular/forms';

export function AddValidationErrors(response: any, form: FormGroup) {
  if (response && response.error) {
    Object.entries(response.error.errors).forEach(([key, value]) => {
      const control = form.get(key.replace(/^[A-Z]./, ($1) => $1.toLowerCase()));
      if (!!control) {
        control.setErrors({
          fluentValidationError: value, // (value as Array<string>).join('\r\n'),
        });
      }
    });
  }
}
