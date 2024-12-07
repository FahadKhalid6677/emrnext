# Use the official .NET 6.0 SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env
WORKDIR /app

# Copy csproj and restore dependencies
COPY *.sln ./
COPY src/EMRNext.API/*.csproj ./src/EMRNext.API/
COPY src/EMRNext.Core/*.csproj ./src/EMRNext.Core/
COPY src/EMRNext.Infrastructure/*.csproj ./src/EMRNext.Infrastructure/
COPY src/EMRNext.Shared/*.csproj ./src/EMRNext.Shared/

# Restore dependencies
RUN dotnet restore "src/EMRNext.API/EMRNext.API.csproj"

# Copy remaining files
COPY . ./

# Build and publish the application
WORKDIR /app/src/EMRNext.API
RUN dotnet publish -c Release -o /app/out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
COPY --from=build-env /app/out .

# Expose the port the app runs on
EXPOSE 80

# Set the entrypoint
ENTRYPOINT ["dotnet", "EMRNext.API.dll"]
