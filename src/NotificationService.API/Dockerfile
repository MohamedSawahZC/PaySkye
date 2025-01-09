FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/NotificationService.API/NotificationService.API.csproj", "NotificationService.API/"]
COPY ["src/NotificationService.Contracts/NotificationService.Contracts.csproj", "NotificationService.Contracts/"]
RUN dotnet restore "NotificationService.API/NotificationService.API.csproj"
COPY src/ .
WORKDIR "/src/NotificationService.API"
RUN dotnet build "NotificationService.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "NotificationService.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NotificationService.API.dll"]