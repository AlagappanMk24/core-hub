import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';
import { RouterModule } from '@angular/router';
import { AuthService } from '../../services/auth/auth.service';
import { ProductService } from '../../services/product/product.service';
import { Product, ProductResponse } from '../../interfaces/products/product.interface';

@Component({
  selector: 'app-product',
  templateUrl: './product.component.html',
  styleUrls: ['./product.component.css'],
  standalone: true,
  imports: [CommonModule, FormsModule, HttpClientModule, RouterModule],
  providers: [ProductService, AuthService],
})
export class ProductComponent implements OnInit {
  products: Product[] = [];
  wishlistedProducts: Product[] = [];
  comparedProducts: Product[] = [];
  errorMessage: string | null = null;
  isLoading: boolean = false;
  displayMode: 'all' | 'trending' = 'trending';

  constructor(
    private productService: ProductService,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    this.loadProducts();
  }

  loadProducts(mode: 'all' | 'trending' = 'trending'): void {
    this.displayMode = mode;
    this.isLoading = true;
    const fetchMethod = mode === 'all'
      ? this.productService.getAllProducts()
      : this.productService.getTrendingProducts();

    fetchMethod.subscribe({
      next: (products: ProductResponse[]) => {
        this.products = products.map(product => ({
          id: product.id,
          name: product.title,
          category: product.categoryName,
          currentPrice: product.isDiscounted && product.discountPrice > 0 ? product.discountPrice : product.price,
          originalPrice: product.isDiscounted && product.discountPrice > 0 ? product.price : undefined,
          image: product.imageUrl || 'assets/images/placeholder.jpg',
          badge: this.getBadge(product),
          isWishlisted: this.wishlistedProducts.some(p => p.id === product.id),
          isCompared: this.comparedProducts.some(p => p.id === product.id)
        }));
        this.isLoading = false;
      },
      error: (error) => {
        this.errorMessage = 'Failed to load products. Please try again later.';
        console.error('Error loading products:', error);
        this.isLoading = false;
      }
    });
  }

  private getBadge(product: ProductResponse): { text: string; type: string } | undefined {
    if (product.isDiscounted && product.discountPrice > 0) {
      const discountPercent = Math.round(((product.price - product.discountPrice) / product.price) * 100);
      return { text: `Sale ${discountPercent}%`, type: 'sale' };
    }
    if (product.isNewArrival) {
      return { text: 'New', type: 'new' };
    }
    if (product.isTrending) {
      return { text: 'Hot Deal', type: 'hot' };
    }
    return undefined;
  }

  toggleWishlist(product: Product): void {
    if (!this.authService.isAuthenticated()) {
      alert('Please log in to manage your wishlist.');
      return;
    }
    product.isWishlisted = !product.isWishlisted;
    if (product.isWishlisted) {
      if (!this.wishlistedProducts.some(p => p.id === product.id)) {
        this.wishlistedProducts.push(product);
      }
    } else {
      this.wishlistedProducts = this.wishlistedProducts.filter(p => p.id !== product.id);
    }
  }

  toggleCompare(product: Product): void {
    if (!this.authService.isAuthenticated()) {
      alert('Please log in to compare products.');
      return;
    }
    product.isCompared = !product.isCompared;
    if (product.isCompared) {
      if (!this.comparedProducts.some(p => p.id === product.id) && this.comparedProducts.length < 4) {
        this.comparedProducts.push(product);
      } else if (this.comparedProducts.length >= 4) {
        product.isCompared = false;
        alert('You can compare a maximum of 4 products at a time.');
      }
    } else {
      this.comparedProducts = this.comparedProducts.filter(p => p.id !== product.id);
    }
  }

  addToCart(product: Product): void {
    if (!this.authService.isAuthenticated()) {
      alert('Please log in to add items to your cart.');
      return;
    }
    this.productService.addToCart({ productId: product.id, count: 1 }).subscribe({
      next: () => {
        alert(`${product.name} has been added to your cart!`);
      },
      error: (error) => {
        this.errorMessage = error.error || 'Failed to add to cart. Please try again.';
        console.error('Error adding to cart:', error);
      }
    });
  }

  getWishlistedProducts(): Product[] {
    return this.wishlistedProducts;
  }

  getComparedProducts(): Product[] {
    return this.comparedProducts;
  }

  clearComparedProducts(): void {
    this.comparedProducts.forEach(product => (product.isCompared = false));
    this.comparedProducts = [];
  }

  switchDisplayMode(mode: 'all' | 'trending'): void {
    this.loadProducts(mode);
  }
}