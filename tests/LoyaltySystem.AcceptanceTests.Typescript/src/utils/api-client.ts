import { APIRequestContext, request } from '@playwright/test';
import { AuthResponse, LoginRequest } from '../models/auth.models';

export class ApiClient {
  protected context: APIRequestContext | null = null;
  protected authToken: string | null = null;
  private readonly baseUrl: string;

  constructor(baseUrl: string) {
    this.baseUrl = baseUrl;
  }

  async init(): Promise<void> {
    this.context = await request.newContext({
      baseURL: this.baseUrl,
      extraHTTPHeaders: {
        'Accept': 'application/json',
        'Content-Type': 'application/json'
      }
    });
  }
  
  async login(credentials: LoginRequest): Promise<AuthResponse> {
    if (!this.context) {
      await this.init();
    }

    const response = await this.context!.post('/api/admin/auth/login', {
      data: credentials
    });

    const authResponse = await response.json() as AuthResponse;
    this.authToken = authResponse.access_token;
    
    return authResponse;
  }
  
  async get<T>(url: string): Promise<T> {
    if (!this.context) {
      await this.init();
    }

    const headers: Record<string, string> = {};
    if (this.authToken) {
      headers['Authorization'] = `Bearer ${this.authToken}`;
    }

    const response = await this.context!.get(url, { headers });
    if (!response.ok()) {
      throw new Error(`GET request failed: ${response.statusText()}`);
    }
    
    return await response.json() as T;
  }
  
  async post<T>(url: string, data: any): Promise<T> {
    if (!this.context) {
      await this.init();
    }

    const headers: Record<string, string> = {};
    if (this.authToken) {
      headers['Authorization'] = `Bearer ${this.authToken}`;
    }

    const response = await this.context!.post(url, {
      data,
      headers
    });

    if (!response.ok()) {
      throw new Error(`POST request failed: ${response.statusText()}`);
    }
    
    return await response.json() as T;
  }
  
  async put<T>(url: string, data: any): Promise<T> {
    if (!this.context) {
      await this.init();
    }

    const headers: Record<string, string> = {};
    if (this.authToken) {
      headers['Authorization'] = `Bearer ${this.authToken}`;
    }

    const response = await this.context!.put(url, {
      data,
      headers
    });

    if (!response.ok()) {
      throw new Error(`PUT request failed: ${response.statusText()}`);
    }
    
    return await response.json() as T;
  }

  async delete<T>(url: string): Promise<T> {
    if (!this.context) {
      await this.init();
    }

    const headers: Record<string, string> = {};
    if (this.authToken) {
      headers['Authorization'] = `Bearer ${this.authToken}`;
    }

    const response = await this.context!.delete(url, { headers });
    
    if (!response.ok()) {
      throw new Error(`DELETE request failed: ${response.statusText()}`);
    }
    
    return await response.json() as T;
  }
  
  async dispose(): Promise<void> {
    if (this.context) {
      await this.context.dispose();
      this.context = null;
    }
  }

  public setAuthToken(token: string) {
    this.authToken = token;
  }
} 