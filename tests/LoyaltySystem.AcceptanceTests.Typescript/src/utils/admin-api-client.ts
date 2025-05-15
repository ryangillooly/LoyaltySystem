import { ApiClient } from './api-client';
import { AuthResponse, LoginRequest } from '../models/auth.models';

export class AdminApiClient extends ApiClient {
    
  constructor() {
    const baseUrl = 'http://localhost:5001';
    super(baseUrl);
  }

  async login(credentials: LoginRequest): Promise<AuthResponse> {
    if (!this.context) {
      await this.init();
    }

    const response = await this.context!.post('/api/auth/login', {
      data: credentials
    });

    // Check for non-OK or non-JSON responses
    if (!response.ok()) {
      const text = await response.text();
      throw new Error(`Login failed: ${response.status()} - ${text}`);
    }

    const contentType = response.headers()['content-type'];
    if (!contentType || !contentType.includes('application/json')) {
      const text = await response.text();
      throw new Error(`Expected JSON but got: ${contentType || 'no content-type'} - ${text}`);
    }

    const authResponse = await response.json() as AuthResponse;
    this.authToken = authResponse.access_token;

    return authResponse;
  }
  
  async getBrands() {
    return await this.get<any[]>('/api/admin/brands');
  }
  async getBrandById(brandId: string) {
    return await this.get<any>(`/api/admin/brands/${brandId}`);
  }
  async createBrand(brandData: any) {
    return await this.post<any>('/api/admin/brands', brandData);
  }
  async getLoyaltyPrograms() {
    return await this.get<any[]>('/api/admin/loyaltyPrograms');
  }
  async getLoyaltyProgramById(programId: string) {
    return await this.get<any>(`/api/admin/loyaltyPrograms/${programId}`);
  }
  async createLoyaltyProgram(programData: any) {
    return await this.post<any>('/api/admin/loyaltyPrograms', programData);
  }
  async getRewardsByProgramId(programId: string) {
    return await this.get<any[]>(`/api/admin/programs/${programId}/rewards`);
  }
  async createReward(programId: string, rewardData: any) {
    return await this.post<any>(`/api/admin/programs/${programId}/rewards`, rewardData);
  }

  /**
   * Register a new user
   */
  async register(registerRequest: any): Promise<any> {
    return await this.post<any>('/api/auth/register', registerRequest);
  }
} 