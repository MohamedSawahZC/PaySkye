FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/NotificationProcessor.Worker/NotificationProcessor.Worker.csproj", "NotificationProcessor.Worker/"]
COPY ["src/NotificationService.Contracts/NotificationService.Contracts.csproj", "NotificationService.Contracts/"]
COPY ["src/NotificationService.API/NotificationService.API.csproj", "NotificationService.API/"]
RUN dotnet restore "NotificationProcessor.Worker/NotificationProcessor.Worker.csproj"
COPY src/ .
WORKDIR "/src/NotificationProcessor.Worker"
RUN dotnet build "NotificationProcessor.Worker.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "NotificationProcessor.Worker.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NotificationProcessor.Worker.dll"]