import {Routes} from '@angular/router';
import {LoginComponent} from './components/auth/login/login';
import {LayoutComponent} from './layout/layout';
import {RegisterComponent} from './components/auth/register/register';
import {AuthGuard} from './components/auth/auth.guard';

// Pages are lazy-loaded: smaller initial bundle = faster first paint,
// especially on the Capacitor Android build.
export const routes: Routes = [
    { path: 'login', component: LoginComponent },
    { path: 'register', component: RegisterComponent },
    {
        path: '',
        component: LayoutComponent,
        canActivate: [AuthGuard],
        children: [
            { path: '', redirectTo: 'home', pathMatch: 'full' },
            {
                path: 'home',
                loadComponent: () =>
                    import('./pages/home/home').then(m => m.HomeComponent),
            },
            {
                path: 'transactions',
                loadComponent: () =>
                    import('./pages/transactions/transactions-page/transactions-page')
                        .then(m => m.TransactionsPageComponent),
            },
            {
                path: 'planning',
                loadComponent: () =>
                    import('./pages/planning/planning').then(m => m.PlanningComponent),
            },
        ]
    },
];
