import {ChangeDetectionStrategy, Component, inject, signal} from '@angular/core';
import {email, form, Field, minLength, required} from "@angular/forms/signals";
import {AuthService} from "../auth.service";
import {RouterLink} from "@angular/router";
import {FormsModule} from "@angular/forms";
import {Router} from "@angular/router";
@Component({
  standalone: true,
  selector: 'app-login',
  templateUrl: './login.html',
    imports: [Field, RouterLink, FormsModule],
  styleUrl: './login.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LoginComponent{
  loginModel = signal({
    email: '',
    password: '',
  });
  loginForm = form(this.loginModel, (fieldPath) => {
    required(fieldPath.email, {message: 'Email is required'});
    email(fieldPath.email, {message: 'Enter a valid email address'});
    required(fieldPath.password, {message: 'Password is required'});
  });

  private auth = inject(AuthService);
  private router = inject(Router);
  serverError = signal<string | null>(null);

  onSubmit() {
    if (this.loginForm().invalid()) {
      return;
    }

    const {email, password } = this.loginModel();
    this.auth.login(email, password).subscribe({
      next: () => {
        this.serverError.set(null);
        this.loginModel.set({email: '', password: ''});
        this.router.navigate(['/'], { queryParams: { logged_in: 'success' } })
            .then(success => {
              if (!success) {
                console.warn('Login Failed');
              }
            });
      },
      error: (err) => {
        this.serverError.set('Invalid credentials')
      }
    });
  }
}
