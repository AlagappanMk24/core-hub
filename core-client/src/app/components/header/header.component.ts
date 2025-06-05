// header.component.ts
import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './header.component.html',
  styleUrls: ['./header.component.css'],
})
export class HeaderComponent {
  @Input() userName: string = 'David Greyhenak';
  @Input() userAvatar: string = 'https://images.unsplash.com/photo-1472099645785-5658abf4ff4e?w=40&h=40&fit=crop&crop=face';
  @Input() hasNotifications: boolean = true;
  
  @Output() themeChanged = new EventEmitter<string>();

  searchQuery: string = '';

  onThemeChange(theme: string): void {
    this.themeChanged.emit(theme);
  }
}