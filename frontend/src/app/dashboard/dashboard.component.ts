import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTableModule } from '@angular/material/table';
import { MatChipsModule } from '@angular/material/chips';
import { BaseChartDirective } from 'ng2-charts';
import { ChartConfiguration, ChartData } from 'chart.js';
import { Chart, registerables } from 'chart.js';
import { DashboardService } from '../core/services/dashboard.service';
import { StudentService } from '../core/services/student.service';
import { DashboardStats, EnrollmentByMonth, Student, StudentsByCourse } from '../models/models';

Chart.register(...registerables);

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [
    CommonModule, RouterLink,
    MatCardModule, MatIconModule, MatButtonModule,
    MatProgressSpinnerModule, MatTableModule, MatChipsModule,
    BaseChartDirective
  ],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss']
})
export class DashboardComponent implements OnInit {
  today = new Date();
  stats: DashboardStats | null = null;
  recentStudents: Student[] = [];
  loadingStats = true;
  loadingCharts = true;
  loadingRecent = true;

  // Bar chart - enrollments by month
  barChartData: ChartData<'bar'> = { labels: [], datasets: [] };
  barChartOptions: ChartConfiguration['options'] = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: { display: false },
      tooltip: { mode: 'index' }
    },
    scales: {
      y: {
        beginAtZero: true,
        ticks: { stepSize: 1 },
        grid: { color: 'rgba(0,0,0,0.05)' }
      },
      x: { grid: { display: false } }
    }
  };

  // Pie chart - students by course
  pieChartData: ChartData<'doughnut'> = { labels: [], datasets: [] };
  pieChartOptions: ChartConfiguration['options'] = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: { position: 'bottom' }
    }
  };

  displayedColumns = ['name', 'course', 'enrollmentDate', 'status'];

  constructor(
    private dashboardService: DashboardService,
    private studentService: StudentService
  ) {}

  ngOnInit(): void {
    this.loadStats();
    this.loadCharts();
    this.loadRecentStudents();
  }

  loadStats(): void {
    this.dashboardService.getStats().subscribe({
      next: res => { this.stats = res.data; this.loadingStats = false; },
      error: () => { this.loadingStats = false; }
    });
  }

  loadCharts(): void {
    this.dashboardService.getEnrollmentsByMonth().subscribe({
      next: res => {
        this.barChartData = {
          labels: res.data.map((d: EnrollmentByMonth) => d.month),
          datasets: [{
            data: res.data.map((d: EnrollmentByMonth) => d.count),
            backgroundColor: 'rgba(21, 101, 192, 0.7)',
            borderColor: '#1565c0',
            borderWidth: 2,
            borderRadius: 6,
            label: 'Enrollments'
          }]
        };
      }
    });

    this.dashboardService.getStudentsByCourse().subscribe({
      next: res => {
        const colors = ['#1565c0', '#2e7d32', '#f57c00', '#7b1fa2', '#c62828', '#00838f'];
        this.pieChartData = {
          labels: res.data.map((d: StudentsByCourse) => d.courseName),
          datasets: [{
            data: res.data.map((d: StudentsByCourse) => d.count),
            backgroundColor: colors.slice(0, res.data.length),
            borderWidth: 0
          }]
        };
        this.loadingCharts = false;
      },
      error: () => { this.loadingCharts = false; }
    });
  }

  loadRecentStudents(): void {
    this.dashboardService.getRecentEnrollments(5).subscribe({
      next: res => { this.recentStudents = res.data; this.loadingRecent = false; },
      error: () => { this.loadingRecent = false; }
    });
  }

  getPhotoUrl(photoPath: string | null | undefined): string {
    return this.studentService.getPhotoUrl(photoPath);
  }

  getInitials(student: Student): string {
    return `${student.firstName[0]}${student.lastName[0]}`.toUpperCase();
  }
}
