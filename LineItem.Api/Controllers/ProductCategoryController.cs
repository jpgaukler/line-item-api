using System.Threading;
using System.Threading.Tasks;
using LineItem.Api.Helpers;
using LineItem.Exceptions;
using LineItem.Models;
using LineItem.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LineItem.Api.Controllers;

[ApiController]
[Route("v{version:apiVersion}/product-categories")]
public class ProductCategoryController : ControllerBase
{
    private readonly IProductCategoryService _productCategoryService;
    private readonly IProductService _productService;

    public ProductCategoryController(IProductCategoryService productCategoryService, IProductService productService)
    {
        _productCategoryService = productCategoryService;
        _productService = productService;
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateAsync(
        [FromBody] CreateProductCategoryRequest request,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var category = new ProductCategory { Name = request.Name };

            var createdCategory =
                await _productCategoryService.CreateAsync(category, HttpContext.GetUserId(), cancellationToken);

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

    [HttpGet("{id:long}/products")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProductsByCategoryIdAsync(
        [FromRoute] long id,
        CancellationToken cancellationToken
    )
    {
        var result = await _productService.RetrieveByCategoryIdAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpPut("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateAsync(
        [FromRoute] long id,
        [FromBody] UpdateProductCategoryRequest request,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var category = new ProductCategory { Name = request.Name };

            var updatedCategory =
                await _productCategoryService.UpdateAsync(id, category, HttpContext.GetUserId(), cancellationToken);

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