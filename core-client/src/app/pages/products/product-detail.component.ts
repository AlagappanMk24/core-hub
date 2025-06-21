import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { ProductService } from '../../services/product/product.service';
import { Product } from '../../interfaces/products/product.interface';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-product-detail',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './product-detail.component.html',
  styleUrls: ['./product-detail.component.css'],
})
export class ProductDetailComponent implements OnInit {
  product: Product | null = null;
  isEditing = false;
  errorMessage = '';
  successMessage = '';

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private productService: ProductService
  ) {}

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    this.productService.getProduct(id).subscribe({
      next: (product) => (this.product = product),
      error: () => {
        this.errorMessage = 'Product not found';
      },
    });
  }

  toggleEdit(): void {
    this.isEditing = !this.isEditing;
  }

  saveProduct(): void {
    if (this.product) {
      this.productService.updateProduct(this.product).subscribe({
        next: () => {
          this.successMessage = 'Product updated successfully';
          this.isEditing = false;
          setTimeout(() => (this.successMessage = ''), 3000);
        },
        error: () => {
          this.errorMessage = 'Failed to update product';
          setTimeout(() => (this.errorMessage = ''), 3000);
        },
      });
    }
  }

  deleteProduct(): void {
    if (this.product && confirm('Are you sure you want to delete this product?')) {
      this.productService.deleteProduct(this.product.id).subscribe({
        next: () => {
          this.successMessage = 'Product deleted successfully';
          setTimeout(() => this.router.navigate(['/products']), 2000);
        },
        error: () => {
          this.errorMessage = 'Failed to delete product';
          setTimeout(() => (this.errorMessage = ''), 3000);
        },
      });
    }
  }
}