import { test, expect } from '@playwright/test';
import { AdminApiClient } from '../../utils/admin-api-client';
import { Credentials } from '../../models/auth.models';

test.describe('Admin Authentication', () => {
  let adminClient: AdminApiClient;
  let credentials: Credentials;

  test.beforeEach(async () => {
    adminClient = new AdminApiClient();
    credentials =  new Credentials('admin', 'admin');
    await adminClient.init();
  });

  test.afterEach(async () => { await adminClient.dispose(); });

  test('should successfully login with valid admin credentials', async () => {
    const authResponse = await adminClient.login(credentials);

    expect(authResponse).toMatchObject({
      access_token: expect.any(String),
      expires_in: 3599,
      refresh_token: null,
      token_type: 'Bearer'
    });
  });

  test('should fail to login with invalid credentials', async () => {
    try {
      await adminClient.login(new Credentials('incorrect', 'incorrect'));
      expect(false, 'Login should have failed with invalid credentials').toBe(true);
    } catch (error) {
      expect(error).toBeDefined();
    }
  });
  
  test('should register via /api/auth/register with Bearer token and validate response structure', async () => {
    const unique = Math.random().toString(36).slice(2)
    const userName = `testuser_${unique}`;
    const email = `testuser_${unique}@example.com`;
    const phone = `+447${Math.floor(100000000 + Math.random() * 900000000)}`; // UK-style, random 9 digits

    // Step 1: Login as admin and get the token
    const loginResponse = await adminClient.login(credentials);

    adminClient.setAuthToken(loginResponse.access_token);

    // Step 2: Prepare registration payload
    const registerPayload = {
      firstName: 'Ryan',
      lastName: 'Gillooly',
      userName: userName,
      email: email,
      phone: phone,
      password: 'ryangillooly',
      confirmPassword: 'ryangillooly',
      roles: ['SuperAdmin']
    };

    // Step 3: Register the new user
    const body = await adminClient.register(registerPayload);

    // Step 4: Validate the response
    expect(body).toMatchObject({
      id: expect.stringMatching("usr_"),
      firstName: registerPayload.firstName,
      lastName: registerPayload.lastName,
      userName: registerPayload.userName,
      email: registerPayload.email,
      status: 'Active',
      customerId: null,
      phone: registerPayload.phone,
      roles: expect.arrayContaining(['User', 'SuperAdmin']),
      isEmailConfirmed: false
    });
  });
}); 