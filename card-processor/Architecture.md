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
- **Testing**: Bonus - unit and integration tests

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

4. more later?

### Known Limitations

1. **Data Persistence**: All data lost on application restart
2. **Memory Constraints**: Limited by available system memory since memory based persistance will be used.
3. **Basic Authentication**: JWT without advanced security
4. **File Size**: Limited by configurable memory constraints (default will be 10 MB)
5. more later?

### Assumptions

1. **Development Environment**: Local development and testing with Docker
2. more later?


***Need to add data model and API endpoints as I continue designing them***