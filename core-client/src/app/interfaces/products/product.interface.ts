export interface ProductResponse {
  id: number;
  title: string;
  categoryName: string;
  price: number;
  discountPrice: number;
  isDiscounted: boolean;
  isNewArrival: boolean;
  isTrending: boolean;
  imageUrl: string | null;
}

export interface Product {
  id: number;
  name: string;
  category: string;
  currentPrice: number;
  originalPrice?: number;
  image: string;
  badge?: {
    text: string;
    type: string;
  };
  isWishlisted: boolean;
  isCompared: boolean;
}

export interface CartViewModel {
  items: CartItemViewModel[];
  totalPrice: number;
}

export interface CartItemViewModel {
  id: number;
  productId: number;
  productName: string;
  price: number;
  discountPrice: number | null;
  count: number;
  imageUrl: string | null;
}

export interface AddToCartRequest {
  productId: number;
  count: number;
}