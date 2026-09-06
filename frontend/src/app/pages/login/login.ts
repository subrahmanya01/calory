import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { UserApi } from '../../services/user-api';
import { Footer } from '../../components/footer/footer';
import { Header } from '../../components/header/header';
import { Spinner } from '../../components/spinner/spinner';

@Component({
  selector: 'app-login',
  imports: [CommonModule, ReactiveFormsModule, RouterLink, Header, Footer, Spinner],
  templateUrl: './login.html',
  styleUrl: './login.css',
})
export class Login {
  private readonly formBuilder = inject(FormBuilder);
  readonly submitting = signal(false);
  readonly error = signal('');
  readonly form = this.formBuilder.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', Validators.required],
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
    const { email, password } = this.form.getRawValue();
    this.api.login(email, password).subscribe({
      next: (response) => {
        localStorage.setItem('calory_token', response.token);
        this.router.navigateByUrl('/home');
      },
      error: () => {
        this.error.set('Those details did not match. Try again or create an account.');
        this.submitting.set(false);
      },
    });
  }
}
