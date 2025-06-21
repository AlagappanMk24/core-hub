// import { CommonModule } from '@angular/common';
// import { Component, OnInit } from '@angular/core';
// import { FormsModule } from '@angular/forms';

// interface Product {
//   id: number;
//   name: string;
//   sku: string;
//   price: number;
//   productsCount: number;
//   views: number;
//   status: 'Active' | 'Inactive';
//   image: string;
//   category: string;
// }

// @Component({
//   selector: 'app-product',
//   templateUrl: './product.component.html',
//   styleUrls: ['./product.component.css'],
//   imports : [FormsModule, CommonModule]
// })
// export class ProductComponent implements OnInit {
  
//   // Filter properties
//   searchTerm: string = '';
//   selectedShow: string = 'all';
//   selectedSort: string = 'default';
//   selectedCategory: string = '';
//   selectedStatus: string = '';
//   selectedPrice: string = '';
//   selectedStore: string = '';
  
//   // Pagination properties
//   currentPage: number = 3;
//   totalPages: number = 24;
  
//   // Product data
//   products: Product[] = [
//     {
//       id: 1,
//       name: 'Gabriela Cashmere Blazer',
//       sku: 'T14118',
//       price: 113.99,
//       productsCount: 1113,
//       views: 14012,
//       status: 'Active',
//       image: 'data:image/svg+xml,%3Csvg xmlns="http://www.w3.org/2000/svg" width="48" height="48" viewBox="0 0 48 48"%3E%3Crect width="48" height="48" fill="%23D4A574"/%3E%3C/svg%3E',
//       category: 'blazer'
//     },
//     {
//       id: 2,
//       name: 'Loewe blend Jacket - Blue',
//       sku: 'T14118',
//       price: 113.99,
//       productsCount: 721,
//       views: 13212,
//       status: 'Active',
//       image: 'data:image/svg+xml,%3Csvg xmlns="http://www.w3.org/2000/svg" width="48" height="48" viewBox="0 0 48 48"%3E%3Crect width="48" height="48" fill="%234A90E2"/%3E%3C/svg%3E',
//       category: 'jacket'
//     },
//     {
//       id: 3,
//       name: 'Sandro - Jacket - Black',
//       sku: 'T14118',
//       price: 113.99,
//       productsCount: 407,
//       views: 8201,
//       status: 'Active',
//       image: 'data:image/svg+xml,%3Csvg xmlns="http://www.w3.org/2000/svg" width="48" height="48" viewBox="0 0 48 48"%3E%3Crect width="48" height="48" fill="%23333333"/%3E%3C/svg%3E',
//       category: 'jacket'
//     },
//     {
//       id: 4,
//       name: 'Adidas By Stella McCartney',
//       sku: 'T14118',
//       price: 113.99,
//       productsCount: 1203,
//       views: 1002,
//       status: 'Active',
//       image: 'data:image/svg+xml,%3Csvg xmlns="http://www.w3.org/2000/svg" width="48" height="48" viewBox="0 0 48 48"%3E%3Crect width="48" height="48" fill="%23E74C3C"/%3E%3C/svg%3E',
//       category: 'jacket'
//     },
//     {
//       id: 5,
//       name: 'Meteo Hooded Wool Jacket',
//       sku: 'T14118',
//       price: 113.99,
//       productsCount: 306,
//       views: 807,
//       status: 'Active',
//       image: 'data:image/svg+xml,%3Csvg xmlns="http://www.w3.org/2000/svg" width="48" height="48" viewBox="0 0 48 48"%3E%3Crect width="48" height="48" fill="%23B8860B"/%3E%3C/svg%3E',
//       category: 'jacket'
//     },
//     {
//       id: 6,
//       name: 'Hida Down Ski Jacket - Red',
//       sku: 'T14118',
//       price: 113.99,
//       productsCount: 201,
//       views: 406,
//       status: 'Active',
//       image: 'data:image/svg+xml,%3Csvg xmlns="http://www.w3.org/2000/svg" width="48" height="48" viewBox="0 0 48 48"%3E%3Crect width="48" height="48" fill="%23DC143C"/%3E%3C/svg%3E',
//       category: 'jacket'
//     },
//     {
//       id: 7,
//       name: 'Dolce & Gabbana',
//       sku: 'T14118',
//       price: 113.99,
//       productsCount: 108,
//       views: 204,
//       status: 'Active',
//       image: 'data:image/svg+xml,%3Csvg xmlns="http://www.w3.org/2000/svg" width="48" height="48" viewBox="0 0 48 48"%3E%3Crect width="48" height="48" fill="%23708090"/%3E%3C/svg%3E',
//       category: 'jacket'
//     }
//   ];
// tickets: any;

