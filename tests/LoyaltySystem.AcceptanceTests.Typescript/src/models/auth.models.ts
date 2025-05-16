// Authentication request models
export interface LoginRequest {
  email?: string;
  username?: string;
  password: string;
}

export class Credentials {
  constructor(username: string, password: string) {
    this.username = username;
    this.password = password;
  }
  username: string;
  password: string;
}

export interface RegisterRequest extends LoginRequest {
  firstName?: string;
  lastName?: string;
  phoneNumber?: string;
}

// Authentication response models
export interface AuthResponse {
  access_token: string;
  token_type: string;
  expires_in: number;
  refresh_token?: string;
}

export interface UserResponse {
  id: string;
  email?: string;
  userName?: string;
  firstName?: string;
  lastName?: string;
  roles: string[];
}

export class AuthResponseDto {
  constructor(status: number, response: AuthResponse) {
    this.status = status;
    this.response = response;
  }
  status: number;
  response: AuthResponse;
}