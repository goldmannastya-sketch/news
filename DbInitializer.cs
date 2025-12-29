using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace NewsPortal.Data;

/// <summary>
/// Creates SQLite schema and inserts seed data (only when DB is empty).
/// </summary>
public sealed class DbInitializer
{
    private readonly string _dbPath;

    public DbInitializer(IConfiguration config, IHostEnvironment env)
    {
        var relative = config["Database:File"] ?? "App_Data/news.db";
        _dbPath = Path.Combine(env.ContentRootPath, relative.Replace('/', Path.DirectorySeparatorChar));
    }

    public void Initialize()
    {
        var dir = Path.GetDirectoryName(_dbPath);
        if (!string.IsNullOrWhiteSpace(dir))
            Directory.CreateDirectory(dir);

        using var conn = new SqliteConnection(new SqliteConnectionStringBuilder { DataSource = _dbPath }.ToString());
        conn.Open();

        using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = @"
PRAGMA foreign_keys = ON;

CREATE TABLE IF NOT EXISTS Categories (
  Id INTEGER PRIMARY KEY AUTOINCREMENT,
  Name TEXT NOT NULL,
  Slug TEXT NOT NULL UNIQUE
);

CREATE TABLE IF NOT EXISTS News (
  Id INTEGER PRIMARY KEY AUTOINCREMENT,
  CategoryId INTEGER NOT NULL,
  Title TEXT NOT NULL,
  Summary TEXT NOT NULL,
  Content TEXT NOT NULL,
  Color TEXT NOT NULL,
  PublishedAt TEXT NOT NULL,
  IsFeatured INTEGER NOT NULL DEFAULT 0,
  FOREIGN KEY(CategoryId) REFERENCES Categories(Id) ON DELETE RESTRICT
);
";
            cmd.ExecuteNonQuery();
        }

