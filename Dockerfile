FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

# Debug: print current directory and list contents
RUN pwd && ls -la

# Explicitly copy nuget.config from the root of the project
COPY nuget.config /src/nuget.config

# Debug: verify nuget.config was copied
RUN ls -la /src

# Copy project files and restore dependencies
COPY ["src/EMRNext.API/EMRNext.API.csproj", "EMRNext.API/"]
COPY ["src/EMRNext.Core/EMRNext.Core.csproj", "EMRNext.Core/"]
COPY ["src/EMRNext.Infrastructure/EMRNext.Infrastructure.csproj", "EMRNext.Infrastructure/"]

# Restore dependencies with detailed logging and no cache
RUN dotnet restore "EMRNext.API/EMRNext.API.csproj" \
    --configfile "/src/nuget.config" \
    --verbosity detailed \
    --no-cache

# Copy entire project and build
COPY . .
WORKDIR "/src/EMRNext.API"
RUN dotnet build "EMRNext.API.csproj" \
    -c Release \
    -o /app/build \
    --no-restore

FROM build AS publish
RUN dotnet publish "EMRNext.API.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EMRNext.API.dll"]
