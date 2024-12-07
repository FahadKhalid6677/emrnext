# Use the official .NET 6.0 SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /app

# Copy solution and project files
COPY EMRNext.sln .
COPY src/EMRNext.API/EMRNext.API.csproj ./src/EMRNext.API/
COPY src/EMRNext.Core/EMRNext.Core.csproj ./src/EMRNext.Core/
COPY src/EMRNext.Infrastructure/EMRNext.Infrastructure.csproj ./src/EMRNext.Infrastructure/
COPY src/EMRNext.Shared/EMRNext.Shared.csproj ./src/EMRNext.Shared/

# Restore dependencies
RUN dotnet restore "./EMRNext.sln"

# Copy entire project contents
COPY . .

# Set working directory for build
WORKDIR /app/src/EMRNext.API

# Build and publish the project
RUN dotnet build "EMRNext.API.csproj" -c Release -o /app/build
RUN dotnet publish "EMRNext.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
COPY --from=build /app/publish .

# Expose the port the app runs on
EXPOSE 80

# Set the entrypoint
ENTRYPOINT ["dotnet", "EMRNext.API.dll"]
