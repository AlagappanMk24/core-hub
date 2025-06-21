export interface Product {
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