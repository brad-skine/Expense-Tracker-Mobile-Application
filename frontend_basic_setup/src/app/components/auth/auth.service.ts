import {inject, Injectable} from '@angular/core';
import {HttpClient} from '@angular/common/http';
import {tap} from 'rxjs';
import {environment} from 'src/environments/environment';

interface AuthResponse {
    token: string;
}

@Injectable({providedIn: 'root',})
// TODO: use signals instead of manual trigger refresh
export class AuthService {
    private apiUrl = environment.apiUrl + "/api/auth";
    private http = inject(HttpClient);

    private storeToken(token: string) {
        localStorage.setItem('token', token);
    }

    getToken(): string | null {
        const token = localStorage.getItem('token');
        if (!token) return null;

        try {
            const payload = JSON.parse(atob(token.split('.')[1]));
            if (payload.exp * 1000 < Date.now()) {
                this.logout();
                return null;
            }
        } catch {
            this.logout();
            return null;
        }

        return token;
    }

    logout() {
        localStorage.removeItem('token');
    }

    isAuthenticated(): boolean {
        return this.getToken() !== null;
    }

    register(email: string, password: string) {
        return this.http.post<AuthResponse>(
            `${this.apiUrl}/register`,
            {email, password}).pipe(tap(res => this.storeToken(res.token)));
    }

    login(email: string, password: string) {
        return this.http.post<AuthResponse>(
            `${this.apiUrl}/login`, {email, password})
            .pipe(tap(res => this.storeToken(res.token)));
    }
}
