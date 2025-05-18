#!/bin/bash

# Install dependencies
npm install dotenv@16.3.1
npm install @playwright/test@1.41.2
npm install typescript@5.3.3
npm install @types/node@20.11.16

echo "Dependencies installed successfully!"
echo "Now you can run: npx playwright install"
echo "And then: npx playwright test" 