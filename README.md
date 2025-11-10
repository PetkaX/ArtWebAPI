# Article API

Простой REST API для управления статьями, тегами и секциями. Построен на **ASP.NET Core 8** с использованием **чистой архитектуры**, **Entity Framework Core** и **xUnit-тестами**.

## 📦 Технологии

- **ASP.NET Core 8** — Web API
- **Entity Framework Core 8** — ORM, миграции
- **PostgreSQL** — основная БД (рекомендуется)
- **C# 12** — позиционные `record`, `Primary Constructors`, `Required` и т.д.
- **xUnit + Moq** — модульные тесты
- **Маппинг** — DTO

## 🗂️ Структура проекта
```
ArticleApi/ 
├── ArticleApi.Application/    # DTO, сервисы, интерфейсы 
├── ArticleApi.Domain/         # Сущности, Value Objects 
├── ArticleApi.Infrastructure/ # EF Core, репозитории, реализации 
├── ArticleApi.Api/            # Контроллеры, Startup, Program.cs 
├── ArticleApi.Tests.Unit/     # Юнит-тесты (xUnit, Moq) 
├── .gitignore 
├── README.md 
└── ArticleApi.sln
```

🔎 Структура решения с пояснениями:
| подпроект проект | пояснение |
|--------|--------|
|ArticleApi.Application/  | ← Бизнес-логика (Use Cases, DTOs, Interfaces) |
|ArticleApi.Domain/ | ← Модели предметной области (Entities, Interfaces)  |
|ArticleApi.Infrastructure/ | ← Реализация репозиториев, DbContext, конфигурация  |
|ArticleApi.Presentation/ | ← Web API (Controllers, Startup, Middleware)  |
|ArticleApi.Tests/  | ← Юнит/интеграционные тесты |

```
ArticleApi.Domain — Чистая доменная модель.
Содержит сущности и абстракции (интерфейсы репозиториев)

ArticleApi.Application — Логика приложения.
Содержит Use Cases, DTOs, валидацию. Зависит от Domain

ArticleApi.Infrastructure — Доступ к данным.
Реализует IArticleRepository с помощью Entity Framework Core + PostgreSQL.

ArticleApi.Presentation — ASP.NET Core Web API.
Точка входа Application.
```

## 🚀 Запуск проекта

### 1. Установка зависимостей

```
bash
dotnet restore
```

### 2. Настройка БД
Скопируйте appsettings.json и создайте appsettings.Development.json:
```
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=articleapi;Username=postgres;Password=yourpassword"
  }
}
```
Рекомендуется использовать PostgreSQL через Docker:
```
docker run -d --name pg-article -e POSTGRES_PASSWORD=yourpassword -p 5432:5432 postgres:16
```

### 3. Применение миграций
```
dotnet ef database update --project ArticleApi.Infrastructure --startup-project ArticleApi.Api
```

### 4. Запуск API
```
dotnet run --project  ArticleApi.Api
```
API будет доступно по адресу: https://localhost:5001 или http://localhost:5000

## 🧪 Запуск тестов

```bash
dotnet test
```
или
```
dotnet test ArticleApi.Tests
```

## 🧩 Основные сущности
| Сущность | Описание |
|--------|--------|
| `Article` | Статья с заголовком, содержанием, датой создания |
| `Tag` | Тег (например, "asp.net", "security") |
| `Section` | Секция (например, "Безопасность"), связана с тегами |
| Связь | `Article` ↔ `Tag` через `ArticleTag`, `Section` ↔ `Tag` через `SectionTag` |

> Статья попадает в секцию, если у неё есть хотя бы один общий тег.


## 🌐 API Endpoints

| Метод | Путь | Описание |
|------|------|--------|
| `GET` | `/api/articles` | Получить все статьи |
| `GET` | `/api/articles/{id}` | Получить статью по ID |
| `POST` | `/api/articles` | Создать статью |
| `PUT` | `/api/articles/{id}` | Обновить статью |
| `DELETE` | `/api/articles/{id}` | Удалить статью |
| `GET` | `/api/tags` | Получить все теги |
| `GET` | `/api/sections` | Получить все секции |
| `GET` | `/api/sections/{id}/articles` | Получить статьи секции |

## 📥 Пример создания статьи

```json
POST /api/articles
Content-Type: application/json

{
  "title": "Новая статья",
  "content": "Содержание...",
  "tagIds": ["a1b2c3d4-..."]
}
```

## ✅ Валидация

- Заголовок: 1–256 символов, не пустой
- Содержание: не пустое
- Теги: от 1 до 256, без дубликатов
- ID тегов должны существовать

## 🧰 Развёртывание (Docker)

Создайте `Dockerfile` в `src/ArticleApi.Api`:

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ArticleApi.Api.dll"]
```
Соберите и запустите:
```
docker build -t articleapi -f src/ArticleApi.Api/Dockerfile .
docker run -d -p 8080:80 --name my-articleapi articleapi
```

## 🧑‍💻 Автор

**Гришко Пётр Юрьевич** — разработчик API
> https://t.me/petkax

