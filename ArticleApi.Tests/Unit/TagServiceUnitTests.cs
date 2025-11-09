using ArticleApi.Application.DTOs;
using ArticleApi.Application.Services;
using ArticleApi.Domain.Entities;
using ArticleApi.Domain.Interfaces;
using Moq;
using Xunit;

namespace ArticleApi.Tests.Unit;

public class TagServiceUnitTests
{
    private readonly Mock<ITagRepository> _mockRepo;
    private readonly TagService _service;
    private readonly CancellationToken _ct = CancellationToken.None;

    public TagServiceUnitTests()
    {
        _mockRepo = new Mock<ITagRepository>();
        _service = new TagService(_mockRepo.Object);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllTags()
    {
        // Arrange
        var tags = new List<Tag>
        {
            TestEntityFactory.CreateTag(Guid.NewGuid(), "Tag1"),
            TestEntityFactory.CreateTag(Guid.NewGuid(), "Tag2")
        };

        _mockRepo.Setup(r => r.GetAllAsync(_ct)).ReturnsAsync(tags);

        // Act
        var result = await _service.GetAllAsync(_ct);

        // Assert
        Assert.Equal(2, result.Count());
        Assert.Contains(result, t => t.Name == "Tag1");
        Assert.Contains(result, t => t.Name == "Tag2");
    }

    [Fact]
    public async Task GetByIdAsync_WhenExists_ReturnsTagDto()
    {
        // Arrange
        var id = Guid.NewGuid();
        var tag = TestEntityFactory.CreateTag(id, "Found");

        _mockRepo.Setup(r => r.GetByIdAsync(id, _ct)).ReturnsAsync(tag);

        // Act
        var result = await _service.GetByIdAsync(id, _ct);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(id, result.Id);
        Assert.Equal("Found", result.Name);
    }

    [Fact]
    public async Task GetByIdAsync_WhenNotExists_ReturnsNull()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockRepo.Setup(r => r.GetByIdAsync(id, _ct)).ReturnsAsync((Tag)null!);

        // Act
        var result = await _service.GetByIdAsync(id, _ct);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_WithValidName_ReturnsTagDto()
    {
        // Arrange
        var dto = TestEntityFactory.CreateCreateTagDto("New Tag");
        var tag = TestEntityFactory.CreateTag(name: dto.Name);

        _mockRepo.Setup(r => r.ExistsByNameAsync("new tag", _ct)).ReturnsAsync(false);
        _mockRepo.Setup(r => r.CreateAsync(It.IsAny<Tag>(), _ct)).ReturnsAsync(tag);

        // Act
        var result = await _service.CreateAsync(dto, _ct);

        // Assert
        Assert.Equal(dto.Name, result.Name);
        Assert.NotEqual(Guid.Empty, result.Id);
    }

    [Fact]
    public async Task CreateAsync_WhenNameIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        var dto = new CreateTagDto(null!);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _service.CreateAsync(dto, _ct));
    }

    [Fact]
    public async Task CreateAsync_WhenNameIsEmpty_ThrowsArgumentException()
    {
        // Arrange
        var dto = TestEntityFactory.CreateCreateTagDto("   ");

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _service.CreateAsync(dto, _ct));
    }

    [Fact]
    public async Task CreateAsync_WhenNameTooLong_ThrowsArgumentException()
    {
        // Arrange
        var dto = TestEntityFactory.CreateCreateTagDto(new string('x', 257));

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateAsync(dto, _ct));
    }

    [Fact]
    public async Task CreateAsync_WhenTagExistsIgnoreCase_ThrowsInvalidOperationException()
    {
        // Arrange
        var dto = TestEntityFactory.CreateCreateTagDto("Existing");
        var existing = TestEntityFactory.CreateTag(name: "existing"); // нормализуется в "existing"

        _mockRepo.Setup(r => r.ExistsByNameAsync("existing", _ct)).ReturnsAsync(true);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateAsync(dto, _ct));
    }

    [Fact]
    public async Task UpdateAsync_WhenTagExists_UpdatesAndReturnsDto()
    {
        // Arrange
        var id = Guid.NewGuid();
        var existing = TestEntityFactory.CreateTag(id, "Old");
        var dto = TestEntityFactory.CreateUpdateTagDto("New");

        _mockRepo.Setup(r => r.GetByIdAsync(id, _ct)).ReturnsAsync(existing);
        _mockRepo.Setup(r => r.ExistsByNameAsync("new", _ct)).ReturnsAsync(false);
        _mockRepo.Setup(r => r.UpdateAsync(It.IsAny<Tag>(), _ct)).Returns(Task.CompletedTask);

        // Act
        var result = await _service.UpdateAsync(id, dto, _ct);

        // Assert
        Assert.Equal("New", result.Name);
    }

    [Fact]
    public async Task UpdateAsync_WhenTagNotFound_Throws()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockRepo.Setup(r => r.GetByIdAsync(id, _ct)).ReturnsAsync((Tag)null!);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.UpdateAsync(id, TestEntityFactory.CreateUpdateTagDto(), _ct));
    }

    [Fact]
    public async Task UpdateAsync_WhenNewNameExistsAnotherTag_Throws()
    {
        // Arrange
        var id = Guid.NewGuid();
        var existing = TestEntityFactory.CreateTag(id, "old");
        var dto = TestEntityFactory.CreateUpdateTagDto("other");

        _mockRepo.Setup(r => r.GetByIdAsync(id, _ct)).ReturnsAsync(existing);
        _mockRepo.Setup(r => r.ExistsByNameAsync("other", _ct)).ReturnsAsync(true);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.UpdateAsync(id, dto, _ct));
    }

    [Fact]
    public async Task UpdateAsync_WhenNewNameSameAsCurrent_DoesNotCheckExistence()
    {
        // Arrange
        var id = Guid.NewGuid();
        var existing = TestEntityFactory.CreateTag(id, "same");
        var dto = TestEntityFactory.CreateUpdateTagDto("Same"); // то же имя, разный регистр

        _mockRepo.Setup(r => r.GetByIdAsync(id, _ct)).ReturnsAsync(existing);
        _mockRepo.Setup(r => r.ExistsByNameAsync("same", _ct)).ReturnsAsync(true); // имитация "уже существует"
        _mockRepo.Setup(r => r.UpdateAsync(It.IsAny<Tag>(), _ct)).Returns(Task.CompletedTask);

        // Act
        var result = await _service.UpdateAsync(id, dto, _ct);

        // Assert
        Assert.Equal("Same", result.Name); // или "same", зависит от нормализации
        // В реальности: если имя не изменилось (после нормализации), ExistsByName можно пропустить
    }

    [Fact]
    public async Task UpdateAsync_WhenNameIsEmpty_ThrowsArgumentException()
    {
        // Arrange
        var id = Guid.NewGuid();
        var existing = TestEntityFactory.CreateTag(id, "old");
        var dto = TestEntityFactory.CreateUpdateTagDto("   ");

        _mockRepo.Setup(r => r.GetByIdAsync(id, _ct)).ReturnsAsync(existing);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.UpdateAsync(id, dto, _ct));
    }

    [Fact]
    public async Task UpdateAsync_WhenNameTooLong_ThrowsArgumentException()
    {
        // Arrange
        var id = Guid.NewGuid();
        var existing = TestEntityFactory.CreateTag(id, "old");
        var dto = TestEntityFactory.CreateUpdateTagDto(new string('x', 257));

        _mockRepo.Setup(r => r.GetByIdAsync(id, _ct)).ReturnsAsync(existing);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.UpdateAsync(id, dto, _ct));
    }

    [Fact]
    public async Task DeleteAsync_WhenTagExists_Deletes()
    {
        // Arrange
        var id = Guid.NewGuid();
        var tag = TestEntityFactory.CreateTag(id);

        _mockRepo.Setup(r => r.GetByIdAsync(id, _ct)).ReturnsAsync(tag);
        _mockRepo.Setup(r => r.DeleteAsync(id, _ct)).Returns(Task.CompletedTask);

        // Act
        await _service.DeleteAsync(id, _ct);

        // Assert
        _mockRepo.Verify(r => r.DeleteAsync(id, _ct), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenTagNotFound_Throws()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockRepo.Setup(r => r.GetByIdAsync(id, _ct)).ReturnsAsync((Tag)null!);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.DeleteAsync(id, _ct));
    }
}