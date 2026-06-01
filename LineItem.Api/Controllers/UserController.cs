using System;
using System.Threading;
using System.Threading.Tasks;
using LineItem.Models;
using LineItem.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LineItem.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/users")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UserModel>> CreateAsync(
        [FromBody] UserModel user,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var id = await _userService.CreateAsync(user, cancellationToken);
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
    public async Task<ActionResult<UserModel>> GetByIdAsync(
        long id,
        CancellationToken cancellationToken
    )
    {
        var result = await _userService.RetrieveByIdAsync(id, cancellationToken);

        return result is null ? NotFound($"User with Id = {id} not found!") : Ok(result);
    }

    [HttpPut]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [Route("{id}")]
    public async Task<ActionResult<UserModel>> UpdateAsync(
        [FromBody] UserModel example,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var result = await _userService.UpdateAsync(
                example,
                cancellationToken
            );

            return result
                ? NotFound($"User with Id = {example.Id} not found!")
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
            var result = await _userService.DeleteAsync(id, cancellationToken);

            return result ? NotFound($"User with Id = {id} not found!") : NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}