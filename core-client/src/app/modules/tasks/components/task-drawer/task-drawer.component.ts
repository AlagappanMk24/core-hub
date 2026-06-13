import { CommonModule } from '@angular/common';
import { Component, Inject, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import {
  MAT_DIALOG_DATA,
  MatDialogRef,
  MatDialogModule,
  MatDialog,
} from '@angular/material/dialog';
import { animate, style, transition, trigger } from '@angular/animations';
import { TaskService } from '../../services/task.service';
import { AuthService } from '../../../../core/services/auth/auth.service';
import {
  Task,
  CreateTaskDto,
  UpdateTaskDto,
  TaskPriority,
  TaskStatus,
  PaginatedResult,
} from '../../../../interfaces/tasks/task.interface';
import { UserListDto, UserService } from '../../../user/services/user.service';
import { NotificationDialogComponent } from '../../../../shared/components/notification/notification-dialog.component';

@Component({
  selector: 'app-task-drawer',
  templateUrl: './task-drawer.component.html',
  styleUrls: ['./task-drawer.component.css'],
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatIconModule,
    MatButtonModule,
    MatInputModule,
    MatSelectModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatCheckboxModule,
    MatProgressSpinnerModule,
    MatDialogModule,
  ],
  animations: [
    trigger('slideIn', [
      transition(':enter', [
        style({ transform: 'translateX(100%)' }),
        animate('300ms ease-out', style({ transform: 'translateX(0)' })),
      ]),
      transition(':leave', [
        animate('300ms ease-in', style({ transform: 'translateX(100%)' })),
      ]),
    ]),
  ],
})
export class TaskDrawerComponent implements OnInit {
  isEditMode = false;
  saving = false;
  loading = false;
  taskId: number | null = null;
  loadingUsers = false;

  task: CreateTaskDto = {
    title: '',
    description: '',
    priority: TaskPriority.Medium,
    dueDate: undefined,
    category: '',
    tag: '',
    assignedToUserId: undefined,
    reminderDate: undefined,
    isRecurring: false,
    recurrencePattern: '',
    estimatedHours: undefined,
    parentTaskId: undefined,
  };

  priorityOptions = [
    {
      value: TaskPriority.Low,
      label: 'Low',
      icon: 'arrow_downward',
      color: '#10b981',
      bg: '#d1fae5',
    },
    {
      value: TaskPriority.Medium,
      label: 'Medium',
      icon: 'remove',
      color: '#3b82f6',
      bg: '#dbeafe',
    },
    {
      value: TaskPriority.High,
      label: 'High',
      icon: 'arrow_upward',
      color: '#f97316',
      bg: '#fed7aa',
    },
    {
      value: TaskPriority.Urgent,
      label: 'Urgent',
      icon: 'warning',
      color: '#ef4444',
      bg: '#fee2e2',
    },
  ];

  recurrencePatterns = [
    { value: 'Daily', label: 'Daily', icon: 'today' },
    { value: 'Weekly', label: 'Weekly', icon: 'date_range' },
    { value: 'Monthly', label: 'Monthly', icon: 'calendar_month' },
    { value: 'Quarterly', label: 'Quarterly', icon: 'calendar_view_month' },
    { value: 'Yearly', label: 'Yearly', icon: 'calendar_today' },
  ];

  users: UserListDto[] = [];
  parentTasks: Task[] = [];
  isAdmin = false;
  isUser = false;
  currentUser: any;

  constructor(
    private taskService: TaskService,
    private authService: AuthService,
    private userService: UserService,
    private dialog: MatDialog,
    private dialogRef: MatDialogRef<TaskDrawerComponent>,
    @Inject(MAT_DIALOG_DATA) public data: { taskId?: number },
  ) {
    if (data?.taskId) {
      this.isEditMode = true;
      this.taskId = data.taskId;
    }
  }

  ngOnInit(): void {
    this.isAdmin = this.authService.hasRole('Admin');
    this.isUser = this.authService.hasRole('User');
    this.currentUser = this.authService.getUserDetail();

    if (this.isEditMode && this.taskId) {
      this.loadTask();
    }
    this.loadUsers();
    this.loadParentTasks();
  }

  loadTask(): void {
    if (!this.taskId) return;
    this.loading = true;
    this.taskService.getTaskById(this.taskId).subscribe({
      next: (task) => {
        this.task = {
          title: task.title,
          description: task.description,
          priority: task.priority,
          dueDate: task.dueDate
            ? this.convertToLocalDate(task.dueDate)
            : undefined,
          category: task.category,
          tag: task.tag,
          assignedToUserId: task.assignedToUserId,
          reminderDate: task.reminderDate
            ? this.convertToLocalDateTime(task.reminderDate)
            : undefined,
          isRecurring: task.isRecurring,
          recurrencePattern: task.recurrencePattern,
          estimatedHours: task.estimatedHours,
          parentTaskId: task.parentTaskId,
        };
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading task:', error);
        this.loading = false;
        this.showErrorDialog(
          'Load Failed',
          'Failed to load task details.',
          error.message || 'Please try again.',
        );
      },
    });
  }

  // ✅ Helper method to convert UTC date to local date string (YYYY-MM-DD)
  private convertToLocalDate(date: Date | string): string {
    const d = new Date(date);
    const year = d.getFullYear();
    const month = String(d.getMonth() + 1).padStart(2, '0');
    const day = String(d.getDate()).padStart(2, '0');
    return `${year}-${month}-${day}`;
  }

