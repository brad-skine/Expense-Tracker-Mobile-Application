import { Routes } from '@angular/router';
import { LoginComponent } from './components/auth/login/login';
import {LayoutComponent} from './layout/layout';
import { RegisterComponent} from "./components/auth/register/register";
import {AuthGuard} from "./components/auth/auth.guard"

export const routes: Routes = [
    { path: 'login', component: LoginComponent },
    {path: 'register', component: RegisterComponent},
    {path: '',  component: LayoutComponent, canActivate: [AuthGuard]},
    { path: '**', redirectTo: 'login' }
];