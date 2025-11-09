namespace ArticleApi.Application.DTOs;
public record CreateArticleDto(
    string Title,
    string Content,
    List<Guid> TagIds);