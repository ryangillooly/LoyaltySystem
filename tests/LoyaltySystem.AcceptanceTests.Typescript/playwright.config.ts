import { defineConfig } from '@playwright/test';

export default defineConfig({
  testDir: './src/tests',
  timeout: 30000,
  fullyParallel: true,
  forbidOnly: !!process.env.CI,
  retries: process.env.CI ? 2 : 0,
  workers: process.env.CI ? 1 : undefined,
  reporter: [
    ['html'],
    ['list']
  ],
  use: {
    baseURL: 'http://localhost:5000',
    extraHTTPHeaders: {
      'Accept': 'application/json',
      'Content-Type': 'application/json'
    },
    trace: 'on-first-retry',
  },
  projects: [
    {
      name: 'admin',
      testMatch: /admin\/.*\.spec\.ts/,
    },
    {
      name: 'customer',
      testMatch: /customer\/.*\.spec\.ts/,
    },
    {
      name: 'staff',
      testMatch: /staff\/.*\.spec\.ts/,
    }
  ],
}); 