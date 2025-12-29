using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using NewsPortal.Models;

namespace NewsPortal.Data;

/// <summary>
/// ADO.NET repository over SQLite (no EF).
/// </summary>
public sealed class SqliteNewsRepository : INewsRepository
{
    private readonly string _connectionString;

    public SqliteNewsRepository(IConfiguration config, IHostEnvironment env)
    {
        var relative = config["Database:File"] ?? "App_Data/news.db";
        var dbPath = Path.Combine(env.ContentRootPath, relative.Replace('/', Path.DirectorySeparatorChar));

        // Ensure folder exists even if repository is used before initializer (safe)
        var dir = Path.GetDirectoryName(dbPath);
        if (!string.IsNullOrWhiteSpace(dir))
            Directory.CreateDirectory(dir);

        _connectionString = new SqliteConnectionStringBuilder { DataSource = dbPath }.ToString();
    }

    private SqliteConnection Open()
    {
        var conn = new SqliteConnection(_connectionString);
        conn.Open();
        using (var pragma = conn.CreateCommand())
        {
            pragma.CommandText = "PRAGMA foreign_keys = ON;";
            pragma.ExecuteNonQuery();
        }
        return conn;
    }

    public IReadOnlyList<Category> GetCategories()
    {
        using var conn = Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT Id, Name, Slug FROM Categories ORDER BY Name;";
        using var r = cmd.ExecuteReader();

        var list = new List<Category>();
        while (r.Read())
        {
            list.Add(new Category
            {
                Id = r.GetInt32(0),
                Name = r.GetString(1),
                Slug = r.GetString(2),
            });
        }
        return list;
    }

    public Category? GetCategoryBySlug(string slug)
    {
        using var conn = Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT Id, Name, Slug FROM Categories WHERE Slug = @slug LIMIT 1;";
        cmd.Parameters.AddWithValue("@slug", slug);
        using var r = cmd.ExecuteReader();
        if (!r.Read()) return null;

        return new Category
        {
            Id = r.GetInt32(0),
            Name = r.GetString(1),
            Slug = r.GetString(2),
        };
    }

    public IReadOnlyList<NewsItem> GetLatest(int limit)
        => QueryNews("ORDER BY datetime(n.PublishedAt) DESC LIMIT @limit;", ("@limit", limit));

    public IReadOnlyList<NewsItem> GetFeaturedForHome(int limit)
        => QueryNews("WHERE n.IsFeatured = 1 ORDER BY datetime(n.PublishedAt) DESC LIMIT @limit;", ("@limit", limit));

    public IReadOnlyList<NewsItem> GetByCategorySlug(string slug, int limit)
        => QueryNews("WHERE c.Slug = @slug ORDER BY datetime(n.PublishedAt) DESC LIMIT @limit;",
            ("@slug", slug), ("@limit", limit));

    public NewsItem? GetById(int id)
    {
        var items = QueryNews("WHERE n.Id = @id LIMIT 1;", ("@id", id));
        return items.Count == 0 ? null : items[0];
    }

    private List<NewsItem> QueryNews(string whereOrderLimitSql, params (string Name, object Value)[] parameters)
    {
        using var conn = Open();
        using var cmd = conn.CreateCommand();

        cmd.CommandText = $@"
SELECT
  n.Id,
  n.CategoryId,
  c.Name,
  c.Slug,
  n.Title,
  n.Summary,
  n.Content,
  n.Color,
  n.PublishedAt,
  n.IsFeatured
FROM News n
JOIN Categories c ON c.Id = n.CategoryId
{whereOrderLimitSql}
";

        foreach (var p in parameters)
            cmd.Parameters.AddWithValue(p.Name, p.Value);

        using var r = cmd.ExecuteReader();
        var list = new List<NewsItem>();

        while (r.Read())
        {
            list.Add(new NewsItem
            {
                Id = r.GetInt32(0),
                CategoryId = r.GetInt32(1),
                CategoryName = r.GetString(2),
                CategorySlug = r.GetString(3),
                Title = r.GetString(4),
                Summary = r.GetString(5),
                Content = r.GetString(6),
                Color = r.GetString(7),
                PublishedAt = DateTime.TryParse(r.GetString(8), out var dt) ? dt : DateTime.UtcNow,
                IsFeatured = r.GetInt32(9) == 1
            });
        }

        return list;
    }

    public int Create(NewsItem item)
    {
        using var conn = Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
INSERT INTO News
(CategoryId, Title, Summary, Content, Color, PublishedAt, IsFeatured)
VALUES
(@catId, @title, @summary, @content, @color, @publishedAt, @isFeatured);
SELECT last_insert_rowid();
";
        cmd.Parameters.AddWithValue("@catId", item.CategoryId);
        cmd.Parameters.AddWithValue("@title", item.Title);
        cmd.Parameters.AddWithValue("@summary", item.Summary);
        cmd.Parameters.AddWithValue("@content", item.Content);
        cmd.Parameters.AddWithValue("@color", string.IsNullOrWhiteSpace(item.Color) ? "#2D6CDF" : item.Color);
        cmd.Parameters.AddWithValue("@publishedAt", item.PublishedAt.ToString("O"));
        cmd.Parameters.AddWithValue("@isFeatured", item.IsFeatured ? 1 : 0);

        var id = (long)cmd.ExecuteScalar()!;
        return (int)id;
    }

    public void Update(NewsItem item)
    {
        using var conn = Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
UPDATE News
SET
  CategoryId = @catId,
  Title = @title,
  Summary = @summary,
  Content = @content,
  Color = @color,
  PublishedAt = @publishedAt,
  IsFeatured = @isFeatured
WHERE Id = @id;
";
        cmd.Parameters.AddWithValue("@id", item.Id);
        cmd.Parameters.AddWithValue("@catId", item.CategoryId);
        cmd.Parameters.AddWithValue("@title", item.Title);
        cmd.Parameters.AddWithValue("@summary", item.Summary);
        cmd.Parameters.AddWithValue("@content", item.Content);
        cmd.Parameters.AddWithValue("@color", string.IsNullOrWhiteSpace(item.Color) ? "#2D6CDF" : item.Color);
        cmd.Parameters.AddWithValue("@publishedAt", item.PublishedAt.ToString("O"));
        cmd.Parameters.AddWithValue("@isFeatured", item.IsFeatured ? 1 : 0);

        cmd.ExecuteNonQuery();
    }

    public void Delete(int id)
    {
        using var conn = Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM News WHERE Id = @id;";
        cmd.Parameters.AddWithValue("@id", id);
        cmd.ExecuteNonQuery();
    }
}
