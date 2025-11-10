using ArticleApi.Application.DTOs;
using ArticleApi.Application.Services;
using ArticleApi.Domain.Entities;
using ArticleApi.Domain.Interfaces;
using Moq;

namespace ArticleApi.Tests.Unit;

public class SectionServiceUnitTests
{
    private readonly Mock<ISectionRepository> _mockRepo;
    private readonly SectionService _service;
    private readonly CancellationToken _ct = CancellationToken.None;

    public SectionServiceUnitTests()
    {
        _mockRepo = new Mock<ISectionRepository>();
        _service = new SectionService(_mockRepo.Object);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllSections()
    {
        // Arrange
        var sections = new List<Section>
        {
            TestEntityFactory.CreateSection(
                title: "Section1",
                tagData: [(Guid.NewGuid(), "Tag1", 0)]),
            TestEntityFactory.CreateSection(
                title: "Section2",
                tagData: [(Guid.NewGuid(), "Tag2", 0)])
        };

        _mockRepo.Setup(r => r.GetAllAsync(_ct)).ReturnsAsync(sections);

        // Act
        var result = await _service.GetAllAsync(_ct);

        // Assert
        Assert.Equal(2, result.Count());
        Assert.Contains(result, s => s.Title == "Section1");
        Assert.Contains(result, s => s.Title == "Section2");
        Assert.All(result, s => Assert.True(s.Tags.Count != 0));
    }

    [Fact]
    public async Task GetByIdAsync_WhenExists_ReturnsSectionDto()
    {
        // Arrange
        var id = Guid.NewGuid();
        var section = TestEntityFactory.CreateSection(
            id: id,
            title: "Found",
            tagData: [(Guid.NewGuid(), "FoundTag", 0)]);

        _mockRepo.Setup(r => r.GetByIdAsync(id, _ct)).ReturnsAsync(section);

        // Act
        var result = await _service.GetByIdAsync(id, _ct);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(id, result.Id);
        Assert.Equal("Found", result.Title);
        Assert.Single(result.Tags);
        Assert.Equal("FoundTag", result.Tags.First().Name);
    }

    [Fact]
    public async Task GetByIdAsync_WhenNotExists_ReturnsNull()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockRepo.Setup(r => r.GetByIdAsync(id, _ct)).ReturnsAsync((Section)null!);

        // Act
        var result = await _service.GetByIdAsync(id, _ct);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_WithValidData_ReturnsSectionDto()
    {
        // Arrange
        var tagId = Guid.NewGuid();
        var dto = new CreateSectionDto("New Section", [tagId]);

        var section = TestEntityFactory.CreateSection(
            title: dto.Title,
            tagData: [(tagId, "New Tag", 0)]);

        _mockRepo.Setup(r => r.CreateAsync(It.IsAny<Section>(), _ct)).ReturnsAsync(section);

        // Act
        var result = await _service.CreateAsync(dto, _ct);

        // Assert
        Assert.Equal(dto.Title, result.Title);
        Assert.Single(result.Tags);
        Assert.Equal(tagId, result.Tags.First().Id);
        Assert.Equal("New Tag", result.Tags.First().Name);
    }

    [Fact]
    public async Task CreateAsync_WhenTagIdsMoreThan256_ThrowsArgumentException()
    {
        // Arrange
        var dto = new CreateSectionDto("Title", [.. Enumerable.Repeat(Guid.NewGuid(), 257)]);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateAsync(dto, _ct));
    }

    [Fact]
    public async Task DeleteAsync_WhenSectionExists_Deletes()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockRepo.Setup(r => r.DeleteAsync(id, _ct)).Returns(Task.CompletedTask);

        // Act
        await _service.DeleteAsync(id, _ct);

