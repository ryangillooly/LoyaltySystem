# Loyalty System Postman Testing Guide

This guide provides instructions for testing the Loyalty System Backend-for-Frontend (BFF) APIs using Postman.

## Setup

### Importing Collections

1. Open Postman
2. Click "Import" in the top left corner
3. Select "File" and then browse to the location of the following files:
   - `LoyaltySystem.Admin.postman_collection.json`
   - `LoyaltySystem.Staff.postman_collection.json`
   - `LoyaltySystem.Customer.postman_collection.json`
4. Click "Import" to add all collections to your Postman workspace

### Configure Environment Variables

1. Create a new Environment in Postman:
   - Click the "Environments" tab in the left sidebar
   - Click the "+" or "Create Environment" button
   - Name it "Loyalty System Local"

2. Add the following variables to your environment:
   - `admin_base_url`: `https://localhost:7001` (or your Admin API URL)
   - `staff_base_url`: `https://localhost:7002` (or your Staff API URL)
   - `customer_base_url`: `https://localhost:7003` (or your Customer API URL)

3. Click "Save" to store your environment

### Starting the APIs

1. Open three terminal windows
2. Navigate to your project directory in each
3. Start each API with the following commands:

```bash
# Terminal 1 - Admin API
cd src/LoyaltySystem.Admin.API
dotnet run

# Terminal 2 - Staff API
cd src/LoyaltySystem.Staff.API
dotnet run

# Terminal 3 - Customer API
cd src/LoyaltySystem.Customer.API
dotnet run
```

## Testing Flow

### 1. Admin API Testing

The Admin API provides comprehensive management capabilities for loyalty programs, customers, and cards.

#### Authentication Flow
1. Use "Admin Login" to authenticate and get a JWT token
2. Copy the token from the response body
3. Set the `admin_token` environment variable to this token value
4. Test "Get Admin Profile" to verify the token works

#### Customer Management Flow
1. Use "Create Customer" to set up a test customer
2. Copy the customer ID from the response and set the `customer_id` environment variable
3. Use "Get Customer by ID" to verify the customer was created
4. Try "Update Customer" to modify details
5. Test "Get Customer Cards" (should be empty initially)

#### Loyalty Program Management Flow
1. Use "Create Program" to set up a test loyalty program
   - You'll need a brand ID - if you don't have one, you may need to create it through direct database access
2. Copy the program ID from the response and set the `program_id` environment variable
3. Use "Get Program by ID" to verify the program was created
4. Try "Add Reward to Program" to create a reward
5. Copy the reward ID and set the `reward_id` environment variable
6. Test "Get Program Rewards" to verify the reward was added

#### Card Management Flow
1. Use "Enroll Customer in Program" to create a loyalty card for the customer
2. Use "Get Customer Cards" to retrieve the card ID
3. Set the `card_id` environment variable to this value
4. Test "Get Card by ID" to verify the card details
5. Try "Update Card Status" to change the card status
6. Test analytics endpoints like "Get Card Count by Status"

### 2. Staff API Testing

The Staff API focuses on point-of-sale operations and store management.

#### Authentication Flow
1. Use "Staff Login" to authenticate and get a JWT token
2. Copy the token and set the `staff_token` environment variable
3. Test "Get Staff Profile" to verify the token works
4. Try "Validate Staff Credentials" to confirm staff role

#### Store Operations Flow
1. Use "Get Current Store" to identify the store associated with the staff account
2. Set the `store_id` environment variable with the store ID from the response
3. Try "Get Store Programs" to see the loyalty programs available at this store
4. Test "Get Store Rewards" to see all rewards across programs
5. Check "Get Store Hours" for operating hours

#### Card Operations Flow
1. Use "Get Card by ID" with the card ID from Admin testing
2. Try "Issue Stamps" to add stamps to a card
3. Test "Add Points" to add points to a card
4. Use "Redeem Reward" to redeem a reward with the card
5. Try "Get Card by QR Code" if you have a QR code value

### 3. Customer API Testing

The Customer API provides a customer-focused view of loyalty programs and cards.

#### Authentication Flow
1. Use "Customer Login" to authenticate and get a JWT token
2. Copy the token and set the `customer_token` environment variable
3. Test "Get Customer Profile" to verify the token works
4. Check "Get Loyalty Summary" for an overview of the customer's loyalty status

#### Loyalty Card Management Flow
1. Use "Get My Cards" to see all cards for the logged-in customer
2. Try "Get Card by ID" with a specific card ID
3. Test "Get Card Transactions" to see transaction history
4. Use "Get Card QR Code" to generate a QR code for a card
5. Try "Get Nearby Stores" to find stores near a location

#### Program Discovery Flow
1. Use "Get Available Programs" to see all active loyalty programs
2. Try "Get Program by ID" to see details of a specific program
3. Test "Get Program Rewards" to see rewards for a program
4. Use "Get Nearby Programs" to find programs near a location
5. Try "Search Programs" to find programs by keyword

## Troubleshooting

- **401 Unauthorized**: Check that your JWT token is valid and set correctly in the environment
- **404 Not Found**: Verify the resource ID is correct and exists
- **400 Bad Request**: Check the request body for missing or invalid fields
- **500 Server Error**: Check the API logs for details on what went wrong

If you encounter HTTPS certificate errors in Postman, you may need to:
1. Go to Settings > General
2. Turn off "SSL certificate verification"
3. Or add your development certificate to Postman's trusted certificates

## Advanced Testing

For more complex test scenarios, consider:

1. Using Postman's Collection Runner to automate test sequences
2. Setting up pre-request scripts to generate dynamic data
3. Creating test scripts to automatically validate responses
4. Using environment variables to share data between requests

Remember to clean up any test data when you're done testing! 