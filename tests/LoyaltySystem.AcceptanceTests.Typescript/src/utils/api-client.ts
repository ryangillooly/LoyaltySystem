import { APIRequestContext, request } from '@playwright/test';
import { AuthResponse, AuthResponseDto, LoginRequest } from '../models/auth.models';

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
  
  async login(credentials: LoginRequest): Promise<any> {
    if (!this.context) {
      await this.init();
    }

    const response = await this.context!.post('/api/admin/auth/login', {
      data: credentials
    });

    const authResponse = await response.json();
    this.authToken = authResponse.access_token;
    
    return new AuthResponseDto(response.status(), authResponse);
  }
  
  protected buildHeaders(): Record<string, string> {
    const headers: Record<string, string> = {
      'Content-Type': 'application/json',
    };
    if (this.authToken) {
      headers['Authorization'] = `Bearer ${this.authToken}`;
    }
    return headers;
  }
  
  async get<T>(url: string): Promise<{ status: number, body: T }> {
    if (!this.context) {
      await this.init();
    }
    const headers = this.buildHeaders();
    const response = await this.context!.get(url, { headers });
    const body = await response.json() as T;
    return { status: response.status(), body };
  }

  async post<T>(url: string, data: any): Promise<{ status: number, body: T }> {
    if (!this.context) {
      await this.init();
    }
    const headers = this.buildHeaders();
    const response = await this.context!.post(url, {
      data,
      headers
    });

    let body: T;
    try {
      body = await response.json();
    } catch {
      // fallback for non-JSON responses
      body = (await response.text()) as any;
    }

    return { status: response.status(), body };
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