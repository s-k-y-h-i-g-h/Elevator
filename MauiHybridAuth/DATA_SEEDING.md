# Data Seeding Guide

This document explains how to use the data seeding functionality in the MauiHybridAuth application.

## Overview

The application includes a comprehensive data seeding system that creates test users, compounds, and intervention ratings for development and testing purposes.

## What Gets Seeded

### Test Users
- **john.doe@test.com** (password: Test123!)
- **jane.smith@test.com** (password: Test123!)
- **bob.wilson@test.com** (password: Test123!)
- **alice.johnson@test.com** (password: Test123!)
- **charlie.brown@test.com** (password: Test123!)

### Compounds
10 pharmaceutical compounds with descriptions:
- Aspirin, Ibuprofen, Acetaminophen, Lisinopril, Metformin
- Atorvastatin, Omeprazole, Amlodipine, Losartan, Sertraline

### Intervention Ratings
- Each compound gets 2-4 random ratings from different users
- Ratings are on a scale of 1-5
- Ratings are randomly assigned to create realistic test data

## Automatic Seeding

Data is automatically seeded when the application starts in **Development** mode. The seeding process:

1. Creates test users if they don't exist
2. Adds compounds if they don't exist
3. Creates random ratings linking users to compounds
4. Logs all operations to the console

## Manual Seeding (Development Only)

### API Endpoints

The following endpoints are available in Development mode:

- **POST** `/api/dev/seed` - Manually trigger data seeding
- **POST** `/api/dev/clear` - Clear all test data
- **GET** `/api/dev/status` - Show current data status (check console logs)

### Example Usage

```bash
# Seed data
curl -X POST https://localhost:7001/api/dev/seed

# Clear data
curl -X POST https://localhost:7001/api/dev/clear

# Check status
curl -X GET https://localhost:7001/api/dev/status
```

## Viewing Seeded Data

### Web Interface
1. Navigate to `/compounds` in the web application
2. You'll see all compounds with their average ratings and rating counts
3. Each compound card shows recent individual ratings

### API Endpoints
- **GET** `/api/compounds` - Get all compounds with ratings (requires authentication)
- **GET** `/api/compounds/{id}/ratings` - Get ratings for a specific compound (requires authentication)

## Authentication

To access the seeded data:
1. Register a new account or use one of the test accounts
2. Log in to the application
3. Navigate to the Compounds page or use the API endpoints

## Development Workflow

1. **First Run**: Data is automatically seeded on first startup
2. **Testing**: Use the test accounts to log in and view data
3. **Reset**: Use `/api/dev/clear` to reset data, then `/api/dev/seed` to reseed
4. **Customization**: Modify `DataSeeder.cs` to add more test data or change existing data

## File Structure

- `Services/DataSeeder.cs` - Main seeding logic
- `Services/DataSeederCommands.cs` - Command-line operations
- `Program.cs` - Service registration and automatic seeding
- `Pages/Compounds.razor` - Web interface for viewing data

## Customization

To add more test data or modify existing data:

1. Edit the `SeedUsersAsync()` method to add more users
2. Edit the `SeedCompoundsAsync()` method to add more compounds
3. Modify the `SeedInterventionRatingsAsync()` method to change rating logic
4. Restart the application or use the manual seeding endpoints

## Notes

- Seeding only occurs in Development environment
- Test data is safe to delete and recreate
- All operations are logged for debugging
- The system prevents duplicate data creation 