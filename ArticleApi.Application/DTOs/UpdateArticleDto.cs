namespace ArticleApi.Application.DTOs;
public record UpdateArticleDto(
    string Title,
    string Content,
    List<Guid> TagIds);