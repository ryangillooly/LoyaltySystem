import { ApiClient } from './api-client';
import { AuthResponse, LoginRequest } from '../models/auth.models';

/**
 * Specialized client for Admin API interactions
 */
export class AdminApiClient extends ApiClient {
  /**
   * Create a new admin client with the admin API base URL
   */
  constructor() {
    // Use a hardcoded URL for now
    const baseUrl = 'http://localhost:5001';
    super(baseUrl);
  }

  /**
   * Login with admin credentials
   */
  async loginAdmin(credentials?: LoginRequest): Promise<AuthResponse> {
    // Use provided credentials or default
    const loginRequest: LoginRequest = credentials || {
      username: 'BusinessAdminUser',
      password: 'Admin123!'
    };

    return await this.login(loginRequest);
  }

  /**
   * Get all brands
   */
  async getBrands() {
    return await this.get<any[]>('/api/admin/brands');
  }

  /**
   * Get brand by ID
   */
  async getBrandById(brandId: string) {
    return await this.get<any>(`/api/admin/brands/${brandId}`);
  }

  /**
   * Create a new brand
   */
  async createBrand(brandData: any) {
    return await this.post<any>('/api/admin/brands', brandData);
  }

  /**
   * Get all loyalty programs
   */
  async getLoyaltyPrograms() {
    return await this.get<any[]>('/api/admin/loyaltyPrograms');
  }

  /**
   * Get loyalty program by ID
   */
  async getLoyaltyProgramById(programId: string) {
    return await this.get<any>(`/api/admin/loyaltyPrograms/${programId}`);
  }

  /**
   * Create a new loyalty program
   */
  async createLoyaltyProgram(programData: any) {
    return await this.post<any>('/api/admin/loyaltyPrograms', programData);
  }

  /**
   * Get rewards for a loyalty program
   */
  async getRewardsByProgramId(programId: string) {
    return await this.get<any[]>(`/api/admin/programs/${programId}/rewards`);
  }

  /**
   * Create a new reward for a loyalty program
   */
  async createReward(programId: string, rewardData: any) {
    return await this.post<any>(`/api/admin/programs/${programId}/rewards`, rewardData);
  }
} 