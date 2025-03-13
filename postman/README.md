# LoyaltySystem Postman Collections

This directory contains Postman collections for testing the LoyaltySystem API.

## Collections

- **LoyaltySystem_Business_API.postman_collection.json**: Collection for Business management endpoints

## Environment

- **LoyaltySystem_Environment.postman_environment.json**: Environment variables for the collections

## Setup Instructions

1. Install [Postman](https://www.postman.com/downloads/)
2. Import the collection and environment files into Postman:
   - Click on "Import" in Postman
   - Drag and drop both JSON files or browse to select them
   - Click "Import"

3. Select the "LoyaltySystem - Development" environment from the dropdown in the top right corner

## Usage Instructions

### Authentication

1. Run the "Login (Get Token)" request in the Authentication folder
2. The authToken environment variable will be automatically set for all subsequent requests

### Business API

The Business API collection is organized to follow the typical workflow:

1. **Create Business** - Creates a new business and stores the ID in the environment variable
2. **Get All Businesses** - Retrieves a paginated list of all businesses
3. **Get Business by ID** - Gets a specific business using the stored ID
4. **Get Business Detail** - Gets detailed information including brands
5. **Update Business** - Updates business information
6. **Get Business Overview** - Gets basic metrics about the business
7. **Get Business Performance** - Gets performance metrics for a date range
8. **Delete Business** - Deletes a business (only possible if it has no brands)

### Request Flow

The requests include tests that automatically set environment variables when appropriate. For example:
- When you create a business, its ID is stored in the "businessId" variable
- This allows you to immediately run other requests that use this ID

### Tests

Each request includes tests that validate:
- Status codes
- Response structure
- Data correctness

You can view the test results in the "Test Results" tab after each request.

## Customization

You can modify the environment variables to point to different environments:

1. Click on the "Environments" tab in Postman
2. Edit the "LoyaltySystem - Development" environment
3. Change the "baseUrl" to point to your desired environment

## Additional Notes

- The DELETE endpoint for businesses will fail if the business has associated brands
- For authentication, replace the credentials in the "Login" request with valid user credentials
- Use variable references (e.g., `{{businessId}}`) in your requests to ensure they work with the stored values 