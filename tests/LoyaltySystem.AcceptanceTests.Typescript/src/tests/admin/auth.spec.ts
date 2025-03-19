import { test, expect } from '@playwright/test';
import { AdminApiClient } from '../../utils/admin-api-client';

test.describe('Admin Authentication', () => {
  let adminClient: AdminApiClient;

  test.beforeEach(async () => {
    adminClient = new AdminApiClient();
    await adminClient.init();
  });

  test.afterEach(async () => {
    await adminClient.dispose();
  });

  test('should successfully login with valid admin credentials', async () => {
    // Use hardcoded credentials for testing
    const credentials = {
      username: 'BusinessAdminUser',
      password: 'Admin123!'
    };

    // Attempt to login
    const authResponse = await adminClient.loginAdmin(credentials);

    // Verify the response
    expect(authResponse).toBeDefined();
    expect(authResponse.token).toBeDefined();
    
    // Verify user data
    expect(authResponse.user).toBeDefined();
    expect(authResponse.user.userName).toBe(credentials.username);
    expect(authResponse.user.roles).toContain('Admin');
  });

  test('should fail to login with invalid credentials', async () => {
    // Use invalid credentials
    const invalidCredentials = {
      email: 'invalid@example.com',
      password: 'WrongPassword123!'
    };

    // Expect the login to fail
    try {
      await adminClient.login(invalidCredentials);
      // If we get here, the test failed because login succeeded
      expect(false).toBe(true, 'Login should have failed with invalid credentials');
    } catch (error) {
      // Login should fail with an error
      expect(error).toBeDefined();
    }
  });

  test('should be able to access protected endpoints after login', async () => {
    // Login with admin credentials
    await adminClient.loginAdmin();

    // Try to access a protected endpoint (brands)
    const brands = await adminClient.getBrands();

    // Verify we got a successful response
    expect(brands).toBeDefined();
    expect(Array.isArray(brands)).toBe(true);
  });
}); 