// src/app/interfaces/task.interface.ts
export enum TaskPriority {
  Low = 0,
  Medium = 1,
  High = 2,
  Urgent = 3,
}

export enum TaskStatus {
  Pending = 0,
  InProgress = 1,
  Completed = 2,
  Cancelled = 3,
  OnHold = 4,
  Overdue = 5,
}

export interface TaskComment {
  id: number;
  taskId: number;
  comment: string;
  userId: number;
  userName: string;
  createdAt: Date;
}

export interface TaskAttachment {
  id: number;
  taskId: number;
  fileName: string;
  fileUrl: string;
  fileSize: number;
  contentType: string;
  createdAt: Date;
}

export interface Task {
  id: number;
  title: string;
  description?: string;
  priority: TaskPriority;
  priorityName: string;
  status: TaskStatus;
  statusName: string;
  dueDate?: Date;
  completedAt?: Date;
  category?: string;
  tag?: string;
  assignedToUserId?: string;
  assignedToUserName?: string;
  createdByUserId?: number;
  createdByUserName?: string;
  parentTaskId?: number;
  parentTaskTitle?: string;
  subtaskCount: number;
  reminderDate?: Date;
  isRecurring: boolean;
  recurrencePattern?: string;
  estimatedHours?: number;
  actualHours?: number;
  commentCount: number;
  attachmentCount: number;
  createdAt: Date;
  updatedAt?: Date;
  comments?: TaskComment[];
  attachments?: TaskAttachment[];
  subtasks?: Task[];
}

export interface CreateTaskDto {
  title: string;
  description?: string;
  priority: TaskPriority;
  dueDate?: Date | string;
  category?: string;
  tag?: string;
  assignedToUserId?: string;
  reminderDate?: Date | string;
  isRecurring: boolean;
  recurrencePattern?: string;
  estimatedHours?: number;
  parentTaskId?: number;
}

export interface UpdateTaskDto {
  title?: string;
  description?: string;
  priority?: TaskPriority;
  status?: TaskStatus;
  dueDate?: Date | string;
  category?: string;
  tag?: string;
  assignedToUserId?: string;
  reminderDate?: Date | string;
  isRecurring?: boolean;
  recurrencePattern?: string;
  estimatedHours?: number;
  actualHours?: number;
  parentTaskId?: number;
}

export interface TaskFilterDto {
   // Filters
  status?: TaskStatus;
  priority?: TaskPriority;
  assignedToUserId?: string;
  category?: string;
  tag?: string;
  dueDateFrom?: Date | string;
  dueDateTo?: Date | string;
  overdue?: boolean;
  myTasks?: boolean;
  searchTerm?: string;
  // Pagination
  page: number;
  pageSize: number;
  
  // Sorting
  sortBy?: string;
  sortDescending?: boolean;
}

export interface TaskStats {
  totalTasks: number;
  myTasks: number;
  tasksAssignedToMe: number;
  tasksCreatedByMe: number;
  pendingTasks: number;
  inProgressTasks: number;
  completedTasks: number;
  cancelledTasks: number;
  onHoldTasks: number;
  overdueTasks: number;
  highPriorityTasks: number;
  urgentPriorityTasks: number;
  tasksDueToday: number;
  tasksDueThisWeek: number;
  tasksDueThisMonth: number;
  tasksByCategory: { [key: string]: number };
  tasksByStatus: { [key: string]: number };
  tasksByPriority: { [key: string]: number };
}

export interface CreateTaskCommentDto {
  comment: string;
}

export interface PaginatedResult<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}