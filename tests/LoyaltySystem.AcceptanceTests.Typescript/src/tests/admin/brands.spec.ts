import { test, expect } from '@playwright/test';
import { AdminApiClient } from '../../utils/admin-api-client';
import { BrandResponse, CreateBrandRequest } from '../../models/brand.models';

test.describe('Admin Brand Management', () => {
  let adminClient: AdminApiClient;
  let testBrandId: string | null = null;

  // Unique identifier for test data to avoid conflicts
  const testId = `test-${Date.now()}`;

  test.beforeEach(async () => {
    adminClient = new AdminApiClient();
    await adminClient.init();
    
    // Login before each test
    await adminClient.loginAdmin();
  });

  test.afterEach(async () => {
    await adminClient.dispose();
  });

  test('should create a brand and retrieve it', async () => {
    // Create brand request
    const createBrandRequest: CreateBrandRequest = {
      name: `Test Brand ${testId}`,
      category: 'Retail',
      description: 'A test brand created by automated tests',
      contact: {
        email: 'test@example.com',
        phone: '+44123456789',
        website: 'https://example.com'
      },
      address: {
        street: '123 Test Street',
        city: 'Test City',
        state: 'Test State',
        postalCode: '12345',
        country: 'Test Country'
      }
    };
    
    // Create the brand
    const createResponse = await adminClient.createBrand(createBrandRequest);
    
    // Verify brand created successfully
    expect(createResponse).toBeDefined();
    expect(createResponse.id).toBeDefined();
    expect(createResponse.name).toBe(createBrandRequest.name);
    
    // Store brand ID for later tests
    testBrandId = createResponse.id;
    
    // Get the brand by ID
    const retrievedBrand = await adminClient.getBrandById(testBrandId);
    
    // Verify the retrieved brand matches what we created
    expect(retrievedBrand).toBeDefined();
    expect(retrievedBrand.id).toBe(testBrandId);
    expect(retrievedBrand.name).toBe(createBrandRequest.name);
    expect(retrievedBrand.category).toBe(createBrandRequest.category);
    expect(retrievedBrand.description).toBe(createBrandRequest.description);
    
    // Verify contact info
    expect(retrievedBrand.contact?.email).toBe(createBrandRequest.contact?.email);
    expect(retrievedBrand.contact?.phone).toBe(createBrandRequest.contact?.phone);
    expect(retrievedBrand.contact?.website).toBe(createBrandRequest.contact?.website);
    
    // Verify address
    expect(retrievedBrand.address?.street).toBe(createBrandRequest.address?.street);
    expect(retrievedBrand.address?.city).toBe(createBrandRequest.address?.city);
    expect(retrievedBrand.address?.state).toBe(createBrandRequest.address?.state);
    expect(retrievedBrand.address?.postalCode).toBe(createBrandRequest.address?.postalCode);
    expect(retrievedBrand.address?.country).toBe(createBrandRequest.address?.country);
  });

  test('should retrieve all brands', async () => {
    // Get all brands
    const brands = await adminClient.getBrands();
    
    // Verify we got a successful response
    expect(brands).toBeDefined();
    expect(Array.isArray(brands)).toBe(true);
    
    // If we created a test brand, verify it's in the list
    if (testBrandId) {
      const foundBrand = brands.find((brand: BrandResponse) => brand.id === testBrandId);
      expect(foundBrand).toBeDefined();
    }
  });

  // More tests for update, delete, etc. would go here
}); 