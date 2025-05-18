import { test, expect } from '@playwright/test';
import { AdminApiClient } from '../../utils/admin-api-client';
import { StaffApiClient } from '../../utils/staff-api-client';
import { getTokenFromMailhog, purgeMailhog } from '../../utils/mailhog';
import { createRegisterRequest } from '../../utils/helpers';

test.describe('Staff Onboarding and Password Reset Journey', () => {
  let adminClient: AdminApiClient;
  let staffClient: StaffApiClient;
  const adminCredentials = { username: 'admin', password: 'admin' };
  const newPassword = 'Staff456!';

  test.beforeEach(async () => {
    adminClient = new AdminApiClient();
    staffClient = new StaffApiClient();
    await adminClient.init();
    await staffClient.init();
    await purgeMailhog();
  });

  test.afterEach(async () => {
    await adminClient.dispose();
    await staffClient.dispose();
  });

  test('should onboard and manage staff user password', async () => {
    // 1. Login as Admin
    const adminLogin = await adminClient.login(adminCredentials);
    expect(adminLogin.response).toMatchObject({
      access_token: expect.any(String),
      expires_in: 3599,
      refresh_token: null,
      token_type: 'Bearer'
    });
    
    adminClient.setAuthToken(adminLogin.response.access_token);

    // 2. Register new Staff user
    const registerPayload = createRegisterRequest('Staff', ['Staff']);
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
      roles: ['User', 'Staff' ],
      is_email_confirmed: false,
    });

    // 3. Login as new Staff user (initial password) - Rejection due to no email confirmation
    const staffLogin = await staffClient.login({ 
      username: registerPayload.username, 
      password: registerPayload.password, 
    });
    expect(staffLogin.response.message).toEqual(["Email has not been confirmed"]);

    // 4. Resend Verification Email
    const resendEmailVerification = await staffClient.resendVerificationEmail({ email: registerPayload.email });
    expect(resendEmailVerification.status).toBe(200);
    expect(resendEmailVerification.body.message).toEqual("If the account exists, a verification email has been sent.");

    // 5. Get Token from Email
    const rawToken = await getTokenFromMailhog(registerPayload.email, 'Email Verification');
    expect(rawToken).not.toBeNull();
    const cleanToken = rawToken!.replace(/\s+/g, '').trim();
    
    // 6. Verify email with token
    const verifyEmail = await staffClient.verifyEmail(cleanToken);
    expect(verifyEmail.status).toBe(200);
    expect(verifyEmail.body).toEqual(`Email verified successfully!`);

    // 7. Forgot password
    const forgotResponse = await staffClient.postToAccount('forgot-password', { username: registerPayload.username });
    expect(forgotResponse.body.message).toEqual('If the account exists, a password reset email has been sent.');

    // 8. Get token from MailHog
    const rawToken2 = await getTokenFromMailhog(registerPayload.email, 'Password Reset Request');
    expect(rawToken2).not.toBeNull();
    const cleanToken2 = rawToken2 ? rawToken2.replace(/\s+/g, '').trim() : null;
    
    // 9. Reset password
    const resetResponse = await staffClient.postToAccount('reset-password', {
      username: registerPayload.username,
      token: cleanToken2,
      new_password: newPassword,
      confirm_password: newPassword
    });
        
    expect(resetResponse.body.message).toContain('Password has been reset');

    // 10. Login as new Staff user with new password (should succeed)
    const newLogin = await staffClient.login({ 
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

    // 11. Login as new Staff user with old password (should fail)
    const oldLogin = await staffClient.login({ 
      username: registerPayload.username, 
      password: registerPayload.password, 
    });
    expect(oldLogin.status).toBe(401);
    expect(oldLogin.response.message).toEqual(['Invalid username/email or password']);
  });
});