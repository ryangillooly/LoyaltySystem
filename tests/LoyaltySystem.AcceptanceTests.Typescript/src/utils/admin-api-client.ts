import { ApiClient } from './api-client';
import { AuthResponse, AuthResponseDto, LoginRequest } from '../models/auth.models';
import { envConfig as config } from './config';

export class AdminApiClient extends ApiClient {
    
  constructor() {
    const baseUrl = config.adminApiUrl;
    super(baseUrl);
  }

  async login(credentials: LoginRequest): Promise<any> {
    if (!this.context) {
      await this.init();
    }
    const response = await this.context!.post('/api/auth/login', {
      data: credentials
    });
    const authResponse = await response.json();
    this.authToken = authResponse.access_token;
    return new AuthResponseDto(response.status(), authResponse);
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
  
  async register(registerRequest: any): Promise<any> {
    return await this.post<any>('/api/auth/register', registerRequest);
  }

  async postToAccount(endpoint: string, data: any): Promise<any> {
    return await this.post<any>(`/api/account/${endpoint}`, data);
  }
} 