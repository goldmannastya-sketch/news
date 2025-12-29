using NewsPortal.Models;

namespace NewsPortal.ViewModels;

public sealed class HomePageViewModel
{
    public IReadOnlyList<NewsItem> MainCards { get; init; } = Array.Empty<NewsItem>();
    public IReadOnlyList<NewsItem> Latest { get; init; } = Array.Empty<NewsItem>();
}