//   constructor() { }

//   ngOnInit(): void {
//     // Initialize component
//     this.loadProducts();
//   }

//   // Initialize/Load products
//   loadProducts(): void {
//     // This method can be used to load products from a service
//     console.log('Loading products...');
//   }

//   // Search and filter methods
//   onSearch(): void {
//     // Implement search functionality
//     console.log('Searching for:', this.searchTerm);
//     this.applyFilters();
//   }

//   onShowChange(): void {
//     // Implement show filter
//     console.log('Show changed to:', this.selectedShow);
//     this.applyFilters();
//   }

//   onSortChange(): void {
//     // Implement sort functionality
//     console.log('Sort changed to:', this.selectedSort);
//     this.applySorting();
//   }

//   onCategoryChange(): void {
//     // Implement category filter
//     console.log('Category changed to:', this.selectedCategory);
//     this.applyFilters();
//   }

//   onStatusChange(): void {
//     // Implement status filter
//     console.log('Status changed to:', this.selectedStatus);
//     this.applyFilters();
//   }

//   onPriceChange(): void {
//     // Implement price filter
//     console.log('Price changed to:', this.selectedPrice);
//     this.applyFilters();
//   }

//   onStoreChange(): void {
//     // Implement store filter
//     console.log('Store changed to:', this.selectedStore);
//     this.applyFilters();
//   }

//   // Product actions
//   editProduct(product: Product): void {
//     console.log('Editing product:', product);
//     // Implement edit functionality
//     // You can open a modal or navigate to edit page
//   }

//   deleteProduct(product: Product): void {
//     console.log('Deleting product:', product);
//     // Implement delete functionality
//     if (confirm(`Are you sure you want to delete ${product.name}?`)) {
//       this.products = this.products.filter(p => p.id !== product.id);
//     }
//   }

//   addProduct(): void {
//     console.log('Adding new product');
//     // Implement add product functionality
//     // You can open a modal or navigate to add product page
//   }

//   // Pagination methods
//   goToPage(page: number): void {
//     if (page >= 1 && page <= this.totalPages) {
//       this.currentPage = page;
//       console.log('Current page:', this.currentPage);
//       // Implement page navigation logic
//       this.loadProductsForPage(page);
//     }
//   }

//   previousPage(): void {
//     if (this.currentPage > 1) {
//       this.currentPage--;
//       console.log('Previous page:', this.currentPage);
//       this.loadProductsForPage(this.currentPage);
//     }
//   }

//   nextPage(): void {
//     if (this.currentPage < this.totalPages) {
//       this.currentPage++;
//       console.log('Next page:', this.currentPage);
//       this.loadProductsForPage(this.currentPage);
//     }
//   }

//   loadProductsForPage(page: number): void {
//     // Implement logic to load products for specific page
//     console.log('Loading products for page:', page);
//   }

//   // View toggle methods
//   toggleListView(): void {
//     // Implement list view toggle
//     console.log('Switched to list view');
//     // Add logic to change view mode
//   }

//   toggleGridView(): void {
//     // Implement grid view toggle
//     console.log('Switched to grid view');
//     // Add logic to change view mode
//   }

//   // Filter and sort methods
//   applyFilters(): void {
//     // Implement comprehensive filtering logic
//     let filteredProducts = [...this.products];

//     // Search filter
//     if (this.searchTerm) {
//       filteredProducts = filteredProducts.filter(product =>
//         product.name.toLowerCase().includes(this.searchTerm.toLowerCase()) ||
//         product.sku.toLowerCase().includes(this.searchTerm.toLowerCase())
//       );
//     }

