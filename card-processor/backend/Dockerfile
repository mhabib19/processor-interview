# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

# Copy solution and project files
COPY CardProcessor.sln ./
COPY src/CardProcessor.API/CardProcessor.API.csproj src/CardProcessor.API/
COPY src/CardProcessor.Core/CardProcessor.Core.csproj src/CardProcessor.Core/
COPY src/CardProcessor.Application/CardProcessor.Application.csproj src/CardProcessor.Application/
COPY src/CardProcessor.Infrastructure/CardProcessor.Infrastructure.csproj src/CardProcessor.Infrastructure/

# Restore dependencies for each project
RUN dotnet restore src/CardProcessor.Core/CardProcessor.Core.csproj
RUN dotnet restore src/CardProcessor.Infrastructure/CardProcessor.Infrastructure.csproj
RUN dotnet restore src/CardProcessor.Application/CardProcessor.Application.csproj
RUN dotnet restore src/CardProcessor.API/CardProcessor.API.csproj

# Copy source code
COPY src/ ./src/

# Build the application
RUN dotnet build src/CardProcessor.API/CardProcessor.API.csproj -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish src/CardProcessor.API/CardProcessor.API.csproj -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

WORKDIR /app

# Copy published application
COPY --from=publish /app/publish .

# Create data directories
RUN mkdir -p /app/data /app/test

# Set environment variables
ENV ASPNETCORE_URLS=http://+:5000

# Expose port
EXPOSE 5000

# Start the application
ENTRYPOINT ["dotnet", "CardProcessor.API.dll"]

