import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { CompanyService } from '../../services/company/company.service';
import { AuthService } from '../../services/auth/auth.service';

interface Company {
  id: number;
  name: string;
}

@Component({
  selector: 'app-select-company',
  templateUrl: './select-company.component.html',
  styleUrls: ['./select-company.component.css'],
  imports: [CommonModule, FormsModule, RouterModule],
  standalone: true
})
export class SelectCompanyComponent implements OnInit {
  companyService = inject(CompanyService);
  authService = inject(AuthService);
  router = inject(Router);

  companies: Company[] = [];
  selectedCompanyId: number = 0;
  errorMessage: string = '';
  isLoading: boolean = false;
  loadingText: string = 'Saving selection...';

  ngOnInit(): void {
    this.loadCompanies();
  }

  loadCompanies(): void {
    this.companyService.getCompanies().subscribe({
      next: (companies) => {
        this.companies = companies;
        if (companies.length > 0) {
          this.selectedCompanyId = companies[0].id;
        }
      },
      error: (error) => {
        this.errorMessage = 'Failed to load companies. Please try again.';
        console.error('Error loading companies:', error);
      }
    });
  }

  saveCompany(): void {
    if (!this.selectedCompanyId) {
      this.errorMessage = 'Please select a company';
      return;
    }

    this.isLoading = true;
    this.authService.updateCompany(this.selectedCompanyId).subscribe({
      next: (response) => {
        if (response.token) {
          localStorage.setItem('authToken', response.token); // Update token with new companyId
        }
        this.loadingText = 'Company selected, redirecting...';
        setTimeout(() => {
          this.isLoading = false;
          this.router.navigate(['/dashboard']);
        }, 2000);
      },
      error: (error) => {
        this.isLoading = false;
        this.errorMessage = error.error?.message || 'Failed to save company selection';
        console.error('Error saving company:', error);
      }
    });
  }
}