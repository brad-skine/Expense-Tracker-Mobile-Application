import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import {Observable, Subject, switchMap, tap} from 'rxjs';
import { environment } from 'src/environments/environment';
import {MonthlySalesModel} from "../../models/monthly_sale.model";

interface AuthResponse {
    token: string;
}
@Injectable({providedIn: 'root',})
// TODO: use signals instead of manual trigger refresh
export class AuthService {
    private apiUrl = environment.apiUrl + "/api/auth";
    private http = inject(HttpClient);

    private storeToken(token: string){
        localStorage.setItem('token', token);
    }
    getToken(): string | null {
        return localStorage.getItem('token');
    }
    logout(){
        localStorage.removeItem('token');
    }

    isAuthenticated(): boolean {
        return localStorage.getItem('token') !== null;
    }

    register(email: string, password: string) {
        return this.http.post<AuthResponse>(
            `${this.apiUrl}/register`,
            {email, password}).pipe(tap(res => this.storeToken(res.token)));
    }

    login(email: string, password: string) {
        return this.http.post<AuthResponse>(
            `${this.apiUrl}/login`, { email, password })
            .pipe(tap(res => this.storeToken(res.token)));
    }
}
