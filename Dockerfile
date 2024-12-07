FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["src/EMRNext.API/EMRNext.API.csproj", "EMRNext.API/"]
COPY ["src/EMRNext.Core/EMRNext.Core.csproj", "EMRNext.Core/"]
COPY ["src/EMRNext.Infrastructure/EMRNext.Infrastructure.csproj", "EMRNext.Infrastructure/"]
RUN dotnet restore "EMRNext.API/EMRNext.API.csproj" /p:RestoreVerbosity=detailed
COPY . .
WORKDIR "/src/EMRNext.API"
RUN dotnet build "EMRNext.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "EMRNext.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EMRNext.API.dll"]
