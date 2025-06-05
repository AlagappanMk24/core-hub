// dashboard-stats.component.ts
import { Component, OnInit, AfterViewInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Chart, registerables, ChartConfiguration } from 'chart.js';

Chart.register(...registerables);

interface Ticket {
  assignee: string;
  avatar: string;
  subject: string;
  status: 'DONE' | 'PROGRESS' | 'ON HOLD' | 'REJECTED';
  lastUpdate: string;
  trackingId: string;
}

interface Project {
  id: number;
  name: string;
  dueDate: string;
  progress: number;
  color: string;
}

interface TodoItem {
  id: number;
  text: string;
  completed: boolean;
}

@Component({
  selector: 'app-dashboard-stats',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './dashboard-stats.component.html',
  styleUrls: ['./dashboard-stats.component.css'],
})
export class DashboardStatsComponent implements OnInit, AfterViewInit {
  tickets: Ticket[] = [
    {
      assignee: 'David Grey',
      avatar: 'https://images.unsplash.com/photo-1472099645785-5658abf4ff4e?w=40&h=40&fit=crop&crop=face',
      subject: 'Fund is not recieved',
      status: 'DONE',
      lastUpdate: 'Dec 5, 2017',
      trackingId: 'WD-12345'
    },
    {
      assignee: 'Stella Johnson',
      avatar: 'https://images.unsplash.com/photo-1494790108755-2616b612b786?w=40&h=40&fit=crop&crop=face',
      subject: 'High loading time',
      status: 'PROGRESS',
      lastUpdate: 'Dec 12, 2017',
      trackingId: 'WD-12346'
    },
    {
      assignee: 'Marina Michel',
      avatar: 'https://images.unsplash.com/photo-1438761681033-6461ffad8d80?w=40&h=40&fit=crop&crop=face',
      subject: 'Website down for one week',
      status: 'ON HOLD',
      lastUpdate: 'Dec 16, 2017',
      trackingId: 'WD-12347'
    },
    {
      assignee: 'John Doe',
      avatar: 'https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?w=40&h=40&fit=crop&crop=face',
      subject: 'Loosing control on server',
      status: 'REJECTED',
      lastUpdate: 'Dec 3, 2017',
      trackingId: 'WD-12348'
    }
  ];

  projects: Project[] = [
    { id: 1, name: 'Herman Beck', dueDate: 'May 15, 2015', progress: 25, color: '#14B8A6' },
    { id: 2, name: 'Messy Adam', dueDate: 'Jul 01, 2015', progress: 80, color: '#F97316' },
    { id: 3, name: 'John Richards', dueDate: 'Apr 12, 2015', progress: 90, color: '#EAB308' },
    { id: 4, name: 'Peter Meggik', dueDate: 'May 15, 2015', progress: 50, color: '#8B5CF6' },
    { id: 5, name: 'Edward', dueDate: 'May 03, 2015', progress: 35, color: '#F97316' },
    { id: 6, name: 'Ronald', dueDate: 'Jun 05, 2015', progress: 65, color: '#3B82F6' }
  ];

  todos: TodoItem[] = [
    { id: 1, text: 'Pick up kids from school', completed: false },
    { id: 2, text: 'Prepare for presentation', completed: true },
    { id: 3, text: 'Print Statements', completed: false },
    { id: 4, text: 'Create invoice', completed: false },
    { id: 5, text: 'Call John', completed: true },
    { id: 6, text: 'Meeting with Alisa', completed: false }
  ];

  newTodo: string = '';
  calendarDays: number[] = [];

  ngOnInit(): void {
    this.generateCalendar();
  }

  ngAfterViewInit(): void {
    setTimeout(() => {
      this.createVisitSalesChart();
      this.createTrafficChart();
    }, 100);
  }

  private generateCalendar(): void {
    // Generate calendar for June 2025
    const daysInMonth = 30;
    const firstDayOfWeek = 0; // June 1, 2025 is a Sunday
    
    // Add empty days for the first week
    for (let i = 0; i < firstDayOfWeek; i++) {
      this.calendarDays.push(0);
    }
    
    // Add days of the month
    for (let day = 1; day <= daysInMonth; day++) {
      this.calendarDays.push(day);
    }
  }

  addTodo(): void {
    if (this.newTodo.trim()) {
      const newId = Math.max(...this.todos.map(t => t.id)) + 1;
      this.todos.push({
        id: newId,
        text: this.newTodo.trim(),
        completed: false
      });
      this.newTodo = '';
    }
  }

  removeTodo(id: number): void {
    this.todos = this.todos.filter(todo => todo.id !== id);
  }

  trackByTodo(index: number, todo: TodoItem): number {
    return todo.id;
  }

  private createVisitSalesChart(): void {
    const canvas = document.getElementById('visitSalesChart') as HTMLCanvasElement;
    if (!canvas) return;
    
    const ctx = canvas.getContext('2d');
    if (!ctx) return;

    const config: ChartConfiguration<'bar'> = {
      type: 'bar',
      data: {
        labels: ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'],
        datasets: [
          {
            label: 'CHN',
            data: [65, 45, 80, 30, 95, 40, 75, 55, 85, 45, 70, 60],
            backgroundColor: '#8B5CF6',
            borderRadius: 4,
            borderSkipped: false,
          },
          {
            label: 'USA',
            data: [45, 65, 50, 70, 55, 85, 45, 75, 55, 75, 50, 80],
            backgroundColor: '#F472B6',
            borderRadius: 4,
            borderSkipped: false,
          },
          {
            label: 'RU',
            data: [25, 35, 30, 50, 35, 55, 25, 45, 35, 45, 30, 50],
            backgroundColor: '#60A5FA',
            borderRadius: 4,
            borderSkipped: false,
          }
        ]
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          legend: {
            display: false
          }
        },
        scales: {
          y: {
            beginAtZero: true,
            grid: {
              color: 'rgba(0,0,0,0.05)'
            },
            ticks: {
              color: '#6B7280'
            }
          },
          x: {
            grid: {
              display: false
            },
            ticks: {
              color: '#6B7280'
            }
          }
        }
      }
    };

    new Chart(ctx, config);
  }

  private createTrafficChart(): void {
    const canvas = document.getElementById('trafficChart') as HTMLCanvasElement;
    if (!canvas) return;
    
    const ctx = canvas.getContext('2d');
    if (!ctx) return;

    const config: ChartConfiguration<'doughnut'> = {
      type: 'doughnut',
      data: {
        labels: ['Search Engines', 'Direct Click', 'Social Media', 'Other'],
        datasets: [{
          data: [30, 24, 25, 21],
          backgroundColor: ['#60A5FA', '#14B8A6', '#F472B6', '#A78BFA'],
          borderWidth: 0,
        }]
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          legend: {
            display: false
          }
        }
      }
    };

    new Chart(ctx, config);
  }
}