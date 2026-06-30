using System.Threading;
using System.Threading.Tasks;
using LineItem.Exceptions;
using LineItem.Models;
using LineItem.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LineItem.Api.Controllers;

[ApiController]
[Route("v{version:apiVersion}/product-drafts")]
public class ProductDraftController : ControllerBase
{
    private readonly IProductDraftService _productDraftService;

    public ProductDraftController(IProductDraftService productDraftService)
    {
        _productDraftService = productDraftService;
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateAsync(
        [FromBody] Product product,
        [FromQuery] long createdBy,
        CancellationToken cancellationToken
    )
    {
        var draft = await _productDraftService.CreateAsync(product, createdBy, cancellationToken);
        return CreatedAtAction(nameof(GetByIdAsync), new { version = "1", id = draft.Id }, draft);
    }

    [HttpPost("from-product/{productId:long}")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateFromProductAsync(
        [FromRoute] long productId,
        [FromQuery] long createdBy,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var draft = await _productDraftService.CreateFromProductAsync(productId, createdBy, cancellationToken);
            return CreatedAtAction(nameof(GetByIdAsync), new { version = "1", id = draft.Id }, draft);
        }
        catch (NotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpGet("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByIdAsync(long id, CancellationToken cancellationToken)
    {
        var result = await _productDraftService.RetrieveByIdAsync(id, cancellationToken);

        return result is not null
            ? Ok(result)
            : NotFound($"Draft with Id = {id} not found!");
    }

    [HttpPut("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateAsync(
        [FromRoute] long id,
        [FromBody] Product product,
        [FromQuery] long updatedBy,
        CancellationToken cancellationToken
    )
    {
        try
        {
            await _productDraftService.UpdateAsync(id, product, updatedBy, cancellationToken);
            return Ok();
        }
        catch (NotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpPost("{id:long}/publish")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> PublishAsync(
        [FromRoute] long id,
        [FromQuery] long createdBy,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var product = await _productDraftService.PublishAsync(id, createdBy, cancellationToken);
            return Ok(product);
        }
        catch (NotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (ValidationException ex)
        {
            return BadRequest(ex.Errors);
        }
    }

    [HttpDelete("{id:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteAsync(long id, CancellationToken cancellationToken)
    {
        await _productDraftService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}