//     // Category filter
//     if (this.selectedCategory) {
//       filteredProducts = filteredProducts.filter(product =>
//         product.category === this.selectedCategory
//       );
//     }

//     // Status filter
//     if (this.selectedStatus) {
//       filteredProducts = filteredProducts.filter(product =>
//         product.status.toLowerCase() === this.selectedStatus.toLowerCase()
//       );
//     }

//     // Show filter
//     if (this.selectedShow !== 'all') {
//       if (this.selectedShow === 'active') {
//         filteredProducts = filteredProducts.filter(product => product.status === 'Active');
//       } else if (this.selectedShow === 'inactive') {
//         filteredProducts = filteredProducts.filter(product => product.status === 'Inactive');
//       }
//     }

//     // Price filter
//     if (this.selectedPrice) {
//       const [minPrice, maxPrice] = this.selectedPrice.split('-').map(p => parseFloat(p));
//       filteredProducts = filteredProducts.filter(product =>
//         product.price >= minPrice && product.price <= maxPrice
//       );
//     }

//     // Update products array with filtered results
//     // In a real application, you might want to keep original data separate
//     console.log('Filtered products:', filteredProducts);
    
//     // Apply sorting after filtering
//     this.applySorting(filteredProducts);
//   }

//   applySorting(productsToSort?: Product[]): void {
//     const products = productsToSort || this.products;
    
//     // Apply sorting
//     if (this.selectedSort === 'name') {
//       products.sort((a, b) => a.name.localeCompare(b.name));
//     } else if (this.selectedSort === 'price') {
//       products.sort((a, b) => a.price - b.price);
//     } else if (this.selectedSort === 'views') {
//       products.sort((a, b) => b.views - a.views);
//     } else if (this.selectedSort === 'default') {
//       // Sort by ID for default ordering
//       products.sort((a, b) => a.id - b.id);
//     }

//     console.log('Sorted products:', products);
//   }

//   resetFilters(): void {
//     this.searchTerm = '';
//     this.selectedShow = 'all';
//     this.selectedSort = 'default';
//     this.selectedCategory = '';
//     this.selectedStatus = '';
//     this.selectedPrice = '';
//     this.selectedStore = '';
//     console.log('Filters reset');
//     this.applyFilters();
//   }

//   // Utility methods
//   formatNumber(num: number): string {
//     return num.toLocaleString();
//   }

//   getStatusClass(status: string): string {
//     return status.toLowerCase();
//   }

//   // Table sorting methods
//   sortByColumn(column: string): void {
//     console.log('Sorting by column:', column);
    
//     switch (column) {
//       case 'name':
//         this.selectedSort = 'name';
//         break;
//       case 'price':
//         this.selectedSort = 'price';
//         break;
//       case 'views':
//         this.selectedSort = 'views';
//         break;
//       case 'products':
//         this.products.sort((a, b) => b.productsCount - a.productsCount);
//         break;
//       case 'status':
//         this.products.sort((a, b) => a.status.localeCompare(b.status));
//         break;
//       default:
//         this.selectedSort = 'default';
//     }
    
//     this.applySorting();
//   }

//   // Bulk operations
//   selectAllProducts(): void {
//     // Implement select all functionality
//     console.log('Select all products');
//   }

//   bulkDelete(): void {
//     // Implement bulk delete functionality
//     console.log('Bulk delete selected products');
//   }

//   bulkStatusChange(status: 'Active' | 'Inactive'): void {
//     // Implement bulk status change
//     console.log('Bulk status change to:', status);
//   }

//   // Export functionality
//   exportProducts(): void {
//     // Implement export functionality
//     console.log('Exporting products...');
//   }

//   // Refresh data
//   refreshData(): void {
//     console.log('Refreshing product data...');
//     this.loadProducts();
//   }
// }

import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';

