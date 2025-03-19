import { test, expect } from '@playwright/test';
import { AdminApiClient } from '../../utils/admin-api-client';

test.describe('Loyalty Program Management Journey', () => {
  let adminClient: AdminApiClient;
  
  // Test data
  const testId = `test-${Date.now()}`;
  let brandId: string;
  let programId: string;
  let rewardId: string;

  test.beforeAll(async () => {
    // Set up API client and login
    adminClient = new AdminApiClient();
    await adminClient.init();
    await adminClient.loginAdmin();
    
    // Create a test brand to use for all tests in this suite
    const brandResponse = await adminClient.createBrand({
      name: `Test Brand for Loyalty Program ${testId}`,
      category: 'Test',
      description: 'Test brand for loyalty program journey testing'
    });
    
    brandId = brandResponse.id;
  });

  test.afterAll(async () => {
    // Clean up
    await adminClient.dispose();
  });

  test('1. Should create a new loyalty program', async () => {
    // Create loyalty program request
    const createProgramRequest = {
      name: `Test Loyalty Program ${testId}`,
      brandId: brandId,
      type: 'Points',
      description: 'A test loyalty program created by automated tests',
      pointsConversionRate: 10, // 10 points per currency unit
      minimumTransactionAmount: 1.00,
      pointsRoundingRule: 'RoundUp',
      termsAndConditions: 'Test terms and conditions',
      enrollmentBonusPoints: 100
    };
    
    // Create the program
    const createResponse = await adminClient.createLoyaltyProgram(createProgramRequest);
    
    // Verify program created successfully
    expect(createResponse).toBeDefined();
    expect(createResponse.id).toBeDefined();
    expect(createResponse.name).toBe(createProgramRequest.name);
    expect(createResponse.brandId).toBe(brandId);
    
    // Store program ID for next tests
    programId = createResponse.id;
  });

  test('2. Should retrieve the created loyalty program by ID', async () => {
    // Get the program by ID
    const retrievedProgram = await adminClient.getLoyaltyProgramById(programId);
    
    // Verify the retrieved program
    expect(retrievedProgram).toBeDefined();
    expect(retrievedProgram.id).toBe(programId);
    expect(retrievedProgram.brandId).toBe(brandId);
    expect(retrievedProgram.type).toBe('Points');
  });

  test('3. Should create a reward for the loyalty program', async () => {
    // Create reward request
    const createRewardRequest = {
      title: `Test Reward ${testId}`,
      description: 'A test reward created by automated tests',
      requiredValue: 500, // 500 points needed
      validFrom: new Date().toISOString(),
      validTo: new Date(Date.now() + 30 * 24 * 60 * 60 * 1000).toISOString() // 30 days from now
    };
    
    // Create the reward
    const createResponse = await adminClient.createReward(programId, createRewardRequest);
    
    // Verify reward created successfully
    expect(createResponse).toBeDefined();
    expect(createResponse.id).toBeDefined();
    expect(createResponse.title).toBe(createRewardRequest.title);
    expect(createResponse.requiredPoints).toBe(createRewardRequest.requiredValue);
    
    // Store reward ID for next tests
    rewardId = createResponse.id;
  });

  test('4. Should retrieve rewards for the loyalty program', async () => {
    // Get rewards for the program
    const rewards = await adminClient.getRewardsByProgramId(programId);
    
    // Verify we got a successful response
    expect(rewards).toBeDefined();
    expect(Array.isArray(rewards)).toBe(true);
    
    // Find our created reward
    const foundReward = rewards.find(r => r.id === rewardId);
    expect(foundReward).toBeDefined();
    expect(foundReward.title).toContain('Test Reward');
  });

  test('5. Should retrieve all loyalty programs including the new one', async () => {
    // Get all programs
    const programs = await adminClient.getLoyaltyPrograms();
    
    // Verify we got a successful response
    expect(programs).toBeDefined();
    expect(Array.isArray(programs)).toBe(true);
    
    // Find our created program
    const foundProgram = programs.find(p => p.id === programId);
    expect(foundProgram).toBeDefined();
    expect(foundProgram.name).toContain('Test Loyalty Program');
    expect(foundProgram.brandId).toBe(brandId);
  });
}); 