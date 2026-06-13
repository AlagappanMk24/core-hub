// src/app/components/tasks/task-list/task-list.component.ts
import { CommonModule } from '@angular/common';
import { Component, OnInit, OnDestroy } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatMenuModule } from '@angular/material/menu';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { Subject, debounceTime, distinctUntilChanged } from 'rxjs';
import { animate, style, transition, trigger } from '@angular/animations';
import { TaskService } from '../../services/task.service';
import { AuthService } from '../../../../core/services/auth/auth.service';
import { DeleteConfirmationDialogComponent } from '../../../../features/common/delete-confirmation/delete-confirmation-dialog.component';
import { NotificationDialogComponent } from '../../../../shared/components/notification/notification-dialog.component';
import {
  Task,
  TaskStatus,
  TaskPriority,
  TaskFilterDto,
  TaskStats,
} from '../../../../interfaces/tasks/task.interface';
import { TaskDrawerComponent } from '../task-drawer/task-drawer.component';

@Component({
  selector: 'app-task-list',
  templateUrl: './task-list.component.html',
  styleUrls: ['./task-list.component.css'],
  animations: [
    trigger('fadeIn', [
      transition(':enter', [
        style({ opacity: 0 }),
        animate('300ms ease', style({ opacity: 1 })),
      ]),
    ]),
    trigger('scaleIn', [
      transition('void => *', [
        style({ transform: 'scale(0.95)', opacity: 0.8 }),
        animate('200ms ease-out', style({ transform: 'scale(1)', opacity: 1 })),
      ]),
      transition('* => *', [
        style({ transform: 'scale(0.95)' }),
        animate('150ms ease-out', style({ transform: 'scale(1)' })),
      ]),
    ]),
    trigger('pulse', [
      transition(':enter', [
        style({ boxShadow: '0 0 0 0 rgba(138, 43, 226, 0.5)' }),
        animate(
          '600ms ease-in-out',
          style({ boxShadow: '0 0 0 8px rgba(138, 43, 226, 0)' }),
        ),
      ]),
    ]),
  ],
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatIconModule,
    MatButtonModule,
    MatMenuModule,
    MatProgressSpinnerModule,
    MatDialogModule,
  ],
})
export class TaskListComponent implements OnInit, OnDestroy {
  tasks: Task[] = [];
  isLoading = false;
  exportingFormat: 'excel' | 'pdf' | null = null;
  isDeleting = false;
  currentPage = 1;
  itemsPerPage = 10;
  totalItems = 0;
  totalPages = 0;
  selectedTaskStatus: string | null = 'All';
  selectedTaskPriority: string | null = null;
  searchTerm = '';

  stats: TaskStats = {
    totalTasks: 0,
    myTasks: 0,
    tasksAssignedToMe: 0,
    tasksCreatedByMe: 0,
    pendingTasks: 0,
    inProgressTasks: 0,
    completedTasks: 0,
    cancelledTasks: 0,
    onHoldTasks: 0,
    overdueTasks: 0,
    highPriorityTasks: 0,
    urgentPriorityTasks: 0,
    tasksDueToday: 0,
    tasksDueThisWeek: 0,
    tasksDueThisMonth: 0,
    tasksByCategory: {},
    tasksByStatus: {},
    tasksByPriority: {},
  };

  // Column visibility
  visibleColumns: { [key: string]: boolean } = {
    title: true,
    priority: true,
    status: true,
    dueDate: true,
    category: true,
    actions: true,
  };

  columnOptions = [
    { key: 'title', label: 'Task Title' },
    { key: 'priority', label: 'Priority' },
    { key: 'status', label: 'Status' },
    { key: 'dueDate', label: 'Due Date' },
    { key: 'category', label: 'Category' },
    { key: 'actions', label: 'Actions' },
  ];

  // Sort properties
  sortField: string = 'dueDate';
  sortDirection: 'asc' | 'desc' = 'asc';

  private searchSubject = new Subject<string>();
  private destroy$ = new Subject<void>();
  isAdmin = false;
  isUser = false;
  isCustomer = false;
  isEditMode: any;

  constructor(
    private taskService: TaskService,
    private authService: AuthService,
    private router: Router,
    private dialog: MatDialog,
  ) {}

