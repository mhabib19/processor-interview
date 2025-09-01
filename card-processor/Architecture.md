# Card Processor - Architecture Document

## System Overview

The Card Processor is a full-stack application that processes credit card transactions from multiple file formats (CSV, JSON, XML) and provides reporting capabilities. This document describes the current implementation.

## Specificaiton

- **Transaction Processing**: Accept and validate transactions from CSV, JSON, and XML files
- **Card Validation**: Validate card numbers and determine card types (Amex, Visa, MasterCard, Discover). Aditionally, I will use Luhn Algorithm to vlaidate real card number.
- **Data Persistence**: Store transactions in memory 
- **Reporting**: Generate summaries by card, card type, and day
- **Web Interface**: React-based UI with Tailwind CSS for data submission (file upload, file size will be configurable) and reporting
- **API Layer**: RESTful API with simple JWT authentication for transaction processing and reporting
- **Testing**: included unit and integration tests

## Architecture Design

### High-Level Architecture

```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   React UI      │    │   .NET Core     │    │   In-Memory     │
│   (Frontend)    │◄──►│   (Backend)     │◄──►│   Storage       │
│   + Tailwind    │    │   + JWT Auth    │    │   + Validation  │
└─────────────────┘    └─────────────────┘    └─────────────────┘
```

### Technology Stack

- **Frontend**: React 19.1.1 + TypeScript + Tailwind CSS + Axios
- **Backend**: .NET Core 8 + C# + JWT Authentication
- **Storage**: In-Memory Repository (no database)
- **Containerization**: Docker + Docker Compose
- **Testing**: XUnit + Moq 

### Backend - Clean Architecture Pattern
```
┌─────────────────────────────────────────────────────────────┐
│                    Presentation Layer                       │
│  ┌─────────────────┐  ┌─────────────────┐  ┌──────────────┐ │
│  │   Controllers   │  │   JWT Auth      │  │   CORS       │ │
│  └─────────────────┘  └─────────────────┘  └──────────────┘ │
└─────────────────────────────────────────────────────────────┘
┌─────────────────────────────────────────────────────────────┐
│                   Application Layer                         │
│  ┌─────────────────┐  ┌─────────────────┐  ┌──────────────┐ │
│  │   Services      │  │   DTOs          │  │   Validators │ │
│  └─────────────────┘  └─────────────────┘  └──────────────┘ │
└─────────────────────────────────────────────────────────────┘
┌─────────────────────────────────────────────────────────────┐
│                     Domain Layer                            │
│  ┌─────────────────┐  ┌─────────────────┐  ┌──────────────┐ │
│  │   Entities      │  │   Value Objects │  │   Interfaces │ │
│  └─────────────────┘  └─────────────────┘  └──────────────┘ │
└─────────────────────────────────────────────────────────────┘
┌─────────────────────────────────────────────────────────────┐
│                 Infrastructure Layer                        │
│  ┌─────────────────┐  ┌─────────────────┐  ┌──────────────┐ │
│  │   Repositories  │  │   File Parsers  │  │   JWT Service│ │
│  └─────────────────┘  └─────────────────┘  └──────────────┘ │
└─────────────────────────────────────────────────────────────┘
```
### Frontend Architecture - React Application Structure
- **State Management**: React useState and useEffect for local component state
- **HTTP Client**: Axios for API communication with JWT token handling
- **UI Framework**: Tailwind CSS with custom design system
- **Authentication**: JWT token management with automatic token refresh. Not planning to add user login/auth.

### Data Model

## Transaction Entity
```csharp
public class Transaction
{
    public Guid Id { get; set; }
    public string CardNumber { get; set; }
    public CardType CardType { get; set; }
    public decimal Amount { get; set; }
    public DateTime Timestamp { get; set; }
    public bool IsValid { get; set; }
    public string? RejectionReason { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
```
## Card Types
```csharp
public enum CardType
{
    Visa,
    MasterCard,
    AmericanExpress,
    Discover,
    Unknown
}

```
### API Endpoints
```
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/token` | Generate JWT token |
| POST | `/api/fileupload/upload` | Upload transaction file |
| POST | `/api/fileupload/process/{fileId}` | Process uploaded file |
| GET | `/api/fileupload/status/{fileId}` | Get processing status |
| DELETE | `/api/fileupload/{fileId}` | Delete uploaded file |
| GET | `/api/fileupload/max-size` | Get configurable file size limit |
| GET | `/api/transactions` | Get transactions with filtering |
| GET | `/api/transactions/{id}` | Get specific transaction by ID |
| GET | `/api/transactions/rejected` | Get rejected transactions |
| GET | `/api/report/by-card` | Report by card |
| GET | `/api/report/by-card-type` | Report by card type (requires cardType parameter) |
| GET | `/api/report/by-day` | Report by day (requires startDate and endDate parameters) |
| GET | `/api/report/rejected` | Rejected transactions report |
| GET | `/api/report/dashboard` | Dashboard statistics (optional dateRange parameter) |

