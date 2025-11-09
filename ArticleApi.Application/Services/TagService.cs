using ArticleApi.Application.DTOs;
using ArticleApi.Application.Validators;
using ArticleApi.Domain.Entities;
using ArticleApi.Domain.Interfaces;

namespace ArticleApi.Application.Services;

/// <summary>
/// Сервис Тэгов.
/// </summary>
public class TagService(ITagRepository tagRepository)
{
    /// <summary>
    /// Получить все Тэги.
    /// </summary>
    /// <param name="ct">токен отмены</param>
    /// <returns></returns>
    public async Task<IEnumerable<TagDto>> GetAllAsync(CancellationToken ct)
    {
        var tags = await tagRepository.GetAllAsync(ct);
        return tags.Select(t => new TagDto(t.Id, t.Name));
    }

    /// <summary>
    /// Получить Тэг по id.
    /// </summary>
    /// <param name="id">идентификатор тэга</param>
    /// <param name="ct">токен отмены</param>
    /// <returns></returns>
    public async Task<TagDto?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var tag = await tagRepository.GetByIdAsync(id, ct);
        return tag is null ? null : new TagDto(tag.Id, tag.Name);
    }

    /// <summary>
    /// Создать тэг
    /// </summary>
    /// <param name="dto">данные для создания</param>
    /// <param name="ct">токен отмены</param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task<TagDto> CreateAsync(CreateTagDto dto, CancellationToken ct)
    {
        // Валидация
        TagValidator.ValidateCreateDto(dto);

        //TODO: убрать в валидатор
        var normalizedName = dto.Name.Trim().ToLowerInvariant();
        if (await tagRepository.ExistsByNameAsync(normalizedName, ct))
            throw new InvalidOperationException("Тег с таким названием уже существует (регистронезависимо).");

        var tag = new Tag
        {
            Id = Guid.NewGuid(),
            Name = dto.Name.Trim()
        };

        var created = await tagRepository.CreateAsync(tag, ct);

        return new TagDto(created.Id, created.Name);
    }

    public async Task<TagDto> UpdateAsync(Guid id, UpdateTagDto dto, CancellationToken ct)
    {
        // Валидация
        TagValidator.ValidateUpdateDto(dto);

        var existingTag = await tagRepository.GetByIdAsync(id, ct) ?? throw new InvalidOperationException("Тег не найден.");
        var normalizedName = dto.Name.Trim().ToLowerInvariant();

        // Проверяем, не существует ли другой тег с таким же именем
        if (await tagRepository.ExistsByNameAsync(normalizedName, ct) &&
            existingTag.Name.ToLowerInvariant() != normalizedName)
        {
            throw new InvalidOperationException("Тег с таким названием уже существует (регистронезависимо).");
        }

        existingTag.Name = dto.Name.Trim();
        await tagRepository.UpdateAsync(existingTag, ct);

        return new TagDto(existingTag.Id, existingTag.Name);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct)
    {
        var exists = await tagRepository.GetByIdAsync(id, ct) is not null;
        if (!exists)
            throw new InvalidOperationException("Тег не найден.");

        await tagRepository.DeleteAsync(id, ct);
    }
}