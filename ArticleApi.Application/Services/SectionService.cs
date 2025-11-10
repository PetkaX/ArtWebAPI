using ArticleApi.Application.DTOs;
using ArticleApi.Domain.Entities;
using ArticleApi.Domain.Interfaces;

namespace ArticleApi.Application.Services;

/// <summary>
/// Сервис Разделов.
/// </summary>
public class SectionService(ISectionRepository sectionRepository)
{
    /// <summary>
    /// Получить Раздел по id.
    /// </summary>
    /// <param name="id">идентификатор раздела</param>
    /// <param name="ct">токен отмены</param>
    /// <returns></returns>
    public async Task<SectionDto?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var section = await sectionRepository.GetByIdAsync(id, ct);
        if (section is null) return null;

        return new SectionDto(
            section.Id,
            section.Title,
            [.. section.SectionTags.OrderBy(st => st.Order).Select(st => new TagDto(st.Tag.Id, st.Tag.Name))]);
    }

    /// <summary>
    /// Получить все разделы.
    /// </summary>
    /// <param name="ct">токен отмены</param>
    /// <returns></returns>
    public async Task<IEnumerable<SectionDto>> GetAllAsync(CancellationToken ct)
    {
        var sections = await sectionRepository.GetAllAsync(ct);
        return sections.Select(s => new SectionDto(
            s.Id,
            s.Title,
            [.. s.SectionTags.OrderBy(st => st.Order).Select(st => new TagDto(st.Tag.Id, st.Tag.Name))]));
    }

    /// <summary>
    /// Создать Раздел.
    /// </summary>
    /// <param name="dto">данные для создания</param>
    /// <param name="ct">токен отмены</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public async Task<SectionDto> CreateAsync(CreateSectionDto dto, CancellationToken ct)
    {
        if (dto.TagIds.Count > 256)
            throw new ArgumentException("Максимум 256 тегов");

        var tagIds = dto.TagIds.Distinct().ToList();
        if (tagIds.Count != dto.TagIds.Count)
            throw new ArgumentException("Дубликаты тегов не допускаются");

        // Предполагаем, что валидация тегов будет внутри репозитория или отдельным сервисом
        var section = new Section
        {
            Id = Guid.NewGuid(),
            Title = dto.Title,
            SectionTags = [.. tagIds.Select((id, index) => new SectionTag
            {
                TagId = id,
                Order = index
            })]
        };

        var created = await sectionRepository.CreateAsync(section, ct);

        return new SectionDto(
            created.Id,
            created.Title,
            [.. created.SectionTags.OrderBy(st => st.Order).Select(st => new TagDto(st.Tag.Id, st.Tag.Name))]);
    }

    /// <summary>
    /// Удалить Раздел по id.
    /// </summary>
    /// <param name="id">идентификатор Раздела</param>
    /// <param name="ct">токен отмены</param>
    /// <returns></returns>
    public async Task DeleteAsync(Guid id, CancellationToken ct)
    {
        await sectionRepository.DeleteAsync(id, ct);
    }


    /// <summary>
    /// Получить список статей по заданному разделу.
    /// </summary>
    /// <param name="sectionId">идентификатор раздела</param>
    /// <param name="ct">токен отмены</param>
    /// <returns></returns>
    public async Task<IEnumerable<ArticleDto>> GetArticlesBySectionAsync(Guid sectionId, CancellationToken ct)
    {
        var articles = await sectionRepository.GetArticlesBySectionAsync(sectionId, ct);
        return [.. articles.Select(a => new ArticleDto(
            a.Id,
            a.Title,
            a.Content,
            a.CreatedAt,
            a.UpdatedAt,
            [.. a.ArticleTags.OrderBy(at => at.Order).Select(at => new TagDto(at.Tag.Id, at.Tag.Name))]
        ))];
    }
    public async Task AutoCreateSectionsAsync(CancellationToken ct)
    {
        await sectionRepository.AutoCreateSectionsAsync(ct);
    }
}