namespace ArticleApi.Application.DTOs;
public record SectionDto(
    Guid Id,
    string Title,
    List<TagDto> Tags);