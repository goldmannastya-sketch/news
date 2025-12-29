using NewsPortal.Models;

namespace NewsPortal.ViewModels;

public sealed class CategoryPageViewModel
{
    public Category Category { get; init; } = new();
    public IReadOnlyList<NewsItem> Items { get; init; } = Array.Empty<NewsItem>();
}
