export interface Company {
  id: number;
  name: string;
  taxId?: string;
  address: Address;
  email: string;
  phoneNumber?: string;
  isDeleted?: boolean;
  createdByUserId?: string;
  createdAt?: Date;
  updatedAt?: Date;
}

export interface Address {
  address1: string;
  address2?: string;
  city: string;
  state: string;
  zipCode: string;
  country?: string;
}