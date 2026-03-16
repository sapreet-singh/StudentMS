// API Response wrapper
export interface ApiResponse<T> {
  success: boolean;
  message: string;
  data: T;
  errors?: string[];
}

export interface PagedResponse<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  hasPrevious: boolean;
  hasNext: boolean;
}

// Auth models
export interface LoginRequest {
  username: string;
  password: string;
}

export interface RegisterRequest {
  username: string;
  email: string;
  password: string;
  roleName: string;
}

export interface AuthResponse {
  token: string;
  refreshToken: string;
  expiresAt: string;
  user: UserInfo;
}

export interface UserInfo {
  id: number;
  username: string;
  email: string;
  role: string;
}

// Student models
export interface Student {
  id: number;
  firstName: string;
  lastName: string;
  fullName: string;
  email: string;
  phone: string;
  dateOfBirth: string;
  enrollmentDate: string;
  courseId: number;
  courseName: string;
  isActive: boolean;
  photoPath?: string;
  createdAt: string;
}

export interface CreateStudentRequest {
  firstName: string;
  lastName: string;
  email: string;
  phone: string;
  dateOfBirth: string;
  enrollmentDate: string;
  courseId: number;
  isActive: boolean;
}

export interface UpdateStudentRequest extends CreateStudentRequest {}

export interface StudentFilter {
  page?: number;
  pageSize?: number;
  search?: string;
  courseId?: number;
  isActive?: boolean;
}

// Course model
export interface Course {
  id: number;
  name: string;
  description: string;
}

// Dashboard models
export interface DashboardStats {
  totalStudents: number;
  activeStudents: number;
  totalCourses: number;
  newEnrollmentsThisMonth: number;
}

export interface EnrollmentByMonth {
  month: string;
  year: number;
  count: number;
}

export interface StudentsByCourse {
  courseName: string;
  count: number;
}
