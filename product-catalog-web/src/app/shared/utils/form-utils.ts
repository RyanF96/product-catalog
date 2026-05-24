import { FormGroup } from '@angular/forms';

export function getFormFieldError(form: FormGroup, fieldName: string): string {
  const control = form.get(fieldName);
  if (!control || !control.errors || !control.touched) return '';

  if (control.errors['required']) return `${fieldName} is required`;
  if (control.errors['min']) return `${fieldName} must be at least ${control.errors['min'].min}`;
  if (control.errors['maxlength']) return `${fieldName} exceeds maximum length`;

  return 'Invalid value';
}
