using ArticleApi.Application.DTOs;
using ArticleApi.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace ArticleApi.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class SectionsController(SectionService sectionService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<SectionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<SectionDto>>> GetAll(CancellationToken ct)
    {
        var sections = await sectionService.GetAllAsync(ct);
        return Ok(sections);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(SectionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SectionDto>> GetById(Guid id, CancellationToken ct)
    {
        var section = await sectionService.GetByIdAsync(id, ct);
        return section is null ? NotFound() : Ok(section);
    }

    /// <summary>
    /// Автоматически создаёт секции для каждого уникального набора тегов (если есть статьи)
    /// </summary>
    /// <remarks>
    /// Пример: статьи с тегами {1, 2} → создаётся секция "1,2"
    /// </remarks>
    [HttpPost("auto")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AutoCreate(CancellationToken ct)
    {
        try
        {
            await sectionService.AutoCreateSectionsAsync(ct);
            return Ok("Автоматическое создание разделов завершено.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Ошибка при автоматическом создании разделов: {ex.Message}");
        }
    }

    [HttpGet("{id:guid}/articles")]
    [ProducesResponseType(typeof(IEnumerable<ArticleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<ArticleDto>>> GetArticles(Guid id, CancellationToken ct)
    {
        var articles = await sectionService.GetArticlesBySectionAsync(id, ct);
        return Ok(articles);
    }
}