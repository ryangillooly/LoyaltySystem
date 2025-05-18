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
    return await this.get<any[]>('/api/brands');
  }
  async getBrandById(brandId: string) {
    return await this.get<any>(`/api/brands/${brandId}`);
  }
  async createBrand(brandData: any) {
    return await this.post<any>('/api/brands', brandData);
  }
  async getLoyaltyPrograms() {
    return await this.get<any[]>('/api/loyaltyPrograms');
  }
  async getLoyaltyProgramById(programId: string) {
    return await this.get<any>(`/api/loyaltyPrograms/${programId}`);
  }
  async createLoyaltyProgram(programData: any) {
    return await this.post<any>('/api/loyaltyPrograms', programData);
  }
  async getRewardsByProgramId(programId: string) {
    return await this.get<any[]>(`/api/programs/${programId}/rewards`);
  }
  async createReward(programId: string, rewardData: any) {
    return await this.post<any>(`/api/programs/${programId}/rewards`, rewardData);
  }
  
  async addRole(userId: string, roles: string[]){
    return await this.post<any>(`/api/auth/users/${userId}/roles/add`, {
      roles: roles
    });
  };
  
  async removeRole(userId: string, roles: string[]){
    return await this.post<any>(`/api/auth/users/${userId}/roles/remove`, {
      roles: roles
    });
  };

  async getRoles(userId: string){
    return await this.get<any>(`/api/auth/users/${userId}/roles`);
  };
  
  async register(registerRequest: any): Promise<any> {
    return await this.post<any>('/api/auth/register', registerRequest);
  }

  async postToAccount(endpoint: string, data: any): Promise<any> {
    return await this.post<any>(`/api/account/${endpoint}`, data);
  }
} 