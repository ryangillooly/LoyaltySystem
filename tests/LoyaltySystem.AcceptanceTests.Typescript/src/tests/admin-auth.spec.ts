import { test, expect } from '@playwright/test';
import { AdminApiClient } from '../utils/admin-api-client';
import { Credentials } from '../models/auth.models';
import { getTokenFromMailhog } from '../utils/mailhog';

test.describe('Admin Authentication', () => {
  let adminClient: AdminApiClient;
  let credentials: Credentials = new Credentials('admin', 'admin');

  test.beforeEach(async () => {
    adminClient = new AdminApiClient();
    await adminClient.init();
  });

  test.afterEach(async () => { await adminClient.dispose(); });

  test('should successfully login with valid admin credentials', async () => {
    const response = await adminClient.login(credentials);
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
    const authResponse = await adminClient.login(invalidCredentials);
    
    expect(authResponse.status).toBe(401);
    expect(authResponse.response.message).toEqual(['Invalid username/email or password']);
  });
  
  test('should successfully register a new user as an Admin', async () => {
    const unique = Math.random().toString(36).slice(2)
    const userName = `testuser_${unique}`;
    const email = `testuser_${unique}@example.com`;
    const phone = `+447${Math.floor(100000000 + Math.random() * 900000000)}`; // UK-style, random 9 digits

    // Step 1: Login as admin and get the token
    const loginResponse = await adminClient.login(credentials);
    adminClient.setAuthToken(loginResponse.response.access_token);

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
    const response = await adminClient.register(registerPayload);

    // Step 4: Validate the response
    expect(response.body).toMatchObject({
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

  test('should fail to register with invalid payload', async () => {
    const login = await adminClient.login(credentials)
    adminClient.setAuthToken(login.response.access_token);
    
    const response = await adminClient.register({});
    expect(response.status).toBe(400);
    expect(response.body).toMatchObject({
      message: [
        "FirstName is required.",
        "FirstName must be between 3 and 100 characters.",
        "LastName is required.",
        "LastName must be between 3 and 100 characters.",
        "Password is required.",
        "Password must be between 6 and 100 characters.",
        "ConfirmPassword is required."
      ]
    })
  });

  test('should process forgotten/reset password request', async () => {
    // 1. Login and get JWT
    const response = await adminClient.login(credentials);
    adminClient.setAuthToken(response.response.access_token);

    // 2. Register new user (using Admin API, as you cannot register via Staff API)
    const unique = Date.now();
    const registerPayload = {
      firstName: 'Admin',
      lastName: 'Admin',
      userName: `admin_${unique}`,
      email: `admin_${unique}@example.com`,
      phone: `07${Math.floor(100000000 + Math.random() * 900000000)}`,
      password: 'admin1',
      confirmPassword: 'admin1',
      roles: ['Admin']
    };
    const register = await adminClient.register(registerPayload);

    expect(register.status).toBe(201);
    expect(register.body).toMatchObject({
      id: expect.stringMatching(/^usr_/),
      firstName: registerPayload.firstName,
      lastName: registerPayload.lastName,
      userName: registerPayload.userName,
      email: registerPayload.email,
      status: 'Active',
      customerId: null,
      phone: registerPayload.phone,
      roles: ['User', 'Admin' ],
      isEmailConfirmed: false,
    });

    // 3. Forgot Password
    const forgotResponse = await adminClient.postToAccount('forgot-password', { email: registerPayload.email });
    expect(forgotResponse.status).toBe(200);
    expect(forgotResponse.body.message).toEqual('If the account exists, a password reset email has been sent.');

    // 4. Get Password Reset Token
    const token = await getTokenFromMailhog(registerPayload.email, 'Password Reset Request');

    // 5. Reset Password
    const resetResponse = await adminClient.postToAccount('reset-password', {
      username: registerPayload.userName,
      token: token,
      newPassword: 'NewPassword123!',
      confirmPassword: 'NewPassword123!'
    });
    expect(resetResponse.status).toBe(200);
    expect(resetResponse.body.message).toContain('Password has been reset');
  });

  test('should process email verification request', async () => {
    // 1. Login and get JWT
    const response = await adminClient.login(credentials);
    adminClient.setAuthToken(response.response.access_token);

    // 2. Register new user (using Admin API, as you cannot register via Staff API)
    const unique = Date.now();
    const registerPayload = {
      firstName: 'Admin',
      lastName: 'Admin',
      userName: `admin_${unique}`,
      email: `admin_${unique}@example.com`,
      phone: `07${Math.floor(100000000 + Math.random() * 900000000)}`,
      password: 'admin1',
      confirmPassword: 'admin1',
      roles: ['Admin']
    };
    const register = await adminClient.register(registerPayload);

    expect(register.status).toBe(201);
    expect(register.body).toMatchObject({
      id: expect.stringMatching(/^usr_/),
      firstName: registerPayload.firstName,
      lastName: registerPayload.lastName,
      userName: registerPayload.userName,
      email: registerPayload.email,
      status: 'Active',
      customerId: null,
      phone: registerPayload.phone,
      roles: ['User', 'Admin' ],
      isEmailConfirmed: false,
    });

    // 3. Get Email Verification Code
    const token = await getTokenFromMailhog(registerPayload.email, 'Email Verification');

    // 4. Verify token with code
    const verifyResponse = await adminClient.postToAccount(`verify-email?token=${token}`, {});
    expect(verifyResponse.status).toBe(200);
    expect(verifyResponse.body).toEqual('Email verified successfully!');
  });

  test('should process resend email verification request', async () => {
    // 1. Login and get JWT
    const response = await adminClient.login(credentials);
    adminClient.setAuthToken(response.response.access_token);

    // 2. Register new user (using Admin API, as you cannot register via Staff API)
    const unique = Date.now();
    const registerPayload = {
      firstName: 'Admin',
      lastName: 'Admin',
      userName: `admin_${unique}`,
      email: `admin_${unique}@example.com`,
      phone: `07${Math.floor(100000000 + Math.random() * 900000000)}`,
      password: 'admin1',
      confirmPassword: 'admin1',
      roles: ['Admin']
    };
    const register = await adminClient.register(registerPayload);

    expect(register.status).toBe(201);
    expect(register.body).toMatchObject({
      id: expect.stringMatching(/^usr_/),
      firstName: registerPayload.firstName,
      lastName: registerPayload.lastName,
      userName: registerPayload.userName,
      email: registerPayload.email,
      status: 'Active',
      customerId: null,
      phone: registerPayload.phone,
      roles: ['User', 'Admin' ],
      isEmailConfirmed: false,
    });

    // 3. Resend Verification Email
    const resendResponse = await adminClient.postToAccount('resend-verification', { email: registerPayload.email });
    expect(resendResponse.status).toBe(200);
    expect(resendResponse.body.message).toEqual('If the account exists, a verification email has been sent.');

    // 3. Get Email Verification Code
    const token = await getTokenFromMailhog(registerPayload.email, 'Email Verification');

    // 4. Verify token with code
    const verifyResponse = await adminClient.postToAccount(`verify-email?token=${token}`, {});
    expect(verifyResponse.status).toBe(200);
    expect(verifyResponse.body).toEqual('Email verified successfully!');
  });
}); 