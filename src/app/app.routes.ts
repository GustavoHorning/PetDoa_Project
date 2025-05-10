import { Routes } from '@angular/router';
import { HomeComponent } from './home/home.component';
import { CadastroComponent } from './cadastro/cadastro.component';


export const routes: Routes = [
    {path: 'cadastro', component: CadastroComponent},
    {path: 'home', component: HomeComponent},

    {path: '', redirectTo: 'home', pathMatch: 'full'}
];
