# Use the official .NET 7.0 SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src

# Copy solution file
COPY ["EMRNext.sln", "./"]

# Copy project files
COPY ["src/EMRNext.API/EMRNext.API.csproj", "EMRNext.API/"]
COPY ["src/EMRNext.Core/EMRNext.Core.csproj", "EMRNext.Core/"]
COPY ["src/EMRNext.Infrastructure/EMRNext.Infrastructure.csproj", "EMRNext.Infrastructure/"]
COPY ["src/EMRNext.Shared/EMRNext.Shared.csproj", "EMRNext.Shared/"]

# Restore dependencies
RUN dotnet restore "EMRNext.sln"

# Copy entire source code
COPY . .

# Set working directory for build
WORKDIR "/src/src/EMRNext.API"

# Build and publish the project
RUN dotnet build "EMRNext.API.csproj" -c Release -o /app/build
RUN dotnet publish "EMRNext.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /app
COPY --from=build /app/publish .

# Expose the port the app runs on
EXPOSE 80

# Set the entrypoint
ENTRYPOINT ["dotnet", "EMRNext.API.dll"]
