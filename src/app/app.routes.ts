import { Routes } from '@angular/router';
import { HomeComponent } from './home/home.component';
import { CadastroComponent } from './cadastro/cadastro.component';
import { LoginComponent } from './login/login.component';
import { DashboardComponent } from './features/dashboard/dashboard.component';
import { AuthCallbackComponent } from './features/auth/auth-callback/auth-callback.component';
import { ShopComponent } from './features/shop/shop.component';
import { QueroAjudarComponent } from './features/quero-ajudar/quero-ajudar.component';
import { AdminLayoutComponent } from './layouts/admin-layout/admin-layout.component';
import { AdminDashboardComponent } from './features/admin/admin-dashboard/admin-dashboard.component';
import { ProductManagementComponent } from './features/admin/product-management/product-management.component'; 
import { DonorManagementComponent } from './features/admin/donor-management/donor-management.component'; // Importe o novo componente



export const routes: Routes = [
    {path: 'cadastro', component: CadastroComponent},
    {path: 'home', component: HomeComponent},
    { 
        path: 'dashboard', component: DashboardComponent
    
    },
    { path: 'login', component: LoginComponent },
    { path: '', component: HomeComponent },
    { path: 'auth-callback', component: AuthCallbackComponent },
    { path: 'loja', component: ShopComponent },
    {path: 'quero-ajudar', component: QueroAjudarComponent},
    {path: 'admin',
    component: AdminLayoutComponent, 
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      { path: 'dashboard', component: AdminDashboardComponent },
      { path: 'products', component: ProductManagementComponent },
      { path: 'donors', component: DonorManagementComponent }, 

    ]
  },
];
