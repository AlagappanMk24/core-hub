export interface ResetPasswordRequest {
    email: string;
    token : string;
    newPassword : string;
    confirmPassword : string;
  }