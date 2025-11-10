using ArticleApi.Domain.Entities;
using ArticleApi.Domain.Interfaces;
using ArticleApi.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ArticleApi.Infrastructure.Repositories;

public class ArticleRepository(ArticleDbContext context) : IArticleRepository
{
    public async Task<IEnumerable<Article>> GetAllAsync(CancellationToken ct) =>
        await context.Articles
            .Include(a => a.ArticleTags)
                .ThenInclude(at => at.Tag)
            .ToListAsync(ct);

    public async Task<Article?> GetByIdAsync(Guid id, CancellationToken ct) =>
        await context.Articles
            .Include(a => a.ArticleTags)
                .ThenInclude(at => at.Tag)
            .FirstOrDefaultAsync(a => a.Id == id, ct);

    public async Task<Article> CreateAsync(Article article, CancellationToken ct)
    {
        await context.Articles.AddAsync(article, ct);
        await context.SaveChangesAsync(ct);
        return article;
    }

    public async Task UpdateAsync(Article article, CancellationToken ct)
    {
        context.Articles.Update(article);
        await context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct)
    {
        var article = await context.Articles.FindAsync([id], ct);
        if (article is not null)
        {
            context.Articles.Remove(article);
            await context.SaveChangesAsync(ct);
        }
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken ct) =>
        await context.Articles.AnyAsync(a => a.Id == id, ct);

    public async Task<List<Guid>> GetExistingTagIdsAsync(List<Guid> tagIds, CancellationToken ct) =>
        await context.Tags
            .Where(t => tagIds.Contains(t.Id))
            .Select(t => t.Id)
            .ToListAsync(ct);

    public async Task<List<Article>> GetArticlesBySectionAsync(Guid sectionId, CancellationToken ct)
    {
        var section = await context.Sections
            .Where(s => s.Id == sectionId)
            .Select(s => new
            {
                s.Id,
                TagIds = s.SectionTags.Select(st => st.Tag.Id).ToList()
            })
            .FirstOrDefaultAsync(ct);

        if (section is null || section.TagIds.Count == 0)
            return [];

        var sectionTagSet = new HashSet<Guid>(section.TagIds);

        return await context.Articles
            .Include(a => a.ArticleTags)
                .ThenInclude(at => at.Tag)
            .Where(a => a.ArticleTags.Count == sectionTagSet.Count &&
                       a.ArticleTags.Select(at => at.Tag.Id).ToHashSet().SetEquals(sectionTagSet))
            .ToListAsync(ct);
    }
}