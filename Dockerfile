# ==== Сборка (Build) ====
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Копируем sln и csproj-файлы для восстановления зависимостей
COPY ["ArticleApi.sln", "./"]
COPY ["ArticleApi.Presentation/ArticleApi.Presentation.csproj", "ArticleApi.Presentation/"]
COPY ["ArticleApi.Application/ArticleApi.Application.csproj", "ArticleApi.Application/"]
COPY ["ArticleApi.Domain/ArticleApi.Domain.csproj", "ArticleApi.Domain/"]
COPY ["ArticleApi.Infrastructure/ArticleApi.Infrastructure.csproj", "ArticleApi.Infrastructure/"]
COPY ["ArticleApi.Tests/ArticleApi.Tests.csproj", "ArticleApi.Tests/"]

# Восстанавливаем зависимости
RUN dotnet restore

# Копируем всё остальное
COPY . .

# Собираем и публикуем API
RUN dotnet publish "ArticleApi.Presentation/ArticleApi.Presentation.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore

# ==== Финальный образ ====
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Устанавливаем локаль (опционально, для логов)
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
ENV LC_ALL=en_US.UTF-8
ENV LANG=en_US.UTF-8

# Открываем порты
EXPOSE 80
EXPOSE 443

# Запуск приложения
ENTRYPOINT ["dotnet", "ArticleApi.Presentation.dll"]