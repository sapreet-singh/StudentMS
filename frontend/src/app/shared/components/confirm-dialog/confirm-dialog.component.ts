import { Component, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

export interface ConfirmDialogData {
  title: string;
  message: string;
  confirmText?: string;
  cancelText?: string;
  confirmColor?: 'primary' | 'warn' | 'accent';
}

@Component({
  selector: 'app-confirm-dialog',
  standalone: true,
  imports: [CommonModule, MatDialogModule, MatButtonModule, MatIconModule],
  template: `
    <div class="confirm-dialog">
      <div class="dialog-icon" [class.warn]="data.confirmColor === 'warn'">
        <mat-icon>{{ data.confirmColor === 'warn' ? 'warning' : 'help' }}</mat-icon>
      </div>
      <h2 mat-dialog-title>{{ data.title }}</h2>
      <mat-dialog-content>
        <p>{{ data.message }}</p>
      </mat-dialog-content>
      <mat-dialog-actions align="end">
        <button mat-stroked-button (click)="dialogRef.close(false)">
          {{ data.cancelText || 'Cancel' }}
        </button>
        <button mat-raised-button [color]="data.confirmColor || 'primary'" (click)="dialogRef.close(true)">
          {{ data.confirmText || 'Confirm' }}
        </button>
      </mat-dialog-actions>
    </div>
  `,
  styles: [`
    .confirm-dialog { padding: 8px; text-align: center; }
    .dialog-icon {
      width: 64px; height: 64px; border-radius: 50%;
      background: #e3f2fd; display: flex; align-items: center;
      justify-content: center; margin: 0 auto 16px;
      mat-icon { font-size: 32px; width: 32px; height: 32px; color: #1565c0; }
    }
    .dialog-icon.warn {
      background: #ffebee;
      mat-icon { color: #c62828; }
    }
    h2 { font-size: 20px; font-weight: 700; margin-bottom: 8px; }
    p { color: #666; font-size: 14px; line-height: 1.6; }
    mat-dialog-actions { padding: 16px 0 0; gap: 10px; }
  `]
})
export class ConfirmDialogComponent {
  constructor(
    public dialogRef: MatDialogRef<ConfirmDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: ConfirmDialogData
  ) {}
}
