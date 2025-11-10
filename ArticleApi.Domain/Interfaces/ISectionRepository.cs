using ArticleApi.Domain.Entities;

namespace ArticleApi.Domain.Interfaces;

public interface ISectionRepository
{
    Task<Section?> GetByIdAsync(Guid sectionId, CancellationToken ct);
    Task<IEnumerable<Section>> GetAllAsync(CancellationToken ct);
    Task<Section> CreateAsync(Section section, CancellationToken ct);
    Task DeleteAsync(Guid sectionId, CancellationToken ct);

    Task<List<Article>> GetArticlesBySectionAsync(Guid sectionId, CancellationToken ct);
    Task AutoCreateSectionsAsync(CancellationToken ct);
}