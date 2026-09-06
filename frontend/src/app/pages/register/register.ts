import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { Footer } from '../../components/footer/footer';
import { Header } from '../../components/header/header';
import { Spinner } from '../../components/spinner/spinner';
import { UserApi } from '../../services/user-api';

@Component({
  selector: 'app-register',
  imports: [ReactiveFormsModule, RouterLink, Header, Footer, Spinner],
  templateUrl: './register.html',
  styleUrl: './register.css',
})
export class Register {
  private readonly formBuilder = inject(FormBuilder);
  readonly submitting = signal(false);
  readonly error = signal('');
  readonly form = this.formBuilder.nonNullable.group({
    firstName: ['', Validators.required],
    lastName: ['', Validators.required],
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(8)]],
  });

  constructor(
    private readonly api: UserApi,
    private readonly router: Router,
  ) {}

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    this.submitting.set(true);
    this.error.set('');
    this.api.register(this.form.getRawValue()).subscribe({
      next: () => this.router.navigateByUrl('/login'),
      error: (response) => {
        this.error.set(
          response.status === 409
            ? 'That email is already registered.'
            : 'We could not create your account. Please try again.',
        );
        this.submitting.set(false);
      },
    });
  }
}
