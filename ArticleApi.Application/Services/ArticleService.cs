using ArticleApi.Application.DTOs;
using ArticleApi.Application.Mappers;
using ArticleApi.Application.Validators;
using ArticleApi.Domain.Entities;
using ArticleApi.Domain.Interfaces;

namespace ArticleApi.Application.Services;

/// <summary>
/// Сервис Статей.
/// </summary>
public class ArticleService(IArticleRepository repository)
{
    /// <summary>
    /// Получить все статьи.
    /// </summary>
    public async Task<IEnumerable<ArticleDto>> GetAllAsync(CancellationToken ct)
    {
        var articles = await repository.GetAllAsync(ct);
        return articles.Select(ArticleMapper.MapToDto);
    }

    /// <summary>
    /// Получить статью по ID.
    /// </summary>
    public async Task<ArticleDto?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var article = await repository.GetByIdAsync(id, ct);
        return article is null ? null : ArticleMapper.MapToDto(article);
    }

    /// <summary>
    /// Создать новую статью.
    /// </summary>
    public async Task<ArticleDto> CreateAsync(CreateArticleDto dto, CancellationToken ct)
    {
        ArticleValidator.ValidateCreateDto(dto);

        // Проверить существование тегов
        var tagIds = dto.TagIds.Distinct().ToList();
        var existingTagIds = await repository.GetExistingTagIdsAsync(tagIds, ct);
        if (existingTagIds.Count != tagIds.Count)
        {
            var missing = string.Join(", ", tagIds.Except(existingTagIds));
            throw new ArgumentException($"Теги не найдены: {missing}");
        }

        var article = new Article
        {
            Id = Guid.NewGuid(),
            Title = dto.Title.Trim(),
            Content = dto.Content,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = null,
            ArticleTags = [.. tagIds.Select((tagId, index) => new ArticleTag
            {
                TagId = tagId,
                Order = index
            })]
        };

        var created = await repository.CreateAsync(article, ct);
        return ArticleMapper.MapToDto(created);
    }

    /// <summary>
    /// Обновить статью.
    /// </summary>
    public async Task<ArticleDto> UpdateAsync(Guid id, UpdateArticleDto dto, CancellationToken ct)
    {
        ArticleValidator.ValidateUpdateDto(dto);

        var existing = await repository.GetByIdAsync(id, ct) ?? throw new InvalidOperationException("Статья не найдена.");

        // Проверить существование тегов
        var tagIds = dto.TagIds.Distinct().ToList();
        var existingTagIds = await repository.GetExistingTagIdsAsync(tagIds, ct);
        if (existingTagIds.Count != tagIds.Count)
        {
            var missing = string.Join(", ", tagIds.Except(existingTagIds));
            throw new ArgumentException($"Теги не найдены: {missing}");
        }

        existing.Title = dto.Title.Trim();
        existing.Content = dto.Content;
        existing.UpdatedAt = DateTime.UtcNow;

        existing.ArticleTags.Clear();
        existing.ArticleTags.AddRange(tagIds.Select((tagId, index) => new ArticleTag
        {
            TagId = tagId,
            Order = index,
            ArticleId = id
        }));

        await repository.UpdateAsync(existing, ct);
        var updatedArticle = await repository.GetByIdAsync(id, ct) ?? throw new Exception("Ошибка обновления данных в БД.");
        return ArticleMapper.MapToDto(updatedArticle);
    }

    /// <summary>
    /// Удалить статью по ID.
    /// </summary>
    public async Task DeleteAsync(Guid id, CancellationToken ct)
    {
        var exists = await repository.ExistsAsync(id, ct);
        if (!exists)
            throw new InvalidOperationException("Статья не найдена.");

        await repository.DeleteAsync(id, ct);
    }
}