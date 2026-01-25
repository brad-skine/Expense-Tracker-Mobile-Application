import { Routes } from '@angular/router';
import { LoginComponent } from './components/auth/login/login';
import {LayoutComponent} from './layout/layout';

export const routes: Routes = [
    { path: 'login', component: LoginComponent },
    {path: '',  component: LayoutComponent},
    { path: '**', redirectTo: 'login' }
];