import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';
import { RouterModule } from '@angular/router';
import { AuthService } from '../../services/auth/auth.service';
import { ProductService } from '../../services/product/product.service';
import { CartViewModel, CartItemViewModel } from '../../interfaces/products/product.interface';

@Component({
  selector: 'app-cart',
  templateUrl: './cart.component.html',
  styleUrls: ['./cart.component.css'],
  standalone: true,
  imports: [CommonModule, FormsModule, HttpClientModule, RouterModule],
  providers: [ProductService, AuthService],
})
export class CartComponent implements OnInit {
  cart: CartViewModel | null = null;
  errorMessage: string | null = null;
  isLoading: boolean = false;

  constructor(
    private productService: ProductService,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    this.loadCart();
  }

  loadCart(): void {
    if (!this.authService.isAuthenticated()) {
      this.errorMessage = 'Please log in to view your cart.';
      this.cart = null;
      return;
    }
    this.isLoading = true;
    this.productService.getCart().subscribe({
      next: (cart) => {
        this.cart = cart;
        this.isLoading = false;
      },
      error: (error) => {
        this.errorMessage = error.error || 'Failed to load cart. Please try again.';
        console.error('Error loading cart:', error);
        this.isLoading = false;
      }
    });
  }

  updateCartItemCount(cartItem: CartItemViewModel, count: number): void {
    if (!this.authService.isAuthenticated()) {
      alert('Please log in to update your cart.');
      return;
    }
    if (count < 1 || count > 1000) {
      this.errorMessage = 'Quantity must be between 1 and 1000.';
      return;
    }
    this.productService.updateCartItemCount(cartItem.id, count).subscribe({
      next: (cart) => {
        this.cart = cart;
      },
      error: (error) => {
        this.errorMessage = error.error || 'Failed to update cart. Please try again.';
        console.error('Error updating cart:', error);
      }
    });
  }

  removeFromCart(cartItemId: number): void {
    if (!this.authService.isAuthenticated()) {
      alert('Please log in to update your cart.');
      return;
    }
    this.productService.removeFromCart(cartItemId).subscribe({
      next: (cart) => {
        this.cart = cart;
      },
      error: (error) => {
        this.errorMessage = error.error || 'Failed to remove from cart. Please try again.';
        console.error('Error removing from cart:', error);
      }
    });
  }

  getCartItemCount(): number {
    return this.cart?.items.reduce((total, item) => total + item.count, 0) || 0;
  }

  getCartTotal(): number {
    return this.cart?.totalPrice || 0;
  }
}