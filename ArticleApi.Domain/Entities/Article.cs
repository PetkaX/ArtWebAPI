using System.ComponentModel.DataAnnotations;

namespace ArticleApi.Domain.Entities;

/// <summary>
/// Статья
/// </summary>
public class Article
{
    public Guid Id { get; set; }

    [Required]
    [MaxLength(256)]
    public string Title { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public List<ArticleTag> ArticleTags { get; set; } = [];
}