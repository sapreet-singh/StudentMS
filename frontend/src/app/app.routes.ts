import { Routes } from '@angular/router';
import { authGuard, guestGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
  {
    path: 'auth',
    canActivate: [guestGuard],
    children: [
      {
        path: 'login',
        loadComponent: () => import('./auth/login/login.component').then(m => m.LoginComponent)
      },
      {
        path: 'register',
        loadComponent: () => import('./auth/register/register.component').then(m => m.RegisterComponent)
      },
      { path: '', redirectTo: 'login', pathMatch: 'full' }
    ]
  },
  {
    path: '',
    canActivate: [authGuard],
    loadComponent: () => import('./shared/components/layout/layout.component').then(m => m.LayoutComponent),
    children: [
      {
        path: 'dashboard',
        loadComponent: () => import('./dashboard/dashboard.component').then(m => m.DashboardComponent)
      },
      {
        path: 'students',
        loadComponent: () => import('./students/list/student-list.component').then(m => m.StudentListComponent)
      },
      {
        path: 'students/new',
        loadComponent: () => import('./students/add-edit/student-add-edit.component').then(m => m.StudentAddEditComponent)
      },
      {
        path: 'students/:id/edit',
        loadComponent: () => import('./students/add-edit/student-add-edit.component').then(m => m.StudentAddEditComponent)
      },
      {
        path: 'students/:id',
        loadComponent: () => import('./students/detail/student-detail.component').then(m => m.StudentDetailComponent)
      }
    ]
  },
  { path: '**', redirectTo: 'dashboard' }
];
