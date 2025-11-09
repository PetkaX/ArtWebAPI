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
    public async Task<ActionResult<IEnumerable<SectionDto>>> GetAll(CancellationToken ct)
    {
        var sections = await sectionService.GetAllAsync(ct);
        return Ok(sections);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<SectionDto>> GetById(Guid id, CancellationToken ct)
    {
        var section = await sectionService.GetByIdAsync(id, ct);
        return section is null ? NotFound() : Ok(section);
    }

    [HttpPost]
    public async Task<ActionResult<SectionDto>> Create(CreateSectionDto dto, CancellationToken ct)
    {
        try
        {
            var result = await sectionService.CreateAsync(dto, ct);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var section = await sectionService.GetByIdAsync(id, ct);
        if (section is null)
            return NotFound();

        await sectionService.DeleteAsync(id, ct);
        return NoContent();
    }

    // GET: api/sections/{id}/articles
    [HttpGet("{id:guid}/articles")]
    public async Task<ActionResult<IEnumerable<ArticleDto>>> GetArticles(Guid id, CancellationToken ct)
    {
        var articles = await sectionService.GetArticlesBySectionAsync(id, ct);
        return Ok(articles);
    }
}