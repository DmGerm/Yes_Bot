# Базовый runtime образ
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

# Создание и использование пользователя
RUN useradd -m appuser
USER appuser

# Билд и публикация приложения
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["DNS_YES_BOT.csproj", "DNS_YES_BOT/"]
RUN dotnet restore "DNS_YES_BOT/DNS_YES_BOT.csproj"

COPY . .
WORKDIR "/src/"
RUN dotnet build "DNS_YES_BOT.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "DNS_YES_BOT.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Финальный runtime образ
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
VOLUME ["/app/data"] 

# Копируем файл token.env в контейнер
COPY token.env /app/token.env  # Добавляем файл token.env в контейнер

VOLUME ["/app/data"] 

ENTRYPOINT ["dotnet", "DNS_YES_BOT.dll"]