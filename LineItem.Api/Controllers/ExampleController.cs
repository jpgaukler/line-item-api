using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LineItem.Models;
using LineItem.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LineItem.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/catalog-products")]
public class ExampleController : ControllerBase
{
    private readonly IExampleService _exampleService;

    public ExampleController(IExampleService exampleService)
    {
        _exampleService = exampleService;
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ExampleModel>> CreateAsync(
        [FromBody] ExampleModel example,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var id = await _exampleService.CreateAsync(example, cancellationToken);
            return CreatedAtRoute(nameof(GetByIdAsync), new { id });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Route("{id}", Name = nameof(GetByIdAsync))]
    public async Task<ActionResult<ExampleModel>> GetByIdAsync(
        int id,
        CancellationToken cancellationToken
    )
    {
        var result = await _exampleService.GetByIdAsync(id, cancellationToken);

        return result is null ? NotFound($"Example (Id = {id}) not found!") : Ok(result);
    }

    [HttpPut]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Route("{id}")]
    public async Task<ActionResult<ExampleModel>> UpdateAsync(
        [FromBody] ExampleModel example,
        CancellationToken cancellationToken
    )
    {
        try
        {
            bool result = await _exampleService.UpdateAsync(
                example,
                cancellationToken
            );

            return result
                ? NotFound($"Example (Id = {example.Id}) not found!")
                : NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Route("{id}")]
    public async Task<IActionResult> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        try
        {
            bool result = await _exampleService.DeleteAsync(id, cancellationToken);

            return result ? NotFound($"Example (Id = {id}) not found!") : NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
