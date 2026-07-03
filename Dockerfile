# Stage 1: Build the application
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy the project files
COPY ["InternMS.Api/InternMS.Api.csproj", "InternMS.Api/"]
COPY ["InternMS.Domain/InternMS.Domain.csproj", "InternMS.Domain/"]
COPY ["InternMS.Infrastructure/InternMS.Infrastructure.csproj", "InternMS.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "InternMS.Api/InternMS.Api.csproj"

# Copy the remaining source code
COPY . .

# Build the application
RUN dotnet build "InternMS.Api/InternMS.Api.csproj" -c Release -o /app/build

# Publish the application
RUN dotnet publish "InternMS.Api/InternMS.Api.csproj" -c Release -o /app/publish

# Stage 2: Create the runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Install curl for healthcheck
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Copy from build stage
COPY --from=build /app/publish .

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=40s --retries=3 \
    CMD curl -f http://localhost:5248/health || exit 1

# Set environment (will be overridden by docker-compose)
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:5248

# Run the application
ENTRYPOINT ["dotnet", "InternMS.Api.dll"]
EXPOSE 5248