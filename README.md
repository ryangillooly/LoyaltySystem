# Loyalty System

A flexible loyalty management system supporting both stamp-based and points-based loyalty programs.

## Project Overview

This system allows businesses to create and manage loyalty programs for their customers. It supports:

- **Stamp-based programs**: Customers collect stamps and redeem rewards when they reach a threshold
- **Points-based programs**: Customers earn points based on purchase amounts and redeem rewards with their points balance
- **Multi-brand support**: Manage loyalty programs across multiple brands and stores
- **Reward management**: Define and manage rewards with validity periods and redemption rules
- **POS integration**: Connect with point-of-sale systems for automatic loyalty tracking

## Architecture

The project follows a clean architecture approach with these key components:

- **Domain Layer**: Core business entities, aggregates, and business rules
- **Application Layer**: Application services, use cases, and domain events
- **Infrastructure Layer**: Data access, external services, and technical concerns
- **API Layer**: REST API endpoints for integration
- **Client Applications**:
  - Merchant Dashboard: Admin web application for managing loyalty programs
  - Staff Companion App: Mobile/tablet application for store staff
  - Customer Portal: Web/mobile application for customers

## Domain Model

The core domain model includes these key entities:

- **Brand**: Business entity that offers loyalty programs
- **Store**: Physical or online location belonging to a brand
- **LoyaltyProgram**: Rules for earning and redeeming loyalty
- **LoyaltyCard**: Customer's membership in a program
- **Transaction**: Record of loyalty activity
- **Reward**: Incentives offered to customers
- **Customer**: End-user with loyalty cards

## Getting Started

### Prerequisites

- .NET 8.0 SDK or later
- PostgreSQL 14 or later
- Node.js and npm (for front-end applications)
- PostGIS extension for PostgreSQL (for geo features)

### Development Setup

1. Clone the repository
   ```
   git clone https://github.com/yourusername/LoyaltySystem.git
   cd LoyaltySystem
   ```

2. Install PostgreSQL and set up the extensions
   ```bash
   # On macOS with Homebrew
   brew install postgresql
   brew install postgis
   
   # On Ubuntu/Debian
   sudo apt-get install postgresql postgresql-contrib postgis
   
   # On Windows
   # Download installer from https://www.postgresql.org/download/windows/
   # For PostGIS, use StackBuilder after installation
   ```

3. Set up the database
   ```bash
   # Create PostgreSQL database and run the creation script
   psql -U postgres -c "CREATE DATABASE loyaltysystem;"
   psql -U postgres -d loyaltysystem -f src/LoyaltySystem.Infrastructure/Data/Scripts/CreatePostgresDatabase.sql
   ```

4. Update connection string (if needed)
   ```
   # Edit the connection string in:
   src/LoyaltySystem.API/appsettings.json
   ```

5. Build and run the API
   ```
   cd src/LoyaltySystem.API
   dotnet build
   dotnet run
   ```

6. Access the API documentation
   ```
   # Open in your browser:
   https://localhost:5001/swagger
   ```

## API Endpoints

The API provides the following main endpoints:

- `/api/brands` - Manage brands and stores
- `/api/loyalty-programs` - Configure loyalty programs and rewards
- `/api/loyalty-cards` - Manage customer loyalty cards
- `/api/customers` - Manage customers and their loyalty memberships

### Key Operations

- **Stamp Issuance**: `POST /api/loyalty-cards/stamps`
- **Points Addition**: `POST /api/loyalty-cards/points`
- **Reward Redemption**: `POST /api/loyalty-cards/redeem`
- **Customer Enrollment**: `POST /api/customers/{customerId}/programs/{programId}/enroll`

## Data Model

The system uses PostgreSQL with these key features:
- **Enums**: For strong type enforcement of loyalty program types, transaction types, etc.
- **JSONB**: For flexible metadata storage
- **PostGIS**: For geospatial functionality with store locations
- **Partitioning**: For efficient handling of large transaction volumes
- **Materialized Views**: For analytics and reporting

Main tables include:
- brands, brand_contacts, brand_addresses
- stores, store_addresses, store_geo_locations, store_operating_hours
- customers
- loyalty_programs, program_expiration_policies
- rewards
- loyalty_cards
- transactions (partitioned by date)
- card_links (for POS integration)

## PostgreSQL Advantages

The system leverages PostgreSQL's advanced features:

1. **Spatial Queries**: Find nearby stores using PostGIS geography functions
2. **JSONB Data**: Flexible transaction metadata without schema changes
3. **Custom Data Types**: Used ENUMs for type-safe representation of status values
4. **Auto-updating Timestamps**: Triggers for automatic timestamp management
5. **Table Partitioning**: Transactions partitioned by date for performance
6. **Materialized Views**: Pre-computed summaries for analytics dashboards

## Project Status

This project is currently in the initial development phase. The core domain model and infrastructure have been implemented, and the API is being developed.

## License

[License information to be added]

## Contact

[Contact information to be added] 