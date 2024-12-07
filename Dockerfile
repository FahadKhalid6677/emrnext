FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /app
COPY . .

# Restore dependencies for all projects
RUN dotnet restore "EMRNext.sln"

# Build and publish the API project
WORKDIR "/app/src/EMRNext.API"
RUN dotnet build "EMRNext.API.csproj" -c Release -o /app/build
RUN dotnet publish "EMRNext.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "EMRNext.API.dll"]
