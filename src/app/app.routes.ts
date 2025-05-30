import { Routes } from '@angular/router';
import { HomeComponent } from './home/home.component';
import { CadastroComponent } from './cadastro/cadastro.component';
import { LoginComponent } from './login/login.component';
import { DashboardComponent } from './features/dashboard/dashboard.component';
import { AuthCallbackComponent } from './features/auth/auth-callback/auth-callback.component';




export const routes: Routes = [
    {path: 'cadastro', component: CadastroComponent},
    {path: 'home', component: HomeComponent},
    { path: 'dashboard', component: DashboardComponent },
    { path: 'login', component: LoginComponent },
    { path: '', component: HomeComponent },
    { path: 'auth-callback', component: AuthCallbackComponent }

];
