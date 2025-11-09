using ArticleApi.Application.DTOs;
using ArticleApi.Domain.Entities;

namespace ArticleApi.Application.Mappers;

/// <summary>
/// Маппинги для сущности Статьи
/// </summary>
public static class ArticleMapper
{
    /// <summary>
    /// Article -> ArticleDto.
    /// </summary>
    public static ArticleDto MapToDto(Article article)
    {
        return new ArticleDto(
            article.Id,
            article.Title,
            article.Content,
            article.CreatedAt,
            article.UpdatedAt,
            [.. article.ArticleTags
                .Where(at => at.Tag != null)
                .OrderBy(at => at.Order)
                .Select(at => new TagDto(at.Tag.Id, at.Tag.Name))]
        );
    }
}
