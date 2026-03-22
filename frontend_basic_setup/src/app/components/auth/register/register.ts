import {ChangeDetectionStrategy, Component, inject, signal} from '@angular/core';
import {email, form, Field, minLength, required} from "@angular/forms/signals";
import {AuthService} from "../auth.service";
import {RouterLink} from "@angular/router";
import {FormsModule} from "@angular/forms";
import {Router} from "@angular/router";
@Component({
    standalone: true,
    selector: 'app-login',
    templateUrl: './register.html',
    imports: [Field, RouterLink, FormsModule],
    styleUrl: '../login/login.scss',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class RegisterComponent{
    loginModel = signal({
        email: '',
        password: '',
    });
    RegisterForm = form(this.loginModel, (fieldPath) => {
        required(fieldPath.email, {message: 'Email is required'});
        email(fieldPath.email, {message: 'Enter a valid email address'});
        required(fieldPath.password, {message: 'Password is required'});
        minLength(fieldPath.password, 8, {message: 'Password must be at least 8 characters'});
    });
    private auth = inject(AuthService)
    private router= inject(Router);
    serverError = signal<string | null>(null);
    onSubmit() {
        if (this.RegisterForm().invalid()) {
            return;
        }

        const {email, password } = this.loginModel();
        this.auth.register(email, password).subscribe({
            next: () => {
                this.serverError.set(null);
                this.loginModel.set({email: '', password: ''});
                this.router.navigate(['/login'], { queryParams: { registered: 'success' } })
                    .then(success => {
                        if (!success) {
                            console.warn('Navigation to login failed!');
                        }
                    });
            },
            error: (err) => {
                this.serverError.set(err.error?.message ?? 'Registration failed');
            }
        });

    }
}
