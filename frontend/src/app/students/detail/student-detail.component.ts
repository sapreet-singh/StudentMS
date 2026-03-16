import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatDividerModule } from '@angular/material/divider';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { StudentService } from '../../core/services/student.service';
import { AuthService } from '../../core/services/auth.service';
import { Student } from '../../models/models';
import { HasRoleDirective } from '../../core/directives/has-role.directive';
import { ConfirmDialogComponent } from '../../shared/components/confirm-dialog/confirm-dialog.component';

@Component({
  selector: 'app-student-detail',
  standalone: true,
  imports: [
    CommonModule, RouterLink,
    MatCardModule, MatButtonModule, MatIconModule,
    MatChipsModule, MatDividerModule, MatProgressSpinnerModule,
    MatSnackBarModule, MatDialogModule, HasRoleDirective
  ],
  templateUrl: './student-detail.component.html',
  styleUrls: ['./student-detail.component.scss']
})
export class StudentDetailComponent implements OnInit {
  student: Student | null = null;
  loading = true;
  today = new Date();

  constructor(
    private studentService: StudentService,
    public authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private snackBar: MatSnackBar,
    private dialog: MatDialog
  ) {}

  ngOnInit(): void {
    const id = +this.route.snapshot.params['id'];
    this.studentService.getStudent(id).subscribe({
      next: res => { this.student = res.data; this.loading = false; },
      error: () => { this.loading = false; this.router.navigate(['/students']); }
    });
  }

  getAge(): number {
    if (!this.student?.dateOfBirth) return 0;
    const birthDate = new Date(this.student.dateOfBirth);
    const ageDifMs = Date.now() - birthDate.getTime();
    const ageDate = new Date(ageDifMs);
    return Math.abs(ageDate.getUTCFullYear() - 1970);
  }

  getPhotoUrl(): string {
    return this.studentService.getPhotoUrl(this.student?.photoPath);
  }

  getInitials(): string {
    if (!this.student) return '';
    return `${this.student.firstName[0]}${this.student.lastName[0]}`.toUpperCase();
  }

  deleteStudent(): void {
    if (!this.student) return;
    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      width: '400px',
      data: {
        title: 'Delete Student',
        message: `Are you sure you want to delete ${this.student.fullName}?`,
        confirmText: 'Delete',
        confirmColor: 'warn'
      }
    });
    dialogRef.afterClosed().subscribe(confirmed => {
      if (confirmed && this.student) {
        this.studentService.deleteStudent(this.student.id).subscribe({
          next: () => {
            this.snackBar.open('Student deleted.', '', { duration: 3000, panelClass: ['success-snack'] });
            this.router.navigate(['/students']);
          }
        });
      }
    });
  }
}
