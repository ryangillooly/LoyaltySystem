// Authentication request models
export interface LoginRequest {
  email?: string;
  username?: string;
  password: string;
}

export interface RegisterRequest extends LoginRequest {
  firstName?: string;
  lastName?: string;
  phoneNumber?: string;
}

// Authentication response models
export interface AuthResponse {
  token: string;
  user: UserResponse;
}

export interface UserResponse {
  id: string;
  email?: string;
  userName?: string;
  firstName?: string;
  lastName?: string;
  roles: string[];
} 