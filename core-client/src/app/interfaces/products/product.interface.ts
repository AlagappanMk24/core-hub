export interface Product {
  id: number;
  name: string;
  description?: string;
  sku?: string;
  unitPrice: number;
  taxTypeId?: number;
  taxType?: string;
  taxRate?: number;
  isActive: boolean;
  category?: string;
  categoryId?: number;
}

export interface ProductFilter {
  pageNumber: number;
  pageSize: number;
  search?: string;
  category?: string;
  isActive?: boolean;
}

export interface ProductCategory {
  id: number;
  name: string;
  description?: string;
}
