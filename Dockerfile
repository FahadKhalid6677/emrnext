FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /app

# Copy project files and restore dependencies
COPY ["src/EMRNext.API/EMRNext.API.csproj", "src/EMRNext.API/"]
COPY ["src/EMRNext.Core/EMRNext.Core.csproj", "src/EMRNext.Core/"]
COPY ["src/EMRNext.Infrastructure/EMRNext.Infrastructure.csproj", "src/EMRNext.Infrastructure/"]
COPY ["src/EMRNext.Shared/EMRNext.Shared.csproj", "src/EMRNext.Shared/"]
COPY ["EMRNext.sln", "./"]

# Restore dependencies
RUN dotnet restore "src/EMRNext.API/EMRNext.API.csproj"

# Copy entire project
COPY . .

# Build the API project
WORKDIR "/app/src/EMRNext.API"
RUN dotnet build "EMRNext.API.csproj" -c Release -o /app/build
RUN dotnet publish "EMRNext.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "EMRNext.API.dll"]
