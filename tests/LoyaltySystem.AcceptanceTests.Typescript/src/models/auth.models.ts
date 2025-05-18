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

export class RegisterUserDto
{
  constructor(
      firstName: string,
      lastName: string,
      username: string,
      email: string,
      phone: string,
      password: string,
      confirmPassword: string,
      roles: string[],
  ) {
    this.first_name = firstName;
    this.last_name = lastName;
    this.username = username;
    this.email = email;
    this.phone = phone;
    this.password = password;
    this.confirm_password = confirmPassword;
    this.roles = roles;
  }

  first_name: string;
  last_name: string;
  username: string;
  email: string;
  phone: string;
  password: string;
  confirm_password: string;
  roles: string[]
}