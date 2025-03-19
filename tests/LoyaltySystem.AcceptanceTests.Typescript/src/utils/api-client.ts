import { APIRequestContext, request } from '@playwright/test';
import { AuthResponse, LoginRequest } from '../models/auth.models';

/**
 * API client utility for managing API requests and authentication
 */
export class ApiClient {
  private context: APIRequestContext | null = null;
  private authToken: string | null = null;
  private readonly baseUrl: string;

  constructor(baseUrl: string) {
    this.baseUrl = baseUrl;
  }

  /**
   * Initialize the API context
   */
  async init(): Promise<void> {
    this.context = await request.newContext({
      baseURL: this.baseUrl,
      extraHTTPHeaders: {
        'Accept': 'application/json',
        'Content-Type': 'application/json'
      }
    });
  }

  /**
   * Authenticate with the API and store the token
   */
  async login(credentials: LoginRequest): Promise<AuthResponse> {
    if (!this.context) {
      await this.init();
    }

    const response = await this.context!.post('/api/admin/auth/login', {
      data: credentials
    });

    const authResponse = await response.json() as AuthResponse;
    this.authToken = authResponse.token;
    
    return authResponse;
  }

  /**
   * Make an authenticated GET request
   */
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

  /**
   * Make an authenticated POST request
   */
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

  /**
   * Make an authenticated PUT request
   */
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

  /**
   * Make an authenticated DELETE request
   */
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

  /**
   * Close the API context
   */
  async dispose(): Promise<void> {
    if (this.context) {
      await this.context.dispose();
      this.context = null;
    }
  }
} 