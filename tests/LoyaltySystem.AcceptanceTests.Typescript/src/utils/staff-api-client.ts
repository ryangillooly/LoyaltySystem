import { ApiClient } from './api-client';
import { AuthResponse, LoginRequest } from '../models/auth.models';
import { envConfig as config } from './config';

export class StaffApiClient extends ApiClient {
    constructor() {
        const baseUrl = config.staffApiUrl;
        super(baseUrl);
    }

    async login(credentials: LoginRequest): Promise<AuthResponse> {
        if (!this.context) {
            await this.init();
        }
        const response = await this.context!.post('/api/auth/login', {
            data: credentials
        });
        const authResponse = await response.json() as AuthResponse;
        this.authToken = authResponse.access_token;
        return authResponse;
    }

    setAuthToken(token: string): void {
        this.authToken = token;
    }
    
    async register(registerRequest: any): Promise<any> {
        return await this.post<any>('/api/auth/register', registerRequest);
    }

    async resendVerificationEmail(registerRequest: any): Promise<any> {
        return await this.post<any>('/api/account/resend-verification', registerRequest);
    }

    async verifyEmail(token: string): Promise<any> {
        return await this.post<any>(`/api/account/verify-email?Token=${token}`, null);
    }

    async postToAccount(endpoint: string, data: any): Promise<any> {
        return await this.post<any>(`/api/account/${endpoint}`, data);
    }
}