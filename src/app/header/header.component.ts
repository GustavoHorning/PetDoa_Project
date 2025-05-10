import { Component } from '@angular/core';
import { RouterModule } from '@angular/router'; // 👈 Importa o módulo necessário

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [RouterModule], // 👈 Aqui está a chave!
  templateUrl: './header.component.html',
  styleUrls: ['./header.component.scss']
})
export class HeaderComponent {}