```
### Authentication

The API uses JWT authentication. Get a token first:

```bash
curl -X GET http://localhost:5000/api/token
```

Use the returned token in subsequent requests:
```bash
curl -H "Authorization: Bearer <your-token>" http://localhost:5000/api/transactions
```
### File Upload Example

```bash
# Upload a file
curl -X POST \
  -H "Authorization: Bearer <your-token>" \
  -F "file=@transactions.csv" \
  -F "isRealData=false" \
  http://localhost:5000/api/fileupload/upload

# Process the uploaded file
curl -X POST \
  -H "Authorization: Bearer <your-token>" \
  http://localhost:5000/api/fileupload/process/{fileId}

# Check processing status
curl -X GET \
  -H "Authorization: Bearer <your-token>" \
  http://localhost:5000/api/fileupload/status/{fileId}

# Delete uploaded file
curl -X DELETE \
  -H "Authorization: Bearer <your-token>" \
  http://localhost:5000/api/fileupload/{fileId}

### Additional API Examples

```bash
# Get transactions with filtering
curl -X GET \
  -H "Authorization: Bearer <your-token>" \
  "http://localhost:5000/api/transactions?page=1&pageSize=10&cardType=Visa&isValid=true"

# Get report by card type
curl -X GET \
  -H "Authorization: Bearer <your-token>" \
  "http://localhost:5000/api/report/by-card-type?cardType=Visa&page=1&pageSize=20"

# Get report by date range
curl -X GET \
  -H "Authorization: Bearer <your-token>" \
  "http://localhost:5000/api/report/by-day?startDate=2024-01-01&endDate=2024-01-31&page=1&pageSize=20"

# Get dashboard statistics
curl -X GET \
  -H "Authorization: Bearer <your-token>" \
  "http://localhost:5000/api/report/dashboard?dateRange=7d"
```
###  Project Structure

```
card-processor/
├── backend/
│   ├── src/
│   │   ├── CardProcessor.API/          # Web API layer
│   │   ├── CardProcessor.Application/  # Application services
│   │   ├── CardProcessor.Core/         # Domain entities
│   │   └── CardProcessor.Infrastructure/ # Data access
│   ├── tests/                          # Unit and integration tests
│   ├── Dockerfile                      # Backend container
│   └── CardProcessor.sln               # Solution file
├── frontend/
│   ├── src/                            # React components
│   ├── public/                         # Static assets
│   ├── package.json                    # Dependencies
│   ├── Dockerfile                      # Frontend container
│   └── nginx.conf                      # Nginx configuration
├── docker-compose.yml                  # Multi-container setup
├── Architecture.md                     # Detailed architecture
└── README.md                           # how to build and run, and more
```

## Trade-offs & Decisions

### Architecture Decisions

1. **In-Memory vs Database Storage**
   - **Decision**: In-memory storage for simplicity and development speed
   - **Rationale**: Faster development
   - **Trade-off**: Data persistence and scalability limitations

2. **JWT Authentication Implementation**
   - **Decision**: Simple JWT authentication for development
   - **Rationale**: Secure API access while maintaining simplicity
   - **Trade-off**: Basic security without advanced security features

3. **Synchronous vs Asynchronous Processing**
   - **Decision**: Will be synchronous processing
   - **Rationale**: Will be simpler implementation
   - **Trade-off**: Will have limited scalability for large transaciton files


### Known Limitations

1. **Data Persistence**: All data lost on application restart
2. **Memory Constraints**: Limited by available system memory since memory based persistance will be used.
3. **Basic Authentication**: JWT without advanced security
4. **File Size**: Limited by configurable memory constraints (default will be 10 MB)

### Assumptions

1. **Development Environment**: Local development and testing with Docker


