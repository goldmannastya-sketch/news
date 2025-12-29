using System.ComponentModel.DataAnnotations;

namespace NewsPortal.Models;

public sealed class NewsItem
{
    public int Id { get; set; }

    [Required]
    public int CategoryId { get; set; }

    public string CategoryName { get; set; } = "";
    public string CategorySlug { get; set; } = "";

    [Required, StringLength(120)]
    public string Title { get; set; } = "";

    [Required, StringLength(240)]
    public string Summary { get; set; } = "";

    [Required]
    public string Content { get; set; } = "";

    /// <summary>
    /// Accent color for the banner (hex like #FF0000)
    /// </summary>
    [Required, StringLength(16)]
    public string Color { get; set; } = "#2D6CDF";

    public DateTime PublishedAt { get; set; } = DateTime.UtcNow;

    public bool IsFeatured { get; set; }
}
