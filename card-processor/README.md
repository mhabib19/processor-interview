# Card Processor

## Architecture

For architecture detail see Architecture.md

## Prerequisites

### For Local Development
- **Backend**: .NET 8 SDK
- **Frontend**: Node.js 20+ and npm

## Quick Start with Docker

The easiest way to run the application is using Docker Compose:

```bash
# Clone the repository (if not already done)
git clone <repository-url>
cd card-processor

# Start all services
docker-compose up --build

# Or run in detached mode
docker-compose up -d --build

# to shut down app
docker-compose down

# to shut down app, and delete created containers and images all at once
docker-compose down --rmi all
```

The application will be available at:
- **Frontend**: http://localhost:3000
- **Backend API**: http://localhost:5000
- **API Documentation**: http://localhost:5000/swagger

## Local Development Setup

### Backend Setup

1. **Navigate to backend directory**:
   ```bash
   cd backend
   ```

2. **Restore dependencies**:
   ```bash
   dotnet restore
   ```

3. **Set environment variables** (create a `.env` file or set them in your shell):
   ```bash
   export JWT_SECRET_KEY="your-super-secret-key-with-at-least-32-characters-for-jwt-signing"
   export JWT_ISSUER="CardProcessor"
   export JWT_AUDIENCE="CardProcessorAPI"
   export JWT_EXPIRATION_HOURS=24
   ```

4. **Run the backend**:
   ```bash
   dotnet run --project src/CardProcessor.API
   ```

The backend will start on http://localhost:5000

### Frontend Setup

1. **Navigate to frontend directory**:
   ```bash
   cd frontend
   ```

2. **Install dependencies**:
   ```bash
   npm install
   ```

3. **Set the API URL** (create a `.env` file):
   ```bash
   echo "REACT_APP_API_URL=http://localhost:5000/api" > .env
   ```

4. **Start the development server**:
   ```bash
   npm start
   ```

The frontend will start on http://localhost:3000

## Testing

### Backend Tests
```bash
cd backend
dotnet test
```

### Frontend Tests
```bash
cd frontend
npm test
```

## Configuration

### Environment Variables

**Backend**:
- `JWT_SECRET_KEY`: Secret key for JWT signing (min 32 characters)
- `JWT_ISSUER`: JWT issuer claim
- `JWT_AUDIENCE`: JWT audience claim
- `JWT_EXPIRATION_HOURS`: Token expiration time in hours

**Frontend**:
- `REACT_APP_API_URL`: Backend API URL

### File Size Limits

The application supports configurable file size limits. Configuration files for backend are in CardProcessor.API/appsettings.json.

"MaxFileSizeMB": 25

Check the current limit:
```bash
curl http://localhost:5000/api/fileupload/max-size
```

## Troubleshooting

### Common Issues

1. **Port conflicts**: Ensure ports 3000 and 5000 are available
2. **JWT token issues**: Verify the secret key is at least 32 characters
3. **File upload failures**: Check file size limits and format
4. **CORS errors**: Ensure the frontend is configured to connect to the correct backend URL

### Docker Issues

```bash
# Clean up containers and volumes
docker-compose down -v

# Rebuild without cache
docker-compose build --no-cache

# Check logs
docker-compose logs backend
docker-compose logs frontend
```
