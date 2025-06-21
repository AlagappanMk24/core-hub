import { Injectable } from '@angular/core';
import { Observable, of, throwError } from 'rxjs';
import { Product } from '../../interfaces/products/product.interface';

@Injectable({
  providedIn: 'root',
})
export class ProductService {
  private products: Product[] = [
    // Same products array as in ProductComponent
    {
      id: 1,
      name: 'Nike Air Force',
      category: 'Footwear',
      totalOrder: 2050,
      delivered: 2000,
      pending: 50,
      status: 'Out of Stock',
      price: '$250',
      rating: 4.8,
      image: 'data:image/svg+xml;base64,PHN2ZyB3aWR0aD0iNDAiIGhlaWdodD0iNDAiIHZpZXdCb3g9IjAgMCA0MCA0MCIgZmlsbD0ibm9uZSIgeG1sbnM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDAvc3ZnIj4KPHJlY3Qgd2lkdGg9IjQwIiBoZWlnaHQ9IjQwIiByeD0iOCIgZmlsbD0iI0Y1RjVGNSIvPgo8cGF0aCBkPSJNMTAgMjhDMTAgMjggMTUgMjUgMjAgMjVDMjUgMjUgMzAgMjggMzAgMjhMMzAgMjBLMTAgMjBLMTAgMjhaIiBmaWxsPSIjMzMzIi8+CjxwYXRoIGQ9Ik0xMiAyMkMxMiAyMiAxNyAxOSAyMiAxOUMyNyAxOSAzMCAyMiAzMCAyMkwzMCAxNEwxMiAxNEwxMiAyMloiIGZpbGw9IiM2NjYiLz4KPC9zdmc+',
      highlighted: true,
      detailImage: 'data:image/svg+xml;base64,PHN2ZyB3aWR0aD0iMjgwIiBoZWlnaHQ9IjE4MCIgdmlld0JveD0iMCAwIDI4MCAxODAiIGZpbGw9Im5vbmUiIHhtbG5zPSJodHRwOi8vd3d3LnczLm9yZy8yMDAwL3N2ZyI+CjxyZWN0IHdpZHRoPSIyODAiIGhlaWdodD0iMTgwIiBmaWxsPSJ1cmwoI2dyYWRpZW50MCkiLz4KPHN2ZyB4PSI5MCIgeT0iNjAiIHdpZHRoPSIxMDAiIGhlaWdodD0iNjAiIHZpZXdCb3g9IjAgMCAxMDAgNjAiIGZpbGw9Im5vbmUiPgo8cGF0aCBkPSJNMTAgNTBDMTAgNTAgMjUgNDAgNTAgNDBDNzUgNDAgOTAgNTAgOTAgNTBMOTAgMjBMMTAgMjBMMTAgNTBaIiBmaWxsPSJhbHBoYSh3aGl0ZSwgMC44KSIvPgo8cGF0aCBkPSJNMTUgMzVDMTUgMzUgMzAgMjUgNTUgMjVDODAgMjUgODUgMzUgODUgMzVMODUgMTBMMTUgMTBMMTUgMzVaIiBmaWxsPSJhbHBoYSh3aGl0ZSwgMC42KSIvPgo8L3N2Zz4KPGRlZnM+CjxsaW5lYXJHcmFkaWVudCBpZD0iZ3JhZGllbnQwIiB4MT0iMCIgeTE9IjAiIHgyPSIyODAiIHkyPSIxODAiIGdyYWRpZW50VW5pdHM9InVzZXJTcGFjZU9uVXNlIj4KPHN0b3Agc3RvcC1jb2xvcj0iI0UzRjJGRCIvPgo8c3RvcCBvZmZzZXQ9IjEiIHN0b3AtY29sb3I9IiNCQkRFRkIiLz4KPC9saW5lYXJHcmFkaWVudD4KPC9kZWZzPgo8L3N2Zz4K',
      description: 'Premium sneakers with superior comfort and style.',
      location: 'AMA Footwear House, New Delhi',
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

  getProducts(): Observable<Product[]> {
    return of(this.products);
  }

  getProduct(id: number): Observable<Product> {
    const product = this.products.find(p => p.id === id);
    return product ? of(product) : throwError(() => new Error('Product not found'));
  }

  updateProduct(product: Product): Observable<Product> {
    const index = this.products.findIndex(p => p.id === product.id);
    if (index !== -1) {
      this.products[index] = { ...product };
      return of(this.products[index]);
    }
    return throwError(() => new Error('Product not found'));
  }

  deleteProduct(id: number): Observable<void> {
    const index = this.products.findIndex(p => p.id === id);
    if (index !== -1) {
      this.products.splice(index, 1);
      return of(void 0);
    }
    return throwError(() => new Error('Product not found'));
  }
}