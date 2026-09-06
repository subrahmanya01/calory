import { Routes } from '@angular/router';

export const routes: Routes = [
    { path: '', pathMatch: 'full', redirectTo: 'home' },
    { path: 'home', loadComponent: () => import('./pages/home/home').then((m) => m.Home) },
    { path: 'query', loadComponent: () => import('./pages/my-query/my-query').then((m) => m.MyQuery) },
    { path: 'goals/history', loadComponent: () => import('./pages/goal-history/goal-history').then((m) => m.GoalHistory) },
    { path: 'login', loadComponent: () => import('./pages/login/login').then((m) => m.Login) },
    { path: 'register', loadComponent: () => import('./pages/register/register').then((m) => m.Register) },
    { path: '**', redirectTo: 'home' },
];
