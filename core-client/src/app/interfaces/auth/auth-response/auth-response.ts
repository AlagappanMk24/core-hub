export interface AuthResponse {
  token: string;
  isSucceeded: true;
  message: string;
  model?: {
    otpToken?: string;
    otpIdentifier? : string;
  };
}