  ngOnInit(): void {
    // Check roles
    this.isAdmin = this.authService.hasRole('Admin');
    this.isUser = this.authService.hasRole('User');
    this.isCustomer = this.authService.hasRole('Customer');

    this.loadTasks();
    this.loadStats();
    this.setupSearch();
    this.loadColumnPreferences();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private setupSearch(): void {
    this.searchSubject
      .pipe(debounceTime(300), distinctUntilChanged())
      .subscribe((searchTerm) => {
        this.searchTerm = searchTerm;
        this.currentPage = 1;
        this.loadTasks();
      });
  }

loadTasks(): void {
  this.isLoading = true;
  const filter: TaskFilterDto = {
    page: this.currentPage,
    pageSize: this.itemsPerPage,
    sortBy: 'dueDate',
    sortDescending: false,
    priority: this.selectedTaskPriority ? this.getPriorityValue(this.selectedTaskPriority) : undefined,
    searchTerm: this.searchTerm || undefined,
    myTasks: this.selectedTaskStatus === 'My Tasks' ? true : undefined
  };

  this.taskService.getTasks(filter).subscribe({
    next: (response: any) => {
      console.log('Raw response from getTasks:', response);
      
      // Handle different response formats
      if (Array.isArray(response)) {
        // Direct array response
        this.tasks = response;
        this.totalItems = response.length;
        this.totalPages = Math.ceil(this.totalItems / this.itemsPerPage);
      } else if (response?.items && Array.isArray(response.items)) {
        // Paginated response with items array
        this.tasks = response.items;
        this.totalItems = response.totalCount || response.items.length;
        this.totalPages = response.totalPages || Math.ceil(this.totalItems / this.itemsPerPage);
      } else if (response?.data && Array.isArray(response.data)) {
        // Response with data array
        this.tasks = response.data;
        this.totalItems = response.total || response.data.length;
        this.totalPages = Math.ceil(this.totalItems / this.itemsPerPage);
      } else {
        // Fallback
        this.tasks = [];
        this.totalItems = 0;
        this.totalPages = 0;
      }
      
      console.log(`Loaded ${this.tasks.length} tasks, Total: ${this.totalItems}, Pages: ${this.totalPages}`);
      
      this.sortTasks();
      this.isLoading = false;
    },
    error: (error) => {
      console.error('Error fetching tasks:', error);
      this.isLoading = false;
      this.tasks = [];
      this.totalItems = 0;
      this.totalPages = 0;
    },
  });
}

  loadStats(): void {
    this.taskService.getTaskStats().subscribe({
      next: (stats) => {
        this.stats = stats;
      },
      error: (error) => {
        console.error('Error fetching task stats:', error);
      },
    });
  }

  onSearch(searchTerm: string): void {
    this.searchSubject.next(searchTerm);
  }

  onSelectTaskStatus(status: string): void {
    this.selectedTaskStatus = status;
    this.selectedTaskPriority = null;
    this.currentPage = 1;
    this.loadTasks();
  }

  onSelectTaskPriority(priority: string): void {
    this.selectedTaskPriority = priority;
    this.selectedTaskStatus = null;
    this.currentPage = 1;
    this.loadTasks();
  }

  getStatusValue(status: string): TaskStatus | undefined {
    switch (status) {
      case 'Pending':
        return TaskStatus.Pending;
      case 'InProgress':
        return TaskStatus.InProgress;
      case 'Completed':
        return TaskStatus.Completed;
      case 'Cancelled':
        return TaskStatus.Cancelled;
      case 'OnHold':
        return TaskStatus.OnHold;
      case 'Overdue':
        return TaskStatus.Overdue;
      default:
        return undefined;
    }
  }

  getPriorityValue(priority: string): TaskPriority | undefined {
    switch (priority) {
      case 'Low':
        return TaskPriority.Low;
      case 'Medium':
        return TaskPriority.Medium;
      case 'High':
        return TaskPriority.High;
      case 'Urgent':
        return TaskPriority.Urgent;
      default:
        return undefined;
    }
  }

  getStatusText(status: TaskStatus): string {
    return TaskStatus[status];
  }

  getPriorityText(priority: TaskPriority): string {
    return TaskPriority[priority];
  }

  getStatusClass(status: TaskStatus): string {
    switch (status) {
      case TaskStatus.Pending:
        return 'status-pending';
      case TaskStatus.InProgress:
        return 'status-in-progress';
      case TaskStatus.Completed:
        return 'status-completed';
      case TaskStatus.Cancelled:
        return 'status-cancelled';
      case TaskStatus.OnHold:
        return 'status-on-hold';
      case TaskStatus.Overdue:
        return 'status-overdue';
      default:
        return '';
    }
  }

  getPriorityClass(priority: TaskPriority): string {
    switch (priority) {
      case TaskPriority.Low:
        return 'priority-low';
      case TaskPriority.Medium:
        return 'priority-medium';
      case TaskPriority.High:
        return 'priority-high';
      case TaskPriority.Urgent:
        return 'priority-urgent';
      default:
        return '';
    }
  }

  getPriorityIcon(priority: TaskPriority): string {
    switch (priority) {
      case TaskPriority.Low:
        return 'arrow_downward';
      case TaskPriority.Medium:
        return 'remove';
      case TaskPriority.High:
        return 'arrow_upward';
      case TaskPriority.Urgent:
        return 'warning';
      default:
        return 'remove';
    }
  }

  getInitials(name: string | undefined): string {
    const displayName = name || 'Unknown';
    return displayName
      .split(' ')
      .map((n) => n.charAt(0).toUpperCase())
      .join('')
      .substring(0, 2);
  }

  getAvatarColor(name: string | undefined): string {
    const colors = [
      '#FF2E63',
      '#00D4B9',
      '#FF6B6B',
      '#FFD93D',
      '#1E90FF',
      '#8A2BE2',
      '#4B0082',
    ];
    const fallbackName = name || 'Unknown';
    const index =
      fallbackName
        .split('')
        .reduce((sum, char) => sum + char.charCodeAt(0), 0) % colors.length;
    return colors[index];
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

  goToPage(page: number): void {
    if (page >= 1 && page <= this.totalPages) {
      this.currentPage = page;
      this.loadTasks();
    }
  }

  nextPage(): void {
    if (this.currentPage < this.totalPages) {
      this.currentPage++;
      this.loadTasks();
    }
  }

  previousPage(): void {
    if (this.currentPage > 1) {
      this.currentPage--;
      this.loadTasks();
    }
  }

  getVisiblePages(): number[] {
    const maxVisiblePages = 5;
    let startPage = Math.max(
      1,
      this.currentPage - Math.floor(maxVisiblePages / 2),
    );
    let endPage = Math.min(this.totalPages, startPage + maxVisiblePages - 1);

    if (endPage - startPage + 1 < maxVisiblePages) {
      startPage = Math.max(1, endPage - maxVisiblePages + 1);
    }

    const visiblePages: number[] = [];
    for (let i = startPage; i <= endPage; i++) {
      visiblePages.push(i);
    }
    return visiblePages;
  }

  onCreateTask(): void {
    const dialogRef = this.dialog.open(TaskDrawerComponent, {
      width: '600px',
      maxWidth: '90vw',
      height: '100vh',
      maxHeight: '100vh',
      position: { right: '0', top: '0' },
      panelClass: ['full-height-dialog', 'task-drawer-panel'],
      disableClose: true,
      autoFocus: true,
      data: {},
    });

    dialogRef.afterClosed().subscribe((result) => {
      if (result?.success) {
        this.loadTasks();
        this.loadStats();
        this.openDialog(
          'success',
          'Task Created',
          `Task "${result.task.title}" has been created successfully!`,
          'Your new task has been added to the list.',
        );
      }
    });
  }

  onEditTask(task: Task): void {
    const dialogRef = this.dialog.open(TaskDrawerComponent, {
      width: '600px',
      maxWidth: '90vw',
      height: '100vh',
      maxHeight: '100vh',
      position: { right: '0', top: '0' },
      panelClass: ['full-height-dialog', 'task-drawer-panel'],
      disableClose: true,
      autoFocus: true,
      data: { taskId: task.id },
    });

    dialogRef.afterClosed().subscribe((result) => {
      if (result?.success) {
        this.loadTasks();
        this.loadStats();
        this.openDialog(
          'success',
          'Task Updated',
          `Task "${result.task.title}" has been updated successfully!`,
          'The task changes have been saved.',
        );
      }
    });
  }

  onViewTask(task: Task): void {
    this.router.navigate([`/tasks/view/${task.id}`]);
  }

  onDeleteTask(task: Task): void {
    const dialogRef = this.dialog.open(DeleteConfirmationDialogComponent, {
      width: '450px',
      disableClose: true,
      panelClass: 'delete-dialog-panel',
      data: {
        title: 'Delete Task',
        message: `Are you sure you want to delete task "${task.title}"? This action cannot be undone.`,
        itemName: task.title,
        confirmText: 'Delete',
        cancelText: 'Cancel',
      },
    });

    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        this.isDeleting = true;
        this.taskService.deleteTask(task.id).subscribe({
          next: () => {
            this.loadTasks();
            this.loadStats();
            this.openDialog(
              'success',
              'Task Deleted Successfully',
              `Task "${task.title}" has been deleted successfully!`,
              'The task has been moved to trash and is no longer visible in your active task list.',
            );
            this.isDeleting = false;
          },
          error: (error) => {
            console.error('Error deleting task:', error);
            this.openDialog(
              'error',
              'Delete Failed',
              'Failed to delete task. Please try again.',
              'The task could not be deleted due to a system error. Please try again.',
            );
            this.isDeleting = false;
          },
        });
      }
    });
  }

  onUpdateStatus(task: Task, status: TaskStatus): void {
    this.taskService.updateTaskStatus(task.id, status).subscribe({
      next: (updatedTask) => {
        const index = this.tasks.findIndex((t) => t.id === updatedTask.id);
        if (index !== -1) {
          this.tasks[index] = updatedTask;
        }
        this.loadStats();
        this.openDialog(
          'success',
          'Status Updated',
          `Task status updated to ${this.getStatusText(status)}`,
          `The task "${task.title}" status has been successfully updated.`,
        );
      },
      error: (error) => {
        console.error('Error updating task status:', error);
        this.openDialog(
          'error',
          'Update Failed',
          'Failed to update task status.',
          'Please try again or contact support if the issue persists.',
        );
      },
    });
  }

  openDialog(
    type: 'success' | 'error' | 'info',
    title: string,
    message: string,
    submessage: string,
  ): void {
    this.dialog.open(NotificationDialogComponent, {
      width: '400px',
      data: { type, title, message, submessage },
    });
  }

  trackByTask(index: number, task: Task): number {
    return task.id;
  }

  // Column visibility methods
  toggleColumn(column: string): void {
    if (this.visibleColumns.hasOwnProperty(column)) {
      this.visibleColumns[column] = !this.visibleColumns[column];
      this.saveColumnPreferences();
    }
  }

  saveColumnPreferences(): void {
    localStorage.setItem(
      'taskColumnVisibility',
      JSON.stringify(this.visibleColumns),
    );
  }

  loadColumnPreferences(): void {
    const saved = localStorage.getItem('taskColumnVisibility');
    if (saved) {
      this.visibleColumns = JSON.parse(saved);
    }
  }

  // Sort methods
  sortBy(field: string): void {
    if (this.sortField === field) {
      this.sortDirection = this.sortDirection === 'asc' ? 'desc' : 'asc';
    } else {
      this.sortField = field;
      this.sortDirection = 'asc';
    }
    this.sortTasks();
  }

  sortTasks(): void {
    this.tasks.sort((a, b) => {
      let aVal = a[this.sortField as keyof Task];
      let bVal = b[this.sortField as keyof Task];

      if (typeof aVal === 'number' && typeof bVal === 'number') {
        return this.sortDirection === 'asc' ? aVal - bVal : bVal - aVal;
      }

      const aStr = String(aVal || '').toLowerCase();
      const bStr = String(bVal || '').toLowerCase();

      return this.sortDirection === 'asc'
        ? aStr.localeCompare(bStr)
        : bStr.localeCompare(aStr);
    });
  }

  // Export methods
  exportTasks(format: 'excel' | 'pdf'): void {
    this.exportingFormat = format;
    // Implement export logic here
    setTimeout(() => {
      this.exportingFormat = null;
    }, 2000);
  }

  printTaskList(): void {
    const printWindow = window.open('', '_blank');
    if (printWindow) {
      printWindow.document.write(`
        <html>
          <head><title>Task List</title>
          <style>
            body { font-family: Arial, sans-serif; margin: 20px; }
            table { width: 100%; border-collapse: collapse; }
            th, td { border: 1px solid #ddd; padding: 8px; text-align: left; }
            th { background-color: #f2f2f2; }
          </style>
          </head>
          <body>
            <h1>Task List</h1>
            ${document.querySelector('.task-table')?.outerHTML || ''}
          </body>
        </html>
      `);
      printWindow.document.close();
      printWindow.print();
    }
  }

  onDuplicateTask(task: Task): void {
    // Implement duplicate logic
    this.openDialog(
      'success',
      'Task Duplicated',
      `Task "${task.title}" has been duplicated!`,
      'A new copy of the task has been created.',
    );
  }

  onAssignTask(task: Task): void {
    // Implement assign logic
    this.openDialog(
      'info',
      'Assign Task',
      `Assign task "${task.title}" to another user`,
      'This feature is coming soon.',
    );
  }
}
