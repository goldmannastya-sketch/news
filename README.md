# NewsPortal (ASP.NET Core MVC + SQLite + ADO.NET)

Это учебный проект под задание:
- Web-приложение (MVC)
- БД SQLite
- Доступ к БД через ADO.NET (Microsoft.Data.Sqlite), без Entity Framework
- Макет/внешний вид: «Новостной портал» с карточками и категориями

## Что внутри
- Главная: `/` (карточки «Главные новости дня» + список последних)
- Категории: `/category/{slug}` (politics/tech/sport/culture)
- Детальная страница: `/news/{id}`
- Admin CRUD: `/admin` (добавить/редактировать/удалить новости)

База данных создаётся автоматически при первом запуске (App_Data/news.db) и заполняется тестовыми данными.

## Запуск локально
1) Установи **.NET SDK 8.0**  
2) Открой терминал в папке проекта (где `NewsPortal.csproj`) и выполни:

```bash
dotnet restore
dotnet run
```

Открой в браузере адрес из консоли (обычно `http://localhost:5288` или `https://localhost:7288`).

## Деплой (вариант через publish)
Собери публикацию:

```bash
dotnet publish -c Release -o out
```

Дальше папку `out` можно заливать на любой хостинг под ASP.NET Core:
- IIS на Windows (shared hosting)
- VPS (Linux/Windows)
- PaaS (Render/Fly/Railway и т.п.) — часто через Docker

> Я не могу выложить проект на хостинг вместо тебя, но исходники и структура готовы: остаётся собрать/опубликовать.

## Где смотреть ADO.NET код
- `Data/SqliteNewsRepository.cs` — все SELECT/INSERT/UPDATE/DELETE через `SqliteConnection/SqliteCommand`.
- `Data/DbInitializer.cs` — создание схемы и сидинг данных.

