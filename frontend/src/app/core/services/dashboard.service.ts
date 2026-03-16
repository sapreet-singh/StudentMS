import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse, DashboardStats, EnrollmentByMonth, Student, StudentsByCourse } from '../../models/models';

@Injectable({ providedIn: 'root' })
export class DashboardService {
  private apiUrl = `${environment.apiUrl}/dashboard`;

  constructor(private http: HttpClient) {}

  getStats(): Observable<ApiResponse<DashboardStats>> {
    return this.http.get<ApiResponse<DashboardStats>>(`${this.apiUrl}/stats`);
  }

  getEnrollmentsByMonth(): Observable<ApiResponse<EnrollmentByMonth[]>> {
    return this.http.get<ApiResponse<EnrollmentByMonth[]>>(`${this.apiUrl}/enrollments-by-month`);
  }

  getStudentsByCourse(): Observable<ApiResponse<StudentsByCourse[]>> {
    return this.http.get<ApiResponse<StudentsByCourse[]>>(`${this.apiUrl}/students-by-course`);
  }

  getRecentEnrollments(count = 5): Observable<ApiResponse<Student[]>> {
    return this.http.get<ApiResponse<Student[]>>(`${this.apiUrl}/recent-enrollments?count=${count}`);
  }
}
