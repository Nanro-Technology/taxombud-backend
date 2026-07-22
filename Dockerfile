# Use the official .NET SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Copy csproj files and restore dependencies to cache them
COPY src/TaxOmbud.API/TaxOmbud.API.csproj src/TaxOmbud.API/
COPY src/TaxOmbud.Application/TaxOmbud.Application.csproj src/TaxOmbud.Application/
COPY src/TaxOmbud.Common/TaxOmbud.Common.csproj src/TaxOmbud.Common/
COPY src/TaxOmbud.Domain/TaxOmbud.Domain.csproj src/TaxOmbud.Domain/
COPY src/TaxOmbud.Infrastructure/TaxOmbud.Infrastructure.csproj src/TaxOmbud.Infrastructure/
COPY src/TaxOmbud.Persistence/TaxOmbud.Persistence.csproj src/TaxOmbud.Persistence/
COPY Directory.Build.props ./

RUN dotnet restore src/TaxOmbud.API/TaxOmbud.API.csproj

# Copy the rest of the application code
COPY src/ src/

# Build and publish the API in release mode
RUN dotnet publish src/TaxOmbud.API/TaxOmbud.API.csproj -c Release -o /app/publish /p:UseAppHost=false

# Use the runtime-only image for deployment
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Create directories for logs and user uploads
RUN mkdir -p logs uploads

# Configure environment variables
ENV ASPNETCORE_URLS=http://+:5000
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 5000

# Set entry point
ENTRYPOINT ["dotnet", "TaxOmbud.API.dll"]
