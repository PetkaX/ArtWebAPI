using ArticleApi.Application.DTOs;

namespace ArticleApi.Application.Validators;

public static class ArticleValidator
{
    /// <summary>
    /// Валидация DTO для создания статьи.
    /// </summary>
    public static void ValidateCreateDto(CreateArticleDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        if (string.IsNullOrWhiteSpace(dto.Title))
            throw new ArgumentException("Название статьи обязательно.", nameof(dto.Title));

        if (dto.Title.Length > 256)
            throw new ArgumentException("Название статьи не может превышать 256 символов.", nameof(dto.Title));

        if (string.IsNullOrWhiteSpace(dto.Content))
            throw new ArgumentException("Содержимое статьи обязательно.", nameof(dto.Content));

        if (dto.TagIds is null)
            throw new ArgumentNullException(nameof(dto.TagIds));

        if (dto.TagIds.Count == 0)
            throw new ArgumentException("Статья должна иметь хотя бы один тег.", nameof(dto.TagIds));

        if (dto.TagIds.Count > 256)
            throw new ArgumentException("Максимум 256 тегов на статью.", nameof(dto.TagIds));

        if (dto.TagIds.Distinct().Count() != dto.TagIds.Count)
            throw new ArgumentException("Теги не должны дублироваться.", nameof(dto.TagIds));
    }

    /// <summary>
    /// Валидация DTO для обновления статьи.
    /// </summary>
    public static void ValidateUpdateDto(UpdateArticleDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        if (string.IsNullOrWhiteSpace(dto.Title))
            throw new ArgumentException("Название статьи обязательно.", nameof(dto.Title));

        if (dto.Title.Length > 256)
            throw new ArgumentException("Название статьи не может превышать 256 символов.", nameof(dto.Title));

        if (string.IsNullOrWhiteSpace(dto.Content))
            throw new ArgumentException("Содержимое статьи обязательно.", nameof(dto.Content));

        if (dto.TagIds is null)
            throw new ArgumentNullException(nameof(dto.TagIds));

        if (dto.TagIds.Count == 0)
            throw new ArgumentException("Статья должна иметь хотя бы один тег.", nameof(dto.TagIds));

        if (dto.TagIds.Count > 256)
            throw new ArgumentException("Максимум 256 тегов на статью.", nameof(dto.TagIds));

        if (dto.TagIds.Distinct().Count() != dto.TagIds.Count)
            throw new ArgumentException("Теги не должны дублироваться.", nameof(dto.TagIds));
    }
}
