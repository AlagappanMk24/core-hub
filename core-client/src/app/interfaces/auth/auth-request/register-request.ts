// register-request.ts
export interface RegisterRequest {
  fullName: string;
  email: string;
  password: string;
  confirmPassword : string;
  phoneNumber: string;
  roles : string[],
  companyId: number | null;
}