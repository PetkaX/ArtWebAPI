namespace ArticleApi.Application.DTOs;
public record CreateSectionDto(
    string Title,
    List<Guid> TagIds);