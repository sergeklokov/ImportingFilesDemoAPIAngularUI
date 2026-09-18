import { HttpClient } from '@angular/common/http';
import { Component, OnInit, signal } from '@angular/core';

interface WeatherForecast {
  date: string;
  temperatureC: number;
  temperatureF: number;
  summary: string;
}

@Component({
  selector: 'app-root',
  templateUrl: './app.html',
  standalone: false,
  styleUrl: './app.css'
})
export class App implements OnInit {
  protected readonly forecasts = signal<WeatherForecast[] | undefined>(undefined);

  constructor(private http: HttpClient) {}

  ngOnInit() {
    this.getForecasts();
  }

  getForecasts() {
    this.http.get<WeatherForecast[]>('/weatherforecast').subscribe({
      next: (result) => {
        this.forecasts.set(result);
      },
      error: (error) => {
        console.error(error);
      }
    });
  }

  protected readonly title = signal('importingfilesdemoapiangularui.client');
}
