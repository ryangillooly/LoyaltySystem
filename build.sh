#!/bin/bash

# Exit on error
set -e

echo "Building LoyaltySystem solution..."

# Restore dependencies
dotnet restore

# Build the solution
dotnet build --configuration Debug

echo "Build completed successfully."

# Optional: Run tests
# dotnet test

echo "The solution is ready to be opened in Rider." 