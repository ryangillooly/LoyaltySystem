import { test, expect } from '@playwright/test';
import { StaffApiClient } from '../utils/staff-api-client';
import { AdminApiClient } from '../utils/admin-api-client';
import { getTokenFromMailhog, purgeMailhog } from '../utils/mailhog';
import { Credentials } from '../models/auth.models';
import { createRegisterRequest } from '../utils/helpers';

test.describe('Staff API Authentication', () => {
  let staffClient: StaffApiClient;
  let adminApiClient: AdminApiClient;
  let staffCredentials: Credentials = { username: 'staff', password: 'admin' }
  let adminCredentials: Credentials = { username: 'admin', password: 'admin' }

  test.beforeEach(async () => {
    staffClient = new StaffApiClient();
    adminApiClient = new AdminApiClient();
    await staffClient.init();
    await adminApiClient.init();
    await purgeMailhog();
  });

  test.afterEach(async () => {
    await staffClient.dispose();
  });

  
  test('should successfully login with valid staff credentials', async () => {
    const response = await staffClient.login(staffCredentials);
    expect(response.status).toBe(200);
    expect(response.response).toMatchObject({
      access_token: expect.any(String),
      expires_in: 3599,
      refresh_token: null,
      token_type: 'Bearer'
    });
  });

  test('should fail to login with invalid credentials', async () => {
    const invalidCredentials = new Credentials('incorrect', 'incorrect');
    const authResponse = await staffClient.login(invalidCredentials);

    expect(authResponse.status).toBe(401);
    expect(authResponse.response.message).toEqual(['Invalid username/email or password']);
  });

  test('should fail to register (not allowed on this API)', async () => {
    const login = await staffClient.login(staffCredentials);
    staffClient.setAuthToken(login.response.access_token);
    
    const response = await staffClient.register({});
    expect(response.status).toBe(400);
    expect(response.body.message).toEqual(['Registration not allowed through Staff API']);
  });
  
  test('should process forgotten/reset password request', async () => {
    // 1. Login and get JWT
    const response = await staffClient.login(adminCredentials);
    adminApiClient.setAuthToken(response.response.access_token);
    
    // 2. Register new user (using Admin API, as you cannot register via Staff API)
    const registerPayload = createRegisterRequest('Staff', ['Staff']);
    const register = await adminApiClient.register(registerPayload);
    
    expect(register.status).toBe(201);
    expect(register.body).toMatchObject({
      id: expect.stringMatching(/^usr_/),
      first_name: registerPayload.first_name,
      last_name: registerPayload.last_name,
      username: registerPayload.username,
      email: registerPayload.email,
      status: 'Active',
      customer_id: null,
      phone: registerPayload.phone,
      roles: ['User', 'Staff' ],
      is_email_confirmed: false,
    });
        
    // 3. Forgot Password
    const forgotResponse = await staffClient.postToAccount('forgot-password', { email: registerPayload.email });
    expect(forgotResponse.status).toBe(200);
    expect(forgotResponse.body.message).toEqual('If the account exists, a password reset email has been sent.');
    
    // 4. Get Password Reset Token
    const token = await getTokenFromMailhog(registerPayload.email, 'Password Reset Request');
    
    // 5. Reset Password
    const resetResponse = await staffClient.postToAccount('reset-password', {
      username: registerPayload.username,
      token: token,
      new_password: 'NewPassword123!',
      confirm_password: 'NewPassword123!'
    });
    expect(resetResponse.status).toBe(200);
    expect(resetResponse.body.message).toContain('Password has been reset');
  });

  test('should process email verification request', async () => {
    // 1. Login and get JWT
    const response = await staffClient.login(adminCredentials);
    adminApiClient.setAuthToken(response.response.access_token);

    // 2. Register new user (using Admin API, as you cannot register via Staff API)
    const registerPayload = createRegisterRequest('Staff', ['Staff'])
    const register = await adminApiClient.register(registerPayload);

    expect(register.status).toBe(201);
    expect(register.body).toMatchObject({
      id: expect.stringMatching(/^usr_/),
      first_name: registerPayload.first_name,
      last_name: registerPayload.last_name,
      username: registerPayload.username,
      email: registerPayload.email,
      status: 'Active',
      customer_id: null,
      phone: registerPayload.phone,
      roles: ['User', 'Staff' ],
      is_email_confirmed: false,
    });
    
    // 3. Get Email Verification Code
    const token = await getTokenFromMailhog(registerPayload.email, 'Email Verification');
    
    // 4. Verify token with code
    const verifyResponse = await staffClient.postToAccount(`verify-email?token=${token}`, {});
    expect(verifyResponse.status).toBe(200);
    expect(verifyResponse.body).toEqual('Email verified successfully!');
  });

  test('should process resend email verification request', async () => {
    // 1. Login and get JWT
    const response = await staffClient.login(adminCredentials);
    adminApiClient.setAuthToken(response.response.access_token);

    // 2. Register new user (using Admin API, as you cannot register via Staff API)
    const registerPayload = createRegisterRequest('Staff', ['Staff'])
    const register = await adminApiClient.register(registerPayload);

    expect(register.status).toBe(201);
    expect(register.body).toMatchObject({
      id: expect.stringMatching(/^usr_/),
      first_name: registerPayload.first_name,
      last_name: registerPayload.last_name,
      username: registerPayload.username,
      email: registerPayload.email,
      status: 'Active',
      customer_id: null,
      phone: registerPayload.phone,
      roles: ['User', 'Staff' ],
      is_email_confirmed: false,
    });

    // 3. Resend Verification Email
    const resendResponse = await staffClient.postToAccount('resend-verification', { email: registerPayload.email });
    expect(resendResponse.status).toBe(200);
    expect(resendResponse.body.message).toEqual('If the account exists, a verification email has been sent.');
    
    // 3. Get Email Verification Code
    const token = await getTokenFromMailhog(registerPayload.email, 'Email Verification');

    // 4. Verify token with code
    const verifyResponse = await staffClient.postToAccount(`verify-email?token=${token}`, {});
    expect(verifyResponse.status).toBe(200);
    expect(verifyResponse.body).toEqual('Email verified successfully!');
  });
}); 