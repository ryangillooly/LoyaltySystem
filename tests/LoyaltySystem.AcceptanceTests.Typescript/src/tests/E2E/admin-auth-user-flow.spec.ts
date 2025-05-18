import { test, expect } from '@playwright/test';
import { AdminApiClient } from '../../utils/admin-api-client';
import { getTokenFromMailhog, purgeMailhog } from '../../utils/mailhog';
import { createRegisterRequest } from '../../utils/helpers';

// E2E User Flow Journey Tests for Admin Authentication
// Covers real-world scenarios involving multiple API calls and state changes

test.describe('Admin Auth User Flow Journey', () => {
  let adminClient: AdminApiClient;
  const newPassword = 'Admin456!';
  const adminCredentials = { username: 'admin', password: 'admin' };

  test.beforeEach(async () => {
    adminClient = new AdminApiClient();
    await adminClient.init();
    await purgeMailhog();
  });

  test.afterEach(async () => { await adminClient.dispose(); });

  test('should onboard and manage admin user password', async () => {
    // 1. Login as Admin
    const adminLogin = await adminClient.login(adminCredentials);
    expect(adminLogin.status).toBe(200);
    expect(adminLogin.response).toMatchObject({
      access_token: expect.any(String),
      expires_in: 3599,
      refresh_token: null,
      token_type: 'Bearer'
    });
    
    adminClient.setAuthToken(adminLogin.response.access_token);
    
    // 2. Register new Admin user
    const registerPayload = createRegisterRequest('Admin', ['Admin'])
    const register = await adminClient.register(registerPayload);
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
      roles: ['User', 'Admin' ],
      is_email_confirmed: false,
    });

    // 3. Login as new Admin user (initial password) - Rejection due to no email confirmation
    const newAdminLogin = await adminClient.login({ 
      username: registerPayload.username, 
      password: registerPayload.password 
    });
    expect(newAdminLogin.response.message).toEqual(["Email has not been confirmed"]);
    
    // 4. Resend Verification Email
    const resendEmailVerification = await adminClient.postToAccount('resend-verification', { email: registerPayload.email });
    expect(resendEmailVerification.status).toBe(200);
    expect(resendEmailVerification.body.message).toEqual("If the account exists, a verification email has been sent.");

    // 5. Get Token from Email
    const verifyToken = await getTokenFromMailhog(registerPayload.email, 'Email Verification');

    // 6. Verify email with token 
    const verifyEmail = await adminClient.postToAccount(`verify-email?Token=${verifyToken}`, null);
    expect(verifyEmail.status).toBe(200);
    expect(verifyEmail.body).toEqual(`Email verified successfully!`);

    // 7. Forgot password
    const forgotResponse = await adminClient.postToAccount('forgot-password', { email: registerPayload.email });
    expect(forgotResponse.body.message).toEqual('If the account exists, a password reset email has been sent.');

    // 8. Get token from MailHog
    const resetToken = await getTokenFromMailhog(registerPayload.email, 'Password Reset Request');

    // 9. Reset password
    const resetResponse = await adminClient.postToAccount('reset-password', {
      username: registerPayload.username,
      token: resetToken,
      new_password: newPassword,
      confirm_password: newPassword
    });
    expect(resetResponse.body.message).toContain('Password has been reset');

    // 10. Login as new Admin user with new password (should succeed)
    const newLogin = await adminClient.login({ 
      username: registerPayload.username, 
      password: newPassword 
    });
    expect(newLogin.status).toBe(200);
    expect(newLogin.response).toMatchObject({
      access_token: expect.any(String),
      expires_in: 3599,
      refresh_token: null,
      token_type: 'Bearer'
    });

    // 11. Login as new Admin user with old password (should fail)
    const oldLogin = await adminClient.login({ 
      username: registerPayload.username, 
      password: registerPayload.password 
    });
    expect(oldLogin.status).toBe(401);
    expect(oldLogin.response.message).toEqual(['Invalid username/email or password']);
  });
}); 