interface Product {
  id: number;
  name: string;
  category: string;
  totalOrder: number;
  delivered: number;
  pending: number;
  status: string;
  price: string;
  rating: number;
  image: string;
  highlighted?: boolean;
  detailImage?: string;
  description?: string;
  location?: string;
}

@Component({
  selector: 'app-product',
  templateUrl: './product.component.html',
  styleUrls: ['./product.component.css'],
  imports : [FormsModule, CommonModule]
})
export class ProductComponent implements OnInit {
  
  products: Product[] = [
    {
      id: 1,
      name: 'Nike Air Force',
      category: 'Footwear',
      totalOrder: 2050,
      delivered: 2000,
      pending: 50,
      status: 'Out of Stock',
      price: '$ 250',
      rating: 4.8,
      image: 'data:image/svg+xml;base64,PHN2ZyB3aWR0aD0iNDAiIGhlaWdodD0iNDAiIHZpZXdCb3g9IjAgMCA0MCA0MCIgZmlsbD0ibm9uZSIgeG1sbnM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDAvc3ZnIj4KPHJlY3Qgd2lkdGg9IjQwIiBoZWlnaHQ9IjQwIiByeD0iOCIgZmlsbD0iI0Y1RjVGNSIvPgo8cGF0aCBkPSJNMTAgMjhDMTAgMjggMTUgMjUgMjAgMjVDMjUgMjUgMzAgMjggMzAgMjhMMzAgMjBMMTAgMjBMMTAgMjhaIiBmaWxsPSIjMzMzIi8+CjxwYXRoIGQ9Ik0xMiAyMkMxMiAyMiAxNyAxOSAyMiAxOUMyNyAxOSAzMCAyMiAzMCAyMkwzMCAxNEwxMiAxNEwxMiAyMloiIGZpbGw9IiM2NjYiLz4KPC9zdmc+',
      highlighted: true,
      detailImage: 'data:image/svg+xml;base64,PHN2ZyB3aWR0aD0iMjgwIiBoZWlnaHQ9IjE4MCIgdmlld0JveD0iMCAwIDI4MCAxODAiIGZpbGw9Im5vbmUiIHhtbG5zPSJodHRwOi8vd3d3LnczLm9yZy8yMDAwL3N2ZyI+CjxyZWN0IHdpZHRoPSIyODAiIGhlaWdodD0iMTgwIiBmaWxsPSJ1cmwoI2dyYWRpZW50MCkiLz4KPHN2ZyB4PSI5MCIgeT0iNjAiIHdpZHRoPSIxMDAiIGhlaWdodD0iNjAiIHZpZXdCb3g9IjAgMCAxMDAgNjAiIGZpbGw9Im5vbmUiPgo8cGF0aCBkPSJNMTAgNTBDMTAgNTAgMjUgNDAgNTAgNDBDNzUgNDAgOTAgNTAgOTAgNTBMOTAgMjBMMTAgMjBMMTAgNTBaIiBmaWxsPSJhbHBoYSh3aGl0ZSwgMC44KSIvPgo8cGF0aCBkPSJNMTUgMzVDMTUgMzUgMzAgMjUgNTUgMjVDODAgMjUgODUgMzUgODUgMzVMODUgMTBMMTUgMTBMMTUgMzVaIiBmaWxsPSJhbHBoYSh3aGl0ZSwgMC42KSIvPgo8L3N2Zz4KPGRlZnM+CjxsaW5lYXJHcmFkaWVudCBpZD0iZ3JhZGllbnQwIiB4MT0iMCIgeTE9IjAiIHgyPSIyODAiIHkyPSIxODAiIGdyYWRpZW50VW5pdHM9InVzZXJTcGFjZU9uVXNlIj4KPHN0b3Agc3RvcC1jb2xvcj0iI0UzRjJGRCIvPgo8c3RvcCBvZmZzZXQ9IjEiIHN0b3AtY29sb3I9IiNCQkRFRkIiLz4KPC9saW5lYXJHcmFkaWVudD4KPC9kZWZzPgo8L3N2Zz4K',
      description: 'Lorem ipsum dolor sit amet, consectetur adipiscing elit. Etiam et turpis molestie, dictum est a, mattis tellus.',
      location: 'AMA Footwear House, New Delhi'
    },
    {
      id: 2,
      name: 'Nike React',
      category: 'Footwear',
      totalOrder: 1350,
      delivered: 1300,
      pending: 50,
      status: 'Avai',
      price: '$ 220',
      rating: 4.7,
      image: 'data:image/svg+xml;base64,PHN2ZyB3aWR0aD0iNDAiIGhlaWdodD0iNDAiIHZpZXdCb3g9IjAgMCA0MCA0MCIgZmlsbD0ibm9uZSIgeG1sbnM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDAvc3ZnIj4KPHJlY3Qgd2lkdGg9IjQwIiBoZWlnaHQ9IjQwIiByeD0iOCIgZmlsbD0iI0Y1RjVGNSIvPgo8cGF0aCBkPSJNMTAgMjZDMTAgMjYgMTYgMjMgMjIgMjNDMjggMjMgMzAgMjYgMzAgMjZMMzAgMThMMTAgMThMMTAgMjZaIiBmaWxsPSIjNDA0MCIvPgo8cGF0aCBkPSJNMTMgMjBDMTMgMjAgMTkgMTcgMjUgMTdDMzEgMTcgMzMgMjAgMzMgMjBMMzMgMTJMMTMgMTJMMTMgMjBaIiBmaWxsPSIjNzA3MCIvPgo8L3N2Zz4K'
    },
    {
      id: 3,
      name: 'Apple Watch',
      category: 'Watch',
      totalOrder: 1200,
      delivered: 1180,
      pending: 20,
      status: 'Avai',
      price: '$ 399',
      rating: 4.9,
      image: 'data:image/svg+xml;base64,PHN2ZyB3aWR0aD0iNDAiIGhlaWdodD0iNDAiIHZpZXdCb3g9IjAgMCA0MCA0MCIgZmlsbD0ibm9uZSIgeG1sbnM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDAvc3ZnIj4KPHJlY3Qgd2lkdGg9IjQwIiBoZWlnaHQ9IjQwIiByeD0iOCIgZmlsbD0iI0Y1RjVGNSIvPgo8cmVjdCB4PSIxMiIgeT0iMTAiIHdpZHRoPSIxNiIgaGVpZ2h0PSIyMCIgcng9IjQiIGZpbGw9IiMzMzMiLz4KPHJlY3QgeD0iMTQiIHk9IjEyIiB3aWR0aD0iMTIiIGhlaWdodD0iMTYiIHJ4PSIyIiBmaWxsPSIjNjY2Ii8+CjxyZWN0IHg9IjE4IiB5PSI4IiB3aWR0aD0iNCIgaGVpZ2h0PSI0IiByeD0iMiIgZmlsbD0iIzMzMyIvPgo8L3N2Zz4K'
    },
    {
      id: 4,
      name: 'Puma RS-X',
      category: 'Footwear',
      totalOrder: 1000,
      delivered: 990,
      pending: 10,
      status: 'Out',
      price: '$ 180',
      rating: 4.5,
      image: 'data:image/svg+xml;base64,PHN2ZyB3aWR0aD0iNDAiIGhlaWdodD0iNDAiIHZpZXdCb3g9IjAgMCA0MCA0MCIgZmlsbD0ibm9uZSIgeG1sbnM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDAvc3ZnIj4KPHJlY3Qgd2lkdGg9IjQwIiBoZWlnaHQ9IjQwIiByeD0iOCIgZmlsbD0iI0Y1RjVGNSIvPgo8cGF0aCBkPSJNOCAyOEM4IDI4IDE0IDI1IDE4IDI1QzIyIDI1IDI4IDI4IDI4IDI4TDI4IDIwTDggMjBMOCAyOFoiIGZpbGw9IiNEMzMiLz4KPHN0cmlwZSB4PSI4IiB5PSIyMCIgd2lkdGg9IjIwIiBoZWlnaHQ9IjgiIGZpbGw9IiNGRkYiLz4KPC9zdmc+'
    },
    {
      id: 5,
      name: 'Kindle',
      category: 'Electronics',
      totalOrder: 800,
      delivered: 775,
      pending: 25,
      status: 'Avai',
      price: '$ 129',
      rating: 4.6,
      image: 'data:image/svg+xml;base64,PHN2ZyB3aWR0aD0iNDAiIGhlaWdodD0iNDAiIHZpZXdCb3g9IjAgMCA0MCA0MCIgZmlsbD0ibm9uZSIgeG1sbnM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDAvc3ZnIj4KPHJlY3Qgd2lkdGg9IjQwIiBoZWlnaHQ9IjQwIiByeD0iOCIgZmlsbD0iI0Y1RjVGNSIvPgo8cmVjdCB4PSIxMCIgeT0iOCIgd2lkdGg9IjIwIiBoZWlnaHQ9IjI0IiByeD0iMiIgZmlsbD0iIzMzMyIvPgo8cmVjdCB4PSIxMiIgeT0iMTAiIHdpZHRoPSIxNiIgaGVpZ2h0PSIyMCIgZmlsbD0iI0ZGRiIvPgo8L3N2Zz4K'
    },
    {
      id: 6,
      name: 'Nike Air Max 1',
      category: 'Footwear',
      totalOrder: 650,
      delivered: 640,
      pending: 10,
      status: 'Avai',
      price: '$ 160',
      rating: 4.4,
      image: 'data:image/svg+xml;base64,PHN2ZyB3aWR0aD0iNDAiIGhlaWdodD0iNDAiIHZpZXdCb3g9IjAgMCA0MCA0MCIgZmlsbD0ibm9uZSIgeG1sbnM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDAvc3ZnIj4KPHJlY3Qgd2lkdGg9IjQwIiBoZWlnaHQ9IjQwIiByeD0iOCIgZmlsbD0iI0Y1RjVGNSIvPgo8cGF0aCBkPSJNOSAyN0M5IDI3IDE1IDI0IDIwIDI0QzI1IDI0IDMxIDI3IDMxIDI3TDMxIDE5TDkgMTlMOSAyN1oiIGZpbGw9IiM0NDQiLz4KPHN0cmlwZSB4PSI5IiB5PSIxOSIgd2lkdGg9IjIyIiBoZWlnaHQ9IjgiIGZpbGw9IiNBQUEiLz4KPC9zdmc+'
    },
    {
      id: 7,
      name: 'AirPods Pro',
      category: 'Electronics',
      totalOrder: 580,
      delivered: 570,
      pending: 10,
      status: 'Available',
      price: '$ 199',
      rating: 4.2,
      image: 'data:image/svg+xml;base64,PHN2ZyB3aWR0aD0iNDAiIGhlaWdodD0iNDAiIHZpZXdCb3g9IjAgMCA0MCA0MCIgZmlsbD0ibm9uZSIgeG1sbnM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDAvc3ZnIj4KPHJlY3Qgd2lkdGg9IjQwIiBoZWlnaHQ9IjQwIiByeD0iOCIgZmlsbD0iI0Y1RjVGNSIvPgo8cmVjdCB4PSIxNCIgeT0iMTAiIHdpZHRoPSIxMiIgaGVpZ2h0PSI4IiByeD0iNCIgZmlsbD0iI0ZGRiIvPgo8Y2lyY2xlIGN4PSIxNyIgY3k9IjI0IiByPSIzIiBmaWxsPSIjRkZGIi8+CjxjaXJjbGUgY3g9IjIzIiBjeT0iMjQiIHI9IjMiIGZpbGw9IiNGRkYiLz4KPC9zdmc+'
    },
    {
      id: 8,
      name: 'Alexa',
      category: 'Electronics',
      totalOrder: 520,
      delivered: 480,
      pending: 40,
      status: 'Out of Stock',
      price: '$ 199',
      rating: 4.2,
      image: 'data:image/svg+xml;base64,PHN2ZyB3aWR0aD0iNDAiIGhlaWdodD0iNDAiIHZpZXdCb3g9IjAgMCA0MCA0MCIgZmlsbD0ibm9uZSIgeG1sbnM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDAvc3ZnIj4KPHJlY3Qgd2lkdGg9IjQwIiBoZWlnaHQ9IjQwIiByeD0iOCIgZmlsbD0iI0Y1RjVGNSIvPgo8Y2lyY2xlIGN4PSIyMCIgY3k9IjIwIiByPSIxMiIgZmlsbD0iIzMzMyIvPgo8Y2lyY2xlIGN4PSIyMCIgY3k9IjIwIiByPSI4IiBmaWxsPSIjNjY2Ii8+CjxyZWN0IHg9IjE2IiB5PSI4IiB3aWR0aD0iOCIgaGVpZ2h0PSI0IiByeD0iMiIgZmlsbD0iIzMzMyIvPgo8L3N2Zz4K'
    },
    {
      id: 9,
      name: 'iPhone 15',
      category: 'Electronics',
      totalOrder: 500,
      delivered: 450,
      pending: 50,
      status: 'Available',
      price: '$ 799',
      rating: 4.2,
      image: 'data:image/svg+xml;base64,PHN2ZyB3aWR0aD0iNDAiIGhlaWdodD0iNDAiIHZpZXdCb3g9IjAgMCA0MCA0MCIgZmlsbD0ibm9uZSIgeG1sbnM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDAvc3ZnIj4KPHJlY3Qgd2lkdGg9IjQwIiBoZWlnaHQ9IjQwIiByeD0iOCIgZmlsbD0iI0Y1RjVGNSIvPgo8cmVjdCB4PSIxMiIgeT0iOCIgd2lkdGg9IjE2IiBoZWlnaHQ9IjI0IiByeD0iNCIgZmlsbD0iI0ZGQzEwNyIvPgo8cmVjdCB4PSIxNCIgeT0iMTAiIHdpZHRoPSIxMiIgaGVpZ2h0PSIyMCIgZmlsbD0iIzMzMyIvPgo8Y2lyY2xlIGN4PSIyMCIgY3k9IjI4IiByPSIyIiBmaWxsPSIjRkZDMTA3Ii8+CjwvZz4K'
    },
    {
      id: 10,
      name: 'Samsung Q-S24 Ultra',
      category: 'Electronics',
      totalOrder: 480,
      delivered: 460,
      pending: 20,
      status: 'Available',
      price: '$ 1500',
      rating: 4.1,
      image: 'data:image/svg+xml;base64,PHN2ZyB3aWR0aD0iNDAiIGhlaWdodD0iNDAiIHZpZXdCb3g9IjAgMCA0MCA0MCIgZmlsbD0ibm9uZSIgeG1sbnM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDAvc3ZnIj4KPHJlY3Qgd2lkdGg9IjQwIiBoZWlnaHQ9IjQwIiByeD0iOCIgZmlsbD0iI0Y1RjVGNSIvPgo8cmVjdCB4PSIxMSIgeT0iNyIgd2lkdGg9IjE4IiBoZWlnaHQ9IjI2IiByeD0iNCIgZmlsbD0iIzIyMiIvPgo8cmVjdCB4PSIxMyIgeT0iOSIgd2lkdGg9IjE0IiBoZWlnaHQ9IjIyIiBmaWxsPSIjNDQ0Ii8+CjxjaXJjbGUgY3g9IjIwIiBjeT0iMjkiIHI9IjIiIGZpbGw9IiM4ODgiLz4KPC9zdmc+'
    },
    {
      id: 11,
      name: 'Adidas Super Nova',
      category: 'Footwear',
      totalOrder: 460,
      delivered: 435,
      pending: 25,
      status: 'Available',
      price: '$ 200',
      rating: 4.1,
      image: 'data:image/svg+xml;base64,PHN2ZyB3aWR0aD0iNDAiIGhlaWdodD0iNDAiIHZpZXdCb3g9IjAgMCA0MCA0MCIgZmlsbD0ibm9uZSIgeG1sbnM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDAvc3ZnIj4KPHJlY3Qgd2lkdGg9IjQwIiBoZWlnaHQ9IjQwIiByeD0iOCIgZmlsbD0iI0Y1RjVGNSIvPgo8cGF0aCBkPSJNOCAyN0M4IDI3IDE0IDI0IDE5IDI0QzI0IDI0IDMwIDI3IDMwIDI3TDMwIDE5TDggMTlMOCAyN1oiIGZpbGw9IiMzMzMiLz4KPHBhdGggZD0iTTEwIDIxQzEwIDIxIDE2IDE4IDIxIDE4QzI2IDE4IDI4IDIxIDI4IDIxTDI4IDEzTDEwIDEzTDEwIDIxWiIgZmlsbD0iIzY2NiIvPgo8cGF0aCBkPSJNMTIgMThMMTUgMTZMMTggMThMMjEgMTZMMjQgMThMMjcgMTZMMjcgMTNMMTIgMTNMMTIgMThaIiBmaWxsPSIjRkZGIi8+Cjwvc3ZnPgo='
    }
  ];

