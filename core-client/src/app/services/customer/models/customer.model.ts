export interface PaginatedResult<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}

export interface Customer {
  id: number;
  name: string;
  email: string;
  phoneNumber: string;
  address: {
    address1: string;
    address2: string;
    city: string;
    state: string;
    country: string;
    zipCode: string;
  };
}

export interface CustomerCreateDto {
  name: string;
  email: string;
  phoneNumber: string;
  address1: string;
  address2?: string;
  city: string;
  state?: string;
  country: string;
  zipCode: string;
}

export interface CustomerUpdateDto {
  id: number;
  name: string;
  email: string;
  phoneNumber: string;
  address1: string;
  address2?: string;
  city: string;
  state?: string;
  country: string;
  zipCode: string;
}

export interface CustomerResponse {
  id: number;
  name: string;
  email: string;
  phoneNumber: string;
  address: {
    address1: string;
    address2?: string;
    city: string;
    state?: string;
    country: string;
    zipCode: string;
  };
}

export interface PaginatedResponse {
  items: CustomerResponse[];
  total: number;
  page: number;
  pageSize: number;
}

export interface CustomerStats {
  allCount: number;
  allChange: number;
  activeCount: number;
  activeChange: number;
  inactiveCount: number;
  inactiveChange: number;
}

export interface CustomerFilterRequest {
  pageNumber: number;
  pageSize: number;
  search?: string;
  status?: string | null;
}