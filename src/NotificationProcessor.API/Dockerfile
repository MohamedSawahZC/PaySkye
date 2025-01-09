FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/NotificationProcessor.API/NotificationProcessor.API.csproj", "NotificationProcessor.API/"]
COPY ["src/NotificationService.Contracts/NotificationService.Contracts.csproj", "NotificationService.Contracts/"]
COPY ["src/NotificationService.API/NotificationService.API.csproj", "NotificationService.API/"]
RUN dotnet restore "NotificationProcessor.API/NotificationProcessor.API.csproj"
COPY src/ .
WORKDIR "/src/NotificationProcessor.API"
RUN dotnet build "NotificationProcessor.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "NotificationProcessor.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NotificationProcessor.API.dll"]