export interface AuthResponse {
  token: string;
  isSucceeded: true;
  message: string;
  data?: {
    otpToken?: string;
    otpIdentifier? : string;
  };
}