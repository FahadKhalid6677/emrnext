FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

# Print current directory and list contents
RUN pwd && ls -la

# Ensure solution file exists
COPY EMRNext.sln .

# Copy project files
COPY src/EMRNext.API/EMRNext.API.csproj src/EMRNext.API/
COPY src/EMRNext.Core/EMRNext.Core.csproj src/EMRNext.Core/
COPY src/EMRNext.Infrastructure/EMRNext.Infrastructure.csproj src/EMRNext.Infrastructure/

# Restore dependencies
RUN dotnet restore "EMRNext.sln" \
    --verbosity detailed \
    || (echo "Restore failed. Detailed information:" && \
        echo "Current directory contents:" && ls -la && \
        echo "Solution file contents:" && cat EMRNext.sln && \
        echo "Project file contents:" && \
        cat src/EMRNext.API/EMRNext.API.csproj && \
        cat src/EMRNext.Core/EMRNext.Core.csproj && \
        cat src/EMRNext.Infrastructure/EMRNext.Infrastructure.csproj)

# Copy the rest of the source code
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
