import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSelectModule } from '@angular/material/select';
import { MatChipsModule } from '@angular/material/chips';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatTooltipModule } from '@angular/material/tooltip';
import { Subject, debounceTime, distinctUntilChanged, takeUntil } from 'rxjs';
import { StudentService } from '../../core/services/student.service';
import { ExportService } from '../../core/services/export.service';
import { CourseService } from '../../core/services/course.service';
import { AuthService } from '../../core/services/auth.service';
import { Course, Student, StudentFilter } from '../../models/models';
import { HasRoleDirective } from '../../core/directives/has-role.directive';
import { ConfirmDialogComponent } from '../../shared/components/confirm-dialog/confirm-dialog.component';

@Component({
  selector: 'app-student-list',
  standalone: true,
  imports: [
    CommonModule, RouterLink, ReactiveFormsModule,
    MatTableModule, MatPaginatorModule, MatInputModule,
    MatFormFieldModule, MatButtonModule, MatIconModule,
    MatSelectModule, MatChipsModule, MatProgressSpinnerModule,
    MatDialogModule, MatSnackBarModule, MatTooltipModule,
    HasRoleDirective
  ],
  templateUrl: './student-list.component.html',
  styleUrls: ['./student-list.component.scss']
})
export class StudentListComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();

  students: Student[] = [];
  courses: Course[] = [];
  loading = false;
  exportingExcel = false;
  exportingPdf = false;

  totalCount = 0;
  currentPage = 1;
  pageSize = 10;

  searchControl = new FormControl('');
  courseFilter = new FormControl('');
  statusFilter = new FormControl('');

  displayedColumns = ['photo', 'name', 'email', 'course', 'enrollmentDate', 'status', 'actions'];

  constructor(
    private studentService: StudentService,
    private exportService: ExportService,
    private courseService: CourseService,
    public authService: AuthService,
    private router: Router,
    private dialog: MatDialog,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit(): void {
    this.loadCourses();
    this.loadStudents();

    this.searchControl.valueChanges.pipe(
      debounceTime(400), distinctUntilChanged(), takeUntil(this.destroy$)
    ).subscribe(() => { this.currentPage = 1; this.loadStudents(); });

    this.courseFilter.valueChanges.pipe(takeUntil(this.destroy$))
      .subscribe(() => { this.currentPage = 1; this.loadStudents(); });

    this.statusFilter.valueChanges.pipe(takeUntil(this.destroy$))
      .subscribe(() => { this.currentPage = 1; this.loadStudents(); });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadCourses(): void {
    this.courseService.getCourses().subscribe(res => { if (res.success) this.courses = res.data; });
  }

  loadStudents(): void {
    this.loading = true;
    const filter: StudentFilter = {
      page: this.currentPage,
      pageSize: this.pageSize,
      search: this.searchControl.value ?? undefined,
      courseId: this.courseFilter.value ? +this.courseFilter.value : undefined,
      isActive: this.statusFilter.value === 'true' ? true : this.statusFilter.value === 'false' ? false : undefined
    };

    this.studentService.getStudents(filter).subscribe({
      next: res => {
        if (res.success) {
          this.students = res.data.items;
          this.totalCount = res.data.totalCount;
        }
        this.loading = false;
      },
      error: () => { this.loading = false; }
    });
  }

  onPageChange(event: PageEvent): void {
    this.currentPage = event.pageIndex + 1;
    this.pageSize = event.pageSize;
    this.loadStudents();
  }

  viewStudent(id: number): void {
    this.router.navigate(['/students', id]);
  }

  editStudent(id: number, event: Event): void {
    event.stopPropagation();
    this.router.navigate(['/students', id, 'edit']);
  }

  deleteStudent(student: Student, event: Event): void {
    event.stopPropagation();
    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      width: '400px',
      data: {
        title: 'Delete Student',
        message: `Are you sure you want to delete ${student.fullName}? This action cannot be undone.`,
        confirmText: 'Delete',
        confirmColor: 'warn'
      }
    });

    dialogRef.afterClosed().subscribe(confirmed => {
      if (confirmed) {
        this.studentService.deleteStudent(student.id).subscribe({
          next: res => {
            if (res.success) {
              this.snackBar.open('Student deleted.', '', { duration: 3000, panelClass: ['success-snack'] });
              this.loadStudents();
            }
          }
        });
      }
    });
  }

  exportExcel(): void {
    this.exportingExcel = true;
    this.exportService.exportExcel().subscribe({
      next: blob => {
        this.exportService.downloadBlob(blob, `students_${Date.now()}.xlsx`);
        this.exportingExcel = false;
      },
      error: () => { this.exportingExcel = false; }
    });
  }

  exportPdf(): void {
    this.exportingPdf = true;
    this.exportService.exportPdf().subscribe({
      next: blob => {
        this.exportService.downloadBlob(blob, `students_${Date.now()}.pdf`);
        this.exportingPdf = false;
      },
      error: () => { this.exportingPdf = false; }
    });
  }

  getPhotoUrl(photoPath: string | null | undefined): string {
    return this.studentService.getPhotoUrl(photoPath);
  }

  getInitials(student: Student): string {
    return `${student.firstName[0] ?? ''}${student.lastName[0] ?? ''}`.toUpperCase();
  }

  clearFilters(): void {
    this.searchControl.setValue('');
    this.courseFilter.setValue('');
    this.statusFilter.setValue('');
  }
}
