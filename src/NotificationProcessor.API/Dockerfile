FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/NotificationProcessor.API/NotificationProcessor.API.csproj", "NotificationProcessor.API/"]
COPY ["src/NotificationService.Contracts/NotificationService.Contracts.csproj", "NotificationService.Contracts/"]
COPY ["src/NotificationService.API/NotificationService.API.csproj", "NotificationService.API/"]
RUN dotnet restore "./NotificationProcessor.API/./NotificationProcessor.API.csproj"
COPY src/ .
WORKDIR "/src/NotificationProcessor.API"
RUN dotnet build "./NotificationProcessor.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./NotificationProcessor.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NotificationProcessor.API.dll"]