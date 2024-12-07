FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

# Print current directory and list contents
RUN pwd && ls -la

# Copy entire source directory, preserving structure
COPY . .

# Verify file structure
RUN find . -name "EMRNext.API.csproj"

# Restore dependencies
RUN dotnet restore "./src/EMRNext.API/EMRNext.API.csproj" \
    --verbosity detailed \
    || (echo "Restore failed. Detailed information:" && \
        echo "Current directory:" && pwd && \
        echo "Directory contents:" && ls -la && \
        echo "Project file location:" && find . -name "EMRNext.API.csproj" && \
        echo "Project file contents:" && \
        cat ./src/EMRNext.API/EMRNext.API.csproj)

# Set working directory to the API project
WORKDIR "/src/EMRNext.API"

# Build the project
RUN dotnet build "EMRNext.API.csproj" \
    -c Release \
    -o /app/build \
    --no-restore \
    || (echo "Build failed. Detailed information:" && \
        echo "Current directory:" && pwd && \
        echo "Project file exists:" && ls -l EMRNext.API.csproj)

FROM build AS publish
RUN dotnet publish "EMRNext.API.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EMRNext.API.dll"]
