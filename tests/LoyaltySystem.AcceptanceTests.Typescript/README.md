# Loyalty System TypeScript Acceptance Tests

This project contains acceptance tests for the Loyalty System using Playwright with TypeScript.

## Prerequisites

- Node.js 18 or later
- npm 8 or later
- Running instance of the Loyalty System APIs

## Installation

### Option 1: Using the setup script
```bash
# Run the setup script
./setup.sh
```

### Option 2: Manual installation
```bash
# Install dependencies
npm install

# Install Playwright browsers
npx playwright install chromium
```

## Configuration

Configuration is managed through environment variables. You can set them in the `.env` file:

```
# API URLs
API_URL=http://localhost:5000
ADMIN_API_URL=http://localhost:5001
CUSTOMER_API_URL=http://localhost:5002
STAFF_API_URL=http://localhost:5003

# Test admin user
ADMIN_USERNAME=admin@example.com
ADMIN_PASSWORD=Admin@123!

# Test staff user
STAFF_USERNAME=staff@example.com
STAFF_PASSWORD=Staff@123!

# Test customer user
CUSTOMER_USERNAME=customer@example.com
CUSTOMER_PASSWORD=Customer@123!
```

## Running Tests

Run all tests:
```bash
npm test
```

Run only admin tests:
```bash
npm run test:admin
```

Run tests with UI:
```bash
npm run test:debug
```

View test reports:
```bash
npm run test:report
```

## Test Structure

The tests are organized as follows:

- `src/models/` - TypeScript interfaces for API models
- `src/utils/` - Utility classes for API interaction
- `src/tests/` - The actual test files
  - `admin/` - Tests for Admin API
  - `customer/` - Tests for Customer API
  - `staff/` - Tests for Staff API

## Troubleshooting

If you encounter errors regarding missing modules like 'dotenv', make sure to run the installation steps above.

For CI/CD environments, you may need to adjust the baseURL in `playwright.config.ts` to point to your API instances. 