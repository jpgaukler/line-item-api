using System.Threading;
using System.Threading.Tasks;
using LineItem.Exceptions;
using LineItem.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LineItem.Api.Controllers;

[ApiController]
[Route("v{version:apiVersion}/product-categories")]
public class ProductCategoryController : ControllerBase
{
    private readonly IProductCategoryService _productCategoryService;

    public ProductCategoryController(IProductCategoryService productCategoryService)
    {
        _productCategoryService = productCategoryService;
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateAsync(
        [FromBody] ProductCategory category,
        [FromQuery] long createdBy,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var createdCategory = await _productCategoryService.CreateAsync(category, createdBy, cancellationToken);
            return CreatedAtAction(
                nameof(GetByIdAsync),
                new { version = "1", id = createdCategory.Id },
                createdCategory
            );
        }
        catch (ValidationException ex)
        {
            return BadRequest(ex.Errors);
        }
    }

    [HttpGet("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByIdAsync([FromRoute] long id, CancellationToken cancellationToken)
    {
        var result = await _productCategoryService.RetrieveByIdAsync(id, cancellationToken);

        return result is not null
            ? Ok(result)
            : NotFound($"Product category with Id = {id} not found!");
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllAsync(CancellationToken cancellationToken)
    {
        var result = await _productCategoryService.RetrieveAllAsync(cancellationToken);
        return Ok(result);
    }

    [HttpPut("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateAsync(
        [FromRoute] long id,
        [FromBody] ProductCategory category,
        [FromQuery] long updatedBy,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var updatedCategory = await _productCategoryService.UpdateAsync(id, category, updatedBy, cancellationToken);

            return updatedCategory is not null
                ? Ok(updatedCategory)
                : NotFound($"Product category with Id = {id} not found!");
        }
        catch (ValidationException ex)
        {
            return BadRequest(ex.Errors);
        }
    }

    [HttpDelete("{id:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteAsync([FromRoute] long id, CancellationToken cancellationToken)
    {
        try
        {
            await _productCategoryService.DeleteAsync(id, cancellationToken);
            return NoContent();
        }
        catch (ValidationException ex)
        {
            return BadRequest(ex.Errors);
        }
    }
}