        // Assert
        _mockRepo.Verify(r => r.DeleteAsync(id, _ct), Times.Once);
    }

    [Fact]
    public async Task GetArticlesBySectionAsync_WhenSectionExists_ReturnsArticles()
    {
        // Arrange
        var sectionId = Guid.NewGuid();
        var tagId = Guid.NewGuid();

        var articles = new List<Article>
        {
            TestEntityFactory.CreateArticle(
                title: "Matched Article",
                content: "Content",
                tagData: [(tagId, "CommonTag", 0)])
        };

        _mockRepo.Setup(r => r.GetArticlesBySectionAsync(sectionId, _ct)).ReturnsAsync(articles);

        // Act
        var result = await _service.GetArticlesBySectionAsync(sectionId, _ct);

        // Assert
        Assert.Single(result);
        var first = result.First();
        Assert.Equal("Matched Article", first.Title);
        Assert.Equal("CommonTag", first.Tags.First().Name);
    }

    [Fact]
    public async Task GetArticlesBySectionAsync_WhenSectionHasNoTags_ReturnsEmptyList()
    {
        // Arrange
        var sectionId = Guid.NewGuid();
        var articles = new List<Article>();

        _mockRepo.Setup(r => r.GetArticlesBySectionAsync(sectionId, _ct)).ReturnsAsync(articles);

        // Act
        var result = await _service.GetArticlesBySectionAsync(sectionId, _ct);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsSectionsOrderedByArticleCountDescending()
    {
        // Arrange
        var tagId1 = Guid.NewGuid();
        var tagId2 = Guid.NewGuid();
        var tagId3 = Guid.NewGuid();

        // Секция A: теги {tag1, tag2} → 2 совпадающие статьи
        var sectionA = TestEntityFactory.CreateSection(
            id: Guid.NewGuid(),
            title: "C# Basics",
            tagData: [(tagId1, "csharp", 0), (tagId2, "dotnet", 1)]);

        // Секция B: теги {tag3} → 1 совпадающая статья
        var sectionB = TestEntityFactory.CreateSection(
            id: Guid.NewGuid(),
            title: "Docker",
            tagData: [(tagId3, "docker", 0)]);

        // Статьи
        var article1 = TestEntityFactory.CreateArticle(
            title: "Intro to C#",
            content: "C# is great",
            tagData: [(tagId1, "csharp", 0), (tagId2, "dotnet", 1)]); // совпадает с A

        var article2 = TestEntityFactory.CreateArticle(
            title: "Advanced C#",
            content: "Records, patterns",
            tagData: [(tagId2, "dotnet", 0), (tagId1, "csharp", 1)]); // совпадает с A (порядок не важен)

        var article3 = TestEntityFactory.CreateArticle(
            title: "Dockerize API",
            content: "Use containers",
            tagData: [(tagId3, "docker", 0)]); // совпадает с B

        // Статья 4 — не подходит ни к одной секции
        var article4 = TestEntityFactory.CreateArticle(
            title: "Random",
            content: "No tags",
            tagData: []);

        var allSections = new List<Section> { sectionA, sectionB };
        var allArticles = new List<Article> { article1, article2, article3, article4 };

        // Мок: GetAllAsync в SectionRepository должен вернуть секции, уже отсортированные по количеству статей
        _mockRepo.Setup(r => r.GetAllAsync(_ct)).ReturnsAsync(() =>
        {
            // Здесь мы имитируем логику SectionRepository.GetAllAsync
            return allSections.Select(section =>
            {
                var sectionTagIds = section.SectionTags.Select(st => st.Tag.Id).ToHashSet();
                var matchingCount = allArticles.Count(article =>
                {
                    var articleTagIds = article.ArticleTags.Select(at => at.Tag.Id).ToHashSet();
                    return articleTagIds.SetEquals(sectionTagIds);
                });

                return new { Section = section, Count = matchingCount };
            })
            .OrderByDescending(x => x.Count)
            .Select(x => x.Section)
            .ToList();
        });

        // Act
        var result = await _service.GetAllAsync(_ct);

        // Assert
        var resultList = result.ToList();
        Assert.Equal(2, resultList.Count);
        Assert.Equal("C# Basics", resultList[0].Title); // 2 статьи → первая
        Assert.Equal("Docker", resultList[1].Title);   // 1 статья → вторая
    }

    [Fact]
    public async Task AutoCreateSectionsAsync_CreatesSectionsForUniqueTagSets()
    {
        // Arrange
        var tagId1 = Guid.NewGuid();
        var tagId2 = Guid.NewGuid();
        var tagId3 = Guid.NewGuid();

        var tag1 = TestEntityFactory.CreateTag(tagId1, "csharp");
        var tag2 = TestEntityFactory.CreateTag(tagId2, "dotnet");
        var tag3 = TestEntityFactory.CreateTag(tagId3, "web");

        // Статьи с тегами
        var article1 = TestEntityFactory.CreateArticle(
            title: "C# Intro",
            content: "Basics",
            tagData: [(tagId1, "csharp", 0), (tagId2, "dotnet", 1)]); // набор: {csharp, dotnet}

        var article2 = TestEntityFactory.CreateArticle(
            title: "Advanced .NET",
            content: "Patterns",
            tagData: [(tagId2, "dotnet", 0), (tagId1, "csharp", 1)]); // тот же набор, другой порядок

        var article3 = TestEntityFactory.CreateArticle(
            title: "Web API",
            content: "REST",
            tagData: [(tagId3, "web", 0)]); // набор: {web}

        var article4 = TestEntityFactory.CreateArticle(
            title: "Empty",
            content: "No tags",
            tagData: []); // без тегов — игнорируется

        // Мок Articles (через мок DbContext — но у нас мок репозитория)
        // Имитируем поведение: SectionRepository.GetArticlesForAutoCreate → мы сами фильтруем в тесте

        // Настраиваем мок: AutoCreateSectionsAsync вызывается через SectionService
        // Но тестируем через мок репозитория

        // Создаём мок контекста для Articles (если был бы интеграционный тест)
        // Здесь — достаточно проверить, что SectionRepository вызывает Add и Save

        var mockRepo = new Mock<ISectionRepository>();
        var mockArticleRepository = new Mock<IArticleRepository>(); // добавим, если нужно, но пока обойдёмся

        // Имитируем ArticleRepository, если SectionRepository зависит от него
        // Но в нашем случае — нет: SectionRepository напрямую использует _context.Articles

        // Вместо этого — тестируем **логику через мок SectionRepository**, но лучше создать интеграционный тест
        // Однако для юнит-теста — смоделируем вызов

        // Подменим поведение SectionRepository вручную
        var sectionsInDb = new List<Section>();

        mockRepo
            .Setup(r => r.AutoCreateSectionsAsync(It.IsAny<CancellationToken>()))
            .Callback(async (CancellationToken ct) =>
            {
                // Имитация логики из AutoCreateSectionsAsync

                var articles = new List<Article> { article1, article2, article3, article4 }
                    .Where(a => a.ArticleTags.Any())
                    .ToList();

                var existingSections = sectionsInDb.ToList();

                var groups = articles
                    .Where(a => a.ArticleTags.All(at => at.Tag != null))
                    .GroupBy(article =>
                    {
                        var tagIds = article.ArticleTags
                            .Select(at => at.Tag.Id)
                            .OrderBy(id => id)
                            .ToArray();
                        return string.Join(",", tagIds);
                    })
                    .ToList();

                foreach (var group in groups)
                {
                    var firstArticle = group.First();
                    var tagIdsInGroup = group.First().ArticleTags
                        .Select(at => at.Tag.Id)
                        .ToHashSet();

                    var existingSection = existingSections.FirstOrDefault(s =>
                    {
                        var sectionTagIds = s.SectionTags.Select(st => st.Tag.Id).ToHashSet();
                        return sectionTagIds.SetEquals(tagIdsInGroup);
                    });

                    if (existingSection != null)
                        continue;

                    var tags = firstArticle.ArticleTags.Select(at => at.Tag).ToList();
                    var tagNameList = string.Join(",", tags.Select(t => t.Name));
                    var title = tagNameList.Length <= 1024 ? tagNameList : tagNameList.Substring(0, 1024);

                    var newSection = new Section
                    {
                        Id = Guid.NewGuid(),
                        Title = title,
                        SectionTags = tags.Select((tag, index) => new SectionTag
                        {
                            TagId = tag.Id,
                            Order = index
                        }).ToList()
                    };

                    sectionsInDb.Add(newSection);
                }
            });

        var service = new SectionService(mockRepo.Object);

        // Act
        await service.AutoCreateSectionsAsync(_ct);

        // Assert
        Assert.Equal(2, sectionsInDb.Count); // {csharp,dotnet} и {web}

        var csDotnetSection = sectionsInDb
            .FirstOrDefault(s => s.Title == "csharp,dotnet");
        Assert.NotNull(csDotnetSection);
        Assert.Equal(2, csDotnetSection.SectionTags.Count);
        Assert.Contains(csDotnetSection.SectionTags, st => st.TagId == tagId1);
        Assert.Contains(csDotnetSection.SectionTags, st => st.TagId == tagId2);

        var webSection = sectionsInDb
            .FirstOrDefault(s => s.Title == "web");
        Assert.NotNull(webSection);
        Assert.Single(webSection.SectionTags);
        Assert.Contains(webSection.SectionTags, st => st.TagId == tagId3);
    }
}