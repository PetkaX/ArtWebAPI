using ArticleApi.Application.DTOs;

namespace ArticleApi.Application.Validators;

public static class TagValidator
{
    /// <summary>
    /// Валидация DTO для создания Тэга.
    /// </summary>
    public static void ValidateCreateDto(CreateTagDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new ArgumentNullException("Название тега обязательно.", nameof(dto.Name));

        if (dto.Name.Length > 256)
            throw new ArgumentException("Название тега не может превышать 256 символов.", nameof(dto.Name));
    }

    /// <summary>
    /// Валидация DTO для обновления Тэга.
    /// </summary>
    public static void ValidateUpdateDto(UpdateTagDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new ArgumentException("Название тега обязательно.", nameof(dto.Name));

        if (dto.Name.Length > 256)
            throw new ArgumentException("Название тега не может превышать 256 символов.", nameof(dto.Name));
    }
}
