import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  ApiResponse, PagedResponse, Student,
  CreateStudentRequest, UpdateStudentRequest, StudentFilter
} from '../../models/models';

@Injectable({ providedIn: 'root' })
export class StudentService {
  private apiUrl = `${environment.apiUrl}/students`;

  constructor(private http: HttpClient) {}

  getStudents(filter: StudentFilter = {}): Observable<ApiResponse<PagedResponse<Student>>> {
    let params = new HttpParams()
      .set('page', filter.page ?? 1)
      .set('pageSize', filter.pageSize ?? 10);
    if (filter.search) params = params.set('search', filter.search);
    if (filter.courseId) params = params.set('courseId', filter.courseId);
    if (filter.isActive !== undefined) params = params.set('isActive', filter.isActive);
    return this.http.get<ApiResponse<PagedResponse<Student>>>(this.apiUrl, { params });
  }

  getStudent(id: number): Observable<ApiResponse<Student>> {
    return this.http.get<ApiResponse<Student>>(`${this.apiUrl}/${id}`);
  }

  createStudent(request: CreateStudentRequest, photo?: File): Observable<ApiResponse<Student>> {
    const formData = this.buildFormData(request, photo);
    return this.http.post<ApiResponse<Student>>(this.apiUrl, formData);
  }

  updateStudent(id: number, request: UpdateStudentRequest, photo?: File): Observable<ApiResponse<Student>> {
    const formData = this.buildFormData(request, photo);
    return this.http.put<ApiResponse<Student>>(`${this.apiUrl}/${id}`, formData);
  }

  deleteStudent(id: number): Observable<ApiResponse<boolean>> {
    return this.http.delete<ApiResponse<boolean>>(`${this.apiUrl}/${id}`);
  }

  uploadPhoto(id: number, photo: File): Observable<ApiResponse<Student>> {
    const formData = new FormData();
    formData.append('photo', photo);
    return this.http.post<ApiResponse<Student>>(`${this.apiUrl}/${id}/photo`, formData);
  }

  getPhotoUrl(photoPath: string | null | undefined): string {
    if (!photoPath) return 'assets/images/default-avatar.png';
    return `${environment.uploadsUrl}/${photoPath}`;
  }

  private buildFormData(request: any, photo?: File): FormData {
    const formData = new FormData();
    Object.keys(request).forEach(key => {
      if (request[key] !== null && request[key] !== undefined) {
        formData.append(key, request[key]);
      }
    });
    if (photo) formData.append('photo', photo);
    return formData;
  }
}
