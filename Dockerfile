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

# Debug: Print out all .csproj files and their locations
RUN echo "Finding all .csproj files:" && find . -name "*.csproj"

# Restore dependencies for the solution
RUN dotnet restore "./EMRNext.sln" \
    --verbosity detailed \
    || (echo "Restore failed. Detailed information:" && \
        echo "Current directory:" && pwd && \
        echo "Directory contents:" && ls -la && \
        echo "Project files:" && find . -name "*.csproj")

# Set working directory to the API project
WORKDIR "/src/src/EMRNext.API"

# Debug: Verify current directory and project file
RUN pwd && ls -la && echo "Project file exists:" && ls -l EMRNext.API.csproj

# Build the project
RUN dotnet build "EMRNext.API.csproj" \
    -c Release \
    -o /app/build \
    --no-restore \
    || (echo "Build failed. Detailed information:" && \
        echo "Current directory:" && pwd && \
        echo "Project file contents:" && cat EMRNext.API.csproj)

FROM build AS publish
RUN dotnet publish "EMRNext.API.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EMRNext.API.dll"]