  filteredProducts: Product[] = [];
  paginatedProducts: Product[] = [];

  categories: string[] = [];
  statuses: string[] = ['Available', 'Out of Stock', 'Out', 'Avai']; // Adjust based on actual status values
  selectedCategory: string = '';
  selectedStatus: string = '';
  searchTerm: string = '';
  sortBy: string = '';
  sortOrder: 'asc' | 'desc' = 'asc';

  itemsPerPage: number = 5;
  currentPage: number = 1;
  totalPages: number = 1;

  hoveredProduct: Product | null = null;
  showDetailCard: boolean = false;
  cardPosition = { x: 0, y: 0 };

  String = String;

  ngOnInit(): void {
    this.categories = [...new Set(this.products.map(p => p.category))];
    this.applyFilters();
  }

  onSearch(): void {
    this.currentPage = 1;
    this.applyFilters();
  }

  onFilter(): void {
    this.currentPage = 1;
    this.applyFilters();
  }

  onSort(): void {
    this.currentPage = 1;
    this.applyFilters();
  }

  toggleSortOrder(): void {
    this.sortOrder = this.sortOrder === 'asc' ? 'desc' : 'asc';
    this.applyFilters();
  }

  onPageSizeChange(): void {
    this.currentPage = 1;
    this.paginate();
  }

