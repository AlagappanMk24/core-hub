// src/app/modules/tasks/components/task-detail/task-detail.component.ts
import { CommonModule } from '@angular/common';
import { Component, OnInit, OnDestroy } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatMenuModule } from '@angular/material/menu';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatSnackBarModule } from '@angular/material/snack-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { Subscription } from 'rxjs';
import { TaskService } from '../../services/task.service';
import { AuthService } from '../../../../core/services/auth/auth.service';
import {
  Task,
  TaskStatus,
  TaskPriority,
  CreateTaskCommentDto
} from '../../../../interfaces/tasks/task.interface';
import { NotificationDialogComponent } from '../../../../shared/components/notification/notification-dialog.component';
import { DeleteConfirmationDialogComponent } from '../../../../features/common/delete-confirmation/delete-confirmation-dialog.component';
import { TaskDrawerComponent } from '../task-drawer/task-drawer.component';

@Component({
  selector: 'app-task-detail',
  templateUrl: './task-detail.component.html',
  styleUrls: ['./task-detail.component.css'],
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    RouterModule,
    MatIconModule,
    MatButtonModule,
    MatMenuModule,
    MatDialogModule,
    MatSnackBarModule,
    MatProgressSpinnerModule
  ]
})
export class TaskDetailComponent implements OnInit, OnDestroy {
  task: Task | null = null;
  loading = false;
  isAdmin = false;
  isUser = false;
  newComment = '';
  selectedFile: File | null = null;
  uploading = false;
  activeTab: 'details' | 'comments' | 'attachments' | 'subtasks' = 'details';
  
  private subscriptions: Subscription = new Subscription();

  constructor(
    private taskService: TaskService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private dialog: MatDialog
  ) {}

  ngOnInit(): void {
    this.isAdmin = this.authService.hasRole('Admin');
    this.isUser = this.authService.hasRole('User');
    
    this.route.params.subscribe(params => {
      if (params['id']) {
        this.loadTask(+params['id']);
      }
    });
  }

  ngOnDestroy(): void {
    this.subscriptions.unsubscribe();
  }

  loadTask(id: number): void {
    this.loading = true;
    this.taskService.getTaskById(id).subscribe({
      next: (task) => {
        this.task = task;
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading task:', error);
        this.loading = false;
        this.showErrorDialog(
          'Load Failed',
          'Failed to load task details.',
          error.message || 'Please try again.'
        );
      }
    });
  }

  updateStatus(status: TaskStatus): void {
    if (!this.task) return;
    
    this.taskService.updateTaskStatus(this.task.id, status).subscribe({
      next: (updatedTask) => {
        this.task = updatedTask;
        this.showSuccessDialog(
          'Status Updated',
          `Task status updated to ${this.getStatusText(status)}`,
          'The task status has been successfully updated.'
        );
      },
      error: (error) => {
        console.error('Error updating status:', error);
        this.showErrorDialog(
          'Update Failed',
          'Failed to update task status.',
          error.message || 'Please try again.'
        );
      }
    });
  }

