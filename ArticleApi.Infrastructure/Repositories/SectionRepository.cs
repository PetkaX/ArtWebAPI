using ArticleApi.Domain.Entities;
using ArticleApi.Domain.Interfaces;
using ArticleApi.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ArticleApi.Infrastructure.Repositories;

public class SectionRepository(ArticleDbContext context) : ISectionRepository
{
    public async Task<Section?> GetByIdAsync(Guid sectionId, CancellationToken ct) =>
        await context.Sections
            .Include(s => s.SectionTags)
                .ThenInclude(st => st.Tag)
            .FirstOrDefaultAsync(s => s.Id == sectionId, ct);

    public async Task<IEnumerable<Section>> GetAllAsync(CancellationToken ct) =>
        await context.Sections
            .Include(s => s.SectionTags)
                .ThenInclude(st => st.Tag)
            .ToListAsync(ct);

    public async Task<Section> CreateAsync(Section section, CancellationToken ct)
    {
        await context.Sections.AddAsync(section, ct);
        await context.SaveChangesAsync(ct);
        return section;
    }

    public async Task DeleteAsync(Guid sectionId, CancellationToken ct)
    {
        var section = await context.Sections.FindAsync([sectionId], ct);
        if (section is not null)
        {
            context.Sections.Remove(section);
            await context.SaveChangesAsync(ct);
        }
    }

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