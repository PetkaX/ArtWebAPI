using System.ComponentModel.DataAnnotations;

namespace ArticleApi.Domain.Entities;

/// <summary>
/// Раздел
/// </summary>
public class Section
{
    public Guid Id { get; set; }

    [Required]
    [MaxLength(1024)]
    public string Title { get; set; } = string.Empty;

    public List<SectionTag> SectionTags { get; set; } = [];
}