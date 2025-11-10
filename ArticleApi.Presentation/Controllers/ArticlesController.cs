using ArticleApi.Application.DTOs;
using ArticleApi.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace ArticleApi.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ArticlesController(ArticleService articleService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ArticleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<ArticleDto>>> GetAll(CancellationToken ct)
    {
        var articles = await articleService.GetAllAsync(ct);
        return Ok(articles);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ArticleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ArticleDto>> GetById(Guid id, CancellationToken ct)
    {
        var article = await articleService.GetByIdAsync(id, ct);
        return article is null ? NotFound() : Ok(article);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ArticleDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ArticleDto>> Create(CreateArticleDto dto, CancellationToken ct)
    {
        try
        {
            var result = await articleService.CreateAsync(dto, ct);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ArticleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Update(Guid id, UpdateArticleDto dto, CancellationToken ct)
    {
        try
        {
            var result = await articleService.UpdateAsync(id, dto, ct);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        try
        {
            await articleService.DeleteAsync(id, ct);
            return NoContent();
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }
    }
}