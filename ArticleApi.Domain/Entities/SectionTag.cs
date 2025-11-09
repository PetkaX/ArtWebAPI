namespace ArticleApi.Domain.Entities;

/// <summary>
/// Связь для упорядоченных тегов у разделов
/// </summary>
public class SectionTag
{
    public Guid SectionId { get; set; }
    public Section Section { get; set; } = null!;

    public Guid TagId { get; set; }
    public Tag Tag { get; set; } = null!;

    public int Order { get; set; }
}