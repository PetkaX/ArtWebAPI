using ArticleApi.Domain.Entities;

namespace ArticleApi.Domain.Interfaces;

public interface IArticleRepository
{
    Task<IEnumerable<Article>> GetAllAsync(CancellationToken ct);
    Task<Article?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<Article> CreateAsync(Article article, CancellationToken ct);
    Task UpdateAsync(Article article, CancellationToken ct);
    Task DeleteAsync(Guid id, CancellationToken ct);
    Task<bool> ExistsAsync(Guid id, CancellationToken ct);

    Task<List<Guid>> GetExistingTagIdsAsync(List<Guid> tagIds, CancellationToken ct);
}