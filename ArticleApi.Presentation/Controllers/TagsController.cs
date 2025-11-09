using ArticleApi.Application.DTOs;
using ArticleApi.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace ArticleApi.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class TagsController(TagService tagService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TagDto>>> GetAll(CancellationToken ct)
    {
        var tags = await tagService.GetAllAsync(ct);
        return Ok(tags);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TagDto>> GetById(Guid id, CancellationToken ct)
    {
        var tag = await tagService.GetByIdAsync(id, ct);
        return tag is null ? NotFound() : Ok(tag);
    }

    [HttpPost]
    public async Task<ActionResult<TagDto>> Create(CreateTagDto dto, CancellationToken ct)
    {
        try
        {
            var result = await tagService.CreateAsync(dto, ct);
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
    public async Task<IActionResult> Update(Guid id, UpdateTagDto dto, CancellationToken ct)
    {
        try
        {
            var result = await tagService.UpdateAsync(id, dto, ct);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        try
        {
            await tagService.DeleteAsync(id, ct);
            return NoContent();
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }
    }
}