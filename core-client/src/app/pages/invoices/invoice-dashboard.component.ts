// invoice-dashboard.component.ts
import { CommonModule } from '@angular/common';
import { Component, ViewChild, ElementRef, AfterViewInit } from '@angular/core';
import { FormsModule } from '@angular/forms';

interface Invoice {
  id: string;
  clientName: string;
  clientAvatar: string;
  product: string;
  date: string;
  amount: number;
  status: string;
}

@Component({
  selector: 'app-invoice-dashboard',
  templateUrl: './invoice-dashboard.component.html',
  styleUrls: ['./invoice-dashboard.component.css'],
  imports: [CommonModule, FormsModule],
})
export class InvoiceDashboardComponent implements AfterViewInit {
  @ViewChild('chartCanvas', { static: false })
  chartCanvas!: ElementRef<HTMLCanvasElement>;

  invoices: Invoice[] = [
    {
      id: '93046',
      clientName: 'Jenny Wilson',
      clientAvatar:
        'data:image/svg+xml;base64,PHN2ZyB3aWR0aD0iMzIiIGhlaWdodD0iMzIiIHZpZXdCb3g9IjAgMCAzMiAzMiIgZmlsbD0ibm9uZSIgeG1sbnM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDAvc3ZnIj4KPGNpcmNsZSBjeD0iMTYiIGN5PSIxNiIgcj0iMTYiIGZpbGw9IiNGRkI5NTciLz4KPHN2ZyB4PSI4IiB5PSI4IiB3aWR0aD0iMTYiIGhlaWdodD0iMTYiIHZpZXdCb3g9IjAgMCAxNiAxNiIgZmlsbD0ibm9uZSI+CjxwYXRoIGQ9Ik04IDEwQzEwLjIwOTEgMTAgMTIgOC4yMDkxNCAxMiA2QzEyIDMuNzkwODYgMTAuMjA5MSAyIDggMkM1Ljc5MDg2IDIgNCAzLjc5MDg2IDQgNkM0IDguMjA5MTQgNS43OTA4NiAxMCA4IDEwWiIgZmlsbD0id2hpdGUiLz4KPHN0cm9rZSBkPSJNMSAxNC41QzEgMTEuNDY4NCAzLjQ2ODQgOSA2LjUgOUgxMS41QzE0LjUzMTYgOSAxNyAxMS40Njg0IDE3IDE0LjVWMTVIMVYxNC41WiIgZmlsbD0id2hpdGUiLz4KPC9zdmc+Cjwvc3ZnPgo=',
      product: 'UX/UI Design Services',
      date: '22 Oct, 2020',
      amount: 2000,
      status: 'Paid',
    },
    {
      id: '23340',
      clientName: 'Wade Warren',
      clientAvatar:
        'data:image/svg+xml;base64,PHN2ZyB3aWR0aD0iMzIiIGhlaWdodD0iMzIiIHZpZXdCb3g9IjAgMCAzMiAzMiIgZmlsbD0ibm9uZSIgeG1sbnM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDAvc3ZnIj4KPGNpcmNsZSBjeD0iMTYiIGN5PSIxNiIgcj0iMTYiIGZpbGw9IiNGRkI5NTciLz4KPHN2ZyB4PSI4IiB5PSI4IiB3aWR0aD0iMTYiIGhlaWdodD0iMTYiIHZpZXdCb3g9IjAgMCAxNiAxNiIgZmlsbD0ibm9uZSI+CjxwYXRoIGQ9Ik04IDEwQzEwLjIwOTEgMTAgMTIgOC4yMDkxNCAxMiA2QzEyIDMuNzkwODYgMTAuMjA5MSAyIDggMkM1Ljc5MDg2IDIgNCAzLjc5MDg2IDQgNkM0IDguMjA5MTQgNS43OTA4NiAxMCA4IDEwWiIgZmlsbD0id2hpdGUiLz4KPHN0cm9rZSBkPSJNMSAxNC41QzEgMTEuNDY4NCAzLjQ2ODQgOSA2LjUgOUgxMS41QzE0LjUzMTYgOSAxNyAxMS40Njg0IDE3IDE0LjVWMTVIMVYxNC41WiIgZmlsbD0id2hpdGUiLz4KPC9zdmc+Cjwvc3ZnPgo=',
      product: 'Net Consultancy Services',
      date: '24 May, 2020',
      amount: 14650,
      status: 'Paid',
    },
    {
      id: '50364',
      clientName: 'Robert Fox',
      clientAvatar:
        'data:image/svg+xml;base64,PHN2ZyB3aWR0aD0iMzIiIGhlaWdodD0iMzIiIHZpZXdCb3g9IjAgMCAzMiAzMiIgZmlsbD0ibm9uZSIgeG1sbnM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDAvc3ZnIj4KPGNpcmNsZSBjeD0iMTYiIGN5PSIxNiIgcj0iMTYiIGZpbGw9IiM4QjVDRjYiLz4KPHN2ZyB4PSI4IiB5PSI4IiB3aWR0aD0iMTYiIGhlaWdodD0iMTYiIHZpZXdCb3g9IjAgMCAxNiAxNiIgZmlsbD0ibm9uZSI+CjxwYXRoIGQ9Ik04IDEwQzEwLjIwOTEgMTAgMTIgOC4yMDkxNCAxMiA2QzEyIDMuNzkwODYgMTAuMjA5MSAyIDggMkM1Ljc5MDg2IDIgNCAzLjc5MDg2IDQgNkM0IDguMjA5MTQgNS43OTA4NiAxMCA4IDEwWiIgZmlsbD0id2hpdGUiLz4KPHN0cm9rZSBkPSJNMSAxNC41QzEgMTEuNDY4NCAzLjQ2ODQgOSA2LjUgOUgxMS41QzE0LjUzMTYgOSAxNyAxMS40Njg0IDE3IDE0LjVWMTVIMVYxNC41WiIgZmlsbD0id2hpdGUiLz4KPC9zdmc+Cjwvc3ZnPgo=',
      product: 'Art Direction',
      date: '1 Feb, 2020',
      amount: 12346,
      status: 'Draft',
    },
    {
      id: '70443',
      clientName: 'Jacob Jones',
      clientAvatar:
        'data:image/svg+xml;base64,PHN2ZyB3aWR0aD0iMzIiIGhlaWdodD0iMzIiIHZpZXdCb3g9IjAgMCAzMiAzMiIgZmlsbD0ibm9uZSIgeG1sbnM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDAvc3ZnIj4KPGNpcmNsZSBjeD0iMTYiIGN5PSIxNiIgcj0iMTYiIGZpbGw9IiNGRkI5NTciLz4KPHN2ZyB4PSI4IiB5PSI4IiB3aWR0aD0iMTYiIGhlaWdodD0iMTYiIHZpZXdCb3g9IjAgMCAxNiAxNiIgZmlsbD0ibm9uZSI+CjxwYXRoIGQ9Ik04IDEwQzEwLjIwOTEgMTAgMTIgOC4yMDkxNCAxMiA2QzEyIDMuNzkwODYgMTAuMjA5MSAyIDggMkM1Ljc5MDg2IDIgNCAzLjc5MDg2IDQgNkM0IDguMjA5MTQgNS43OTA4NiAxMCA4IDEwWiIgZmlsbD0id2hpdGUiLz4KPHN0cm9rZSBkPSJNMSAxNC41QzEgMTEuNDY4NCAzLjQ2ODQgOSA2LjUgOUgxMS41QzE0LjUzMTYgOSAxNyAxMS40Njg0IDE3IDE0LjVWMTVIMVYxNC41WiIgZmlsbD0id2hpdGUiLz4KPC9zdmc+Cjwvc3ZnPgo=',
      product: 'App Application Development',
      date: '21 Sep, 2020',
      amount: 18752,
      status: 'Paid',
    },
    {
      id: '93457',
      clientName: 'Esther Howard',
      clientAvatar:
        'data:image/svg+xml;base64,PHN2ZyB3aWR0aD0iMzIiIGhlaWdodD0iMzIiIHZpZXdCb3g9IjAgMCAzMiAzMiIgZmlsbD0ibm9uZSIgeG1sbnM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDAvc3ZnIj4KPGNpcmNsZSBjeD0iMTYiIGN5PSIxNiIgcj0iMTYiIGZpbGw9IiMzQjgyRjYiLz4KPHN2ZyB4PSI4IiB5PSI4IiB3aWR0aD0iMTYiIGhlaWdodD0iMTYiIHZpZXdCb3g9IjAgMCAxNiAxNiIgZmlsbD0ibm9uZSI+CjxwYXRoIGQ9Ik04IDEwQzEwLjIwOTEgMTAgMTIgOC4yMDkxNCAxMiA2QzEyIDMuNzkwODYgMTAuMjA5MSAyIDggMkM1Ljc5MDg2IDIgNCAzLjc5MDg2IDQgNkM0IDguMjA5MTQgNS43OTA4NiAxMCA4IDEwWiIgZmlsbD0id2hpdGUiLz4KPHN0cm9rZSBkPSJNMSAxNC41QzEgMTEuNDY4NCAzLjQ2ODQgOSA2LjUgOUgxMS41QzE0LjUzMTYgOSAxNyAxMS40Njg0IDE3IDE0LjVWMTVIMVYxNC41WiIgZmlsbD0id2hpdGUiLz4KPC9zdmc+Cjwvc3ZnPgo=',
      product: 'Software Testings and QA',
      date: '8 Sep, 2020',
      amount: 10652,
      status: 'Draft',
    },
  ];

