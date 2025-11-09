using ArticleApi.Application.DTOs;
using ArticleApi.Application.Services;
using ArticleApi.Domain.Entities;
using ArticleApi.Domain.Interfaces;
using Moq;
using Xunit;

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
    public async Task CreateAsync_WhenTagIdsHasDuplicates_ThrowsArgumentException()
    {
        // Arrange
        var tagId = Guid.NewGuid();
        var dto = new CreateSectionDto("Title", [tagId]);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateAsync(dto, _ct));
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
}