FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

# Verbose debug logging
RUN pwd && ls -la && echo "Current directory contents:"

# Copy the entire project directory
COPY . .

# Verbose debug logging
RUN pwd && ls -la && echo "Checking nuget.config:" && find . -name nuget.config

# Restore dependencies with maximum verbosity and no cache
RUN dotnet restore "EMRNext.API/EMRNext.API.csproj" \
    --configfile "nuget.config" \
    --verbosity diagnostic \
    --no-cache \
    || (echo "Restore failed. Checking project files:" && cat "EMRNext.API/EMRNext.API.csproj")

WORKDIR "/src/EMRNext.API"
RUN dotnet build "EMRNext.API.csproj" \
    -c Release \
    -o /app/build \
    --verbosity diagnostic \
    || (echo "Build failed. Checking build logs." && exit 1)

FROM build AS publish
RUN dotnet publish "EMRNext.API.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore \
    --verbosity diagnostic \
    || (echo "Publish failed. Checking publish logs." && exit 1)

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EMRNext.API.dll"]