  ngAfterViewInit(): void {
    this.drawChart();
  }

  private drawChart(): void {
    const canvas = this.chartCanvas.nativeElement;
    const ctx = canvas.getContext('2d');

    if (!ctx) return;

    const centerX = canvas.width / 2;
    const centerY = canvas.height / 2;
    const radius = 70;
    const innerRadius = 45;

    // Chart data
    const data = [
      { label: 'Paid', value: 42, color: '#ff6b6b' },
      { label: 'Pending', value: 32, color: '#4ecdc4' },
      { label: 'Draft', value: 16, color: '#45b7d1' },
      { label: 'OverDue', value: 10, color: '#96ceb4' },
    ];

    let currentAngle = -Math.PI / 2; // Start from top

    // Draw segments
    data.forEach((segment) => {
      const sliceAngle = (segment.value / 100) * 2 * Math.PI;

      // Draw outer arc
      ctx.beginPath();
      ctx.arc(
        centerX,
        centerY,
        radius,
        currentAngle,
        currentAngle + sliceAngle
      );
      ctx.arc(
        centerX,
        centerY,
        innerRadius,
        currentAngle + sliceAngle,
        currentAngle,
        true
      );
      ctx.closePath();
      ctx.fillStyle = segment.color;
      ctx.fill();

      currentAngle += sliceAngle;
    });

    // Draw inner circle (white)
    ctx.beginPath();
    ctx.arc(centerX, centerY, innerRadius, 0, 2 * Math.PI);
    ctx.fillStyle = '#ffffff';
    ctx.fill();
  }

  onTabClick(tab: string): void {}

  onGenerateReports(): void {}

  onExport(): void {}

  onCreateInvoice(): void {}

  onInvoiceAction(invoiceId: string): void {}
}
