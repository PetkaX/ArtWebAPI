using ArticleApi.Application.DTOs;
using ArticleApi.Domain.Entities;

namespace ArticleApi.Tests.Unit;

/// <summary>
/// Вспомогательная фабрика для создания тестовых сущностей с корректными навигационными свойствами.
/// Имитирует поведение EF Core после .Include().
/// </summary>
public static class TestEntityFactory
{
    // === Tag ===
    public static Tag CreateTag(Guid? id = null, string name = "Test Tag")
    {
        return new Tag
        {
            Id = id ?? Guid.NewGuid(),
            Name = name,
            ArticleTags = new List<ArticleTag>(),
            SectionTags = new List<SectionTag>()
        };
    }

    // === Article ===
    public static Article CreateArticle(
     Guid? id = null,
     string title = "Test Article",
     string content = "Test Content",
     List<(Guid tagId, string tagName, int order)> tagData = null!)
    {
        var article = new Article
        {
            Id = id ?? Guid.NewGuid(),
            Title = title,
            Content = content,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = null,
            ArticleTags = new List<ArticleTag>()
        };

        if (tagData != null)
        {
            foreach (var (tagId, tagName, order) in tagData)
            {
                var tag = CreateTag(tagId, tagName);

                var articleTag = new ArticleTag
                {
                    ArticleId = article.Id,
                    TagId = tagId,
                    Order = order,
                    Article = article,
                    Tag = tag
                };

                article.ArticleTags.Add(articleTag);

                tag.ArticleTags.Add(articleTag);
            }
        }

        return article;
    }

    // === Section ===
    public static Section CreateSection(
        Guid? id = null,
        string title = "Test Section",
        List<(Guid tagId, string tagName, int order)> tagData = null!)
    {
        var section = new Section
        {
            Id = id ?? Guid.NewGuid(),
            Title = title,
            SectionTags = new List<SectionTag>()
        };

        if (tagData != null)
        {
            foreach (var (tagId, tagName, order) in tagData)
            {
                var tag = CreateTag(tagId, tagName);
                section.SectionTags.Add(new SectionTag
                {
                    SectionId = section.Id,
                    TagId = tagId,
                    Order = order,
                    Section = section,
                    Tag = tag
                });

                tag.SectionTags.Add(new SectionTag
                {
                    SectionId = section.Id,
                    TagId = tagId,
                    Order = order,
                    Section = section,
                    Tag = tag
                });
            }
        }

        return section;
    }

    // === ArticleTag ===
    public static ArticleTag CreateArticleTag(Guid articleId, Guid tagId, string tagName, int order = 0)
    {
        var article = new Article { Id = articleId };
        var tag = CreateTag(tagId, tagName);

        return new ArticleTag
        {
            ArticleId = articleId,
            TagId = tagId,
            Order = order,
            Article = article,
            Tag = tag
        };
    }

    // === SectionTag ===
    public static SectionTag CreateSectionTag(Guid sectionId, Guid tagId, string tagName, int order = 0)
    {
        var section = new Section { Id = sectionId };
        var tag = CreateTag(tagId, tagName);

        return new SectionTag
        {
            SectionId = sectionId,
            TagId = tagId,
            Order = order,
            Section = section,
            Tag = tag
        };
    }

    // === DTOs ===
    public static CreateArticleDto CreateCreateArticleDto(
        string title = "New Article",
        string content = "Content",
        List<Guid> tagIds = null!)
    {
        return new CreateArticleDto(
            title,
            content,
            tagIds ?? new List<Guid> { Guid.NewGuid() }
        );
    }

    public static UpdateArticleDto CreateUpdateArticleDto(
        string title = "Updated Article",
        string content = "Updated Content",
        List<Guid> tagIds = null!)
    {
        return new UpdateArticleDto(
            title,
            content,
            tagIds ?? new List<Guid> { Guid.NewGuid() }
        );
    }

    public static CreateTagDto CreateCreateTagDto(string name = "New Tag") =>
        new(name);

    public static UpdateTagDto CreateUpdateTagDto(string name = "Updated Tag") =>
        new(name);
}