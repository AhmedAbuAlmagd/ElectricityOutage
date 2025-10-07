# STA Electricity Outage Management System

## Overview
This system manages electricity cutting down incidents, transferring data from Staging Tables Area (STA) to Fact Tables Area (FTA).

## Components Completed ✅

### 1. REST API Web Service (`STA.Electricity.API`)
- **Test Data Generation APIs:**
  - `POST /api/testdata/cabin-incidents` - Generate cabin incidents (Source A)
  - `POST /api/testdata/cable-incidents` - Generate cable incidents (Source B)
- **Sync API:**
  - `POST /api/sync/sync?source=A` - Run SP_Create and SP_Close

### 2. Performance Features
- Rate limiting (10 req/sec, 100 req/min, 1000 req/hour)
- Response compression (Brotli + Gzip)
- Fast JSON serialization
- SQL injection protection

### 3. Console Application (`STA.Electricity.ConsoleApp`)
- Runs forever with graceful shutdown
- Multithreading and parallel processing
- Calls REST APIs with rate limiting
- Executes stored procedures

### 4. Database Stored Procedures
- SP_Create - Transfer open incidents from STA to FTA
- SP_Close - Close incidents in FTA
- Hierarchy management procedures
- Customer impact calculation functions

## How to Run

### 1. Start the API
```bash
cd STA.Electricity.API
dotnet run
```
API will be available at: http://localhost:5000

### 2. Test the APIs
Use the provided `STA.Electricity.API.http` file to test endpoints:
- Generate cabin incidents with different scenarios
- Generate cable incidents with different scenarios
- Run sync operations

### 3. Run the Console Application
```bash
cd STA.Electricity.ConsoleApp
dotnet run
```

## API Endpoints

### Generate Test Data
```http
POST /api/testdata/cabin-incidents
Content-Type: application/json

{
  "count": 5,
  "scenario": "planned"  // planned, emergency, global, mixed
}
```

```http
POST /api/testdata/cable-incidents
Content-Type: application/json

{
  "count": 3,
  "scenario": "emergency"
}
```

### Sync Data
```http
POST /api/sync/sync?source=A
```

## Business Scenarios
- **Planned**: Scheduled maintenance with planned start/end times
- **Emergency**: Unplanned outages, some may be global
- **Global**: Wide-area outages affecting multiple regions
- **Mixed**: Random combination of all scenarios

## Next Steps
- [ ] Create Web Portal for incident management
- [ ] Add authentication and authorization
- [ ] Implement advanced search and filtering
- [ ] Add reporting and analytics

## Database Connection
Update connection string in `appsettings.json`:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=.;Database=ElectricityOutageDB;Trusted_Connection=true;TrustServerCertificate=true"
}
```
