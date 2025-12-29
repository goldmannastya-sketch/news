using NewsPortal.Models;

namespace NewsPortal.Data;

public interface INewsRepository
{
    IReadOnlyList<Category> GetCategories();
    Category? GetCategoryBySlug(string slug);

    IReadOnlyList<NewsItem> GetLatest(int limit);
    IReadOnlyList<NewsItem> GetFeaturedForHome(int limit);

    IReadOnlyList<NewsItem> GetByCategorySlug(string slug, int limit);
    NewsItem? GetById(int id);

    int Create(NewsItem item);
    void Update(NewsItem item);
    void Delete(int id);
}
