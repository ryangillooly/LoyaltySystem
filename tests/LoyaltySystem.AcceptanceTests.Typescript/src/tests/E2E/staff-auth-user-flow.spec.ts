import { test, expect } from '@playwright/test';
import { AdminApiClient } from '../../utils/admin-api-client';
import { StaffApiClient } from '../../utils/staff-api-client';
import { getTokenFromMailhog, purgeMailhog } from '../../utils/mailhog';

test.describe('Staff Onboarding and Password Reset Journey', () => {
  let adminClient: AdminApiClient;
  let staffClient: StaffApiClient;
  const adminCredentials = { username: 'admin', password: 'admin' };

  // Generate unique staff user details
  const unique = Date.now();
  const staffUsername = `staff_${unique}`;
  const staffEmail = `staff_${unique}@example.com`;
  const initialPassword = 'Staff123!';
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
    expect(adminLogin).toMatchObject({
      access_token: expect.any(String),
      expires_in: 3599,
      refresh_token: null,
      token_type: 'Bearer'
    });
    
    adminClient.setAuthToken(adminLogin.access_token);

    // 2. Register new Staff user
    const registerPayload = {
      firstName: 'Test',
      lastName: 'Staff',
      userName: staffUsername,
      email: staffEmail,
      phone: `07${Math.floor(100000000 + Math.random() * 900000000)}`,
      password: initialPassword,
      confirmPassword: initialPassword,
      roles: ['Staff']
    };
    const registerResponse = await adminClient.register(registerPayload);
    expect(registerResponse).toMatchObject({
      id: expect.any(String),
      email: staffEmail
    });

    // 3. Login as new Staff user (initial password) - Rejection due to no email confirmation
    const staffLogin = await staffClient.login({ 
      username: staffUsername, 
      password: initialPassword 
    });
    expect(staffLogin).toMatchObject({ message: ["Email has not been confirmed"] });

    // 4. Resend Verification Email
    const resendEmailVerification = await staffClient.resendVerificationEmail({ email: staffEmail });
    expect(staffLogin).toMatchObject({ message: ["Email has not been confirmed"] });

    // 5. Get Token from Email
    const rawToken = await getTokenFromMailhog(staffEmail, 'Email Verification');
    expect(rawToken).not.toBeNull();
    const cleanToken = rawToken!.replace(/\s+/g, '').trim();
    
    console.log(`Email Confirmation Token: ${cleanToken}`);

    // 6. Verify email with token
    const verifyEmail = await staffClient.verifyEmail(cleanToken);
    expect(verifyEmail).toBe(`Email verified successfully!`);

    // 7. Forgot password
    const forgotResponse = await staffClient.postToAccount('forgot-password', { email: staffEmail });
    expect(forgotResponse.message).toBe('If the account exists, a password reset email has been sent.');

    // 8. Get token from MailHog
    const rawToken2 = await getTokenFromMailhog(staffEmail, 'Password Reset Request');
    expect(rawToken2).not.toBeNull();
    const cleanToken2 = rawToken2 ? rawToken2.replace(/\s+/g, '').trim() : null;

    console.log(`Password Reset Token: ${cleanToken2}`);
    
    // 9. Reset password
    const resetResponse = await staffClient.postToAccount('reset-password', {
      username: staffUsername,
      token: cleanToken2,
      newPassword,
      confirmPassword: newPassword
    });
    expect(resetResponse.message).toContain('Password has been reset');

    // 10. Login as new Staff user with new password (should succeed)
    const newLogin = await staffClient.login({ 
      username: staffUsername, 
      password: newPassword 
    });
    expect(newLogin).toMatchObject({
      access_token: expect.any(String),
      expires_in: 3599,
      refresh_token: null,
      token_type: 'Bearer'
    });

    // 11. Login as new Staff user with old password (should fail)
    const oldLogin = await staffClient.login({ 
      username: staffUsername, 
      password: initialPassword 
    });
    expect(oldLogin).toMatchObject({
      message: [ `Invalid username/email or password` ]
    })
  });
});