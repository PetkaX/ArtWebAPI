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

    public async Task<IEnumerable<Section>> GetAllAsync(CancellationToken ct)
    {
        // 1. Загружаем все секции с тегами
        var sections = await context.Sections
            .Include(s => s.SectionTags)
                .ThenInclude(st => st.Tag)
            .ToListAsync(ct);

        // 2. Загружаем все статьи с тегами
        var articles = await context.Articles
            .Include(a => a.ArticleTags)
                .ThenInclude(at => at.Tag)
            .ToListAsync(ct);

        // 3. Для каждой секции — подсчитываем количество статей с совпадающими тегами
        var sectionWithCounts = sections.Select(section =>
        {
            var sectionTagIds = section.SectionTags.Select(st => st.Tag.Id).ToHashSet();

            var matchingArticles = articles.Count(article =>
            {
                var articleTagIds = article.ArticleTags.Select(at => at.Tag.Id).ToHashSet();
                return articleTagIds.SetEquals(sectionTagIds);
            });

            return new
            {
                Section = section,
                ArticleCount = matchingArticles
            };
        })
        .OrderByDescending(x => x.ArticleCount) // сортировка по количеству статей по убыванию
        .Select(x => x.Section);

        return sectionWithCounts;
    }

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
    public async Task AutoCreateSectionsAsync(CancellationToken ct)
    {
        // 1. Получаем все статьи с тегами
        var articles = await context.Articles
            .Include(a => a.ArticleTags)
                .ThenInclude(at => at.Tag)
            .Where(a => a.ArticleTags.Any()) // только статьи с тегами
            .ToListAsync(ct);

        if (!articles.Any())
            return;

        // 2. Группируем статьи по множеству тегов
        var groups = articles
            .Where(a => a.ArticleTags.All(at => at.Tag != null))
            .GroupBy(article =>
            {
                var tagIds = article.ArticleTags
                    .Select(at => at.Tag.Id)
                    .OrderBy(id => id) // нормализация порядка для сравнения
                    .ToArray();
                return string.Join(",", tagIds);
            })
            .ToList();

        // 3. Получаем существующие секции с тегами
        var existingSections = await context.Sections
            .Include(s => s.SectionTags)
                .ThenInclude(st => st.Tag)
            .ToListAsync(ct);

        // 4. Для каждой группы
        foreach (var group in groups)
        {
            var firstArticle = group.First();
            var tagIdsInGroup = group.First().ArticleTags
                .Select(at => at.Tag.Id)
                .ToHashSet();

            // Проверяем, есть ли уже секция с таким набором тегов
            var existingSection = existingSections.FirstOrDefault(s =>
            {
                var sectionTagIds = s.SectionTags.Select(st => st.Tag.Id).ToHashSet();
                return sectionTagIds.SetEquals(tagIdsInGroup);
            });

            if (existingSection != null)
                continue; // уже есть

            // Создаём новую секцию
            var tags = firstArticle.ArticleTags.Select(at => at.Tag).ToList();
            var tagNameList = string.Join(",", tags.Select(t => t.Name));
            var sectionTitle = tagNameList.Length <= 1024
                ? tagNameList
                : tagNameList.Substring(0, 1024); // обрезка

            var newSection = new Section
            {
                Id = Guid.NewGuid(),
                Title = sectionTitle,
                SectionTags = tags.Select((tag, index) => new SectionTag
                {
                    SectionId = Guid.NewGuid(), // будет установлен при добавлении
                    TagId = tag.Id,
                    Order = index
                }).ToList()
            };

            context.Sections.Add(newSection);
            existingSections.Add(newSection); // чтобы не создавать дубликаты
        }

        await context.SaveChangesAsync(ct);
    }
}