        // Seed only if empty
        long categoryCount;
        using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = "SELECT COUNT(*) FROM Categories;";
            categoryCount = (long)cmd.ExecuteScalar()!;
        }

        if (categoryCount == 0)
        {
            Seed(conn);
        }
    }

    private static void Seed(SqliteConnection conn)
    {
        // Fixed categories to match prototype navigation
        var categories = new (string Name, string Slug)[]
        {
            ("Политика", "politics"),
            ("Технологии", "tech"),
            ("Спорт", "sport"),
            ("Культура", "culture")
        };

        var catIds = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        using (var tx = conn.BeginTransaction())
        {
            foreach (var c in categories)
            {
                using var cmd = conn.CreateCommand();
                cmd.Transaction = tx;
                cmd.CommandText = "INSERT INTO Categories (Name, Slug) VALUES (@name, @slug); SELECT last_insert_rowid();";
                cmd.Parameters.AddWithValue("@name", c.Name);
                cmd.Parameters.AddWithValue("@slug", c.Slug);
                var id = (long)cmd.ExecuteScalar()!;
                catIds[c.Slug] = (int)id;
            }

            // News items (some featured) — texts are original (not from учебника/сокурсников)
            var now = DateTime.UtcNow;
            var news = new[]
            {
                new {
                    CategorySlug="politics",
                    Title="Саммит лидеров стран",
                    Summary="Ключевые вопросы обсуждены на международном саммите. Подробности повестки и первые итоги.",
                    Content="На встрече лидеров обсуждались вопросы экономического сотрудничества, безопасности и гуманитарных инициатив. Стороны договорились продолжить консультации и подготовить совместное заявление.",
                    Color="#E53935",
                    IsFeatured=true
                },
                new {
                    CategorySlug="politics",
                    Title="Новые законодательные инициативы",
                    Summary="В парламент внесены предложения по улучшению цифровых сервисов и поддержке регионов.",
                    Content="Инициативы касаются упрощения процедур для граждан и бизнеса, а также повышения прозрачности государственных услуг. Ожидается обсуждение профильными комитетами.",
                    Color="#1E40AF",
                    IsFeatured=true
                },
                new {
                    CategorySlug="tech",
                    Title="Искусственный интеллект в сервисах",
                    Summary="Компании ускоряют внедрение ИИ-решений в поддержку клиентов и аналитику.",
                    Content="Практика показывает, что автоматизация типовых запросов снижает нагрузку на операторов и улучшает SLA. Важно контролировать качество ответов и корректность данных.",
                    Color="#06B6D4",
                    IsFeatured=true
                },
                new {
                    CategorySlug="tech",
                    Title="Зелёная энергетика",
                    Summary="Новые разработки помогают повышать эффективность солнечных панелей и накопителей.",
                    Content="Исследователи сообщают о росте КПД и снижении себестоимости. В ближайшие годы ожидаются пилотные проекты в регионах с высокой инсоляцией.",
                    Color="#10B981",
                    IsFeatured=true
                },
                new {
                    CategorySlug="sport",
                    Title="Футбольный чемпионат",
                    Summary="Команды завершают подготовку к старту сезона. Тренеры делают ставку на молодёжь.",
                    Content="В предсезонных матчах заметны тактические эксперименты и ротация состава. Ожидается высокий интерес болельщиков к первым турами.",
                    Color="#16A34A",
                    IsFeatured=true
                },
                new {
                    CategorySlug="culture",
                    Title="Культурное событие года",
                    Summary="Открылась выставка современного искусства с участием авторов из разных стран.",
                    Content="Экспозиция объединяет инсталляции, живопись и цифровые работы. Организаторы подготовили образовательную программу и встречи с художниками.",
                    Color="#7C3AED",
                    IsFeatured=true
                },

                // Additional small cards
                new { CategorySlug="tech", Title="Квантовые компьютеры", Summary="Ученые представили новый прототип устойчивых кубитов.", Content="Детали прототипа показывают улучшение стабильности. Это приближает практические сценарии, но предстоит масштабирование.", Color="#334155", IsFeatured=false },
                new { CategorySlug="tech", Title="Биотехнологии", Summary="Стартапы предлагают быстрые тесты и персонализированную медицину.", Content="Технологии ускоряют диагностику. Важны стандарты качества и защита данных пациентов.", Color="#64748B", IsFeatured=false },
                new { CategorySlug="politics", Title="Выборы в регионах", Summary="Начался период предвыборной агитации, наблюдатели готовятся к мониторингу.", Content="Комиссии уточняют списки и логистику. Кандидаты активизировали встречи с избирателями.", Color="#475569", IsFeatured=false },
                new { CategorySlug="politics", Title="Бюджет депутaтов", Summary="Рассмотрены поправки к бюджету и социальным программам.", Content="Обсуждаются приоритеты расходов и поддержка инфраструктурных проектов. Решения будут уточняться ко второму чтению.", Color="#0F766E", IsFeatured=false },
                new { CategorySlug="culture", Title="Театральная премьера", Summary="Состоялась премьера нового спектакля в национальном театре.", Content="Постановка получила высокие оценки критиков за сценографию и игру актеров. Запланированы дополнительные показы.", Color="#4338CA", IsFeatured=false },
                new { CategorySlug="sport", Title="Олимпийская подготовка", Summary="Сборные усиливают тренировки и проходят медицинские обследования.", Content="Фокус на восстановлении и индивидуальных планах подготовки. Специалисты отмечают рост результатов по ряду дисциплин.", Color="#F97316", IsFeatured=false }
            };

            foreach (var n in news)
            {
                using var cmd = conn.CreateCommand();
                cmd.Transaction = tx;
                cmd.CommandText = @"
INSERT INTO News
(CategoryId, Title, Summary, Content, Color, PublishedAt, IsFeatured)
VALUES
(@catId, @title, @summary, @content, @color, @publishedAt, @isFeatured);
";
                cmd.Parameters.AddWithValue("@catId", catIds[n.CategorySlug]);
                cmd.Parameters.AddWithValue("@title", n.Title);
                cmd.Parameters.AddWithValue("@summary", n.Summary);
                cmd.Parameters.AddWithValue("@content", n.Content);
                cmd.Parameters.AddWithValue("@color", n.Color);
                cmd.Parameters.AddWithValue("@publishedAt", now.AddDays(-new Random(n.Title.GetHashCode()).Next(0, 25)).ToString("O"));
                cmd.Parameters.AddWithValue("@isFeatured", n.IsFeatured ? 1 : 0);
                cmd.ExecuteNonQuery();
            }

            tx.Commit();
        }
    }
}
