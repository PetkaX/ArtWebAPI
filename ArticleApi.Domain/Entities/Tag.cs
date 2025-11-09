using System.ComponentModel.DataAnnotations;

namespace ArticleApi.Domain.Entities;

/// <summary>
/// Тэг
/// </summary>
public class Tag
{
    public Guid Id { get; set; }

    [Required]
    [MaxLength(256)]
    public string Name { get; set; } = string.Empty;

    public List<ArticleTag> ArticleTags { get; set; } = [];
    public List<SectionTag> SectionTags { get; set; } = [];
}
