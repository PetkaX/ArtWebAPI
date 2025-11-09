using ArticleApi.Application.DTOs;
using ArticleApi.Application.Services;
using ArticleApi.Domain.Entities;
using ArticleApi.Domain.Interfaces;
using Moq;
using Xunit;

namespace ArticleApi.Tests.Unit;

public class ArticleServiceUnitTests
{
    private readonly Mock<IArticleRepository> _mockRepo;
    private readonly ArticleService _service;
    private readonly CancellationToken _ct = CancellationToken.None;

    public ArticleServiceUnitTests()
    {
        _mockRepo = new Mock<IArticleRepository>();
        _service = new ArticleService(_mockRepo.Object);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsMappedDtos()
    {
        // Arrange
        var tagId1 = Guid.NewGuid();
        var tagId2 = Guid.NewGuid();

        var articles = new List<Article>
        {
            TestEntityFactory.CreateArticle(
                title: "Test 1",
                content: "Content 1",
                tagData:
                [
                    (tagId1, "Tag1", 0),
                    (tagId2, "Tag2", 1)
                ]),
            TestEntityFactory.CreateArticle(
                title: "Test 2",
                content: "Content 2",
                tagData:
                [
                    (tagId1, "Tag1", 0)
                ])
        };

        _mockRepo.Setup(r => r.GetAllAsync(_ct)).ReturnsAsync(articles);

        // Act
        var result = await _service.GetAllAsync(_ct);

        // Assert
        Assert.Equal(2, result.Count());
        Assert.All(result, dto => Assert.NotNull(dto.Title));
        Assert.Contains(result.First().Tags, t => t.Name == "Tag1");
    }

    [Fact]
    public async Task GetByIdAsync_WhenExists_ReturnsMappedDto()
    {
        // Arrange
        var id = Guid.NewGuid();
        var article = TestEntityFactory.CreateArticle(
            id: id,
            title: "Found",
            tagData: [(Guid.NewGuid(), "FoundTag", 0)]);

        _mockRepo.Setup(r => r.GetByIdAsync(id, _ct)).ReturnsAsync(article);

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
        _mockRepo.Setup(r => r.GetByIdAsync(id, _ct)).ReturnsAsync((Article)null!);

        // Act
        var result = await _service.GetByIdAsync(id, _ct);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_WithValidData_ReturnsArticleDto()
    {
        // Arrange
        var tagId = Guid.NewGuid();
        var dto = TestEntityFactory.CreateCreateArticleDto(
            title: "New Title",
            content: "New Content",
            tagIds: [tagId]);

        var article = TestEntityFactory.CreateArticle(
            title: dto.Title,
            content: dto.Content,
            tagData: [(tagId, "New Tag", 0)]);

        _mockRepo.Setup(r => r.GetExistingTagIdsAsync(It.IsAny<List<Guid>>(), _ct))
            .ReturnsAsync([tagId]);

        _mockRepo.Setup(r => r.CreateAsync(It.IsAny<Article>(), _ct)).ReturnsAsync(article);

        // Act
        var result = await _service.CreateAsync(dto, _ct);

        // Assert
        Assert.Equal(dto.Title, result.Title);
        Assert.Equal(dto.Content, result.Content);
        Assert.Equal(article.Id, result.Id);
        Assert.Single(result.Tags);
        Assert.Equal(tagId, result.Tags.First().Id);
        Assert.Equal("New Tag", result.Tags.First().Name);
    }

    [Fact]
    public async Task CreateAsync_WhenTitleIsEmpty_ThrowsArgumentException()
    {
        // Arrange
        var dto = TestEntityFactory.CreateCreateArticleDto(title: "");

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateAsync(dto, _ct));
    }

    [Fact]
    public async Task CreateAsync_WhenTitleTooLong_ThrowsArgumentException()
    {
        // Arrange
        var dto = TestEntityFactory.CreateCreateArticleDto(title: new string('x', 257));

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateAsync(dto, _ct));
    }

