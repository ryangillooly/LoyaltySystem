import { test, expect } from '@playwright/test';
import { StaffApiClient } from '../utils/staff-api-client';
import { getTokenFromMailhog, purgeMailhog } from '../utils/mailhog';

test.describe('Staff API Authentication', () => {
  let staffClient: StaffApiClient;

  test.beforeEach(async () => {
    staffClient = new StaffApiClient();
    await staffClient.init();
    await purgeMailhog();
  });

  test.afterEach(async () => {
    await staffClient.dispose();
  });

  /*
  test('should login with valid staff credentials', async () => {
    const credentials = { username: 'staff', password: 'admin' };
    const response = await staffClient.login(credentials);
    expect(response).toMatchObject({
      access_token: expect.any(String),
      token_type: expect.any(String),
      expires_in: expect.any(Number)
    });
  });

  test('should fail to login with invalid credentials', async () => {
    const credentials = { username: 'notarealuser', password: 'WrongPassword!' };
    try {
      await staffClient.login(credentials);
      expect(false, 'Login should have failed with invalid credentials').toBe(true);
    } catch (error) {
      expect(error).toBeDefined();
    }
  });

  test('should fail to register (not allowed)', async () => {
    const unique = Date.now();
    const registerPayload = {
      firstName: 'Test',
      lastName: 'Staff',
      userName: `staffuser_${unique}`,
      email: `staffuser_${unique}@example.com`,
      phone: `07${Math.floor(100000000 + Math.random() * 900000000)}`,
      password: 'Staff123!',
      confirmPassword: 'Staff123!',
      roles: ['Staff']
    };
    try {
      await staffClient.register(registerPayload);
      expect(false, 'Registration should not be allowed').toBe(true);
    } catch (error) {
      expect(error).toBeDefined();
    }
  });
  */
  
  test('should process forgotton/reset password request', async () => {
    const staffEmail = 'staff@loyaltysystem.com'
    
    const forgotResponse = await staffClient.postToAccount('forgot-password', { email: staffEmail });
    expect(forgotResponse.message).toBe('If the account exists, a password reset email has been sent.');
    
    const rawToken = await getTokenFromMailhog(staffEmail, 'Password Reset Request', /use this token: ([\s\S]+)/i);
    expect(rawToken).not.toBeNull();

    const cleanToken = rawToken ? rawToken.replace(/\s+/g, '').trim() : null;
    console.log('Reset Token:', cleanToken);
    
    const resetResponse = await staffClient.postToAccount('reset-password', {
      username: 'staff',
      token: cleanToken,
      newPassword: 'NewPassword123!',
      confirmPassword: 'NewPassword123!'
    });
    expect(resetResponse.message).toContain('Password has been reset');
  });

  test('should accept email verification', async () => {
    const response = await staffClient.postToAccount('verify-email?token=fake-or-real-token', {});
    expect(response).toContain('Email verified');
  });

  test('should accept resend verification request', async () => {
    const response = await staffClient.postToAccount('resend-verification', { email: 'staff@loyaltysystem.com' });
    expect(response.message).toContain('verification email');
  });
}); 