  addComment(): void {
    if (!this.task || !this.newComment.trim()) return;
    
    const comment: CreateTaskCommentDto = { comment: this.newComment };
    this.taskService.addComment(this.task.id, comment).subscribe({
      next: (newComment) => {
        if (this.task) {
          if (!this.task.comments) this.task.comments = [];
          this.task.comments.push(newComment);
          this.task.commentCount = (this.task.commentCount || 0) + 1;
        }
        this.newComment = '';
        this.showSuccessDialog(
          'Comment Added',
          'Your comment has been added successfully.',
          'The comment is now visible to all team members.'
        );
      },
      error: (error) => {
        console.error('Error adding comment:', error);
        this.showErrorDialog(
          'Add Failed',
          'Failed to add comment.',
          error.message || 'Please try again.'
        );
      }
    });
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      this.selectedFile = input.files[0];
      this.uploadAttachment();
    }
  }

  uploadAttachment(): void {
    if (!this.task || !this.selectedFile) return;
    
    this.uploading = true;
    this.taskService.uploadAttachment(this.task.id, this.selectedFile).subscribe({
      next: (attachment) => {
        if (this.task) {
          if (!this.task.attachments) this.task.attachments = [];
          this.task.attachments.push(attachment);
          this.task.attachmentCount = (this.task.attachmentCount || 0) + 1;
        }
        this.selectedFile = null;
        this.uploading = false;
        this.showSuccessDialog(
          'Attachment Added',
          'File uploaded successfully.',
          `"${attachment.fileName}" has been attached to this task.`
        );
      },
      error: (error) => {
        console.error('Error uploading attachment:', error);
        this.uploading = false;
        this.showErrorDialog(
          'Upload Failed',
          'Failed to upload attachment.',
          error.message || 'Please try again.'
        );
      }
    });
  }

  deleteAttachment(attachmentId: number, fileName: string): void {
    if (!this.task) return;
    
    const dialogRef = this.dialog.open(DeleteConfirmationDialogComponent, {
      width: '400px',
      data: {
        title: 'Delete Attachment',
        message: `Are you sure you want to delete "${fileName}"?`,
        confirmText: 'Delete',
        cancelText: 'Cancel'
      }
    });
    
    dialogRef.afterClosed().subscribe(result => {
      if (result && this.task) {
        this.taskService.deleteAttachment(this.task.id, attachmentId).subscribe({
          next: () => {
            if (this.task && this.task.attachments) {
              this.task.attachments = this.task.attachments.filter(a => a.id !== attachmentId);
              this.task.attachmentCount = (this.task.attachmentCount || 1) - 1;
            }
            this.showSuccessDialog(
              'Attachment Deleted',
              'File deleted successfully.',
              'The attachment has been removed from this task.'
            );
          },
          error: (error) => {
            console.error('Error deleting attachment:', error);
            this.showErrorDialog(
              'Delete Failed',
              'Failed to delete attachment.',
              error.message || 'Please try again.'
            );
          }
        });
      }
    });
  }

  downloadAttachment(attachmentId: number, fileName: string): void {
    if (!this.task) return;
    
    this.taskService.downloadAttachment(this.task.id, attachmentId).subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = fileName;
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        window.URL.revokeObjectURL(url);
      },
      error: (error) => {
        console.error('Error downloading attachment:', error);
        this.showErrorDialog(
          'Download Failed',
          'Failed to download attachment.',
          error.message || 'Please try again.'
        );
      }
    });
  }

  editTask(): void {
    if (!this.task) return;
    
    const dialogRef = this.dialog.open(TaskDrawerComponent, {
      width: '600px',
      maxWidth: '90vw',
      height: '100vh',
      maxHeight: '100vh',
      position: { right: '0', top: '0' },
      panelClass: ['full-height-dialog', 'task-drawer-panel'],
      disableClose: true,
      data: { taskId: this.task.id }
    });
    
    dialogRef.afterClosed().subscribe(result => {
      if (result?.success && this.task) {
        this.loadTask(this.task.id);
      }
    });
  }

  deleteTask(): void {
    if (!this.task) return;
    
    const dialogRef = this.dialog.open(DeleteConfirmationDialogComponent, {
      width: '400px',
      data: {
        title: 'Delete Task',
        message: `Are you sure you want to delete "${this.task.title}"?`,
        submessage: 'This action cannot be undone. All comments and attachments will be permanently deleted.',
        confirmText: 'Delete',
        cancelText: 'Cancel'
      }
    });
    
    dialogRef.afterClosed().subscribe(result => {
      if (result && this.task) {
        this.taskService.deleteTask(this.task.id).subscribe({
          next: () => {
            this.router.navigate(['/tasks/list']);
            this.showSuccessDialog(
              'Task Deleted',
              'Task deleted successfully.',
              'The task has been moved to trash.'
            );
          },
          error: (error) => {
            console.error('Error deleting task:', error);
            this.showErrorDialog(
              'Delete Failed',
              'Failed to delete task.',
              error.message || 'Please try again.'
            );
          }
        });
      }
    });
  }

  duplicateTask(): void {
    if (!this.task) return;
    
    // Implement duplicate functionality if backend supports
    this.showInfoDialog(
      'Feature Coming Soon',
      'Task duplication will be available soon.',
      'This feature is currently under development.'
    );
  }

  getStatusText(status: TaskStatus): string {
    return TaskStatus[status];
  }

  getPriorityText(priority: TaskPriority): string {
    return TaskPriority[priority];
  }

  getPriorityClass(priority: TaskPriority): string {
    switch (priority) {
      case TaskPriority.Low: return 'priority-low';
      case TaskPriority.Medium: return 'priority-medium';
      case TaskPriority.High: return 'priority-high';
      case TaskPriority.Urgent: return 'priority-urgent';
      default: return '';
    }
  }

  getStatusClass(status: TaskStatus): string {
    switch (status) {
      case TaskStatus.Pending: return 'status-pending';
      case TaskStatus.InProgress: return 'status-in-progress';
      case TaskStatus.Completed: return 'status-completed';
      case TaskStatus.Cancelled: return 'status-cancelled';
      case TaskStatus.OnHold: return 'status-on-hold';
      case TaskStatus.Overdue: return 'status-overdue';
      default: return '';
    }
  }

  getPriorityIcon(priority: TaskPriority): string {
    switch (priority) {
      case TaskPriority.Low: return 'arrow_downward';
      case TaskPriority.Medium: return 'remove';
      case TaskPriority.High: return 'arrow_upward';
      case TaskPriority.Urgent: return 'warning';
      default: return 'remove';
    }
  }

  formatFileSize(bytes: number): string {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
  }

  getDaysUntilDue(dueDate?: Date): number {
    if (!dueDate) return 0;
    const today = new Date();
    today.setHours(0, 0, 0, 0);
    const due = new Date(dueDate);
    due.setHours(0, 0, 0, 0);
    const diffTime = due.getTime() - today.getTime();
    return Math.ceil(diffTime / (1000 * 60 * 60 * 24));
  }

  isOverdue(dueDate?: Date): boolean {
    if (!dueDate) return false;
    return new Date(dueDate) < new Date() && this.getDaysUntilDue(dueDate) < 0;
  }

  goBack(): void {
    this.router.navigate(['/tasks/list']);
  }

  private showSuccessDialog(title: string, message: string, submessage: string): void {
    this.dialog.open(NotificationDialogComponent, {
      width: '400px',
      data: {
        type: 'success',
        title: title,
        message: message,
        submessage: submessage,
        buttonText: 'OK'
      }
    });
  }

  private showErrorDialog(title: string, message: string, submessage: string): void {
    this.dialog.open(NotificationDialogComponent, {
      width: '400px',
      data: {
        type: 'error',
        title: title,
        message: message,
        submessage: submessage,
        buttonText: 'OK'
      }
    });
  }

  private showInfoDialog(title: string, message: string, submessage: string): void {
    this.dialog.open(NotificationDialogComponent, {
      width: '400px',
      data: {
        type: 'info',
        title: title,
        message: message,
        submessage: submessage,
        buttonText: 'OK'
      }
    });
  }

  setActiveTab(tab: 'details' | 'comments' | 'attachments' | 'subtasks'): void {
    this.activeTab = tab;
  }
}