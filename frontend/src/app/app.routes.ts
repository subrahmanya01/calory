import { Routes } from '@angular/router';

export const routes: Routes = [
    { path: '', pathMatch: 'full', redirectTo: 'home' },
    { path: 'home', loadComponent: () => import('./pages/home/home').then((m) => m.Home) },
    { path: 'login', loadComponent: () => import('./pages/login/login').then((m) => m.Login) },
    { path: 'register', loadComponent: () => import('./pages/register/register').then((m) => m.Register) },
    { path: '**', redirectTo: 'home' },
];
