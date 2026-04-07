import {Routes} from '@angular/router';
import {LoginComponent} from './components/auth/login/login';
import {LayoutComponent} from './layout/layout';
import {RegisterComponent} from './components/auth/register/register';
import {AuthGuard} from './components/auth/auth.guard';
import {HomeComponent} from './pages/home/home';
import {TransactionsPageComponent} from "./pages/transactions/transactions-page/transactions-page";

export const routes: Routes = [
    { path: 'login', component: LoginComponent },
    { path: 'register', component: RegisterComponent },
    {
        path: '',
        component: LayoutComponent,
        canActivate: [AuthGuard],
        children: [
            { path: '', redirectTo: 'home', pathMatch: 'full' },
            { path: 'home', component: HomeComponent },
            { path: 'transactions', component: TransactionsPageComponent },
        ]
    },
];
