namespace ArticleApi.Domain.Entities;

/// <summary>
/// Связь для упорядоченных тегов у статей
/// </summary>
public class ArticleTag
{
    public Guid ArticleId { get; set; }
    public Article Article { get; set; } = null!;

    public Guid TagId { get; set; }
    public Tag Tag { get; set; } = null!;

    public int Order { get; set; }
}