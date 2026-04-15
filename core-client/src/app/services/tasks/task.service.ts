// src/app/services/task.service.ts
import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import {
  Task,
  CreateTaskDto,
  UpdateTaskDto,
  TaskFilterDto,
  TaskStats,
  TaskComment,
  CreateTaskCommentDto,
  TaskAttachment
} from '../../interfaces/tasks/task.interface';

@Injectable({
  providedIn: 'root'
})
export class TaskService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiBaseUrl}/task`;

  // Task CRUD
  getTasks(filter: TaskFilterDto): Observable<Task[]> {
    let params = new HttpParams()
      .set('page', filter.page.toString())
      .set('pageSize', filter.pageSize.toString())
      .set('sortDescending', filter.sortDescending.toString());

    if (filter.status !== undefined) params = params.set('status', filter.status);
    if (filter.priority !== undefined) params = params.set('priority', filter.priority);
    if (filter.assignedToUserId) params = params.set('assignedToUserId', filter.assignedToUserId);
    if (filter.category) params = params.set('category', filter.category);
    if (filter.tag) params = params.set('tag', filter.tag);
    if (filter.dueDateFrom) params = params.set('dueDateFrom', filter.dueDateFrom.toISOString());
    if (filter.dueDateTo) params = params.set('dueDateTo', filter.dueDateTo.toISOString());
    if (filter.overdue !== undefined) params = params.set('overdue', filter.overdue);
    if (filter.myTasks !== undefined) params = params.set('myTasks', filter.myTasks);
    if (filter.searchTerm) params = params.set('searchTerm', filter.searchTerm);
    if (filter.sortBy) params = params.set('sortBy', filter.sortBy);

    return this.http.get<Task[]>(this.apiUrl, { params });
  }

  getMyTasks(filter: TaskFilterDto): Observable<Task[]> {
    let params = new HttpParams()
      .set('page', filter.page.toString())
      .set('pageSize', filter.pageSize.toString())
      .set('sortDescending', filter.sortDescending.toString());

    if (filter.status !== undefined) params = params.set('status', filter.status);
    if (filter.priority !== undefined) params = params.set('priority', filter.priority);
    if (filter.category) params = params.set('category', filter.category);
    if (filter.tag) params = params.set('tag', filter.tag);
    if (filter.searchTerm) params = params.set('searchTerm', filter.searchTerm);
    if (filter.sortBy) params = params.set('sortBy', filter.sortBy);

    return this.http.get<Task[]>(`${this.apiUrl}/my-tasks`, { params });
  }

  getTaskById(id: number): Observable<Task> {
    return this.http.get<Task>(`${this.apiUrl}/${id}`);
  }

  createTask(task: CreateTaskDto): Observable<Task> {
    return this.http.post<Task>(this.apiUrl, task);
  }

  updateTask(id: number, task: UpdateTaskDto): Observable<Task> {
    return this.http.put<Task>(`${this.apiUrl}/${id}`, task);
  }

  deleteTask(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  // Task Actions
  updateTaskStatus(id: number, status: number): Observable<Task> {
    return this.http.patch<Task>(`${this.apiUrl}/${id}/status?status=${status}`, {});
  }

  completeTask(id: number): Observable<{ message: string }> {
    return this.http.patch<{ message: string }>(`${this.apiUrl}/${id}/complete`, {});
  }

  assignTask(id: number, userId: number): Observable<Task> {
    return this.http.patch<Task>(`${this.apiUrl}/${id}/assign/${userId}`, {});
  }

  // Comments
  getComments(taskId: number): Observable<TaskComment[]> {
    return this.http.get<TaskComment[]>(`${this.apiUrl}/${taskId}/comments`);
  }

  addComment(taskId: number, comment: CreateTaskCommentDto): Observable<TaskComment> {
    return this.http.post<TaskComment>(`${this.apiUrl}/${taskId}/comments`, comment);
  }

  // Attachments
  getAttachments(taskId: number): Observable<TaskAttachment[]> {
    return this.http.get<TaskAttachment[]>(`${this.apiUrl}/${taskId}/attachments`);
  }

  uploadAttachment(taskId: number, file: File): Observable<TaskAttachment> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post<TaskAttachment>(`${this.apiUrl}/${taskId}/attachments`, formData);
  }

  deleteAttachment(taskId: number, attachmentId: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${taskId}/attachments/${attachmentId}`);
  }

  downloadAttachment(taskId: number, attachmentId: number): Observable<Blob> {
    return this.http.get(`${this.apiUrl}/${taskId}/attachments/${attachmentId}/download`, {
      responseType: 'blob'
    });
  }

  // Stats and Reports
  getTaskStats(): Observable<TaskStats> {
    return this.http.get<TaskStats>(`${this.apiUrl}/stats`);
  }

  getOverdueTasks(): Observable<Task[]> {
    return this.http.get<Task[]>(`${this.apiUrl}/overdue`);
  }

  getTasksDueSoon(days: number = 3): Observable<Task[]> {
    return this.http.get<Task[]>(`${this.apiUrl}/due-soon?days=${days}`);
  }

  getTasksDueToday(): Observable<Task[]> {
    return this.http.get<Task[]>(`${this.apiUrl}/due-today`);
  }
}