    [Fact]
    public async Task CreateAsync_WhenContentIsEmpty_ThrowsArgumentException()
    {
        // Arrange
        var dto = TestEntityFactory.CreateCreateArticleDto(content: "");

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateAsync(dto, _ct));
    }

    [Fact]
    public async Task CreateAsync_WhenTagIdsIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        var dto = new CreateArticleDto("Title", "Content", null!);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _service.CreateAsync(dto, _ct));
    }

    [Fact]
    public async Task CreateAsync_WhenTagIdsEmpty_ThrowsArgumentException()
    {
        // Arrange
        var dto = TestEntityFactory.CreateCreateArticleDto(tagIds: []);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateAsync(dto, _ct));
    }

    [Fact]
    public async Task CreateAsync_WhenTagIdsHasDuplicates_ThrowsArgumentException()
    {
        // Arrange
        var tagId = Guid.NewGuid();
        var dto = TestEntityFactory.CreateCreateArticleDto(tagIds: [tagId]);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateAsync(dto, _ct));
    }

    [Fact]
    public async Task CreateAsync_WhenTagDoesNotExist_ThrowsArgumentException()
    {
        // Arrange
        var tagId = Guid.NewGuid();
        var dto = TestEntityFactory.CreateCreateArticleDto(tagIds: [tagId]);

        _mockRepo.Setup(r => r.GetExistingTagIdsAsync(It.IsAny<List<Guid>>(), _ct))
            .ReturnsAsync([]); // тег не найден

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateAsync(dto, _ct));
    }

    [Fact]
    public async Task UpdateAsync_WhenArticleExists_UpdatesAndReturnsDto()
    {
        // Arrange
        var id = Guid.NewGuid();
        var tagId = Guid.NewGuid();
        var tagData = new List<(Guid, string, int)> { (tagId, "OldTag", 0) };

        // Исходная статья (для обновления)
        var existing = TestEntityFactory.CreateArticle(
            id: id,
            title: "Old",
            content: "Old",
            tagData: tagData);

        // Обновлённая статья (для возврата после перезагрузки)
        var updatedArticle = TestEntityFactory.CreateArticle(
            id: id,
            title: "New",
            content: "New",
            tagData: tagData);

        var dto = new UpdateArticleDto("New", "New", [tagId]);

        // Мок:
        // первый вызов — получить статью для обновления
        // повторный GetByIdAsync должен вернуть обновлённую сущность
        _mockRepo.SetupSequence(r => r.GetByIdAsync(id, _ct))
            .ReturnsAsync(existing)
            .ReturnsAsync(updatedArticle);

        // Мок: проверка существования тегов
        _mockRepo.Setup(r => r.GetExistingTagIdsAsync(It.IsAny<List<Guid>>(), _ct))
            .ReturnsAsync([tagId]);

        // Мок: обновление
        _mockRepo.Setup(r => r.UpdateAsync(It.IsAny<Article>(), _ct)).Returns(Task.CompletedTask);

        // Act
        var result = await _service.UpdateAsync(id, dto, _ct);

        // Assert
        Assert.Equal("New", result.Title);
        Assert.Equal("New", result.Content);
        Assert.Single(result.Tags);
        Assert.Equal("OldTag", result.Tags.First().Name);
        Assert.Equal(tagId, result.Tags.First().Id);
    }

    [Fact]
    public async Task UpdateAsync_WhenArticleNotFound_Throws()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockRepo.Setup(r => r.GetByIdAsync(id, _ct)).ReturnsAsync((Article)null!);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.UpdateAsync(id, TestEntityFactory.CreateUpdateArticleDto(), _ct));
    }

    [Fact]
    public async Task UpdateAsync_WhenTitleIsEmpty_ThrowsArgumentException()
    {
        // Arrange
        var id = Guid.NewGuid();
        var existing = TestEntityFactory.CreateArticle(id: id);
        var dto = TestEntityFactory.CreateUpdateArticleDto(title: "");

        _mockRepo.Setup(r => r.GetByIdAsync(id, _ct)).ReturnsAsync(existing);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.UpdateAsync(id, dto, _ct));
    }

    [Fact]
    public async Task DeleteAsync_WhenArticleExists_Deletes()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockRepo.Setup(r => r.ExistsAsync(id, _ct)).ReturnsAsync(true);
        _mockRepo.Setup(r => r.DeleteAsync(id, _ct)).Returns(Task.CompletedTask);

        // Act
        await _service.DeleteAsync(id, _ct);

        // Assert
        _mockRepo.Verify(r => r.DeleteAsync(id, _ct), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenArticleNotFound_Throws()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockRepo.Setup(r => r.ExistsAsync(id, _ct)).ReturnsAsync(false);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.DeleteAsync(id, _ct));
    }
}