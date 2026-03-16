import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSelectModule } from '@angular/material/select';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { StudentService } from '../../core/services/student.service';
import { CourseService } from '../../core/services/course.service';
import { Course } from '../../models/models';

@Component({
  selector: 'app-student-add-edit',
  standalone: true,
  imports: [
    CommonModule, RouterLink, ReactiveFormsModule,
    MatFormFieldModule, MatInputModule, MatButtonModule,
    MatIconModule, MatSelectModule, MatDatepickerModule,
    MatNativeDateModule, MatSlideToggleModule,
    MatProgressSpinnerModule, MatSnackBarModule
  ],
  templateUrl: './student-add-edit.component.html',
  styleUrls: ['./student-add-edit.component.scss']
})
export class StudentAddEditComponent implements OnInit {
  form!: FormGroup;
  isEdit = false;
  studentId?: number;
  loading = false;
  saving = false;
  courses: Course[] = [];
  photoFile?: File;
  photoPreview?: string;
  maxDate = new Date();

  constructor(
    private fb: FormBuilder,
    private studentService: StudentService,
    private courseService: CourseService,
    private route: ActivatedRoute,
    private router: Router,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit(): void {
    this.initForm();
    this.loadCourses();

    this.studentId = this.route.snapshot.params['id'] ? +this.route.snapshot.params['id'] : undefined;
    this.isEdit = !!this.studentId;

    if (this.isEdit) this.loadStudent();
  }

  initForm(): void {
    this.form = this.fb.group({
      firstName: ['', [Validators.required, Validators.maxLength(100)]],
      lastName: ['', [Validators.required, Validators.maxLength(100)]],
      email: ['', [Validators.required, Validators.email]],
      phone: ['', [Validators.required, Validators.pattern(/^\+?[0-9\s\-\(\)]+$/)]],
      dateOfBirth: ['', Validators.required],
      enrollmentDate: ['', Validators.required],
      courseId: ['', Validators.required],
      isActive: [true]
    });
  }

  loadCourses(): void {
    this.courseService.getCourses().subscribe(res => { if (res.success) this.courses = res.data; });
  }

  loadStudent(): void {
    this.loading = true;
    this.studentService.getStudent(this.studentId!).subscribe({
      next: res => {
        if (res.success) {
          const s = res.data;
          this.form.patchValue({
            firstName: s.firstName,
            lastName: s.lastName,
            email: s.email,
            phone: s.phone,
            dateOfBirth: new Date(s.dateOfBirth),
            enrollmentDate: new Date(s.enrollmentDate),
            courseId: s.courseId,
            isActive: s.isActive
          });
          if (s.photoPath) {
            this.photoPreview = this.studentService.getPhotoUrl(s.photoPath);
          }
        }
        this.loading = false;
      },
      error: () => { this.loading = false; }
    });
  }

  onPhotoSelected(event: Event): void {
    const file = (event.target as HTMLInputElement).files?.[0];
    if (!file) return;

    const allowed = ['image/jpeg', 'image/jpg', 'image/png'];
    if (!allowed.includes(file.type)) {
      this.snackBar.open('Only JPG/PNG files allowed.', 'Close', { duration: 3000, panelClass: ['error-snack'] });
      return;
    }
    if (file.size > 2 * 1024 * 1024) {
      this.snackBar.open('File size must be less than 2MB.', 'Close', { duration: 3000, panelClass: ['error-snack'] });
      return;
    }

    this.photoFile = file;
    const reader = new FileReader();
    reader.onload = e => { this.photoPreview = e.target?.result as string; };
    reader.readAsDataURL(file);
  }

  removePhoto(): void {
    this.photoFile = undefined;
    this.photoPreview = undefined;
  }

  onSubmit(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.saving = true;

    const value = this.form.value;
    const request = {
      ...value,
      dateOfBirth: new Date(value.dateOfBirth).toISOString(),
      enrollmentDate: new Date(value.enrollmentDate).toISOString()
    };

    const obs = this.isEdit
      ? this.studentService.updateStudent(this.studentId!, request, this.photoFile)
      : this.studentService.createStudent(request, this.photoFile);

    obs.subscribe({
      next: res => {
        if (res.success) {
          this.snackBar.open(
            this.isEdit ? 'Student updated!' : 'Student created!',
            '', { duration: 3000, panelClass: ['success-snack'] }
          );
          this.router.navigate(['/students']);
        } else {
          this.snackBar.open(res.message, 'Close', { duration: 4000, panelClass: ['error-snack'] });
        }
        this.saving = false;
      },
      error: () => { this.saving = false; }
    });
  }
}
