using ArticleApi.Domain.Entities;

namespace ArticleApi.Domain.Interfaces;

public interface ITagRepository
{
    Task<IEnumerable<Tag>> GetAllAsync(CancellationToken ct);
    Task<Tag?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<Tag> CreateAsync(Tag tag, CancellationToken ct);
    Task UpdateAsync(Tag tag, CancellationToken ct);
    Task DeleteAsync(Guid id, CancellationToken ct);

    // Проверка существования тега по имени (регистронезависимо)
    Task<bool> ExistsByNameAsync(string normalizedName, CancellationToken ct);

    // Получить теги по списку ID
    Task<List<Tag>> GetByIdsAsync(List<Guid> ids, CancellationToken ct);
}