  // ✅ Helper method to convert UTC datetime to local datetime string (YYYY-MM-DDThh:mm)
  private convertToLocalDateTime(date: Date | string): string {
    const d = new Date(date);
    const year = d.getFullYear();
    const month = String(d.getMonth() + 1).padStart(2, '0');
    const day = String(d.getDate()).padStart(2, '0');
    const hours = String(d.getHours()).padStart(2, '0');
    const minutes = String(d.getMinutes()).padStart(2, '0');
    return `${year}-${month}-${day}T${hours}:${minutes}`;
  }

  loadUsers(): void {
    this.loadingUsers = true;

    // Get current user's company ID from auth
    const userDetail = this.authService.getUserDetail();
    const companyId = userDetail?.companyId
      ? parseInt(userDetail.companyId)
      : null;

    // If admin, load all users; otherwise load users from same company
    if (this.isAdmin && companyId) {
      this.userService.getUsersByCompany(companyId).subscribe({
        next: (users) => {
          this.users = users;
          this.loadingUsers = false;
        },
        error: (error) => {
          console.error('Error loading users:', error);
          this.loadingUsers = false;
          this.showErrorDialog(
            'Load Failed',
            'Failed to load users list.',
            error.message || 'Please try again.',
          );
        },
      });
    } else {
      this.userService.getUserList().subscribe({
        next: (users) => {
          this.users = users;
          this.loadingUsers = false;
        },
        error: (error) => {
          console.error('Error loading users:', error);
          this.loadingUsers = false;
          this.showErrorDialog(
            'Load Failed',
            'Failed to load users list.',
            error.message || 'Please try again.',
          );
        },
      });
    }
  }

  getUserDisplayName(user: UserListDto): string {
    return user.fullName || user.userName || user.email;
  }

  loadParentTasks(): void {
    // Load potential parent tasks (excluding current task if in edit mode)
    const filter = {
      page: 1,
      pageSize: 100,
      sortBy: 'dueDate',
      sortDescending: false,
    };

    this.taskService.getTasks(filter).subscribe({
      next: (response: PaginatedResult<Task>) => {
        // Extract items array from paginated response
        const tasks = response.items || [];
        
        if (this.isEditMode && this.taskId) {
          // Exclude current task and its subtasks from parent options
          this.parentTasks = tasks.filter(
            (task: Task) => task.id !== this.taskId && task.parentTaskId !== this.taskId,
          );
        } else {
          this.parentTasks = tasks;
        }
      },
      error: (error) => {
        console.error('Error loading parent tasks:', error);
      },
    });
  }

  saveTask(): void {
    if (!this.validateForm()) return;

    this.saving = true;

    // Prepare task data with proper date handling
    const taskToSave = {
      ...this.task,
      dueDate: this.task.dueDate
        ? new Date(this.task.dueDate).toISOString()
        : undefined,
      reminderDate: this.task.reminderDate
        ? new Date(this.task.reminderDate).toISOString()
        : undefined,
    };

    if (this.isEditMode && this.taskId) {
      const updateDto: UpdateTaskDto = taskToSave;
      this.taskService.updateTask(this.taskId, updateDto).subscribe({
        next: (updatedTask) => {
          this.saving = false;
          this.dialogRef.close({ success: true, task: updatedTask });
        },
        error: (error) => {
          this.saving = false;
          console.error('Error updating task:', error);
          this.showErrorDialog(
            'Update Failed',
            'Failed to update task.',
            error.error?.message || error.message || 'Please try again.',
          );
        },
      });
    } else {
      this.taskService.createTask(taskToSave).subscribe({
        next: (newTask) => {
          this.saving = false;
          this.dialogRef.close({ success: true, task: newTask });
        },
        error: (error) => {
          console.error('Error creating task:', error);
          this.saving = false;
          this.showErrorDialog(
            'Creation Failed',
            'Failed to create task.',
            error.error?.message || error.message || 'Please try again.',
          );
        },
      });
    }
  }

  validateForm(): boolean {
    if (!this.task.title?.trim()) {
      this.showErrorDialog(
        'Validation Error',
        'Task title is required.',
        'Please enter a title for the task.',
      );
      return false;
    }
    return true;
  }

  close(): void {
    this.dialogRef.close();
  }

  private showErrorDialog(
    title: string,
    message: string,
    submessage: string,
  ): void {
    this.dialog.open(NotificationDialogComponent, {
      width: '400px',
      data: {
        type: 'error',
        title: title,
        message: message,
        submessage: submessage,
        buttonText: 'OK',
      },
    });
  }

  getPriorityIcon(priority: TaskPriority): string {
    const option = this.priorityOptions.find((p) => p.value === priority);
    return option?.icon || 'remove';
  }

  getPriorityColor(priority: TaskPriority): string {
    const option = this.priorityOptions.find((p) => p.value === priority);
    return option?.color || '#6b7280';
  }

  getPriorityBg(priority: TaskPriority): string {
    const option = this.priorityOptions.find((p) => p.value === priority);
    return option?.bg || '#f3f4f6';
  }

  getPriorityLabel(priority: TaskPriority): string {
    const option = this.priorityOptions.find((p) => p.value === priority);
    return option?.label || 'Medium';
  }
}