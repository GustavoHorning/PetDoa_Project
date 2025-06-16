import { Component, OnInit } from '@angular/core';
import { CommonModule, CurrencyPipe } from '@angular/common';
import { BaseChartDirective } from 'ng2-charts';
import { Chart, ChartConfiguration, ChartOptions, LineController, LineElement, PointElement, LinearScale, CategoryScale, Title, Tooltip, Legend } from 'chart.js';
import { AdminService, AdminDashboardDto } from '../../../core/services/admin.service';

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [CommonModule, CurrencyPipe, BaseChartDirective],
  templateUrl: './admin-dashboard.component.html',
  styleUrls: ['./admin-dashboard.component.scss']
})
export class AdminDashboardComponent implements OnInit {
  isLoading = true;
  dashboardData: AdminDashboardDto | null = null;

  // Configuração do Gráfico de Linhas
  public lineChartData: ChartConfiguration<'line'>['data'] = {
    labels: [],
    datasets: [
      {
        data: [],
        label: 'Arrecadação Diária (R$)',
        fill: true,
        tension: 0.4,
        borderColor: '#14b8a6', // Teal 500
        backgroundColor: 'rgba(20, 184, 166, 0.1)',
        pointBackgroundColor: '#14b8a6',
        pointBorderColor: '#fff',
        pointHoverBackgroundColor: '#fff',
        pointHoverBorderColor: '#14b8a6'
      }
    ]
  };
  public lineChartOptions: ChartOptions<'line'> = {
    responsive: true,
    maintainAspectRatio: false
  };
  public lineChartLegend = true;

  constructor(private adminService: AdminService) 
  {
     Chart.register(
      LineController,
      LineElement,
      PointElement,
      LinearScale,
      CategoryScale,
      Title,
      Tooltip,
      Legend
    );
  }

  ngOnInit(): void {
    this.adminService.getDashboardData().subscribe({
      next: (data) => {
        if (data) {
          this.dashboardData = data;
          this.setupChartData(data.dailyRevenueLast30Days);
        }
        this.isLoading = false;
      },
      error: (err) => {
        console.error("Erro ao carregar dados do dashboard de admin:", err);
        this.isLoading = false;
      }
    });
  }

  private setupChartData(dailyData: { date: string, amount: number }[] | undefined): void {
    if (!dailyData) return;
    const newChartData: ChartConfiguration<'line'>['data'] = {
      labels: dailyData.map(d => d.date),
      datasets: [
        {
          ...this.lineChartData.datasets[0],
          data: dailyData.map(d => d.amount) 
        }
      ]
    };

    this.lineChartData = newChartData;
  }
}