  applyFilters(): void {
    this.filteredProducts = this.products.filter(product =>
      (!this.selectedCategory || product.category === this.selectedCategory) &&
      (!this.selectedStatus || product.status === this.selectedStatus) &&
      (!this.searchTerm || product.name.toLowerCase().includes(this.searchTerm.toLowerCase()))
    );

    if (this.sortBy) {
      this.filteredProducts.sort((a, b) => {
        const valA = a[this.sortBy as keyof Product];
        const valB = b[this.sortBy as keyof Product];

        if (typeof valA === 'number' && typeof valB === 'number') {
          return this.sortOrder === 'asc' ? valA - valB : valB - valA;
        } else {
          return this.sortOrder === 'asc'
            ? String(valA).localeCompare(String(valB))
            : String(valB).localeCompare(String(valA));
        }
      });
    }

    this.paginate();
  }

  paginate(): void {
    this.totalPages = Math.ceil(this.filteredProducts.length / this.itemsPerPage);
    const start = (this.currentPage - 1) * this.itemsPerPage;
    const end = start + this.itemsPerPage;
    this.paginatedProducts = this.filteredProducts.slice(start, end);
  }

  goToPage(page: number): void {
    if (page >= 1 && page <= this.totalPages) {
      this.currentPage = page;
      this.paginate();
    }
  }

  getPageNumbers(): number[] {
    const pages: number[] = [];
    for (let i = 1; i <= this.totalPages; i++) {
      pages.push(i);
    }
    return pages;
  }

  onProductHover(product: Product, event: MouseEvent): void {
    this.hoveredProduct = product;
    this.showDetailCard = true;
    this.cardPosition.x = event.clientX + 20;
    this.cardPosition.y = event.clientY + 20;
  }

  onProductLeave(): void {
    this.hoveredProduct = null;
    this.showDetailCard = false;
  }
}