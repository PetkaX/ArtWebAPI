namespace ArticleApi.Application.DTOs;
public record ArticleDto(
    Guid Id,
    string Title,
    string Content,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    List<TagDto> Tags);