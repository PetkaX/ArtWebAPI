using ArticleApi.Domain.Entities;
using ArticleApi.Domain.Interfaces;
using ArticleApi.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ArticleApi.Infrastructure.Repositories;

public class TagRepository(ArticleDbContext context) : ITagRepository
{
    public async Task<IEnumerable<Tag>> GetAllAsync(CancellationToken ct) =>
        await context.Tags.ToListAsync(ct);

    public async Task<Tag?> GetByIdAsync(Guid id, CancellationToken ct) =>
        await context.Tags.FindAsync([id], ct);

    public async Task<Tag> CreateAsync(Tag tag, CancellationToken ct)
    {
        await context.Tags.AddAsync(tag, ct);
        await context.SaveChangesAsync(ct);
        return tag;
    }

    public async Task UpdateAsync(Tag tag, CancellationToken ct)
    {
        context.Tags.Update(tag);
        await context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct)
    {
        var tag = await context.Tags.FindAsync([id], ct);
        if (tag is not null)
        {
            context.Tags.Remove(tag);
            await context.SaveChangesAsync(ct);
        }
    }

    public async Task<bool> ExistsByNameAsync(string normalizedName, CancellationToken ct) =>
        await context.Tags.AnyAsync(t => t.Name == normalizedName, ct);

    public async Task<List<Tag>> GetByIdsAsync(List<Guid> ids, CancellationToken ct) =>
        await context.Tags
            .Where(t => ids.Contains(t.Id))
            .ToListAsync(ct);
}